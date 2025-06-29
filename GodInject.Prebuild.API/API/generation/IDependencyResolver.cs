using Microsoft.CodeAnalysis;

namespace GodInject.Prebuild.generation.collectors
{
    /// <summary>
    /// Finds and resolves missing dependencies
    /// </summary>
    public interface IDependencyResolver
    {
        public DocumentInfo[] ResolveDocuments(string path, string[] missingSymbols, ProjectId projectId);
        public PortableExecutableReference[] ResolveReferences(string path, string[] missingReferences);
    }
}
