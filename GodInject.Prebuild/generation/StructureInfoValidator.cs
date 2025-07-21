using GodInject.Prebuild.API.generation;
using GodInject.Prebuild.API.IO;
using GodInject.Prebuild.API.logging;
using System.Runtime.CompilerServices;
namespace GodInject.Prebuild.generation
{
    public class StructureInfoValidator(StructuresInfo structuresInfo, ILogger logger) : IStructureInfoValidator
    {
        public List<KeyValuePair<string, FileSignature>> MissingFiles = [];
        public StructuresInfo StructuresInfo { get; set; } = structuresInfo;

        public void Switch(StructuresInfo structureInfo)
        {
            MissingFiles.Clear();
            StructuresInfo = structureInfo;
        }

        public void ProcessStructureInfo(IEnumerable<(string path, FileMetaRef metadata)> files)
        {
            logger.LogInfo("Starting structures validation");
            Dictionary<string, FileSignature> missingFilesCopy = StructuresInfo.FilePaths.ToDictionary();

            foreach (var (path, _) in files)
            {
                missingFilesCopy.Remove(path);
            }

            MissingFiles = missingFilesCopy.ToList();
            logger.LogInfo($"Ended structures validation with {MissingFiles.Count} missing files");
        }

        public void ValidateAndFixFile(string path, FileMetaRef fileMetadata)
        {
            if(StructuresInfo == null) return;
            if (Path.GetExtension(path) != ".cs") return;

            AddIfNotInStructure(path, fileMetadata);
            foreach ((string missingPath, FileSignature fileSignature) in MissingFiles)
            {
                bool filesSignaturesMatch = fileMetadata.RealSize == fileSignature.SizeInBytes && fileMetadata.LastWriteTime == fileSignature.LastWriteTime;

                if (filesSignaturesMatch)
                {
                    foreach (var nameAndStructure in StructuresInfo.NameAndStructureInfo)
                    {
                        (string structuresName, StructureInfo structuresInfo) = nameAndStructure;
                        if (structuresInfo.FilePath == missingPath)
                        {
                            structuresInfo.FilePath = path;
                            StructuresInfo.NameAndStructureInfo[structuresName] = structuresInfo;
                        }
                    }
                    break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddIfNotInStructure(string path, FileMetaRef fileMetadata)
        {
            if (!StructuresInfo.FilePaths.ContainsKey(path))
            {
                FileSignature fileSignature = new()
                {
                    LastWriteTime = fileMetadata.LastWriteTime,
                    SizeInBytes = fileMetadata.RealSize
                };

                StructuresInfo.FilePaths.Add(path, fileSignature);
            }
        }
    }
}
