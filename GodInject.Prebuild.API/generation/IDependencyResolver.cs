using Microsoft.CodeAnalysis;

namespace GodInject.Prebuild.API.generation
{
    /// <summary>
    /// Finds and resolves missing dependencies during generation.
    /// </summary>
    public interface IDependencyResolver
    {
        /// <summary>
        /// Attempts to resolve documents that define the given missing symbols.
        /// </summary>
        /// <param name="missingSymbols">The symbols that are currently unresolved.</param>
        /// <param name="projectId">The project ID for which the resolution is being performed.</param>
        /// <returns>An array of <see cref="DocumentInfo"/> representing resolved documents.</returns>
        DocumentInfo[] ResolveDocuments(string[] missingSymbols, ProjectId projectId);
    }
}
