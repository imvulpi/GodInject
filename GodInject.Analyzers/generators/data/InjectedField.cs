using Microsoft.CodeAnalysis;

namespace GodInject.Analyzers.generators.data
{
    public struct InjectedField
    {
        public IFieldSymbol PropertySymbol { get; set; }
        public string ServiceKey { get; set; }
        public InjectedField(IFieldSymbol symbol, string serviceKey)
        {
            PropertySymbol = symbol;
            ServiceKey = serviceKey;
        }
    }
}
