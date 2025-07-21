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
            GenerationRegistries generationRegistries = new GenerationRegistries(new CsFileRegistry(), new GeneratorRegistry(), new MissingSymbolRegistry());

            DependencyCollector dependencyCollector = new(generationRegistries, generationDataContext);
            DependencyResolver dependencyResolver = new(generationDataContext);
            StructureInfoValidator infoValidator = new(generationDataContext.StructuresInfo, logger);

            return new GenerationContext(
                generationDataContext,
                generationRegistries,
                new GenerationTools(dependencyCollector, dependencyResolver, infoValidator)
            );
        }
    }
}
