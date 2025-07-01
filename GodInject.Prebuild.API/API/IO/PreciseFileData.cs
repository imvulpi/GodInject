#nullable enable

namespace GodInject.Prebuild.API.IO
{
    /// <summary>
    /// More manageable structure for file metadata.
    /// </summary>
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
}
