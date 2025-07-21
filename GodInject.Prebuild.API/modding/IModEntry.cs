using GodInject.Prebuild.API.contexts;

namespace GodInject.Prebuild.API.modding
{
    /// <summary>
    /// The main entry point for any mod.
    /// <para>Must be used together with <see cref="ModEntryAttribute"/>.</para>
    /// </summary>
    public interface IModEntry
    {
        /// <summary>
        /// Called during mod loading, before any generation takes place.
        /// </summary>
        /// <param name="modContext">The framework context providing access to shared systems and services.</param>
        void Initialize(FrameworkContext modContext);
    }
}
