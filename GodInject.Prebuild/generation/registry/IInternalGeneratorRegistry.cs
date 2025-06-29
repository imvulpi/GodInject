using GodInject.Prebuild.API.generation;

namespace GodInject.Prebuild.generation.registry
{
    internal interface IInternalGeneratorRegistry : IGeneratorRegistry
    {
        public IList<IGenerator> GetGenerators();
    }
}
