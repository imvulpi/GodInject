using Microsoft.CodeAnalysis;
namespace GodInject.Prebuild.API.generation
{
    /// <summary>
    /// Defines a code generator with a simple lifecycle: <see cref="Start"/> is called once at the beginning,
    /// followed by <see cref="Generate(Document, SyntaxNode?, SemanticModel?)"/> for each document,
    /// and ending with <see cref="End"/>.
    /// </summary>
    /// <remarks>
    /// To register a generator, use <see cref="IGeneratorRegistry"/>.
    /// To report missing symbols during generation, use <see cref="IMissingSymbolsRegistry"/>.
    /// </remarks>
    public interface IGenerator
    {
        /// <summary>
        /// Called once before generation begins.
        /// </summary>
        void Start();

        /// <summary>
        /// Called for each document to perform generation logic.
        /// </summary>
        /// <param name="document">The document being processed.</param>
        /// <param name="syntaxRoot">The syntax root of the document, if available.</param>
        /// <param name="semanticModel">The semantic model of the document, if available.</param>
        void Generate(Document document, SyntaxNode? syntaxRoot, SemanticModel? semanticModel);

        /// <summary>
        /// Called once after all documents have been processed.
        /// </summary>
        void End();
    }
}
