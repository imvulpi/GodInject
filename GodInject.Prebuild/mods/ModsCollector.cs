using GodInject.Prebuild.API.IO;
using System.Reflection;

namespace GodInject.Prebuild.mods
{
    internal class ModsCollector
    {
        public ModsCollector() { }
        public Assembly[] CollectMods(string path)
        {
            var files = FastFileRetriever.GetFilesRecursive(path, "*");
            List<Assembly> mods = new();
            foreach ((string filePath, FileMetaRef metadata) in files)
            {
                if ((metadata.FileAttributes & 16) != 16 &&
                    Path.GetExtension(filePath) == ".dll")
                {
                    mods.Add(Assembly.LoadFrom(filePath));
                }
            }
            return mods.ToArray();
        }
    }
}
