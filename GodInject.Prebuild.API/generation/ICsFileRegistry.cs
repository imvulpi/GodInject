namespace GodInject.Prebuild.API.generation
{
    public interface ICsFileRegistry
    {
        public IList<string> GetCsFilePaths();
        public void Add(string path);
        public void Remove(string path);
    }
}
