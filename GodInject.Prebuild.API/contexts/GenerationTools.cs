using GodInject.Prebuild.API.generation;

namespace GodInject.Prebuild.API.contexts
{
    /// <summary>
    /// Contains tool classes used during or prior to the generation process.
    /// </summary>
    /// <remarks>
    /// These tools assist with dependency collection, dependency resolution using <see cref="StructuresInfo"/>,
    /// and validation of structures to ensure correctness or identify necessary corrections.
    /// </remarks>
    public class GenerationTools(IGenerationFileCollector generationFileCollector, IDependencyResolver dependencyResolver, IStructureInfoValidator structureInfoValidator)
    {
        /// <summary>
        /// Collects dependencies required during generation.
        /// </summary>
        public IGenerationFileCollector FileCollector { get; set; } = generationFileCollector;

        /// <summary>
        /// Resolves dependencies usually by using <see cref="StructuresInfo"/> and other context data.
        /// </summary>
        public IDependencyResolver DependencyResolver { get; set; } = dependencyResolver;

        /// <summary>
        /// Validates <see cref="StructuresInfo"/> to ensure it is correct or identify necessary corrections.
        /// </summary>
        public IStructureInfoValidator StructureInfoValidator { get; set; } = structureInfoValidator;
    }
}
