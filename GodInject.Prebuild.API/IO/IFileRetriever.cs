namespace GodInject.Prebuild.API.IO
{
    /// <summary>
    /// Interface for file retrievers, such as <see cref="FastFileRetriever"/>, used to retrieve file metadata efficiently.
    /// </summary>
    public interface IFileRetriever
    {
        /// <summary>
        /// Retrieves file and directory metadata for a given path and search pattern.
        /// </summary>
        /// <param name="path">The root directory to search in.</param>
        /// <param name="searchPattern">Win API Pattern (Not regex)</param>
        /// <param name="initialCapacity">Optional initial capacity hint for internal allocation.</param>
        /// <returns>A <see cref="FileMetaRef"/> containing metadata results.</returns>
        FileMetaRef GetFilesAndDirectories(string path, string searchPattern, int initialCapacity = 0);
    }
}
