namespace GodInject.Prebuild.API.generation
{
    /// <summary>
    /// Interface for managing a collection of C# source file paths.
    /// </summary>
    /// <remarks>
    /// Used to turn the paths into documents used in generation
    /// </remarks>
    public interface ICsFileRegistry
    {
        /// <summary>
        /// Gets the list of registered C# file paths.
        /// </summary>
        /// <returns>A list of paths to .cs files.</returns>
        IList<string> GetCsFilePaths();

        /// <summary>
        /// Adds a C# file path to the registry.
        /// </summary>
        /// <param name="path">The path to the .cs file to add.</param>
        void Add(string path);

        /// <summary>
        /// Removes a C# file path from the registry.
        /// </summary>
        /// <param name="path">The path to the .cs file to remove.</param>
        void Remove(string path);
    }

}
