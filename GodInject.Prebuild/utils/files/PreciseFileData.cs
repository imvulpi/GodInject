#nullable enable

namespace GodInject.Prebuild.files
{
    public struct PreciseFileData
    {
        public long CreationTime;
        public long LastAccessTime;
        public long LastWriteTime;
        public long? ChangeTime;
        public long RealSize;
        public long? AllocationSize;
        public uint FileAttributes;
        public string? AlternateName;
        public string Name;
        public string Path;
    }

    public enum SourceType
    {
        WinNT,
        Win32,
        Other
    }
}
