namespace GodInject.Prebuild.API.generation
{
    public interface ICsFileRegistry
    {
        public string[] GetPaths();
        public void Add(string path);
        public void Remove(string path);
    }
}
