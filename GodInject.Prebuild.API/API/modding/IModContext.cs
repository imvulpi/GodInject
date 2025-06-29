using GodInject.Prebuild.API.generation;
using GodInject.Prebuild.API.logging;

namespace GodInject.Prebuild.API.modding
{
    public interface IModContext
    {
        public T Resolve<T>();
        public bool TryResolve<T>(out T service);
        public void Replace<T>(T service);
        public ILogger Logger { get; }
        public IGeneratorRegistry GeneratorRegistry { get; }
    }
}
