using GodInject.Prebuild.API.IO;
using Microsoft.CodeAnalysis;

namespace GodInject.Prebuild.API.generation
{
    /// <summary>
    /// Collects dependencies of projects being processed
    /// </summary>
    public interface IDependencyCollector
    {
        // TODO: hooks for checking the documents/references (to pass/dismiss)
        void CollectDocument(string path, FileMetaRef metadata);
        MetadataReference[] CollectExecReferences();
    }
}
