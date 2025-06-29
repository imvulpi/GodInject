namespace GodInject.Prebuild.API.modding
{
    public class ModEntryAttribute : Attribute
    {
        public string Name { get; }
        public string Version { get; }
        public string Author { get; }
        public string Description { get; }

        public ModEntryAttribute(string name)
        {
            Name = name;
        }

        public ModEntryAttribute(string name, string version, string author, string description = "")
        {
            Name = name;
            Version = version;
            Author = author;
            Description = description;
        }
    }
}
