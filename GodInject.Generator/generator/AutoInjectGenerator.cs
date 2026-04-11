using GodInject.Generator.generator.data;
using GodInject.Prebuild.API.data;
using GodInject.Prebuild.API.generation;
using GodInject.Prebuild.API.IO;
using GodInject.Prebuild.API.logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Security.AccessControl;

namespace GodInject.Generator.generator
{
    /// <summary>
    /// Generates files that resolve properties and fields marked with [Inject] attribute
    /// </summary>
    /// <param name="executionPaths">Execution paths used for getting the generation output</param>
    /// <param name="missingSymbolsRegistry">A registry in which the missing symbols should be registered</param>
    internal class AutoInjectGenerator(ExecutionPaths executionPaths,
        IMissingSymbolsRegistry missingSymbolsRegistry) : IGenerator
    {
        public ExecutionPaths ExecutionPaths { get; set; } = executionPaths;
        public IMissingSymbolsRegistry MissingSymbolsRegistry { get; set; } = missingSymbolsRegistry;
        public ILogger? Logger { get; set; }
        private readonly InjectClassBuilder injectClassBuilder = new();
        private FileMetaRef generatorOutputFiles;

        /// <summary>
        /// Suffix before the .cs (file.cs with the suffix: file<c>.injection.g</c>.cs
        /// </summary>
        /// <remarks>
        /// Used to distinguish this generator files from other generators
        /// </remarks>
        private const string FILE_BEFORE_EXTENSION_SUFFIX = ".injection.g";

        /// <summary>
        /// Logs the start and collects files inside of the <see cref="ExecutionPaths.GenerationOutputDirPath"/> - Used in the generation for removing invalid generated class files when the processed document doesn't use [Inject] attribute.
        /// </summary>
        public void Start() {
            Logger?.LogInfo($"[{ModEntry.ModuleName}] Generator Starts");
            FileMetaRef fileMetaRef = FastFileRetriever.GetFiles(ExecutionPaths.GenerationOutputDirPath, "*");
            generatorOutputFiles = fileMetaRef;
        }

        /// <summary>
        /// Main generation logic, processes the document and provided with it the semantic model and syntax root.
        /// </summary>
        /// <param name="document">The document to be processed</param>
        /// <param name="syntaxRoot">Syntax root of the document</param>
        /// <param name="semanticModel">Semantic model of the document</param>
        public async void Generate(Document document, SyntaxNode? syntaxRoot, SemanticModel? semanticModel)
        {
            if (syntaxRoot == null || semanticModel == null)
                return;

            uint createdFiles = 0;
            bool shouldProcess = false;
            var classDeclarations = syntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>();
            foreach (var classSyntax in classDeclarations)
            {
                if (semanticModel.GetDeclaredSymbol(classSyntax) is not INamedTypeSymbol classSymbol)
                    continue;

                shouldProcess = false;
                InjectedDataMembers injectedDataMembers = new InjectedDataMembers(classSymbol, MissingSymbolsRegistry);

                if (classSymbol.BaseType != null && classSymbol.BaseType.Locations.Length == 0)
                {
                    string? baseType = classSymbol.BaseType.ToString();
                    if (baseType == "object")
                    {
                        shouldProcess = true;
                    }
                    else if (baseType != null)
                    {
                        MissingSymbolsRegistry.Add(baseType);
                    }
                }
                else
                {
                    shouldProcess = true;
                }

                if (shouldProcess)
                {
                    createdFiles = await ProcessClassFile(classSymbol, injectedDataMembers);
                }
            }

            if(createdFiles == 0)
            {
                RemoveInvalidDocument(document);
            }
            return;
        }

        /// <summary>
        /// Removes an invalid file or creates a new generated file (injection.g.cs)
        /// </summary>
        /// <param name="classSymbol">Class symbol to process</param>
        /// <param name="injectedDataMembers">Injected fields and properties</param>
        /// <returns>How many files were created (0 or 1)</returns>
        private async Task<uint> ProcessClassFile(INamedTypeSymbol classSymbol, InjectedDataMembers injectedDataMembers)
        {
            if (injectedDataMembers.InjectedFields.Length <= 0 && injectedDataMembers.InjectedProperties.Length <= 0)
            {
                return 0;
            }

            string className = classSymbol.Name;
            string newSource = injectClassBuilder.CreateClass(classSymbol, injectedDataMembers, true, false);

            await File.WriteAllTextAsync(Path.Join(ExecutionPaths.GenerationOutputDirPath, $"{className}{FILE_BEFORE_EXTENSION_SUFFIX}.cs"), newSource);
            return 1;
        }

        /// <summary>
        /// Removes a generated file assosiated with an invalid document
        /// </summary>
        /// <remarks>
        /// When no inject attributes are found in a changed file its invalid
        /// </remarks>
        /// <param name="document">Document of which generated file to delete</param>
        private void RemoveInvalidDocument(Document document)
        {
            if (document.FilePath == null) return;
            for (int i = 0; i < generatorOutputFiles.Count; i++)
            {
                FileMetaRef fileMetaRef = generatorOutputFiles[i];
                string regularName = fileMetaRef.Name.Replace(FILE_BEFORE_EXTENSION_SUFFIX, "");
                if (regularName == Path.GetFileName(document.FilePath))
                {
                    string deletePath = Path.Join(ExecutionPaths.GenerationOutputDirPath, fileMetaRef.Name);
                    File.Delete(deletePath);
                }
            }
        }

        /// <summary>
        /// Ends the generator with a log
        /// </summary>
        public void End() {
            Logger?.LogInfo($"[{ModEntry.ModuleName}] Generator Ends");
        }
    }
}
