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
            List<Assembly> mods = [];
            foreach ((string filePath, FileMetaRef metadata) in files)
            {
                bool isNotADirectory = (metadata.FileAttributes & (uint)FileAttributes.Directory) != (uint)FileAttributes.Directory;
                if (isNotADirectory && Path.GetExtension(filePath) == ".dll")
                {
                    mods.Add(Assembly.LoadFrom(filePath));
                }
            }
            return mods.ToArray();
        }
    }
}
