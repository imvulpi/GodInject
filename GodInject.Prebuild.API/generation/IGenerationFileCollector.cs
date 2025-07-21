using GodInject.Prebuild.API.IO;
using Microsoft.CodeAnalysis;

namespace GodInject.Prebuild.API.generation
{
    /// <summary>
    /// Collects files to be later turned into <see cref="DocumentInfo"/> and processed by generators.
    /// </summary>
    public interface IGenerationFileCollector
    {
        /// <summary>
        /// Registers a file using the specified path and metadata.
        /// </summary>
        /// <param name="path">The file path of the .</param>
        /// <param name="metadata">Metadata associated with the dependency.</param>
        void CollectDependency(string path, FileMetaRef metadata);

        /// <summary>
        /// Collects metadata references required for generation.
        /// </summary>
        /// <returns>An array of <see cref="MetadataReference"/> objects used during generation.</returns>
        MetadataReference[] CollectExecReferences();
    }
}
