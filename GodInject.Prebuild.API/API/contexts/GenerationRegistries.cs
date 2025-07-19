using GodInject.Prebuild.API.generation;

namespace GodInject.Prebuild.API.contexts
{
    public class GenerationRegistries
    {
        public GenerationRegistries(ICsFileRegistry csFileRegistry, IGeneratorRegistry generatorRegistry, IMissingSymbolsRegistry missingSymbolsRegistry)
        {
            CsFileRegistry = csFileRegistry;
            GeneratorRegistry = generatorRegistry;
            MissingSymbolsRegistry = missingSymbolsRegistry;
        }

        public ICsFileRegistry CsFileRegistry { get; set; }
        public IGeneratorRegistry GeneratorRegistry { get; set; }
        public IMissingSymbolsRegistry MissingSymbolsRegistry { get; set; }
    }
}
