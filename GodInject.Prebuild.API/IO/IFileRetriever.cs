namespace GodInject.Prebuild.API.IO
{
    /// <summary>
    /// Interface for file retrievers used in ex. <see cref="FastFileRetriever"/>
    /// </summary>
    public interface IFileRetriever
    {
        public FileMetaRef GetFilesAndDirectories(string path, string searchPattern, int initialCapacity = 0);
    }
}
