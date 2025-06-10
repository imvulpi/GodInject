using System.Runtime.InteropServices;

namespace GodInject.Prebuild.files
{
    public static class FilesystemRetriever
    {
        private static readonly IFileRetriever _fileRetriever;
        static FilesystemRetriever()
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

        public static FileMetaRef GetFilesAndDirectories(string path, string searchPattern, int initialCapacity = 0)
        {
            return _fileRetriever.GetFilesAndDirectories(path, searchPattern, initialCapacity);
        }
    }
}
