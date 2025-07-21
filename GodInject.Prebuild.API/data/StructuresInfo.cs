using MemoryPack;

namespace GodInject.Prebuild.API.generation
{
    /// <summary>
    /// Stores a simplified structure of the project C# files (.cs) and it's structures
    /// </summary>
    /// <remarks>
    /// Used to resolve missing symbols in the project during generation.
    ///</remarks>
    [MemoryPackable]
    public partial class StructuresInfo
    {
        /// <summary>
        /// Maps file paths (as keys) to their corresponding file signatures.
        /// </summary>
        /// <remarks>
        /// <see cref="FileSignature"/>s are used to locate files whenether they're moved
        /// </remarks>
        public Dictionary<string, FileSignature> FilePaths { get; set; } = new();

        /// <summary>
        /// Maps structure names to their structure information.
        /// </summary>
        public Dictionary<string, StructureInfo> NameAndStructureInfo { get; set; } = new();

        /// <summary>
        /// Converts a string representation of a structure type to an actual <see cref="StructureType"/>
        /// </summary>
        /// <param name="structureString">String representation of a structure type</param>
        /// <returns>Actual <see cref="StructureType"/> type</returns>
        public static StructureType GetStructureType(string structureString)
        {
            StructureType structure = StructureType.Unknown;
            switch (structureString)
            {
                case "class":
                    structure = StructureType.Class;
                    break;
                case "struct":
                    structure = StructureType.Struct;
                    break;
                case "interface":
                    structure = StructureType.Interface;
                    break;
                case "record":
                    structure = StructureType.Record;
                    break;
            }
            return structure;
        }
    }

    /// <summary>
    /// Stores pretty minimal information about a C# structure
    /// </summary>
    /// <param name="type">Type of the structure</param>
    /// <param name="namespace">Namespace the structure is in.</param>
    [MemoryPackable]
    public partial struct StructureInfo(StructureType type, string @namespace)
    {
        /// <summary>
        /// Type of the structure
        /// </summary>
        public StructureType Type { get; set; } = type;
        /// <summary>
        /// Namespace the structure is in.
        /// </summary>
        public string Namespace { get; set; } = @namespace;
        /// <summary>
        /// File path of the structure
        /// </summary>
        public string FilePath { get; set; } = "";
    }

    /// <summary>
    /// Fast and lightweight signature of a file used in <see cref="StructuresInfo"/>
    /// </summary>
    /// <remarks>
    /// Used to locate files whenether they're moved.
    /// - Technically it can be incorrect, but for this to happen it would need to be moved, NOT changed and have the same size in bytes and last write time as some other file. Using it provides a fast signature with negligible error levels
    /// </remarks>
    [MemoryPackable]
    public partial class FileSignature
    {
        /// <summary>
        /// Size in bytes of the file
        /// </summary>
        public long SizeInBytes { get; set; }

        /// <summary>
        /// Last write time of the file in Windows file time
        /// </summary>
        /// <remarks>
        /// Represents the number of 100-nanosecond intervals since January 1, 1601 (UTC).
        /// </remarks>
        public long LastWriteTime { get; set; }
    }

    /// <summary>
    /// Represents C# structure type
    /// </summary>
    public enum StructureType
    {
        Class,
        Struct,
        Interface,
        Enum,
        Record,
        Unknown
    }
}
