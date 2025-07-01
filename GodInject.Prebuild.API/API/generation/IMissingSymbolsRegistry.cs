namespace GodInject.Prebuild.API.generation
{

    /// <summary>
    /// Registry mostly used by generators to add missing symbols preventing them from analyzing other symbols
    /// The framework will attempt to resolve the missing symbols in the next run.
    /// </summary>
    public interface IMissingSymbolsRegistry
    {
        public void AddMissingSymbol(string symbol);
        public void RemoveMissingSymbol(string symbol);
    }
}
