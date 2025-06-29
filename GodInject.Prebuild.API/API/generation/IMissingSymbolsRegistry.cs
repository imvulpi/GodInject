namespace GodInject.Prebuild.API.generation
{
    public interface IMissingSymbolsRegistry
    {
        public void AddMissingSymbol(string symbol);
        public void RemoveMissingSymbol(string symbol);
    }
}
