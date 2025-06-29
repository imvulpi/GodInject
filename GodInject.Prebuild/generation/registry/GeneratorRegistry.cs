using GodInject.Prebuild.API.generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodInject.Prebuild.generation.registry
{
    internal class GeneratorRegistry : IInternalGeneratorRegistry
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
