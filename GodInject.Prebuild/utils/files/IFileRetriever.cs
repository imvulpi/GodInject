namespace GodInject.Prebuild.files
{
    public interface IFileRetriever
    {
        public FileMetaRef GetFilesAndDirectories(string path, string searchPattern, int initialCapacity = 0);
    }
}
