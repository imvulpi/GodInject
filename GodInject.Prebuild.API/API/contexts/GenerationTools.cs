using GodInject.Prebuild.API.generation;

namespace GodInject.Prebuild.API.contexts
{
    public class GenerationTools
    {
        public GenerationTools(IDependencyCollector dependencyCollector, IDependencyResolver dependencyResolver, IStructureInfoValidator structureInfoValidator)
        {
            DependencyCollector = dependencyCollector;
            DependencyResolver = dependencyResolver;
            StructureInfoValidator = structureInfoValidator;
        }

        public IDependencyCollector DependencyCollector { get; set; }
        public IDependencyResolver DependencyResolver { get; set; }
        public IStructureInfoValidator StructureInfoValidator { get; set; }
    }
}
