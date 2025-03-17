using DryIoc;
using DryIoc.ImTools;
using System;

namespace GodInject.container
{
    /// <summary>
    /// Simple wrapper for DryIoc container.<br></br>
    /// Exposes basic methods, for more advanced situations use <see cref="GetContainer"/> and invoke DryIoc methods directly.
    /// </summary>
    public static class InjectContainer
    {
        static InjectContainer()
        {
            Container = new Container(rules => rules
                .WithoutUseInterpretation()
                .WithExpressionGeneration(false) // Disables FastExpressionCompiler and other that cause errors with AOT
            );
        }

        private static readonly IContainer Container = null;

        public static IContainer GetContainer()
        {
            return Container;
        }

        public static T Resolve<T>(IfUnresolved ifUnresolved = IfUnresolved.Throw)
        {
            return Container.Resolve<T>(ifUnresolved);
        }

        public static T Resolve<T>(object serviceKey,
            IfUnresolved ifUnresolved = IfUnresolved.Throw, Type requiredServiceType = null,
            object[] args = null)
        {
            return Container.Resolve<T>(serviceKey, ifUnresolved, requiredServiceType, args);
        }

        public static void Register<TService, TImplementation>(IReuse reuse = null, Made made = null, 
            IfAlreadyRegistered? ifAlreadyRegistered = null, Setup setup = null, object serviceKey = null) where TImplementation : TService
        {
            Container.Register<TService, TImplementation>(reuse, made, setup, ifAlreadyRegistered, serviceKey);
        }

        public static void Register<T>(T instance, IfAlreadyRegistered? ifAlreadyRegistered = null, Setup setup = null, object serviceKey = null)
        {
            Container.RegisterInstance(instance, ifAlreadyRegistered, setup, serviceKey);
        }

        public static void RegisterDelegate<T>(Func<IResolverContext, T> factoryDelegate,
            IReuse reuse = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
            object serviceKey = null)
        {
            Container.RegisterDelegate(factoryDelegate, reuse, setup, ifAlreadyRegistered, serviceKey);
        }

        public static void RegisterSingleton<TService>()
        {
            Container.Register<TService>(Reuse.Singleton);
        }

        public static void RegisterScoped<TService>()
        {
            Container.Register<TService>(Reuse.Scoped);
        }

        public static void RegisterTransient<TService>()
        {
            Container.Register<TService>(Reuse.Transient);
        }

        public static void UnRegistered<T>(object serviceKey = null, FactoryType factoryType = FactoryType.Service, Func<Factory, bool> condition = null)
        {
            Container.Unregister<T>(serviceKey, factoryType, condition);
        }

        public static bool IsRegistered<T>(object serviceKey = null, FactoryType factoryType = FactoryType.Service, Func<Factory, bool> condition = null)
        {
            return Container.IsRegistered<T>(serviceKey, factoryType, condition);
        }

        public static bool ClearCache<T>(FactoryType? factoryType, object serviceKey)
        {
            return Container.ClearCache<T>(factoryType, serviceKey);
        }
    }
}
