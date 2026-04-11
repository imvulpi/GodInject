using DryIoc;

namespace GodInject.container
{
    /// <summary>
    /// Wrapper for DryIoc container that provides a unified API for the generator and allows later switching.
    /// <para>Using it directly for resolving is bad, make use of <see cref="InjectAttribute"/> and the generators.</para>
    /// </summary>
    /// <remarks>
    /// Exposes basic methods, for more advanced situations use <see cref="GetContainer"/> and invoke DryIoc methods directly.
    /// </remarks>
    public static class InjectContainer
    {
        static InjectContainer()
        {
            Container = new Container(rules => rules
                .WithoutUseInterpretation()
                .WithExpressionGeneration(false) // Disables FastExpressionCompiler and other that cause errors with AOT
            );
        }

        /// <summary>
        /// The container instance used in the resolution and registration
        /// </summary>
        private static readonly IContainer Container = null;

        /// <summary>
        /// Gets the <see cref="Container"/> instance
        /// </summary>
        /// <returns>A container instance</returns>
        public static IContainer GetContainer()
        {
            return Container;
        }

        /// <summary>
        /// Resolves a <typeparamref name="T"/> or if it can't find it - <paramref name="ifUnresolved"/> action is performed
        /// </summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <param name="ifUnresolved">Action to be performed when dependency is not found</param>
        /// <returns>A resolved <typeparamref name="T"/></returns>
        public static T Resolve<T>(IfUnresolved ifUnresolved = IfUnresolved.Throw)
        {
            return Container.Resolve<T>(ifUnresolved);
        }

        /// <summary>
        /// Resolves a <typeparamref name="T"/> with a service key or if it can't find it - <paramref name="ifUnresolved"/> action is performed
        /// </summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <param name="serviceKey">Service key used to resolve <typeparamref name="T"/> (It distinguishes the same types/interfaces)</param>
        /// <param name="ifUnresolved">Action to be performed when dependency is not found</param>
        /// <returns>A resolved <typeparamref name="T"/></returns>
        public static T Resolve<T>(object serviceKey,
            IfUnresolved ifUnresolved = IfUnresolved.Throw, Type requiredServiceType = null,
            object[] args = null)
        {
            return Container.Resolve<T>(serviceKey, ifUnresolved, requiredServiceType, args);
        }

        /// <summary>
        /// Registers a new service and its implementation, the creation is performed by the container when resolving using the <see cref="IReuse"/>
        /// </summary>
        /// <typeparam name="TService">Service</typeparam>
        /// <typeparam name="TImplementation">Implementation of the service</typeparam>
        public static void Register<TService, TImplementation>(IReuse reuse = null, Made made = null, 
            IfAlreadyRegistered? ifAlreadyRegistered = null, Setup setup = null, object serviceKey = null) where TImplementation : TService
        {
            Container.Register<TService, TImplementation>(reuse, made, setup, ifAlreadyRegistered, serviceKey);
        }

        /// <summary>
        /// Registers an <typeparamref name="T"/> instance
        /// </summary>
        /// <param name="serviceKey">Service key that can be used to distinguish this <typeparamref name="T"/> from other registered</param>
        public static void Register<T>(T instance, IfAlreadyRegistered? ifAlreadyRegistered = null, Setup setup = null, object serviceKey = null)
        {
            Container.RegisterInstance(instance, ifAlreadyRegistered, setup, serviceKey);
        }

        /// <summary>
        /// Registers a delegate of <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        public static void RegisterDelegate<T>(Func<IResolverContext, T> factoryDelegate,
            IReuse reuse = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
            object serviceKey = null)
        {
            Container.RegisterDelegate(factoryDelegate, reuse, setup, ifAlreadyRegistered, serviceKey);
        }

        /// <summary>
        /// Wrapper around <see cref="Register{TService, TImplementation}(IReuse, Made, IfAlreadyRegistered?, Setup, object)"/> but already sets <see cref="IReuse"/> to <see cref="Reuse.Singleton"/>
        /// </summary>
        /// <typeparam name="TService">Service to register</typeparam>
        public static void RegisterSingleton<TService>()
        {
            Container.Register<TService>(Reuse.Singleton);
        }

        /// <summary>
        /// Wrapper around <see cref="Register{TService, TImplementation}(IReuse, Made, IfAlreadyRegistered?, Setup, object)"/> but already sets <see cref="IReuse"/> to <see cref="Reuse.Scoped"/>
        /// </summary>
        /// <typeparam name="TService">Service to register</typeparam>
        public static void RegisterScoped<TService>()
        {
            Container.Register<TService>(Reuse.Scoped);
        }

        /// <summary>
        /// Wrapper around <see cref="Register{TService, TImplementation}(IReuse, Made, IfAlreadyRegistered?, Setup, object)"/> but already sets <see cref="IReuse"/> to <see cref="Reuse.Transient"/>
        /// </summary>
        /// <typeparam name="TService">Service to register</typeparam>
        public static void RegisterTransient<TService>()
        {
            Container.Register<TService>(Reuse.Transient);
        }

        /// <summary>
        /// Unregisters a service
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="serviceKey">Service key used to register the service</param>
        public static void Unregister<T>(object serviceKey = null, FactoryType factoryType = FactoryType.Service, Func<Factory, bool> condition = null)
        {
            Container.Unregister<T>(serviceKey, factoryType, condition);
        }
        
        /// <summary>
        /// Checks whether a service is registered
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="serviceKey">Service key that was used to register the service</param>
        /// <returns>True if registered; otherwise false</returns>
        public static bool IsRegistered<T>(object serviceKey = null, FactoryType factoryType = FactoryType.Service, Func<Factory, bool> condition = null)
        {
            return Container.IsRegistered<T>(serviceKey, factoryType, condition);
        }

        /// <summary>
        /// Clears the cache for specified <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>True if target service was found; otherwise false.</returns>
        public static bool ClearCache<T>(FactoryType? factoryType, object serviceKey)
        {
            return Container.ClearCache<T>(factoryType, serviceKey);
        }
    }
}
