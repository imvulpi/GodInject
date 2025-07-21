using GodInject.Prebuild.API.generation;

namespace GodInject.Prebuild.API.contexts
{
    /// <summary>
    /// Holds registry interfaces used during the generation process.
    /// </summary>
    /// <remarks>
    /// Provides access to various registries responsible for managing files used in generation, generators,
    /// and tracking missing symbols encountered during generation.
    /// </remarks>
    public class GenerationRegistries(ICsFileRegistry csFileRegistry, IGeneratorRegistry generatorRegistry, IMissingSymbolsRegistry missingSymbolsRegistry)
    {

        /// <summary>
        /// Registry for managing user C# (.cs) files that are later included in the generation.
        /// </summary>
        public ICsFileRegistry CsFileRegistry { get; set; } = csFileRegistry;

        /// <summary>
        /// Registry for managing generators used in the generation process.
        /// </summary>
        /// <remarks>
        /// Mods will use this registry to register their generators
        /// </remarks>
        public IGeneratorRegistry GeneratorRegistry { get; set; } = generatorRegistry;

        /// <summary>
        /// Registry for tracking symbols that are missing during generation.
        /// </summary>
        /// <remarks>
        /// The registry is used to resolve the missing symbols, 
        /// mods should register their missing symbols and continue with generation.
        /// </remarks>
        public IMissingSymbolsRegistry MissingSymbolsRegistry { get; set; } = missingSymbolsRegistry;
    }
}
