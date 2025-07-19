using GodInject.Prebuild.API.contexts;
using GodInject.Prebuild.API.data;
using GodInject.Prebuild.API.generation;
using GodInject.Prebuild.API.logging;
using GodInject.Prebuild.generation.registry;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;

namespace GodInject.Prebuild.generation
{
    internal class MainRunner
    {
        public MainRunner(FrameworkContext frameworkContext)
        {
            DependencyCollector = frameworkContext.generationContext.GenerationTools.DependencyCollector;
            DependencyResolver = frameworkContext.generationContext.GenerationTools.DependencyResolver;
            GenerationInfoCoupler = frameworkContext.generationContext.GenerationDataContext.GenerationInfoCoupler;
            ExecutionPaths = frameworkContext.runtimeContext.ExecutionPaths;
            Logger = frameworkContext.runtimeContext.Logger;
            CsFileRegistry = frameworkContext.generationContext.GenerationRegistry.CsFileRegistry;
            StructureInfoCoupler = frameworkContext.generationContext.GenerationDataContext.StructuresInfoCoupler;
            StructuresInfo = frameworkContext.generationContext.GenerationDataContext.StructuresInfo;
            MissingSymbolsRegistry = frameworkContext.generationContext.GenerationRegistry.MissingSymbolsRegistry;
            GeneratorRegistry = frameworkContext.generationContext.GenerationRegistry.GeneratorRegistry;
        }

        public IDependencyCollector DependencyCollector { get; private set; }
        public IDependencyResolver DependencyResolver { get; private set; }
        public IDataCoupler<GenerationInfo> GenerationInfoCoupler { get; private set; }
        public ExecutionPaths ExecutionPaths { get; private set; }
        public ILogger Logger { get; private set; }
        public ICsFileRegistry CsFileRegistry { get; set; }
        public IGeneratorRegistry GeneratorRegistry { get; private set; }
        public IMissingSymbolsRegistry MissingSymbolsRegistry { get; set; }
        public IDataCoupler<StructuresInfo> StructureInfoCoupler { get; set; }
        public StructuresInfo? StructuresInfo { get; set; }

        private int currentFails = 0;
        private int maxFails = 2;
        public async void Run() {
            Logger.LogInfo("Running");
            ProjectInfo projectInfo = CreateProject();
            AdhocWorkspace workspace = new AdhocWorkspace();
            Project project = workspace.AddProject(projectInfo);
            GenerationInfo currentGenerationInfo = new()
            {
                DocumentsCheckedCount = 0,
            };
            
            IList<IGenerator> generators = GeneratorRegistry.GetGenerators();
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
                        if (!StructuresInfo.NameAndStructureInfo.TryAdd(name, info))
                        {
                            StructuresInfo.NameAndStructureInfo[name] = info;
                        }
                    }
                }

                ICollection<string> missingSymbols = MissingSymbolsRegistry.GetMissingSymbols();
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
                    var resolvedDocuments = DependencyResolver.ResolveDocuments(ExecutionPaths.ProjectDirPath, missingSymbols.ToArray(), projectInfo.Id);
                    Solution solution = workspace.CurrentSolution;
                    foreach (var document in resolvedDocuments)
                    {
                        solution = solution.AddDocument(document);
                    }
                    workspace.TryApplyChanges(solution);
                    project = workspace.CurrentSolution.GetProject(projectInfo.Id);

                    var updatedProject = workspace.CurrentSolution.GetProject(projectInfo.Id);
                    var compilation = await updatedProject.GetCompilationAsync();
                    MissingSymbolsRegistry.GetMissingSymbols().Clear();
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
            Logger.LogInfo("Stopped");
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
