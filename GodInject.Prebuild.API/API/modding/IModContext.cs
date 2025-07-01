using GodInject.Prebuild.API.generation;
using GodInject.Prebuild.API.logging;

namespace GodInject.Prebuild.API.modding
{
    /// <summary>
    /// Context provided to mods, allows resolving and replacing dependencies in a container.
    /// By default provides a logger and generator registry without the need to resolve
    /// </summary>
    public interface IModContext
    {
        public T Resolve<T>();
        public bool TryResolve<T>(out T service);
        public void Replace<T>(T service);
        public ILogger Logger { get; }
        public IGeneratorRegistry GeneratorRegistry { get; }
    }
}
