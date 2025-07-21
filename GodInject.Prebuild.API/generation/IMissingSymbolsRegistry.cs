namespace GodInject.Prebuild.API.generation
{
    /// <summary>
    /// Registry used by generators to report missing symbols that block further analysis.
    /// </summary>
    /// <remarks>
    /// The framework will attempt to resolve these missing symbols in the next generation run (rerun).
    /// </remarks>
    public interface IMissingSymbolsRegistry
    {
        /// <summary>
        /// Gets the collection of currently registered missing symbols.
        /// </summary>
        /// <returns>A collection of symbol names.</returns>
        ICollection<string> GetMissingSymbols();

        /// <summary>
        /// Adds a missing symbol to the registry.
        /// </summary>
        /// <param name="symbol">The name of the missing symbol.</param>
        void Add(string symbol);

        /// <summary>
        /// Removes a symbol from the registry.
        /// </summary>
        /// <param name="symbol">The symbol to remove.</param>
        void Remove(string symbol);
    }

}
