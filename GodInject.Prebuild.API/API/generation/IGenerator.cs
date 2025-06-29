using Microsoft.CodeAnalysis;
namespace GodInject.Prebuild.API.generation
{
    /// <summary>
    /// Generators follow a simple cycle, firstly the <see cref="Start"/> gets called, then for each
    /// document the <see cref="Generate(Document)"/> is called, finishing with the <see cref="End"/>
    /// </summary>
    public interface IGenerator
    {
        public void Start();
        public void Generate(Document document);
        public void End();
    }
}
