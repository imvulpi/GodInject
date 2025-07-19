namespace GodInject.Prebuild.API.contexts
{
    public class FrameworkContext
    {
        public FrameworkContext(RuntimeContext runtimeContext, GenerationContext generationContext)
        {
            this.runtimeContext = runtimeContext;
            this.generationContext = generationContext;
        }

        public RuntimeContext runtimeContext { get; set; }
        public GenerationContext generationContext { get; set; }
    }
}
