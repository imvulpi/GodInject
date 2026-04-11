namespace GodInject.Prebuild.API.contexts
{
    /// <summary>
    /// A wrapper context that provides access to all generation systems and dependencies.
    /// </summary>
    /// <remarks>
    /// Holds references to <see cref="GenerationDataContext"/>, <see cref="GenerationRegistries"/>,
    /// and <see cref="GenerationTools"/>. Used as a centralized access point for generation logic.
    /// </remarks>
    public class GenerationContext
    {
        public GenerationContext(GenerationDataContext generationDataContext, GenerationRegistries generationRegistry, GenerationTools generationTools)
        {
            Data = generationDataContext;
            Registries = generationRegistry;
            Tools = generationTools;
        }

        public GenerationDataContext Data { get; set; }
        public GenerationRegistries Registries { get; set; }
        public GenerationTools Tools { get; set; }
    }
}
