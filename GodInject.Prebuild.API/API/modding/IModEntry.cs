using GodInject.Prebuild.API.contexts;

namespace GodInject.Prebuild.API.modding
{
    /// <summary>
    /// The main entry of any mod.
    /// !!! Must be used with <see cref="ModEntryAttribute"/>
    /// </summary>
    public interface IModEntry
    {
        /// <summary>
        /// Function that will get called during mod loading,
        /// before any generation occurs.
        /// </summary>
        public void Initialize(FrameworkContext modContext);
    }
}
