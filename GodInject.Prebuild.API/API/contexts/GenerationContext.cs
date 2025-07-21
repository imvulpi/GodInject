namespace GodInject.Prebuild.API.contexts
{
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
