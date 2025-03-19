using Microsoft.CodeAnalysis;

namespace GodInject.Analyzers.generators.data
{
    public struct InjectedProperty
    {
        public IPropertySymbol PropertySymbol { get; set; }
        public string ServiceKey { get; set; }
        public InjectedProperty(IPropertySymbol symbol, string serviceKey)
        {
            PropertySymbol = symbol;
            ServiceKey = serviceKey;
        }
    }
}
