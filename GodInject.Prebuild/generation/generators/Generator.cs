using GodInject.Prebuild.generation.generators.workspace;
using GodInject.Prebuild.generators.builder;
using GodInject.Prebuild.generators.data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.Json;

namespace GodInject.Prebuild.generation.generators
{
    public class Generator
    {
        public Generator(ExecutionSettings executionSettings, GenerationPaths generationPaths) 
        {
            ExecutionSettings = executionSettings;
            GenerationPaths = generationPaths;
            LastGenerationInfo = GetLastGenerationInfo();
        }

        public ExecutionSettings ExecutionSettings { get; set; }
        public GenerationPaths GenerationPaths { get; set; }

        private readonly InjectClassBuilder injectClassBuilder = new();
        private GenerationInfo CurrentGenerationInfo = new();
        private GenerationInfo LastGenerationInfo;
        public async Task<int> Generate()
        {
            WorkspaceManager workspaceManager = new(LastGenerationInfo);
            Project project = workspaceManager.CreateProject(GenerationPaths.CSProjectPath, ExecutionSettings.DllPaths);

            while (true)
            {
                HashSet<string> missingDependencies = await ProcessDocuments(project);
                
                if (missingDependencies.Count == 0) break;
                else
                {
                    Console.WriteLine($"Missing deps: {string.Join(", ", missingDependencies)}");
                    var result = workspaceManager.ResolveDependencies(project.Id, missingDependencies.ToArray());
                    project = result.Project;
                    await ProcessDocuments(project, result);
                    break;
                }
            }

            CurrentGenerationInfo.WasSuccessful = true;
            CurrentGenerationInfo.LastRun = DateTime.Now.ToFileTime();
            SaveGenerationInfo();

            return 0;
        }

        private GenerationInfo GetLastGenerationInfo()
        {
            string generationInfoPath = Path.Join(GenerationPaths.GeneratorFilesPath, "generation_info.json");
            if (File.Exists(generationInfoPath))
            {
                string generationInfo = File.ReadAllText(generationInfoPath);
                return JsonSerializer.Deserialize<GenerationInfo>(generationInfo);
            }

            return new GenerationInfo()
            {
                LastRun = 0,
                FilesCheckedCount = 0,
                FilesGeneratedCount = 0,
                FilesIgnoredCount = 0,
                WasSuccessful = false,
            };
        }

        private void SaveGenerationInfo()
        {
            string generationInfoPath = Path.Join(GenerationPaths.GeneratorFilesPath, "generation_info.json");
            File.WriteAllText(generationInfoPath, JsonSerializer.Serialize(CurrentGenerationInfo));
        }

        private async Task<HashSet<string>> ProcessDocuments(Project project, DepsResolutionResult? result = null)
        {
            HashSet<string> missingDependencies = new();
            foreach (var document in project.Documents)
            {
                CurrentGenerationInfo.FilesCheckedCount++;
                var syntaxRoot = await document.GetSyntaxRootAsync();
                var semanticModel = await document.GetSemanticModelAsync();
                if (syntaxRoot == null || semanticModel == null)
                    return null;

                var classDeclarations = syntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>();
                foreach (var classSyntax in classDeclarations)
                {
                    if (semanticModel.GetDeclaredSymbol(classSyntax) is not INamedTypeSymbol classSymbol)
                        continue;
                    
                    InjectedDataMembers injectedDataMembers = new InjectedDataMembers();
                    injectedDataMembers.InjectedFields = injectedDataMembers.GetInjectedFields(classSymbol);
                    injectedDataMembers.InjectedProperties = injectedDataMembers.GetInjectedProperties(classSymbol);
 
                    if (classSymbol.BaseType.Locations.Length == 0)
                    {
                        string baseType = classSymbol.BaseType.ToString();
                        if(baseType == "object")
                        {
                            HandleClassFileCreation(classSymbol, injectedDataMembers);
                            continue;
                        }
                        if (result != null)
                        {
                            if(result.Value.DependenciesNotResolved.Contains(classSymbol.BaseType.ToString()))
                            {
                                HandleUnresolvedClassCreation(classSymbol, injectedDataMembers, result.Value);
                                continue;
                            }
                        }
                        missingDependencies.Add(classSymbol.BaseType.ToString());
                    }
                    else
                    {
                        HandleClassFileCreation(classSymbol, injectedDataMembers);
                    }
                }
            }
            return missingDependencies;
        }

        private void HandleUnresolvedClassCreation(INamedTypeSymbol classSymbol, InjectedDataMembers injectedDataMembers, DepsResolutionResult result)
        {
            string className = classSymbol.Name;
            var managedInjection = classSymbol.GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass.Name == "ManagedInjection");

            string newSource = "";
            switch (ExecutionSettings.MissingDependenciesActions)
            {
                case MissingDependenciesActions.Throw:
                    throw new Exception($"Expected error appeared, Throwed due to missing dependencies and current execution settings.\n" +
                        $"{string.Join(", ", result.DependenciesNotResolved)}");
                case MissingDependenciesActions.InjectInConstructor:
                    newSource = injectClassBuilder.CreateClass(classSymbol, injectedDataMembers, false, true);
                    break;
                case MissingDependenciesActions.InjectLikeGodot:
                    throw new NotImplementedException("Injecting like godot is not implemented yet");
            }

            if (newSource != null)
            {
                CurrentGenerationInfo.FilesGeneratedCount++;
                File.WriteAllText(Path.Join(GenerationPaths.GeneratedFilesOutputPath, $"{className}.g.cs"), newSource);
            }
        }

        private void HandleClassFileCreation(INamedTypeSymbol classSymbol, InjectedDataMembers injectedDataMembers)
        {
            string className = classSymbol.Name;
            var managedInjection = classSymbol.GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass.Name == "ManagedInjection");

            string newSource;
            if (managedInjection == null)
            {
                newSource = injectClassBuilder.CreateClass(classSymbol, injectedDataMembers, ExecutionSettings.CreateManualInjectMethod, true);
            }
            else if (managedInjection.ConstructorArguments[0].Value is bool allowsParameterless && allowsParameterless)
            {
                newSource = injectClassBuilder.CreateClass(classSymbol, injectedDataMembers, true, true);
            }
            else
            {
                newSource = injectClassBuilder.CreateClass(classSymbol, injectedDataMembers, true, false);
            }

            if (newSource != null)
            {
                CurrentGenerationInfo.FilesGeneratedCount++;
                File.WriteAllText(Path.Join(GenerationPaths.GeneratedFilesOutputPath, $"{className}.g.cs"), newSource);
            }
        }
    }
}
