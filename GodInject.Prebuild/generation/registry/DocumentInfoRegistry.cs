using Microsoft.CodeAnalysis;

namespace GodInject.Prebuild.generation.registry
{
    internal class DocumentInfoRegistry : IInternalCsFileRegistry
    {
        List<string> _documents = [];

        public string[] GetDocuments()
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
