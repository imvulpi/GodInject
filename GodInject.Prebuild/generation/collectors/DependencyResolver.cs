using GodInject.Prebuild.API.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace GodInject.Prebuild.generation.collectors
{
    public class DependencyResolver : IDependencyResolver
    {
        public DocumentInfo[] ResolveDocuments(string path, string[] missingSymbols, ProjectId projectId)
        {
            List<DocumentInfo> resolvedDocuments = new();
            var files = FastFileRetriever.GetFilesRecursive(path, "*");
            foreach ((string filePath, FileMetaRef fileMeta) in files)
            {
                if ((fileMeta.FileAttributes & (uint)FileAttributes.Directory) == (uint)FileAttributes.Directory)
                    continue;

                for (int o = 0; o < missingSymbols.Length; o++)
                {
                    string missingClass = missingSymbols[o];

                    if (fileMeta.Name.Contains(missingClass, StringComparison.OrdinalIgnoreCase))
                    {
                        DocumentInfo document = DocumentInfo.Create(
                            DocumentId.CreateNewId(projectId),
                            Path.GetFileNameWithoutExtension(filePath),
                            null,
                            SourceCodeKind.Script,
                            TextLoader.From(TextAndVersion.Create(SourceText.From(File.ReadAllText(filePath)), VersionStamp.Create(), filePath)),
                            filePath
                        );
                        resolvedDocuments.Add(document);
                    }
                }                
            }
            return resolvedDocuments.ToArray();
        }

        public PortableExecutableReference[] ResolveReferences(string path, string[] missingReferences)
        {
            throw new NotImplementedException();
        }
    }
}
