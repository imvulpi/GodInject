using Microsoft.CodeAnalysis;

namespace GodInject.Prebuild.generation.collectors
{
    /// <summary>
    /// Collects dependencies of projects being processed
    /// </summary>
    public interface IDependencyCollector
    {
        // TODO: hooks for checking the documents/references (to pass/dismiss)
        DocumentInfo[] CollectDocuments(string path, ProjectId projectId);
        PortableExecutableReference[] CollectExecReferences();
    }
}
