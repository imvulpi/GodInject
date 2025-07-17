using GodInject.Prebuild.API.data;
using GodInject.Prebuild.API.generation;
using GodInject.Prebuild.API.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace GodInject.Prebuild.generation.collectors
{
    public class DependencyCollector : IDependencyCollector
    {
        public DependencyCollector(GenerationInfo? lastGenerationInfo, string[] dllPaths, string outputPath, ICsFileRegistry registry)
        {
            LastGenerationInfo = lastGenerationInfo;
            DllPaths = dllPaths;
            OutputPath = outputPath;
            DocumentInfoRegistry = registry;
        }

        public GenerationInfo? LastGenerationInfo { get; set; }
        public string[] DllPaths { get; set; } 
        public string OutputPath { get; set; }
        public ICsFileRegistry DocumentInfoRegistry { get; set; }

        public void CollectDocument(string path, FileMetaRef metadata)
        {
            if ((metadata.FileAttributes & (uint)FileAttributes.Directory) != 16)
            {
                if (!metadata.Name.EndsWith("cs"))
                    return;

                if (LastGenerationInfo == null || 
                    (LastGenerationInfo != null && LastGenerationInfo.LastRun < metadata.LastWriteTime))
                {
                    DocumentInfoRegistry.Add(path);
                }
                
            }
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
