
using GodInject.Prebuild.API.collections;

namespace GodInject.Prebuild.API.IO
{
    /// <summary>
    /// File retriever for operating systems other than windows.
    /// Uses the commonly used <see cref="Directory"/> methods
    /// </summary>
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
