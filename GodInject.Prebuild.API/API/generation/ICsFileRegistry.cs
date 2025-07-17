namespace GodInject.Prebuild.API.generation
{
    public interface ICsFileRegistry
    {
        public void Add(string document);
        public void Remove(string document);
    }
}
