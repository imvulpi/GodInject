using GodInject.Prebuild.API.generation;

namespace GodInject.Prebuild.generation.registry
{
    /// <inheritdoc cref="IGeneratorRegistry"/>
    internal class GeneratorRegistry : IGeneratorRegistry
    {
        internal List<IGenerator> Generators = [];

        public void Add(IGenerator generator)
        {
            Generators.Add(generator);
        }

        public void Remove(IGenerator generator)
        {
            Generators.Remove(generator);
        }

        public IList<IGenerator> GetGenerators()
        {
            return Generators;
        }
    }
}
