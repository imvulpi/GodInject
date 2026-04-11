using GodInject.Generator.generator;
using GodInject.Prebuild.API.contexts;
using GodInject.Prebuild.API.data;
using GodInject.Prebuild.API.generation;
using GodInject.Prebuild.API.logging;
using GodInject.Prebuild.API.modding;

namespace GodInject.Generator
{
    /// <summary>
    /// The entry of the generation module.
    /// </summary>
    [ModEntry(ModuleName, ModuleVersion, ModuleAuthor,
        "Generated automatic resolution of injections based on a global container" +
        "Resolves dependencies marked with [Inject] Attributes in a partial class")]
    public class ModEntry : IModEntry
    {
        public const string ModuleName = "AutoInject";
        public const string ModuleVersion = "2.0.0rc.1";
        public const string ModuleAuthor = "Vulpi";

        /// <summary>
        /// Initialization of the module, Creates a generator class and registers it.
        /// </summary>
        /// <param name="frameworkContext">Generation framework context</param>
        public void Initialize(FrameworkContext frameworkContext)
        {
            ILogger logger = frameworkContext.Runtime.Logger;
            logger.LogInfo($"[{ModuleName}][{ModuleVersion}] Module starts initialization");

            ExecutionPaths executionPaths = frameworkContext.Runtime.ExecutionPaths;
            IMissingSymbolsRegistry missingSymbolsRegistry = frameworkContext.Generation.Registries.MissingSymbolsRegistry;
            IGenerator generator = new AutoInjectGenerator(executionPaths, missingSymbolsRegistry)
            {
                Logger = logger,
            };

            logger.LogInfo($"[{ModuleName}][{ModuleVersion}] Registering generator");
            IGeneratorRegistry registry = frameworkContext.Generation.Registries.GeneratorRegistry;
            registry.Add(generator);
            logger.LogInfo($"[{ModuleName}][{ModuleVersion}] Generator was registered");
        }
    }
}
