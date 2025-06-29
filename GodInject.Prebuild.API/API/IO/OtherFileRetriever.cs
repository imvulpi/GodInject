using GodInject.Prebuild.utils.collections;

namespace GodInject.Prebuild.API.IO
{
    public class OtherFileRetriever : IFileRetriever
    {
        public FileMetaRef GetFilesAndDirectories(string path, string searchPattern, int initialCapacity = 0)
        {
            string[] paths = Directory.GetFiles(path, searchPattern);
            RangeList<FileInfo> infos = new RangeList<FileInfo>(paths.Length);
            for (int i = 0; i < paths.Length; i++)
            {
                infos[i] = new(paths[i]);
            }
            return new FileMetaRef(infos);
        }
    }
}
