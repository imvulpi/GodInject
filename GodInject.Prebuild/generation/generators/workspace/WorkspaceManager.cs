using GodInject.Prebuild.generation.generators.workspace.collection;
using Microsoft.CodeAnalysis;

namespace GodInject.Prebuild.generation.generators.workspace
{
    public struct DepsResolutionResult
    {
        public Project Project { get; set; }
        public string[] DependenciesNotResolved { get; set; }
    }

    /// <summary>
    /// Handles creation and filling of workspace and resolving missing dependencies of workspace,
    /// it also collects dependencies.
    /// </summary>
    public class WorkspaceManager
    {
        public WorkspaceManager(GenerationInfo lastGenerationInfo) { 
            GenerationInfo = lastGenerationInfo;
            Initialize();
        }

        public AdhocWorkspace Workspace { get; set; }
        private DependencyCollector DependencyCollector;
        private GenerationInfo GenerationInfo;
        Dictionary<ProjectId, string> ProjectPaths = new();

        public void Initialize()
        {
            Workspace = new AdhocWorkspace();
            DependencyCollector = new(GenerationInfo);
        }

        public Project CreateProject(string projectPath, string[] dllPaths)
        {
            string projectDir = Path.GetDirectoryName(projectPath);
            string projectName = Path.GetFileNameWithoutExtension(projectPath);
            var projectId = ProjectId.CreateNewId();

            List<DocumentInfo> localDocuments = new();
            DependencyCollector.GetLocalDocuments(projectDir, projectId, ref localDocuments);

            // Processing relative paths
            for (int i = 0; i < dllPaths.Length; i++)
            {
                string dll = dllPaths[i];
                if (!Path.IsPathRooted(dll))
                {
                    dllPaths[i] = Path.GetFullPath(Path.Combine(projectDir, dll));
                }
            }

            var references = DependencyCollector.GetReferences(dllPaths);
            var projectInfo = ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                projectName,
                projectName,
                LanguageNames.CSharp
            ).WithMetadataReferences(references).WithDocuments(localDocuments);

            ProjectPaths.Add(projectId, projectPath);
            return Workspace.AddProject(projectInfo);
        }

        public DepsResolutionResult ResolveDependencies(ProjectId projectId, string[] missingDependencies)
        {
            if (ProjectPaths.TryGetValue(projectId, out var projectPath)) {
                List<DocumentInfo> dependencies = new List<DocumentInfo>();
                DependencyCollector.GetMissingDocuments(projectPath, projectId, missingDependencies, ref dependencies);

                if (dependencies.Count < missingDependencies.Length)
                {
                    Console.WriteLine("Generator could not resolve all dependencies");
                }

                Solution solution = Workspace.CurrentSolution;
                foreach (var dependency in dependencies)
                {
                    solution = solution.AddDocument(dependency);
                }

                Workspace.TryApplyChanges(solution);
                return new DepsResolutionResult()
                {
                    DependenciesNotResolved = missingDependencies,
                    Project = Workspace.CurrentSolution.GetProject(projectId)
                };
            }
            throw new NullReferenceException("Project was not registered first in the workspace manager");
        }
    }
}
