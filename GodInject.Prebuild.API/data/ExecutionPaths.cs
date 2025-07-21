namespace GodInject.Prebuild.API.data
{
    /// <summary>
    /// Stores absolute paths to key files and directories within the project.
    /// </summary>
    /// <remarks>
    /// Ensures that all directory paths referenced are valid and accessible.
    /// </remarks>
    public class ExecutionPaths
    {
        public ExecutionPaths(string csProjectPath, ExecutionSettings executionSettings)
        {
            string? projectDir = Path.GetDirectoryName(csProjectPath);
            ArgumentNullException.ThrowIfNull(executionSettings, nameof(executionSettings));
            ArgumentNullException.ThrowIfNull(projectDir);

            CsprojPath = csProjectPath ?? throw new ArgumentNullException(nameof(csProjectPath));
            FrameworkFilesDirPath = Path.GetFullPath(Path.Combine(projectDir, executionSettings.GeneratorFilesRelativePath));
            GenerationOutputDirPath = Path.GetFullPath(Path.Combine(FrameworkFilesDirPath, executionSettings.GeneratedFilesOutputDirName));
            ProjectDirPath = projectDir;
            ModsDirPath = Path.GetFullPath(Path.Combine(ProjectDirPath, executionSettings.GeneratorModsRelativePath));
            GenerationInfoPath = Path.Join(FrameworkFilesDirPath, "generation.info");

            Directory.CreateDirectory(FrameworkFilesDirPath);
            Directory.CreateDirectory(GenerationOutputDirPath);
            Directory.CreateDirectory(ModsDirPath);
        }

        /// <summary>
        /// Absolute path to the project's C# project file (.csproj).
        /// </summary>
        public string CsprojPath { get; set; }

        /// <summary>
        /// Absolute path to the generation info file.
        /// </summary>
        public string GenerationInfoPath { get; set; }

        /// <summary>
        /// Absolute path to the directory containing framework files.
        /// </summary>
        public string FrameworkFilesDirPath { get; set; }

        /// <summary>
        /// Absolute path to the directory where generation output is/should saved.
        /// </summary>
        /// <remarks>
        /// Technically a mod can choose a different path to save the output
        /// </remarks>
        public string GenerationOutputDirPath { get; set; }

        /// <summary>
        /// Absolute path to the root project directory.
        /// </summary>
        public string ProjectDirPath { get; set; }

        /// <summary>
        /// Absolute path to the directory containing mods.
        /// </summary>
        public string ModsDirPath { get; set; }
    }
}
