using GodInject.Prebuild.API.generation;
using GodInject.Prebuild.API.IO;
using GodInject.Prebuild.API.logging;
namespace GodInject.Prebuild.generation
{
    public class StructureInfoValidator : IStructureInfoValidator
    {
        public StructureInfoValidator(StructuresInfo structuresInfo, ILogger logger)
        {
            _logger = logger;
            StructuresInfo = structuresInfo;
        }

        public List<KeyValuePair<string, FileSignature>> MissingFiles = [];
        public StructuresInfo StructuresInfo { get; set; }
        private ILogger _logger;

        public void Switch(StructuresInfo structureInfo)
        {
            MissingFiles.Clear();
            StructuresInfo = structureInfo;
        }
        
        public void ProcessStructureInfo() {
            _logger.LogInfo("Starting validation");
            if (StructuresInfo == null) return;
            foreach (var kvp in StructuresInfo.FilePaths)
            {
                (string path, FileSignature _) = kvp;
                if (!File.Exists(path))
                {
                    MissingFiles.Add(kvp);
                    StructuresInfo.FilePaths.Remove(path);
                }
            }
            _logger.LogInfo("Ended validation");
        }

        public void ValidateFile(string path, FileMetaRef fileMetadata)
        {
            if(StructuresInfo == null) return;
            if (Path.GetExtension(path) == ".cs")
            {
                if (!StructuresInfo.FilePaths.ContainsKey(path))
                {
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
