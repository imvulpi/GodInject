using GodInject.Prebuild.API.generation;
using GodInject.Prebuild.API.IO;
using GodInject.Prebuild.API.logging;
using System.Runtime.CompilerServices;
namespace GodInject.Prebuild.generation
{
    /// <summary>
    /// Validates <see cref="StructuresInfo"/>, finds missing paths within it and attempts to fix it.
    /// <para>
    /// Call <see cref="ProcessStructureInfo(IEnumerable{ValueTuple{string, FileMetaRef}})"/> once BEFORE <see cref="ValidateAndFixFile(string, FileMetaRef)"/>
    /// </para>
    /// <para>
    /// Call <see cref="ValidateAndFixFile(string, FileMetaRef)"/> in a loop of project files in order to 
    /// find missing paths and fix the structures of missing files.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The methods need to be called by the program in specific order and implementation, this allows for better performance, as this way the validator doesn't need to collect files multiple times.
    /// </remarks>
    /// <param name="structuresInfo">Structure info to be validated</param>
    /// <param name="logger">Logger to be used for logging</param>
    public class StructureInfoValidator(StructuresInfo structuresInfo, ILogger logger) : IStructureInfoValidator
    {
        /// <summary>
        /// Missing files in the <see cref="StructuresInfo"/> (moved/deleted)
        /// </summary>
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

        /// <summary>
        /// Adds a file's path and medatada to the <see cref="StructuresInfo.FilePaths"/> if it's not there.
        /// </summary>
        /// <param name="path">Path of the file</param>
        /// <param name="fileMetadata">Metadata of the file</param>
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
