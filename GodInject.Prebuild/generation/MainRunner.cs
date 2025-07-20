using GodInject.Prebuild.API.contexts;
using GodInject.Prebuild.API.data;
using GodInject.Prebuild.API.generation;
using GodInject.Prebuild.API.logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GodInject.Prebuild.generation
{
    internal class MainRunner(FrameworkContext frameworkContext)
    {
        public FrameworkContext FrameworkContext { get; private set; } = frameworkContext;
        public IDependencyCollector DependencyCollector => FrameworkContext.generationContext.GenerationTools.DependencyCollector;
        public IDependencyResolver DependencyResolver => FrameworkContext.generationContext.GenerationTools.DependencyResolver;
        public IDataCoupler<GenerationInfo> GenerationInfoCoupler => FrameworkContext.generationContext.GenerationDataContext.GenerationInfoCoupler;
        public ExecutionPaths ExecutionPaths => FrameworkContext.runtimeContext.ExecutionPaths;
        public IDataCoupler<StructuresInfo> StructureInfoCoupler => FrameworkContext.generationContext.GenerationDataContext.StructuresInfoCoupler;
        public StructuresInfo StructuresInfo => FrameworkContext.generationContext.GenerationDataContext.StructuresInfo;
        public ILogger Logger => FrameworkContext.runtimeContext.Logger;
        public ICsFileRegistry CsFileRegistry => FrameworkContext.generationContext.GenerationRegistry.CsFileRegistry;
        public IGeneratorRegistry GeneratorRegistry => FrameworkContext.generationContext.GenerationRegistry.GeneratorRegistry;
        public IMissingSymbolsRegistry MissingSymbolsRegistry => FrameworkContext.generationContext.GenerationRegistry.MissingSymbolsRegistry;

        private int currentFails = 0;
        private int maxFails = 2;
        public async Task Run() {
            await Logger.LogInfo("Running");

            AdhocWorkspace workspace = new();
            ProjectInfo? projectInfo = CreateProject();
            if (projectInfo == null) return;
            Project project = workspace.AddProject(projectInfo);

            GenerationInfo currentGenerationInfo = new()
            {
                DocumentsCheckedCount = 0,
            };

            IList<IGenerator> generators = GeneratorRegistry.GetGenerators();

            await Logger.LogInfo($"Starting to process {generators.Count} Generators");
            StartGenerators(generators);
            await Logger.LogInfo("Starting the main loop");
            while (true)
            {
                currentGenerationInfo.DocumentsCheckedCount = 0;
                foreach (var document in project.Documents)
                {
                    var syntaxRoot = await document.GetSyntaxRootAsync();
                    var semanticModel = await document.GetSemanticModelAsync();
                    currentGenerationInfo.DocumentsCheckedCount++;

                    // get syntax trees here for optimaztion
                    for (int i = 0; i < generators.Count; i++)
                    {
                        var generator = generators[i];
                        generator.Generate(document, syntaxRoot, semanticModel);
                    }

                    if (syntaxRoot == null) continue;
                    ReconstructStructuresInfo(syntaxRoot, document);
                }

                ICollection<string> missingSymbols = MissingSymbolsRegistry.GetMissingSymbols();
                if (missingSymbols.Count > 0)
                {
                    await Logger.LogInfo("Trying to resolve missing symbols");
                    if (currentFails == maxFails)
                    {
                        currentGenerationInfo.WasSuccessful = false;
                        await Logger.LogError($"Generators failed due to missing symbols, ending now, " +
                            $"the missing symbols: {string.Join(", ", missingSymbols)}");
                        break;
                    }
                    else
                    {
                        Project? newProject = await ResolveMissingSymbols(workspace, projectInfo, missingSymbols);
                        if (newProject != null) project = newProject;
                    }
                }
                else
                {
                    currentGenerationInfo.WasSuccessful = true;
                    break;
                }
            }


            EndGenerators(generators);
            currentGenerationInfo.LastRun = DateTime.Now.ToFileTimeUtc();
            await GenerationInfoCoupler.SaveAsync(currentGenerationInfo);
            await StructureInfoCoupler.SaveAsync(StructuresInfo);
            await Logger.LogInfo("Stopped");
        }


        private async Task<Project?> ResolveMissingSymbols(AdhocWorkspace workspace, ProjectInfo projectInfo, ICollection<string> missingSymbols)
        {
            await Logger.LogInfo("Trying to resolve missing symbols");
            currentFails++;

            var resolvedDocuments = DependencyResolver.ResolveDocuments(ExecutionPaths.ProjectDirPath, missingSymbols.ToArray(), projectInfo.Id);
            Solution solution = workspace.CurrentSolution;
            foreach (var document in resolvedDocuments)
            {
                solution = solution.AddDocument(document);
            }

            workspace.TryApplyChanges(solution);
            Project? newProject = workspace.CurrentSolution.GetProject(projectInfo.Id);
            MissingSymbolsRegistry.GetMissingSymbols().Clear();
            return newProject;
        }

        private void ReconstructStructuresInfo(SyntaxNode syntaxRoot, Document? document)
        {
            var types = syntaxRoot.DescendantNodes().OfType<TypeDeclarationSyntax>();
            var enums = syntaxRoot.DescendantNodes().OfType<EnumDeclarationSyntax>();

            List<(string, StructureInfo)> structureInfos = new List<(string, StructureInfo)>();
            var namespaceName = "";
            foreach (var type in types)
            {
                if (type.Parent is BaseNamespaceDeclarationSyntax namespaceSyntax)
                {
                    namespaceName = namespaceSyntax.Name.ToString();
                }

                var name = type.Identifier.Text;
                StructureType structureType = StructureType.Unknown;
                switch (type.Keyword.Text)
                {
                    case "class":
                        structureType = StructureType.Class;
                        break;
                    case "struct":
                        structureType = StructureType.Struct;
                        break;
                    case "interface":
                        structureType = StructureType.Interface;
                        break;
                    case "record":
                        structureType = StructureType.Record;
                        break;
                }

                structureInfos.Add((name, new StructureInfo(structureType, namespaceName)));
            }

            foreach (var type in enums)
            {
                structureInfos.Add((type.Identifier.Text, new StructureInfo(StructureType.Enum, namespaceName)));
            }

            foreach (var kvp in structureInfos)
            {
                (string name, StructureInfo info) = kvp;
                info.FilePath = document.FilePath;
                info.Namespace = namespaceName;
                if (!StructuresInfo.NameAndStructureInfo.TryAdd(name, info))
                {
                    StructuresInfo.NameAndStructureInfo[name] = info;
                }
            }
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

        private ProjectInfo? CreateProject()
        {
            string projectName = Path.GetFileNameWithoutExtension(ExecutionPaths.CsprojPath);
            ProjectId projectId = ProjectId.CreateNewId(projectName);

            var referencesArray = DependencyCollector.CollectExecReferences();
            var documents = CsFileRegistry.GetPaths();
            DocumentInfo[] documentInfos = new DocumentInfo[documents.Length];
            for (int i = 0; i < documents.Length; i++)
            {
                string path = documents[i];
                documentInfos[i] = DocumentInfo.Create(
                    DocumentId.CreateNewId(projectId),
                    Path.GetFileNameWithoutExtension(path),
                    null,
                    SourceCodeKind.Script,
                    TextLoader.From(TextAndVersion.Create(SourceText.From(File.ReadAllText(path)), VersionStamp.Create(), path)),
                    path
                );
            }

            List<MetadataReference> references =
            [
                .. referencesArray,
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            ];

            var projectInfo = ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                projectName,
                projectName,
                LanguageNames.CSharp
            ).WithMetadataReferences(references).WithDocuments(documentInfos);
            return projectInfo;
        }
    }
}
