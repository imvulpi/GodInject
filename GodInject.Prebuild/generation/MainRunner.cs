using GodInject.Prebuild.API.contexts;
using GodInject.Prebuild.API.data;
using GodInject.Prebuild.API.generation;
using GodInject.Prebuild.API.logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GodInject.Prebuild.generation
{
    /// <summary>
    /// The main generation loop seperated from initialization for better clarity and readablity.
    /// <para>
    /// Simplified logic: Creation of workspace and project -> Starting generators -> 
    /// Main loop -> Iteration on documents (passed to generators, structure reevaluated) -> Resolving missing symbols (breaks on 0 missing, breaks on fails exceeding limit or LOOPS BACK -> (main loop BREAK logic) Ends generators -> saves generation info and structures info.
    /// </para>
    /// </summary>
    /// <param name="frameworkContext">Framework context with valid dependencies</param>
    internal class MainRunner(FrameworkContext frameworkContext)
    {
        public FrameworkContext FrameworkContext { get; private set; } = frameworkContext;
        public IGenerationFileCollector GenerationFileCollector => FrameworkContext.Generation.Tools.FileCollector;
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

        /// <summary>
        /// Performs the frameworks generation calling the generators.
        /// </summary>
        /// <remarks>
        /// Creates the project, runs generators, reevaluates structures, processes missing symbols and ends generators, saving cache data
        /// </remarks>
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

        /// <summary>
        /// Processes missing symbols, checks whether there are any / fails / attempts to resolve them
        /// </summary>
        /// <param name="workspace">Workspace used to resolve the missing symbols</param>
        /// <param name="projectInfo">Project info used to resolve the missing symbols</param>
        /// <returns>null if <see cref="maxFails"/> is reached or fails; otherwise the project with attempted resolution of missing symbols</returns>
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

        /// <summary>
        /// Attempts to resolve missing symbols using <see cref="DependencyResolver"/>
        /// </summary>
        /// <param name="workspace">Workspace to apply changes in</param>
        /// <param name="projectInfo">Project info for new documents creation</param>
        /// <param name="missingSymbols">Missing symbols that need to be resolved</param>
        /// <returns>Project with resolved symbols if resolution went ok</returns>
        private async Task<Project?> ResolveMissingSymbols(AdhocWorkspace workspace, ProjectInfo projectInfo, ICollection<string> missingSymbols)
        {
            await Logger.LogInfo("RUNS resolution of missing symbols");

            var resolvedDocuments = DependencyResolver.ResolveDocuments(missingSymbols.ToArray(), projectInfo.Id);
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

        /// <summary>
        /// performs a reevaluation of the <paramref name="document"/>'s structure provided the document's <paramref name="syntaxRoot"/>
        /// </summary>
        /// <param name="syntaxRoot">Syntax root of the document</param>
        /// <param name="document">The document to be reevaluated</param>
        /// <returns>Reevaluation task</returns>
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

        /// <summary>
        /// Creates a project with colected metadata references by <see cref=" GenerationFileCollector"/> and document infos from <see cref="GetDocumentInfos(ProjectId)"/> as well as some common system references.
        /// </summary>
        /// <returns>A fully setup project info ready to be added into the workspace</returns>
        private ProjectInfo? CreateProject()
        {
            Logger.LogInfo($"RUNS creation of the project");
            string projectName = Path.GetFileNameWithoutExtension(ExecutionPaths.CsprojPath);
            ProjectId projectId = ProjectId.CreateNewId(projectName);

            MetadataReference[] usersReferences = GenerationFileCollector.CollectExecReferences();
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

        /// <summary>
        /// Creates <see cref="DocumentInfo"/>s from the project's root directory
        /// </summary>
        /// <remarks>
        /// It gets the project file paths by using <see cref="CsFileRegistry"/>
        /// which it then turns into document infos.
        /// <para>The document infos need the filepath to be provided.</para>
        /// </remarks>
        /// <param name="projectId">Project id for creating the document</param>
        /// <returns>A array of <see cref="DocumentInfo"/>s of the project files.</returns>
        private DocumentInfo[] GetDocumentInfos(ProjectId projectId)
        {
            IList<string> filePaths = CsFileRegistry.GetCsFilePaths();
            DocumentInfo[] documentInfos = new DocumentInfo[filePaths.Count];
            for (int i = 0; i < filePaths.Count; i++)
            {
                string path = filePaths[i];
                documentInfos[i] = DocumentInfo.Create(
                    DocumentId.CreateNewId(projectId),
                    Path.GetFileName(path),
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
