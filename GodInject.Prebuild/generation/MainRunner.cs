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
        public IDependencyCollector DependencyCollector => FrameworkContext.Generation.Tools.DependencyCollector;
        public IDependencyResolver DependencyResolver => FrameworkContext.Generation.Tools.DependencyResolver;
        public IDataCoupler<GenerationInfo> GenerationInfoCoupler => FrameworkContext.Generation.Data.GenerationInfoCoupler;
        public ExecutionPaths ExecutionPaths => FrameworkContext.Runtime.ExecutionPaths;
        public IDataCoupler<StructuresInfo> StructureInfoCoupler => FrameworkContext.Generation.Data.StructuresInfoCoupler;
        public StructuresInfo StructuresInfo => FrameworkContext.Generation.Data.StructuresInfo;
        public ILogger Logger => FrameworkContext.Runtime.Logger;
        public ICsFileRegistry CsFileRegistry => FrameworkContext.Generation.Registries.CsFileRegistry;
        public IGeneratorRegistry GeneratorRegistry => FrameworkContext.Generation.Registries.GeneratorRegistry;
        public IMissingSymbolsRegistry MissingSymbolsRegistry => FrameworkContext.Generation.Registries.MissingSymbolsRegistry;
        public GenerationInfo CurrentGenerationInfo = new();

        private int currentFails = 0;
        private int maxFails = 2;
        public async Task Run() {
            await Logger.LogInfo("RUNS main loop");

            AdhocWorkspace workspace = new();
            ProjectInfo? projectInfo = CreateProject();
            if (projectInfo == null) return;
            Project project = workspace.AddProject(projectInfo);

            IList<IGenerator> generators = GeneratorRegistry.GetGenerators();
            await StartGenerators(generators);
            while (true)
            {
                CurrentGenerationInfo.DocumentsCheckedCount = 0;
                foreach (Document document in project.Documents)
                {
                    var syntaxRoot = await document.GetSyntaxRootAsync();
                    var semanticModel = await document.GetSemanticModelAsync();
                    CurrentGenerationInfo.DocumentsCheckedCount++;

                    for (int i = 0; i < generators.Count; i++)
                    {
                        var generator = generators[i];
                        generator.Generate(document, syntaxRoot, semanticModel);
                    }

                    if (syntaxRoot == null) continue;
                    await ReconstructStructuresInfo(syntaxRoot, document);
                }

                Project? reprocessedProject = await ProcessMissingSymbols(workspace, projectInfo);
                if(reprocessedProject == null) break;
                project = reprocessedProject;
            }


            await EndGenerators(generators);
            CurrentGenerationInfo.LastRun = DateTime.Now.ToFileTimeUtc();
            await GenerationInfoCoupler.SaveAsync(CurrentGenerationInfo);
            await StructureInfoCoupler.SaveAsync(StructuresInfo);
            await Logger.LogInfo("ENDS main loop");
        }

        private async Task<Project?> ProcessMissingSymbols(AdhocWorkspace workspace, ProjectInfo projectInfo)
        {
            await Logger.LogInfo("RUNS the processing of missing symbols");

            Project? newProject;
            ICollection<string> missingSymbols = MissingSymbolsRegistry.GetMissingSymbols();
            if (missingSymbols.Count > 0)
            {
                if (currentFails == maxFails)
                {
                    CurrentGenerationInfo.WasSuccessful = false;
                    await Logger.LogError($"Generators failed due to missing symbols, ending now, " +
                        $"the missing symbols: {string.Join(", ", missingSymbols)}");
                    newProject = null;
                }
                else
                {
                    currentFails++;
                    newProject = await ResolveMissingSymbols(workspace, projectInfo, missingSymbols);
                }
            }
            else
            {
                CurrentGenerationInfo.WasSuccessful = true;
                newProject = null;
            }

            return newProject;
        }

        private async Task<Project?> ResolveMissingSymbols(AdhocWorkspace workspace, ProjectInfo projectInfo, ICollection<string> missingSymbols)
        {
            await Logger.LogInfo("RUNS resolution of missing symbols");

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

        private async Task ReconstructStructuresInfo(SyntaxNode syntaxRoot, Document document)
        {
            await Logger.LogInfo($"RUNS the reconstruction of structures for {document.Name}");
            var types = syntaxRoot.DescendantNodes().OfType<TypeDeclarationSyntax>();
            var enums = syntaxRoot.DescendantNodes().OfType<EnumDeclarationSyntax>();

            List<(string, StructureInfo)> structureInfos = [];
            var namespaceName = "";
            foreach (var type in types)
            {
                if (type.Parent is BaseNamespaceDeclarationSyntax namespaceSyntax)
                {
                    namespaceName = namespaceSyntax.Name.ToString();
                }

                var name = type.Identifier.Text;
                StructureType structureType = StructuresInfo.GetStructureType(name);
                structureInfos.Add((name, new StructureInfo(structureType, namespaceName)));
            }

            foreach (var type in enums)
            {
                structureInfos.Add((type.Identifier.Text, new StructureInfo(StructureType.Enum, namespaceName)));
            }

            string documentFilePath = document.FilePath == null ? "" : document.FilePath;

            if (document.FilePath == null)
            {
                await Logger.LogError($"Reconstructed {document.Name} structures will be incorrect, the FilePath is unset");
            }

            foreach (var kvp in structureInfos)
            {
                (string name, StructureInfo info) = kvp;

                info.FilePath = documentFilePath;
                info.Namespace = namespaceName;
                if (!StructuresInfo.NameAndStructureInfo.TryAdd(name, info))
                {
                    StructuresInfo.NameAndStructureInfo[name] = info;
                }
            }
        }

        private async Task StartGenerators(ICollection<IGenerator> generators)
        {
            await Logger.LogInfo("Starts generators");
            foreach (var generator in generators)
            {
                generator.Start();
            }
        }

        private async Task EndGenerators(ICollection<IGenerator> generators)
        {
            await Logger.LogInfo("ENDS generators");
            foreach (var generator in generators)
            {
                generator.End();
            }
        }

        private ProjectInfo? CreateProject()
        {
            Logger.LogInfo($"RUNS creation of the project");
            string projectName = Path.GetFileNameWithoutExtension(ExecutionPaths.CsprojPath);
            ProjectId projectId = ProjectId.CreateNewId(projectName);

            MetadataReference[] usersReferences = DependencyCollector.CollectExecReferences();
            DocumentInfo[] documentInfos = GetDocumentInfos(projectId);
            List<MetadataReference> references =
            [
                .. usersReferences,
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
            Logger.LogInfo($"Project contains {references.Count} references and {documentInfos.Length} documents");
            return projectInfo;
        }

        private DocumentInfo[] GetDocumentInfos(ProjectId projectId)
        {
            IList<string> filePaths = CsFileRegistry.GetCsFilePaths();
            DocumentInfo[] documentInfos = new DocumentInfo[filePaths.Count];
            for (int i = 0; i < filePaths.Count; i++)
            {
                string path = filePaths[i];
                documentInfos[i] = DocumentInfo.Create(
                    DocumentId.CreateNewId(projectId),
                    Path.GetFileNameWithoutExtension(path),
                    null,
                    SourceCodeKind.Script,
                    TextLoader.From(TextAndVersion.Create(SourceText.From(File.ReadAllText(path)), VersionStamp.Create(), path)),
                    path
                );
            }
            return documentInfos;
        }
    }
}
