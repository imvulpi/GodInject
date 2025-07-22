using Microsoft.CodeAnalysis;

namespace GodInject.Generator.generator.data
{
    /// <summary>
    /// A property that is marked with [Inject] attribute
    /// contains the service key that is used in the Inject attribute constructor
    /// </summary>
    public struct InjectedProperty(IPropertySymbol symbol, string? serviceKey)
    {
        /// <summary>
        /// The symbol of the property marked with [Inject]
        /// </summary>
        public IPropertySymbol PropertySymbol { get; set; } = symbol;

        /// <summary>
        /// Service key of the property, or null if no symbol key is used.
        /// </summary>
        public string? ServiceKey { get; set; } = serviceKey;
    }
}
