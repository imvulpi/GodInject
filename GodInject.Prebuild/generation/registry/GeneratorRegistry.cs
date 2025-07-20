using GodInject.Prebuild.API.generation;

namespace GodInject.Prebuild.generation.registry
{
    internal class GeneratorRegistry : IGeneratorRegistry
    {
        public GeneratorRegistry()
        {
            Generators = new List<IGenerator>();
        }
        internal List<IGenerator> Generators { get; set; }

        public void AddGenerator(IGenerator generator)
        {
            Generators.Add(generator);
        }

        public void RemoveGenerator(IGenerator generator)
        {
            Generators.Remove(generator);
        }

        public IList<IGenerator> GetGenerators()
        {
            return Generators;
        }
    }
}
