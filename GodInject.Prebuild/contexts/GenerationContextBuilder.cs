using GodInject.Prebuild.API.contexts;
using GodInject.Prebuild.API.logging;
using GodInject.Prebuild.generation;
using GodInject.Prebuild.generation.collectors;
using GodInject.Prebuild.generation.registry;

namespace GodInject.Prebuild.contexts
{
    public class GenerationContextBuilder(ILogger logger)
    {
        public async Task<GenerationContext> BuildAsync(RuntimeContext runtimeContext)
        {
            GenerationDataContext generationDataContext = await new GenerationDataContextBuilder(logger).BuildAsync(runtimeContext.ExecutionPaths, runtimeContext.ExecutionSettings);
            StructureInfoValidator infoValidator = new(generationDataContext.StructuresInfo, logger);
            DependencyResolver dependencyResolver = new(generationDataContext.StructuresInfo);
            CsFileRegistry csFileRegistry = new();
            DependencyCollector dependencyCollector = new(generationDataContext.GenerationInfo, generationDataContext.AbsoluteDllPaths, runtimeContext.ExecutionPaths.GenerationOutputDirPath, csFileRegistry);

            return new GenerationContext(
                generationDataContext,
                new GenerationRegistries(csFileRegistry, new GeneratorRegistry(), new MissingSymbolRegistry()),
                new GenerationTools(dependencyCollector, dependencyResolver, infoValidator)
            );
        }
    }
}
