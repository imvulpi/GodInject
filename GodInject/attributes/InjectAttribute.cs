namespace GodInject
{
    /// <summary>
    /// Attribute for the generator to know what properties or fields to inject, includes a service key which will be used to resolve the dependency.
    /// </summary>
    /// <remarks>
    /// Mark the property/field with attribute like this:
    /// <para><c>[Inject]</c> public IMyInterface SomeInterface { get; set; }</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class InjectAttribute : Attribute
    {
        public InjectAttribute() { }
        public InjectAttribute(object key)
        {
            Key = key;
        }

        /// <summary>
        /// Service key to be used when resolving the dependency
        /// </summary>
        public object Key { get; set; }
    }
}
