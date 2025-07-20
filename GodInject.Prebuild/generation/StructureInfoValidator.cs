using GodInject.Prebuild.API.generation;
using GodInject.Prebuild.API.IO;
using GodInject.Prebuild.API.logging;
namespace GodInject.Prebuild.generation
{
    public class StructureInfoValidator(StructuresInfo structuresInfo, ILogger logger) : IStructureInfoValidator
    {
        public List<KeyValuePair<string, FileSignature>> MissingFiles = [];
        public StructuresInfo StructuresInfo { get; set; } = structuresInfo;
        private ILogger _logger = logger;

        public void Switch(StructuresInfo structureInfo)
        {
            MissingFiles.Clear();
            StructuresInfo = structureInfo;
        }

        public void ProcessStructureInfo(IEnumerable<(string path, FileMetaRef metadata)> files)
        {
            _logger.LogInfo("Starting validation");
            Dictionary<string, FileSignature> missingFilesCopy = StructuresInfo.FilePaths.ToDictionary();

            foreach (var (path, _) in files)
            {
                if (missingFilesCopy.ContainsKey(path))
                {
                    missingFilesCopy.Remove(path);
                }
            }

            MissingFiles = missingFilesCopy.ToList();
            _logger.LogInfo($"Ended validation with {MissingFiles.Count} missing files");
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
