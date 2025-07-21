namespace GodInject.Prebuild.API.data
{
    /// <summary>
    /// Defines configuration settings and relative paths used within the project.
    /// </summary>
    /// <remarks>
    /// All relative paths are relative to the directory containing the user project's .csproj file,
    /// not to the generation directory or the program executable.
    /// </remarks>
    public class ExecutionSettings
    {
        /// <summary>
        /// Default relative path for generator files.
        /// </summary>
        public const string GENERATOR_FILES_DEFAULT = "./generators";

        /// <summary>
        /// Relative path to the generator files directory. Defaults to <see cref="GENERATOR_FILES_DEFAULT"/>.
        /// </summary>
        public string GeneratorFilesRelativePath { get; set; } = GENERATOR_FILES_DEFAULT;

        /// <summary>
        /// Name of the directory where generated files are output.
        /// </summary>
        public string GeneratedFilesOutputDirName { get; set; } = "generated";

        /// <summary>
        /// Relative paths to external references used by the generator.
        /// </summary>
        public string[] ReferencesRelativePaths { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Relative path to the directory containing generator mods.
        /// </summary>
        public string GeneratorModsRelativePath { get; set; } = "./generators/mods";
    }

}
