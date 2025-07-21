using GodInject.Prebuild.API.generation;

namespace GodInject.Prebuild.generation.registry
{
    /// <inheritdoc cref="IMissingSymbolsRegistry"/>
    internal class MissingSymbolRegistry : IMissingSymbolsRegistry
    {
        internal HashSet<string> MissingSymbols = [];
        public void Add(string symbol)
        {
            MissingSymbols.Add(symbol);
        }

        public void Remove(string symbol)
        {
            MissingSymbols.Remove(symbol);
        }

        public ICollection<string> GetMissingSymbols()
        {
            return MissingSymbols;
        }
    }
}
