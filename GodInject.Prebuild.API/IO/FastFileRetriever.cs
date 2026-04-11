using System.Runtime.InteropServices;

namespace GodInject.Prebuild.API.IO
{
    /// <summary>
    /// Provides fast access to files and their metadata, it's faster than <see cref="Directory"/> on Windows
    /// </summary>
    /// <remarks>
    /// Utilizes native system APIs (ex. <c>ntdll.dll</c> or <c>kernel32.dll</c>) for performance gains.
    /// Offers significant speedups on Windows, with moderate benefits on other operating systems.
    /// </remarks>
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

        /// <summary>
        /// Utilizes the chosen <see cref="_fileRetriever"/> for your system to get files in a specific directory
        /// </summary>
        /// <remarks>
        /// Only gets files from one directory, it doesn't recurse
        /// </remarks>
        /// <param name="path">The root directory to search in.</param>
        /// <param name="searchPattern">Win API Pattern (Not regex) default to *</param>
        /// <param name="initialCapacity">Optional initial capacity hint for internal allocation</param>
        /// <returns>A struct with file names and metadata which can be operated on using indexes</returns>
        public static FileMetaRef GetFiles(string path, string searchPattern = "*", int initialCapacity = 0)
        {
            return _fileRetriever.GetFilesAndDirectories(path, searchPattern, initialCapacity);
        }

        public static IEnumerable<(string path, FileMetaRef metadata)> GetFilesRecursive(string path, string searchPattern, string[]? excludeDirs = null, int initialCapacity = 0)
        {
            uint directoryBitValue = (uint)FileAttributes.Directory;
            FileMetaRef fileMetaRef = _fileRetriever.GetFilesAndDirectories(path, searchPattern, initialCapacity);
            foreach (var fileMeta in fileMetaRef)
            {
                if((fileMeta.FileAttributes & directoryBitValue) == directoryBitValue)
                {
                    string dirPath = Path.Join(path, fileMeta.Name);
                    yield return (dirPath, fileMeta);
                    if (excludeDirs != null && !excludeDirs.Contains(dirPath))
                    {
                        foreach (var childFileMeta in GetFilesRecursive(dirPath, searchPattern, excludeDirs, initialCapacity))
                        {
                            yield return childFileMeta;
                        }
                    }
                }
                else
                {
                    yield return (Path.Join(path, fileMeta.Name), fileMeta);
                }
            }
        }

        /// <summary>
        /// Checks whether ntdll.dll is available to be used in users workspace.
        /// </summary>
        /// <returns>true if ntdll.dll is available; false otherwise</returns>
        private static bool IsNtQueryDirectoryFileAvailable()
        {
            nint hModule = GetModuleHandle("ntdll.dll");
            if (hModule == nint.Zero)
                return false;

            nint proc = GetProcAddress(hModule, "NtQueryDirectoryFile");
            return proc != nint.Zero;
        }

    }
}
