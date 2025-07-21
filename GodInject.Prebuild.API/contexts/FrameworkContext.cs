namespace GodInject.Prebuild.API.contexts
{
    public class FrameworkContext
    {
        public FrameworkContext(RuntimeContext runtimeContext, GenerationContext generationContext)
        {
            this.Runtime = runtimeContext;
            this.Generation = generationContext;
        }

        public RuntimeContext Runtime { get; set; }
        public GenerationContext Generation { get; set; }
    }
}
