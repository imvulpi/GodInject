using GodInject.Prebuild.API.generation;
using Microsoft.CodeAnalysis;

namespace GodInject.Prebuild.generation.registry
{
    internal interface IInternalCsFileRegistry : ICsFileRegistry
    {
        public string[] GetDocuments();
    }
}
