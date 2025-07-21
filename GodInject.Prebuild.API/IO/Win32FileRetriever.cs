using GodInject.Prebuild.API.collections;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace GodInject.Prebuild.API.IO
{
    /// <summary>
    /// File retriever using Win32 API, allows retrieving files from older windows systems.
    /// Uses kernel32.dll, is more stable
    /// </summary>
    public class Win32FileRetriever : IFileRetriever
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WIN32_FIND_DATA
        {
            public FileAttributes dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [Flags]
        enum FindExInfoLevels : int
        {
            Standard = 0,
            Basic = 1
        }

        enum FindExSearchOps : int
        {
            NameMatch = 0,
            LimitToDirectories = 1,
            LimitToDevices = 2
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr FindFirstFileEx(
            string lpFileName,
            FindExInfoLevels fInfoLevelId,
            out WIN32_FIND_DATA lpFindFileData,
            FindExSearchOps fSearchOp,
            IntPtr lpSearchFilter,
            uint dwAdditionalFlags);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool FindNextFile(
            IntPtr hFindFile,
            out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll")]
        static extern bool FindClose(IntPtr hFindFile);

        public IEnumerable<PreciseFileData> EnumerateFiles(string path, string searchPattern = "*")
        {
            var findData = new WIN32_FIND_DATA();
            string searchPath = Path.Combine(path, searchPattern);
            IntPtr findHandle = FindFirstFileEx(
                searchPath,
                FindExInfoLevels.Basic, // Basic info = faster than standard
                out findData,
                FindExSearchOps.NameMatch,
                IntPtr.Zero,
                0);

            if (findHandle == new IntPtr(-1))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            try
            {
                do
                {
                    string fileName = findData.cFileName;
                    if (fileName != "." && fileName != "..")
                    {
                        long combinedCreationTime = (((long)(findData.ftCreationTime.dwHighDateTime) << 32) | (uint)findData.ftCreationTime.dwLowDateTime);
                        long combinedlastAcessTime = (((long)(findData.ftLastAccessTime.dwHighDateTime) << 32) | (uint)findData.ftLastAccessTime.dwLowDateTime);
                        long combinedlastWriteTime = (((long)(findData.ftLastWriteTime.dwHighDateTime) << 32) | (uint)findData.ftLastWriteTime.dwLowDateTime);
                        PreciseFileData preciseFileData = new PreciseFileData()
                        {
                            CreationTime = combinedCreationTime,
                            LastAccessTime = combinedlastAcessTime,
                            LastWriteTime = combinedlastWriteTime,
                            ChangeTime = null,
                            RealSize = (long)findData.nFileSizeHigh << 32 | findData.nFileSizeLow,
                            Name = fileName,
                            AlternateName= findData.cAlternateFileName,
                            Path=Path.Combine(path, fileName),
                        };
                        yield return preciseFileData;
                    }
                }
                while (FindNextFile(findHandle, out findData));
            }
            finally
            {
                FindClose(findHandle);
            }
        }

        public FileMetaRef GetFilesAndDirectories(string path, string searchPattern = "*", int initialCapacity = 0)
        {
            var data = new RangeList<WIN32_FIND_DATA>(initialCapacity);
            var findData = new WIN32_FIND_DATA();
            string searchPath = Path.Combine(path, searchPattern);
            IntPtr findHandle = FindFirstFileEx(
                searchPath,
                FindExInfoLevels.Basic, // Basic info = faster than standard
                out findData,
                FindExSearchOps.NameMatch,
                IntPtr.Zero,
                0);

            if (findHandle == new IntPtr(-1))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            try
            {
                do
                {
                    string fileName = findData.cFileName;
                    if (fileName != "." && fileName != "..")
                    {
                        data.Add(findData);
                    }
                }
                while (FindNextFile(findHandle, out findData));
            }
            finally
            {
                FindClose(findHandle);
            }

            return new(data);
        }
    }
}
