using Microsoft.CodeAnalysis;

namespace GodInject.Generator.generator.data
{
    /// <summary>
    /// A field that is marked with [Inject] attribute
    /// contains the service key that is used in the Inject attribute constructor
    /// </summary>
    public struct InjectedField(IFieldSymbol symbol, string? serviceKey)
    {
        /// <summary>
        /// The symbol of the field marked with [Inject]
        /// </summary>
        public IFieldSymbol FieldSymbol { get; set; } = symbol;

        /// <summary>
        /// Service key of the field, or null if no symbol key is used.
        /// </summary>
        public string? ServiceKey { get; set; } = serviceKey;
    }
}
