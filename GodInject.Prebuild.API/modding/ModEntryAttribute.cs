namespace GodInject.Prebuild.API.modding
{
    /// <summary>
    /// Marks a class as the entry point of a mod.
    /// <para>Required for mod discovery and loading by the framework.</para>
    /// </summary>
    /// <remarks>
    /// Contains basic metadata about the mod.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
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
