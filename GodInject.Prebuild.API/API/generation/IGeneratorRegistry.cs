namespace GodInject.Prebuild.API.generation
{
    /// <summary>
    /// Registry used to add custom generators to the framework,
    /// You can also remove the custom generators.
    /// </summary>
    public interface IGeneratorRegistry
    {
        public IList<IGenerator> GetGenerators();
        public void AddGenerator(IGenerator generator);
        public void RemoveGenerator(IGenerator generator);
    }
}
