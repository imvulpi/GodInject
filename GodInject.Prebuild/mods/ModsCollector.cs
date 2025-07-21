using GodInject.Prebuild.API.IO;
using System.Reflection;

namespace GodInject.Prebuild.mods
{
    /// <summary>
    /// Collects external mods in a specific path, doesn't evaluate their validity.
    /// </summary>
    internal class ModsCollector
    {
        public ModsCollector() { }
        /// <summary>
        /// Collects possible .dll mods from a directory and loads their assembly.
        /// </summary>
        /// <param name="directoryPath">Path to search for .dlls</param>
        /// <returns>Loaded assemblies of possible mods</returns>
        public Assembly[] CollectMods(string directoryPath)
        {
            var files = FastFileRetriever.GetFilesRecursive(directoryPath, "*");
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
