using MemoryPack;

namespace GodInject.Prebuild.API.generation
{
    /// <summary>
    /// A class for structures in a program for easy dependency resolution
    /// </summary>
    [MemoryPackable]
    public partial class StructuresInfo
    {
        public Dictionary<string, FileSignature> FilePaths { get; set; } = [];
        public Dictionary<string, StructureInfo> NameAndStructureInfo { get; set; } = [];
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

    [MemoryPackable]
    public partial struct StructureInfo(StructureType type, string @namespace)
    {
        public StructureType Type { get; set; } = type;
        public string Namespace { get; set; } = @namespace;
        public string FilePath { get; set; } = "";
    }

    [MemoryPackable]
    public partial class FileSignature
    {
        public long SizeInBytes { get; set; }
        public long LastWriteTime { get; set; }
    }


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
