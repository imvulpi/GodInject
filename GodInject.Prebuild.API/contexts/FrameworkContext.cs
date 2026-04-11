namespace GodInject.Prebuild.API.contexts
{
    /// <summary>
    /// Represents the main context of the framework, containing all core services and dependencies
    /// initialized during the startup phase.
    /// </summary>
    /// <remarks>
    /// This context is passed to the main loop and external modules (mods), providing them access to
    /// shared systems and registries. Mods can use this context to replace default implementations of
    /// interfaces, register their own components, or extend core functionality dynamically.
    /// </remarks>
    public class FrameworkContext(RuntimeContext runtimeContext, GenerationContext generationContext)
    {
        /// <summary>
        /// Provides access to runtime systems and state.
        /// See <see cref="RuntimeContext"/>.
        /// </summary>
        public RuntimeContext Runtime { get; set; } = runtimeContext;

        /// <summary>
        /// Provides access to generation systems and data.
        /// See <see cref="GenerationContext"/>.
        /// </summary>
        public GenerationContext Generation { get; set; } = generationContext;
    }
}
