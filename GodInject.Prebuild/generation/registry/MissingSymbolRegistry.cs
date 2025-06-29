namespace GodInject.Prebuild.generation.registry
{
    internal class MissingSymbolRegistry : IInternalMissingSymbolsRegistry
    {
        internal HashSet<string> MissingSymbols = new HashSet<string>();
        public void AddMissingSymbol(string symbol)
        {
            MissingSymbols.Add(symbol);
        }

        public ICollection<string> GetMissingSymbols()
        {
            return MissingSymbols;
        }

        public void RemoveMissingSymbol(string symbol)
        {
            MissingSymbols.Remove(symbol);
        }
    }
}
