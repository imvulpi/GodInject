namespace GodInject.Prebuild.files
{
    public class OtherFileRetriever : IFileRetriever
    {
        public FileMetaRef GetFilesAndDirectories(string path, string searchPattern, int initialCapacity = 0)
        {
            string[] paths = Directory.GetFiles(path, searchPattern);
            FileInfo[] infos = new FileInfo[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                infos[i] = new(paths[i]);
            }
            return new FileMetaRef(infos);
        }
    }
}
