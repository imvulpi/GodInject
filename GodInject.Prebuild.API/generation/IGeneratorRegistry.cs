namespace GodInject.Prebuild.API.generation
{
    /// <summary>
    /// Registry used to add custom generators to the framework,
    /// You can also remove the custom generators.
    /// </summary>
    public interface IGeneratorRegistry
    {
        public IList<IGenerator> GetGenerators();
        public void Add(IGenerator generator);
        public void Remove(IGenerator generator);
    }
}
