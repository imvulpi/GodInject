using DryIoc;
using GodInject.Prebuild.API.data;
using GodInject.Prebuild.API.generation;
using GodInject.Prebuild.API.logging;
using GodInject.Prebuild.generation.collectors;
using GodInject.Prebuild.generation.registry;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;

namespace GodInject.Prebuild.generation
{
    internal class MainRunner
    {
        public MainRunner(IDependencyCollector collector, IDependencyResolver resolver, IDataCoupler<GenerationInfo> generationInfoCoupler, 
            ExecutionPaths executionPaths, ILogger logger, IInternalCsFileRegistry registry, IDataCoupler<StructuresInfo> structureCoupler, StructuresInfo? structuresInfo)
        {
            Collector = collector;
            Resolver = resolver;
            GenerationInfoCoupler = generationInfoCoupler;
            ExecutionPaths = executionPaths;
            Logger = logger;
            Registry = registry;
            StructureCoupler = structureCoupler;
            StructuresInfo = structuresInfo;
        }

        public IDependencyCollector Collector { get; private set; }
        public IDependencyResolver Resolver { get; private set; }
        public IDataCoupler<GenerationInfo> GenerationInfoCoupler { get; private set; }
        public ExecutionPaths ExecutionPaths { get; private set; }
        public ILogger Logger { get; private set; }
        public IInternalCsFileRegistry Registry { get; set; }
        public IDataCoupler<StructuresInfo> StructureCoupler { get; set; }
        public StructuresInfo? StructuresInfo { get; set; }

        private int currentFails = 0;
        private int maxFails = 2;
        public async void Run() {
            ProjectInfo projectInfo = CreateProject();
            AdhocWorkspace workspace = new AdhocWorkspace();
            Project project = workspace.AddProject(projectInfo);
            GenerationInfo currentGenerationInfo = new()
            {
                DocumentsCheckedCount = 0,
            };
            
            IInternalGeneratorRegistry generatorRegistry = Dependencies.Container.Resolve<IInternalGeneratorRegistry>();
            IInternalMissingSymbolsRegistry symbolsRegistry = Dependencies.Container.Resolve<IInternalMissingSymbolsRegistry>();
            IList<IGenerator> generators = generatorRegistry.GetGenerators();
            StructuresInfo ??= new StructuresInfo();

            Logger.LogInfo($"Starting to process {generators.Count} Generators");
            StartGenerators(generators);

            while (true)
            {
                project = workspace.CurrentSolution.GetProject(projectInfo.Id);
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
                        generator.Generate(document);
                    }

                    if (syntaxRoot == null) continue;

                    var types = syntaxRoot.DescendantNodes().OfType<TypeDeclarationSyntax>();
                    var enums = syntaxRoot.DescendantNodes().OfType<EnumDeclarationSyntax>();
                    
                    List<(string, StructureInfo)> structureInfos = new List<(string, StructureInfo)>();
                    var namespaceName = "";
                    foreach (var type in types)
                    {
                        if (type.Parent is BaseNamespaceDeclarationSyntax namespaceSyntax) {
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
                        if (!StructuresInfo.NameAndStructureInfo.ContainsKey(name))
                        {
                            StructuresInfo.NameAndStructureInfo.Add(name, info);
                        }
                        else
                        {
                            StructuresInfo.NameAndStructureInfo[name] = info;
                        }
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

                    Debugger.Launch();
                    currentFails++;
                    var resolvedDocuments = Resolver.ResolveDocuments(ExecutionPaths.ProjectDirPath, missingSymbols.ToArray(), projectInfo.Id);
                    Solution solution = workspace.CurrentSolution;
                    foreach (var document in resolvedDocuments)
                    {
                        solution = solution.AddDocument(document);
                    }
                    workspace.TryApplyChanges(solution);
                    project = workspace.CurrentSolution.GetProject(projectInfo.Id);

                    var updatedProject = workspace.CurrentSolution.GetProject(projectInfo.Id);
                    var compilation = await updatedProject.GetCompilationAsync();
                    symbolsRegistry.GetMissingSymbols().Clear();
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
            await StructureCoupler.SaveAsync(StructuresInfo);
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

            var referencesArray = Collector.CollectExecReferences();
            var documents = Registry.GetDocuments();
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
