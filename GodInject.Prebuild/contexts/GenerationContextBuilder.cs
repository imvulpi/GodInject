using GodInject.Prebuild.API.contexts;
using GodInject.Prebuild.API.logging;
using GodInject.Prebuild.generation;
using GodInject.Prebuild.generation.collectors;
using GodInject.Prebuild.generation.registry;

namespace GodInject.Prebuild.contexts
{
    /// <summary>
    /// Builds the <see cref="GenerationContext"/> using <see cref="BuildAsync(RuntimeContext)"/>
    /// </summary>
    /// <param name="logger">Logger to be used in new instances and logging</param>
    public class GenerationContextBuilder(ILogger logger)
    {
        /// <summary>
        /// Creates the <see cref="GenerationContext"/> using <paramref name="runtimeContext"/> and <see cref="logger"/> instance
        /// </summary>
        /// <param name="runtimeContext">Runtime context to be used in building of the context</param>
        /// <returns>A <see cref="GenerationContext"/> with filled dependencies</returns>
        public async Task<GenerationContext> BuildAsync(RuntimeContext runtimeContext)
        {
            await logger.LogInfo("Creating generation context");
            GenerationDataContext generationDataContext = await new GenerationDataContextBuilder(logger).BuildAsync(runtimeContext.ExecutionPaths, runtimeContext.ExecutionSettings);
            GenerationRegistries generationRegistries = new GenerationRegistries(new CsFileRegistry(), new GeneratorRegistry(), new MissingSymbolRegistry());

            GenerationFileCollector dependencyCollector = new(generationRegistries, generationDataContext);
            DependencyResolver dependencyResolver = new(generationDataContext);
            StructureInfoValidator infoValidator = new(generationDataContext.StructuresInfo, logger);

            await logger.LogInfo("Created generation context");
            return new GenerationContext(
                generationDataContext,
                generationRegistries,
                new GenerationTools(dependencyCollector, dependencyResolver, infoValidator)
            );
        }
    }
}
