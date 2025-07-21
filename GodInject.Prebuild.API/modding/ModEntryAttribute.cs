namespace GodInject.Prebuild.API.modding
{
    /// <summary>
    /// Used to mark the mod entry, this is used to load the mods, without the attribute no mod will get found.
    /// Contains basic information about the mod,
    /// </summary>
    public class ModEntryAttribute : Attribute
    {
        public string Name { get; }
        public string Version { get; } = "";
        public string Author { get; } = "";
        public string Description { get; } = "";

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
