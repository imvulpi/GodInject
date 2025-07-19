namespace GodInject.Prebuild.API.contexts
{
    public class GenerationContext
    {
        public GenerationContext(GenerationDataContext generationDataContext, GenerationRegistries generationRegistry, GenerationTools generationTools)
        {
            GenerationDataContext = generationDataContext;
            GenerationRegistry = generationRegistry;
            GenerationTools = generationTools;
        }

        public GenerationDataContext GenerationDataContext { get; set; }
        public GenerationRegistries GenerationRegistry { get; set; }
        public GenerationTools GenerationTools { get; set; }
    }
}
