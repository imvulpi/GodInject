using System.Runtime.InteropServices;

namespace GodInject.Prebuild.API.IO
{
    /// <summary>
    /// Used to get files and files metadata from system faster than <see cref="Directory"/>
    /// Significant improvements on Windows, lesser improvements on Linux and other OSes.
    /// </summary>
    public static class FastFileRetriever
    {
        private static readonly IFileRetriever _fileRetriever;
        static FastFileRetriever()
        {
            if (OperatingSystem.IsWindows())
            {
                if (IsNtQueryDirectoryFileAvailable())
                {
                    _fileRetriever = new WinNTFileRetriever();
                }
                else
                {
                    _fileRetriever = new Win32FileRetriever();
                }
            }
            else
            {
                _fileRetriever = new OtherFileRetriever();
            }
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern nint GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        static extern nint GetProcAddress(nint hModule, string procName);
        private static bool IsNtQueryDirectoryFileAvailable()
        {
            nint hModule = GetModuleHandle("ntdll.dll");
            if (hModule == nint.Zero)
                return false;
            
            nint proc = GetProcAddress(hModule, "NtQueryDirectoryFile");
            return proc != nint.Zero;
        }

        public static FileMetaRef GetFiles(string path, string searchPattern, int initialCapacity = 0)
        {
            return _fileRetriever.GetFilesAndDirectories(path, searchPattern, initialCapacity);
        }

        public static IEnumerable<(string path, FileMetaRef metadata)> GetFilesRecursive(string path, string searchPattern, int initialCapacity = 0)
        {
            uint directoryBitValue = (uint)FileAttributes.Directory;
            FileMetaRef fileMetaRef = _fileRetriever.GetFilesAndDirectories(path, searchPattern, initialCapacity);
            foreach (var fileMeta in fileMetaRef)
            {
                if((fileMeta.FileAttributes & directoryBitValue) == directoryBitValue)
                {
                    string dirPath = Path.Join(path, fileMeta.Name);
                    yield return (dirPath, fileMeta);
                    foreach (var childFileMeta in GetFilesRecursive(dirPath, searchPattern, initialCapacity))
                    {
                        yield return childFileMeta;
                    }
                }
                else
                {
                    yield return (Path.Join(path, fileMeta.Name), fileMeta);
                }
            }
        }
    }
}
