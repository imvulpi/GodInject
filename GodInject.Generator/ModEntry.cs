using GodInject.Generator.injection_generator;
using GodInject.Prebuild.API.data;
using GodInject.Prebuild.API.generation;
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
        public void Initialize(IModContext modContext)
        {
            modContext.Logger.LogInfo($"[{ModuleName}][{ModuleVersion}] Module starts initialization");
            
            ExecutionSettings executionSettings = modContext.Resolve<ExecutionSettings>();
            ExecutionPaths executionPaths = modContext.Resolve<ExecutionPaths>();
            IMissingSymbolsRegistry missingSymbolsRegistry = modContext.Resolve<IMissingSymbolsRegistry>();
            IGenerator generator = new AutoInjectGenerator(executionSettings, executionPaths, missingSymbolsRegistry)
            {
                Logger = modContext.Logger,
            };
            IGeneratorRegistry registry = modContext.Resolve<IGeneratorRegistry>();
            modContext.Logger.LogInfo($"{ModuleName} {ModuleVersion} - {registry}");
            registry.AddGenerator(generator);
            modContext.Logger.LogInfo($"Added generator");
        }
    }
}
