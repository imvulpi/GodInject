using GodInject.Prebuild.API.data;
using GodInject.Prebuild.API.generation;
using GodInject.Prebuild.API.logging;
using GodInject.Prebuild.injection_generator;
using GodInject.Prebuild.injection_generator.data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GodInject.Generator.injection_generator
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

        public void Start() {
            Logger?.LogInfo($"[{ModEntry.ModuleName}] Generator Starts");
        }

        public async void Generate(Document document)
        {
            var syntaxRoot = await document.GetSyntaxRootAsync();
            var semanticModel = await document.GetSemanticModelAsync();
            if (syntaxRoot == null || semanticModel == null)
                return;

            var classDeclarations = syntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>();
            foreach (var classSyntax in classDeclarations)
            {
                if (semanticModel.GetDeclaredSymbol(classSyntax) is not INamedTypeSymbol classSymbol)
                    continue;

                InjectedDataMembers injectedDataMembers = new InjectedDataMembers(classSymbol);
                if (classSymbol.BaseType != null && classSymbol.BaseType.Locations.Length == 0)
                {
                    string? baseType = classSymbol.BaseType.ToString();
                    if (baseType == "object")
                    {
                        CreateClassFile(classSymbol, injectedDataMembers);
                        continue;
                    }
                    if(baseType != null)
                        MissingSymbolsRegistry.AddMissingSymbol(baseType);
                }
                else
                {
                    CreateClassFile(classSymbol, injectedDataMembers);
                }
            }
            return;
        }

        private void CreateClassFile(INamedTypeSymbol classSymbol, InjectedDataMembers injectedDataMembers)
        {
            if (injectedDataMembers.InjectedFields.Length <= 0 && injectedDataMembers.InjectedProperties.Length <= 0)
                return;

            string className = classSymbol.Name;
            string newSource = injectClassBuilder.CreateClass(classSymbol, injectedDataMembers, true, false);

            if (newSource != null)
            {
                File.WriteAllText(Path.Join(ExecutionPaths.GenerationOutputDirPath, $"{className}.g.cs"), newSource);
            }
        }

        public void End() {
            Logger?.LogInfo($"[{ModEntry.ModuleName}] Generator Ends");
        }
    }
}
