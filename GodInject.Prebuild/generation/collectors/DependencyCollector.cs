using GodInject.Prebuild.API.data;
using GodInject.Prebuild.API.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace GodInject.Prebuild.generation.collectors
{
    public class DependencyCollector : IDependencyCollector
    {
        public DependencyCollector(GenerationInfo? lastGenerationInfo, string[] dllPaths, string outputPath)
        {
            LastGenerationInfo = lastGenerationInfo;
            DllPaths = dllPaths;
            OutputPath = outputPath;
        }

        public GenerationInfo? LastGenerationInfo { get; set; }
        public string[] DllPaths { get; set; } 
        public string OutputPath { get; set; }

        public DocumentInfo[] CollectDocuments(string path, ProjectId projectId)
        {
            List<DocumentInfo> documents = new();
            var files = FastFileRetriever.GetFilesRecursive(path, "*", [OutputPath]);
            foreach (var file in files)
            {
                FileMetaRef fileMeta = file.metadata;
                string filePath = file.path;
                if ((fileMeta.FileAttributes & (uint)FileAttributes.Directory) != 16)
                {
                    if (!fileMeta.Name.EndsWith("cs"))
                        continue;

                    if (LastGenerationInfo == null || 
                       (LastGenerationInfo != null && LastGenerationInfo.LastRun < fileMeta.LastWriteTime))
                    {
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
            return documents.ToArray();
        }

        public PortableExecutableReference[] CollectExecReferences()
        {
            PortableExecutableReference[] references = new PortableExecutableReference[DllPaths.Length];
            for (int i = 0; i < DllPaths.Length; i++)
            {
                references[i] = MetadataReference.CreateFromFile(DllPaths[i]);
            }

            return references;
        }
    }
}
