#nullable enable

namespace GodInject.Prebuild.API.IO
{
    /// <summary>
    /// More manageable structure for file metadata.
    /// To be used when writing onto a disk
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
