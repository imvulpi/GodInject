using GodInject.Prebuild.API.IO;
namespace GodInject.Prebuild.generation
{
    public class StructureInfoValidator
    {
        public StructureInfoValidator(StructuresInfo? structuresInfo)
        {
            StructuresInfo = structuresInfo;
        }
        public StructuresInfo? StructuresInfo { get; set; }
        public List<KeyValuePair<string, FileSignature>> MissingFiles = [];
        public void Validate() {
            if (StructuresInfo == null) return;
            foreach (var kvp in StructuresInfo.FilePaths)
            {
                (string path, FileSignature _) = kvp;
                if (!File.Exists(path))
                {
                    Console.WriteLine("Invalid file found");
                    MissingFiles.Add(kvp);
                    StructuresInfo.FilePaths.Remove(path);
                }
            }
        }

        public void ProcessFile(string path, FileMetaRef fileMetadata)
        {
            if(StructuresInfo == null) return;
            if (Path.GetExtension(path) == ".cs")
            {
                if (!StructuresInfo.FilePaths.ContainsKey(path))
                {
                    Console.WriteLine($"\nFound a new file: {path}");
                    StructuresInfo.FilePaths.Add(path, new FileSignature() { LastWriteTime = fileMetadata.LastWriteTime, SizeInBytes = fileMetadata.RealSize });
                }
                foreach (var pathAndSignature in MissingFiles)
                {
                    (string missingPath, FileSignature fileSignature) = pathAndSignature;
                    if (fileMetadata.RealSize == fileSignature.SizeInBytes && fileMetadata.LastWriteTime == fileSignature.LastWriteTime)
                    {
                        foreach (var nameAndStructure in StructuresInfo.NameAndStructureInfo)
                        {
                            (string nameStructure, StructureInfo structureInfo) = nameAndStructure;
                            if (structureInfo.FilePath == missingPath)
                            {
                                Console.WriteLine($"Found a missing path in a structure {nameStructure}");
                                structureInfo.FilePath = path;
                                StructuresInfo.NameAndStructureInfo[nameStructure] = structureInfo;
                            }
                        }
                        break;
                    }
                }
            }
        }
    }
}
