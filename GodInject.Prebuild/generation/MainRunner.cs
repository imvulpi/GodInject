using DryIoc;
using GodInject.Prebuild.API.data;
using GodInject.Prebuild.API.generation;
using GodInject.Prebuild.API.logging;
using GodInject.Prebuild.generation.collectors;
using GodInject.Prebuild.generation.registry;
using Microsoft.CodeAnalysis;

namespace GodInject.Prebuild.generation
{
    internal class MainRunner
    {
        public MainRunner(IDependencyCollector collector, IDependencyResolver resolver, IDataCoupler<GenerationInfo> generationInfoCoupler, ExecutionPaths executionPaths, ILogger logger)
        {
            Collector = collector;
            Resolver = resolver;
            GenerationInfoCoupler = generationInfoCoupler;
            ExecutionPaths = executionPaths;
            Logger = logger;
        }

        public IDependencyCollector Collector { get; private set; }
        public IDependencyResolver Resolver { get; private set; }
        public IDataCoupler<GenerationInfo> GenerationInfoCoupler { get; private set; }
        public ExecutionPaths ExecutionPaths { get; private set; }
        public ILogger Logger { get; private set; }

        private int currentFails = 0;
        private int maxFails = 2;
        public void Run() {
            ProjectInfo projectInfo = CreateProject();
            AdhocWorkspace workspace = new AdhocWorkspace();
            Project project = workspace.AddProject(projectInfo);
            GenerationInfo currentGenerationInfo = new()
            {
                FilesGeneratedCount = 0,
                FilesCheckedCount = 0,
                FilesIgnoredCount = 0,
            };

            IInternalGeneratorRegistry generatorRegistry = Dependencies.Container.Resolve<IInternalGeneratorRegistry>();
            IInternalMissingSymbolsRegistry symbolsRegistry = Dependencies.Container.Resolve<IInternalMissingSymbolsRegistry>();
            IList<IGenerator> generators = generatorRegistry.GetGenerators();

            Logger.LogInfo($"Starting to process {generators.Count} Generators");
            StartGenerators(generators);

            while (true)
            {
                foreach (var document in project.Documents)
                {
                    currentGenerationInfo.FilesCheckedCount++;
                    for (int i = 0; i < generators.Count; i++)
                    {
                        var generator = generators[i];
                        generator.Generate(document);
                    }
                }

                ICollection<string> missingSymbols = symbolsRegistry.GetMissingSymbols();
                if (missingSymbols.Count > 0)
                {
                    if (currentFails == maxFails)
                    {
                        currentGenerationInfo.WasSuccessful = false;
                        Logger.LogError($"Generators failed due to missing symbols, ending now, " +
                            $"the missing symbols: {string.Join(", ", missingSymbols)}");
                        break;
                    }

                    currentFails++;
                    Resolver.ResolveDocuments(ExecutionPaths.ProjectDirPath, missingSymbols.ToArray(), projectInfo.Id);
                }
                else
                {
                    currentGenerationInfo.WasSuccessful = true;
                    break;
                }
            }

            EndGenerators(generators);
            currentGenerationInfo.LastRun = DateTime.Now.ToFileTimeUtc();
            GenerationInfoCoupler.SaveAsync(currentGenerationInfo);
        }

        private void StartGenerators(IList<IGenerator> generators)
        {
            foreach (var generator in generators)
            {
                generator.Start();
            }
        }

        private void EndGenerators(IList<IGenerator> generators)
        {
            foreach (var generator in generators)
            {
                generator.End();
            }
        }

        private ProjectInfo CreateProject()
        {
            string projectName = Path.GetFileNameWithoutExtension(ExecutionPaths.CsprojPath);
            ProjectId projectId = ProjectId.CreateNewId(projectName);

            var references = Collector.CollectExecReferences();
            var documents = Collector.CollectDocuments(ExecutionPaths.ProjectDirPath, projectId);

            var projectInfo = ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                projectName,
                projectName,
                LanguageNames.CSharp
            ).WithMetadataReferences(references).WithDocuments(documents);
            return projectInfo;
        }
    }
}
