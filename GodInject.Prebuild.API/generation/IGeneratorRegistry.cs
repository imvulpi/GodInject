namespace GodInject.Prebuild.API.generation
{
    /// <summary>
    /// Registry for managing custom generators used by the framework.
    /// </summary>
    /// <remarks>
    /// Allows adding and removing <see cref="IGenerator"/> instances at runtime.
    /// </remarks>
    public interface IGeneratorRegistry
    {
        /// <summary>
        /// Gets the list of registered generators.
        /// </summary>
        /// <returns>A list of <see cref="IGenerator"/> instances currently registered.</returns>
        IList<IGenerator> GetGenerators();

        /// <summary>
        /// Adds a custom generator to the registry.
        /// </summary>
        /// <param name="generator">The generator to add.</param>
        void Add(IGenerator generator);

        /// <summary>
        /// Removes a custom generator from the registry.
        /// </summary>
        /// <param name="generator">The generator to remove.</param>
        void Remove(IGenerator generator);
    }
}
