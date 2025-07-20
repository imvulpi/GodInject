using Microsoft.CodeAnalysis;
namespace GodInject.Prebuild.API.generation
{
    /// <summary>
    /// Generators follow a simple cycle, firstly the <see cref="Start"/> gets called, then for each
    /// document the <see cref="Generate(Document)"/> is called, finishing with the <see cref="End"/>
    /// 
    /// To register a generator use <see cref="IGeneratorRegistry"/>
    /// To add missing symbols use <see cref="IMissingSymbolsRegistry"/>
    /// </summary>
    public interface IGenerator
    {
        public void Start();
        public void Generate(Document document, SyntaxNode? syntaxRoot, SemanticModel? semanticModel);
        public void End();
    }
}
