using GodInject.Prebuild.API.contexts;
using GodInject.Prebuild.API.data;
using GodInject.Prebuild.API.generation;
using GodInject.Prebuild.API.logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Runtime.CompilerServices;

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
            await Logger.LogInfo("Running");

            AdhocWorkspace workspace = new();
            ProjectInfo? projectInfo = CreateProject();
            if (projectInfo == null) return;
            Project project = workspace.AddProject(projectInfo);

            IList<IGenerator> generators = GeneratorRegistry.GetGenerators();
            await Logger.LogInfo($"Starting to process {generators.Count} Generators");
            StartGenerators(generators);
            await Logger.LogInfo("Starting the main loop");
            while (true)
            {
                CurrentGenerationInfo.DocumentsCheckedCount = 0;
                foreach (var document in project.Documents)
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
                    ReconstructStructuresInfo(syntaxRoot, document);
                }

                Project? reprocessedProject = await ProcessMissingSymbols(workspace, projectInfo);
                if(reprocessedProject == null) break;
                project = reprocessedProject;
            }


            EndGenerators(generators);
            CurrentGenerationInfo.LastRun = DateTime.Now.ToFileTimeUtc();
            await GenerationInfoCoupler.SaveAsync(CurrentGenerationInfo);
            await StructureInfoCoupler.SaveAsync(StructuresInfo);
            await Logger.LogInfo("Stopped");
        }

        private async Task<Project?> ProcessMissingSymbols(AdhocWorkspace workspace, ProjectInfo projectInfo)
        {
            ICollection<string> missingSymbols = MissingSymbolsRegistry.GetMissingSymbols();
            if (missingSymbols.Count > 0)
            {
                await Logger.LogInfo("Trying to resolve missing symbols");
                if (currentFails == maxFails)
                {
                    CurrentGenerationInfo.WasSuccessful = false;
                    await Logger.LogError($"Generators failed due to missing symbols, ending now, " +
                        $"the missing symbols: {string.Join(", ", missingSymbols)}");
                    return null;
                }
                else
                {
                    currentFails++;
                    Project? newProject = await ResolveMissingSymbols(workspace, projectInfo, missingSymbols);
                    return newProject;
                }
            }
            else
            {
                CurrentGenerationInfo.WasSuccessful = true;
                return null;
            }
        }

        private async Task<Project?> ResolveMissingSymbols(AdhocWorkspace workspace, ProjectInfo projectInfo, ICollection<string> missingSymbols)
        {
            await Logger.LogInfo("Trying to resolve missing symbols");

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
                StructureType structureType = StructuresInfo.GetStructureType(name);
                structureInfos.Add((name, new StructureInfo(structureType, namespaceName)));
            }

            foreach (var type in enums)
            {
                structureInfos.Add((type.Identifier.Text, new StructureInfo(StructureType.Enum, namespaceName)));
            }

            foreach (var kvp in structureInfos)
            {
                (string name, StructureInfo info) = kvp;
                if(document.FilePath == null)
                {

                }
                info.FilePath = document.FilePath;
                info.Namespace = namespaceName;
                if (!StructuresInfo.NameAndStructureInfo.TryAdd(name, info))
                {
                    StructuresInfo.NameAndStructureInfo[name] = info;
                }
            }
        }

        private void StartGenerators(ICollection<IGenerator> generators)
        {
            foreach (var generator in generators)
            {
                generator.Start();
            }
        }

        private void EndGenerators(ICollection<IGenerator> generators)
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
