using GodInject.Prebuild.API.generation;

namespace GodInject.Prebuild.generation.registry
{
    /// <inheritdoc cref="ICsFileRegistry"/>
    internal class CsFileRegistry : ICsFileRegistry
    {
        internal List<string> CsFilePaths = [];

        public void Add(string document)
        {
            CsFilePaths.Add(document);
        }

        public void Remove(string document)
        {
            CsFilePaths.Remove(document);
        }

        public IList<string> GetCsFilePaths()
        {
            return CsFilePaths;
        }
    }
}
