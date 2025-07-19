using GodInject.Prebuild.API.generation;

namespace GodInject.Prebuild.generation.registry
{
    internal class CsFileRegistry : ICsFileRegistry
    {
        List<string> _documents = [];

        public string[] GetPaths()
        {
            return _documents.ToArray();
        }

        public void Add(string document)
        {
            _documents.Add(document);
        }

        public void Remove(string document)
        {
            _documents.Remove(document);
        }
    }
}
