using GodInject.Prebuild.API.IO;

namespace GodInject.Prebuild.API.generation
{
    /// <summary>
    /// Validates <see cref="StructuresInfo"/> and helps locate moved C# files.
    /// </summary>
    /// <remarks>
    /// Used during generation to update or verify the accuracy of structure information and file paths.
    /// </remarks>
    public interface IStructureInfoValidator
    {
        /// <summary>
        /// Gets the current <see cref="StructuresInfo"/> instance used for validation.
        /// </summary>
        StructuresInfo StructuresInfo { get; }

        /// <summary>
        /// Replaces the current <see cref="StructuresInfo"/> with a new instance.
        /// </summary>
        /// <param name="structureInfo">The new structure info to use.</param>
        void Switch(StructuresInfo structureInfo);

        /// <summary>
        /// Processes a set of files and records missing files in the <see cref="StructuresInfo"/>
        /// </summary>
        /// <param name="files">A collection of file paths and their associated metadata.</param>
        void ProcessStructureInfo(IEnumerable<(string path, FileMetaRef metadata)> files);

        /// <summary>
        /// Validates a single file's path and metadata against the structure info.
        /// </summary>
        /// <remarks>
        /// This also attempts to fix missing files recorded by <see cref=" ProcessStructureInfo(IEnumerable{ValueTuple{string, FileMetaRef}})"/>
        /// </remarks>
        /// <param name="path">The path to the file being validated.</param>
        /// <param name="fileMetadata">The file's metadata</param>
        void ValidateAndFixFile(string path, FileMetaRef fileMetadata);
    }

}
