using GodInject.Prebuild.API.contexts;
using GodInject.Prebuild.API.data;
using GodInject.Prebuild.API.generation;
using GodInject.Prebuild.API.IO;
using Microsoft.CodeAnalysis;

namespace GodInject.Prebuild.generation.collectors
{
    public class DependencyCollector(GenerationRegistries generationRegistries, GenerationDataContext generationData) : IDependencyCollector
    {
        public GenerationRegistries GenerationRegistries { get; private set; } = generationRegistries;
        public GenerationDataContext GenerationData { get; private set; } = generationData;
        public GenerationInfo? LastGenerationInfo => GenerationData.GenerationInfo;

        public void CollectDocument(string path, FileMetaRef metadata)
        {
            bool isNotADirectory = (metadata.FileAttributes & (uint)FileAttributes.Directory) != (uint)FileAttributes.Directory;
            if (isNotADirectory)
            {
                if (!metadata.Name.EndsWith("cs"))
                    return;

                bool shouldRegenerate = LastGenerationInfo == null || LastGenerationInfo.LastRun < metadata.LastWriteTime;
                if (shouldRegenerate)
                {
                    GenerationRegistries.CsFileRegistry.Add(path);
                }
            }
        }

        public MetadataReference[] CollectExecReferences()
        {
            PortableExecutableReference[] references = new PortableExecutableReference[GenerationData.AbsoluteDllPaths.Length];
            for (int i = 0; i < GenerationData.AbsoluteDllPaths.Length; i++)
            {
                references[i] = MetadataReference.CreateFromFile(GenerationData.AbsoluteDllPaths[i]);
            }

            return references;
        }
    }
}
