using System;
namespace GodInject.Prebuild.API.generation
{
    public interface IGeneratorRegistry
    {
        public void AddGenerator(IGenerator generator);
        public void RemoveGenerator(IGenerator generator);
    }
}
