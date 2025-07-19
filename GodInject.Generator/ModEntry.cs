using GodInject.Generator.injection_generator;
using GodInject.Prebuild.API.contexts;
using GodInject.Prebuild.API.data;
using GodInject.Prebuild.API.generation;
using GodInject.Prebuild.API.logging;
using GodInject.Prebuild.API.modding;

namespace GodInject.Generator
{
    [ModEntry(ModuleName, ModuleVersion, ModuleAuthor,
        "Generated automatic resolution of injections based on a global container" +
        "Resolves dependencies marked with [Inject] Attributes in a partial class")]
    public class ModEntry : IModEntry
    {
        public const string ModuleName = "AutoInject";
        public const string ModuleVersion = "2.0.0a";
        public const string ModuleAuthor = "Vulpi";
        public void Initialize(FrameworkContext frameworkContext)
        {
            ILogger logger = frameworkContext.runtimeContext.Logger;
            logger.LogInfo($"[{ModuleName}][{ModuleVersion}] Module starts initialization");

            ExecutionSettings executionSettings = frameworkContext.runtimeContext.ExecutionSettings;
            ExecutionPaths executionPaths = frameworkContext.runtimeContext.ExecutionPaths;
            IMissingSymbolsRegistry missingSymbolsRegistry = frameworkContext.generationContext.GenerationRegistry.MissingSymbolsRegistry;
            IGenerator generator = new AutoInjectGenerator(executionSettings, executionPaths, missingSymbolsRegistry)
            {
                Logger = logger,
            };

            logger.LogInfo($"[{ModuleName}][{ModuleVersion}] Adding generator");
            IGeneratorRegistry registry = frameworkContext.generationContext.GenerationRegistry.GeneratorRegistry;
            registry.AddGenerator(generator);
            logger.LogInfo($"[{ModuleName}][{ModuleVersion}] Generator was added");
        }
    }
}
