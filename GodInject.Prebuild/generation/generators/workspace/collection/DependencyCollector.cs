using GodInject.Prebuild.files;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace GodInject.Prebuild.generation.generators.workspace.collection
{
    public class DependencyCollector
    {
        public GenerationInfo LastGenerationInfo { get; set; }
        private List<(string basePath, FileMetaRef metadata)> CachedFileMetadata { get; set; }
        public DependencyCollector(GenerationInfo lastGenerationInfo)
        {
            LastGenerationInfo = lastGenerationInfo;
        }

        public void GetLocalDocuments(string directoryPath, ProjectId projectId, ref List<DocumentInfo> documents)
        {
            CachedFileMetadata = new List<(string basePath, FileMetaRef metadata)>();
            FileMetaRef fileMetaRefs = FilesystemRetriever.GetFilesAndDirectories(directoryPath, "*", 0);
            CachedFileMetadata.Add((directoryPath, fileMetaRefs));
            for (int i = 0; i < fileMetaRefs.Length; i++)
            {
                FileMetaRef fileMeta = fileMetaRefs[i];
                if((fileMeta.FileAttributes | (uint)FileAttributes.Directory) == 16)
                {
                    GetLocalDocuments(Path.Join(directoryPath, fileMeta.Name), projectId, ref documents);
                }
                else
                {
                    if (!fileMeta.Name.EndsWith("cs")){
                        continue;
                    }
                    if (LastGenerationInfo.LastRun < fileMeta.LastWriteTime)
                    {
                        string filePath = Path.Join(directoryPath, fileMeta.Name);
                        DocumentInfo document = DocumentInfo.Create(
                            DocumentId.CreateNewId(projectId),
                            Path.GetFileNameWithoutExtension(filePath),
                            null,
                            SourceCodeKind.Script,
                            TextLoader.From(TextAndVersion.Create(SourceText.From(File.ReadAllText(filePath)), VersionStamp.Create(), filePath)),
                            filePath
                        );
                        documents.Add(document);
                    }
                }
            }
        }

        public void GetMissingDocuments(string searchDir, ProjectId projectId, 
            string[] missingClasses, ref List<DocumentInfo> resolvedDocuments)
        {
            if (CachedFileMetadata == null)
            {
                FileMetaRef fileMetaRefs = FilesystemRetriever.GetFilesAndDirectories(searchDir, "*.cs", 0);
                for (int i = 0; i < fileMetaRefs.Length; i++)
                {
                    FileMetaRef fileMeta = fileMetaRefs[i];
                    if ((fileMeta.FileAttributes | (uint)FileAttributes.Directory) != 0)
                    {
                        GetMissingDocuments(Path.Join(searchDir, fileMeta.Name), projectId, missingClasses, ref resolvedDocuments);
                    }
                    else
                    {
                        for (int o = 0; o < missingClasses.Length; o++)
                        {
                            string missingClass = missingClasses[o];

                            if (fileMeta.Name.Contains(missingClass, StringComparison.OrdinalIgnoreCase))
                            {
                                string filePath = Path.Join(searchDir, fileMeta.Name);
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
                }
            }
            else
            {
                // Resolving from cache
                for (int i = 0; i < CachedFileMetadata.Count; i++)
                {
                    (string basePath, FileMetaRef fileMetas) = CachedFileMetadata[i];
                    for (int j = 0; j < fileMetas.Length; j++)
                    {
                        FileMetaRef fileMeta = fileMetas[j];
                        for (int o = 0; o < missingClasses.Length; o++)
                        {
                            string missingClass = missingClasses[o];

                            if (fileMeta.Name.Contains(missingClass, StringComparison.OrdinalIgnoreCase))
                            {
                                string filePath = Path.Join(searchDir, fileMeta.Name);
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
                }

            }
        }

        public PortableExecutableReference[] GetReferences(string[] dllPaths)
        {
            PortableExecutableReference[] references = new PortableExecutableReference[dllPaths.Length];
            for (int i = 0; i < dllPaths.Length; i++)
            {
                references[i] = MetadataReference.CreateFromFile(dllPaths[i]);
            }

            return references;
        }
    }
}
