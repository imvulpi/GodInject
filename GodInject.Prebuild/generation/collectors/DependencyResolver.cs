using GodInject.Prebuild.API.contexts;
using GodInject.Prebuild.API.generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace GodInject.Prebuild.generation.collectors
{
    /// <inheritdoc cref="IDependencyResolver"/>
    public class DependencyResolver(GenerationDataContext generationData) : IDependencyResolver
    {
        public DocumentInfo[] ResolveDocuments(string[] missingSymbols, ProjectId projectId)
        {
            List<DocumentInfo> resolvedDocuments = [];
            foreach (string missing in missingSymbols)
            {
                if (generationData.StructuresInfo.NameAndStructureInfo.TryGetValue(missing, out var structureInfo))
                {
                    DocumentInfo document = DocumentInfo.Create(
                        DocumentId.CreateNewId(projectId),
                        Path.GetFileNameWithoutExtension(structureInfo.FilePath),
                        null,
                        SourceCodeKind.Regular,
                        TextLoader.From(TextAndVersion.Create(SourceText.From(File.ReadAllText(structureInfo.FilePath)), VersionStamp.Create(), structureInfo.FilePath)),
                        structureInfo.FilePath
                    );
                    resolvedDocuments.Add(document);
                }
            }
            return [.. resolvedDocuments];
        }
    }
}
