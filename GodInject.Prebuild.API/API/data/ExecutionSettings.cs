namespace GodInject.Prebuild.API.data
{
    /// <summary>
    /// Data class defining rules/paths used in the project
    /// </summary>
    public class ExecutionSettings
    {
        public const string GENERATOR_FILES_DEFAULT = "./generator"; 
        public string GeneratorFilesRelativePath { get; set; } = GENERATOR_FILES_DEFAULT;
        public string GeneratedFilesOutputDirName { get; set; } = "generated";
        public string[] ReferencesRelativePaths { get; set; } = Array.Empty<string>();
        public string GeneratorModsRelativePath { get; set; } = "./generator/mods";
    }
}
