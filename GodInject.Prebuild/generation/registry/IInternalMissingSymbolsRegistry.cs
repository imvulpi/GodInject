using GodInject.Prebuild.API.generation;

namespace GodInject.Prebuild.generation.registry
{
    internal interface IInternalMissingSymbolsRegistry : IMissingSymbolsRegistry
    {
        public ICollection<string> GetMissingSymbols();
    }
}
