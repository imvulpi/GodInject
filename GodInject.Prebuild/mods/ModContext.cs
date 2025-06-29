using DryIoc;
using GodInject.Prebuild.API.generation;
using GodInject.Prebuild.API.logging;
using GodInject.Prebuild.API.modding;

namespace GodInject.Prebuild.mods
{
    internal class ModContext : IModContext
    {
        public ModContext(IContainer container)
        {
            _container = container;
        }
        public ILogger Logger => _container.Resolve<ILogger>(IfUnresolved.ReturnDefault);
        public IGeneratorRegistry GeneratorRegistry => _container.Resolve<IGeneratorRegistry>(IfUnresolved.ReturnDefault);
        private readonly IContainer _container;
        public void Replace<T>(T service)
        {
            _container.RegisterInstance<T>(service, IfAlreadyRegistered.Replace);
        }

        public T Resolve<T>()
        {
            if (typeof(T).IsPublic){
                return _container.Resolve<T>(IfUnresolved.ReturnDefault);
            }
            return default;
        }
        
        public bool TryResolve<T>(out T service)
        {
            if (_container.IsRegistered<T>()){
                service = _container.Resolve<T>();
                return true;
            }
            service = default;
            return false;
        }
    }
}
