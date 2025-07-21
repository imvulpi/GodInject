using GodInject.Generator.generator.data;
using GodInject.Prebuild.API.data;
using GodInject.Prebuild.API.generation;
using GodInject.Prebuild.API.IO;
using GodInject.Prebuild.API.logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GodInject.Generator.generator
{
    internal class AutoInjectGenerator : IGenerator
    {
        public AutoInjectGenerator(ExecutionSettings executionSettings, 
            ExecutionPaths executionPaths, 
            IMissingSymbolsRegistry symbolsRegistry)
        {
            ExecutionSettings = executionSettings;
            ExecutionPaths = executionPaths;
            MissingSymbolsRegistry = symbolsRegistry;
        }
        public ExecutionSettings ExecutionSettings { get; set; }
        public ExecutionPaths ExecutionPaths { get; set; }
        public IMissingSymbolsRegistry MissingSymbolsRegistry { get; set; }
        public ILogger? Logger { get; set; }
        private readonly InjectClassBuilder injectClassBuilder = new();
        private FileMetaRef generatorOutputFiles;
        public void Start() {
            Logger?.LogInfo($"[{ModEntry.ModuleName}] Generator Starts");
            FileMetaRef fileMetaRef = FastFileRetriever.GetFiles(ExecutionPaths.GenerationOutputDirPath, "*");
            generatorOutputFiles = fileMetaRef;
        }

        public async void Generate(Document document, SyntaxNode? syntaxRoot, SemanticModel? semanticModel)
        {
            if (syntaxRoot == null || semanticModel == null)
                return;

            var classDeclarations = syntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>();
            foreach (var classSyntax in classDeclarations)
            {
                if (semanticModel.GetDeclaredSymbol(classSyntax) is not INamedTypeSymbol classSymbol)
                    continue;

                InjectedDataMembers injectedDataMembers = new InjectedDataMembers(classSymbol, MissingSymbolsRegistry);

                if (classSymbol.BaseType != null && classSymbol.BaseType.Locations.Length == 0)
                {
                    string? baseType = classSymbol.BaseType.ToString();
                    if (baseType == "object")
                    {
                        await ProcessClassFile(document, classSymbol, injectedDataMembers);
                        continue;
                    }
                    if(baseType != null)
                        MissingSymbolsRegistry.Add(baseType);
                }
                else
                {
                    await ProcessClassFile(document, classSymbol, injectedDataMembers);
                }
            }
            return;
        }

        private async Task ProcessClassFile(Document document, INamedTypeSymbol classSymbol, InjectedDataMembers injectedDataMembers)
        {
            if (injectedDataMembers.InjectedFields.Length <= 0 && injectedDataMembers.InjectedProperties.Length <= 0)
            {
                if (document.FilePath == null) return;                
                for (int i = 0; i < generatorOutputFiles.Count; i++)
                {
                    FileMetaRef fileMetaRef = generatorOutputFiles[i];
                    string regularName = fileMetaRef.Name.Replace(".injected.g", "");
                    if(regularName == Path.GetFileName(document.FilePath))
                    {
                        string deletePath = Path.Join(ExecutionPaths.GenerationOutputDirPath, fileMetaRef.Name);
                        File.Delete(deletePath);
                    }
                }
                return;
            }

            string className = classSymbol.Name;
            string newSource = injectClassBuilder.CreateClass(classSymbol, injectedDataMembers, true, false);

            if (newSource != null)
            {
                await File.WriteAllTextAsync(Path.Join(ExecutionPaths.GenerationOutputDirPath, $"{className}.injected.g.cs"), newSource);
            }
        }

        public void End() {
            Logger?.LogInfo($"[{ModEntry.ModuleName}] Generator Ends");
        }
    }
}
