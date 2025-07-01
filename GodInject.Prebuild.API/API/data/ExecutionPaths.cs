namespace GodInject.Prebuild.API.data
{
    /// <summary>
    /// Data class used for storing absolute paths to certain special directories in the project
    /// </summary>
    public class ExecutionPaths
    {
        public ExecutionPaths(string csProjectPath, ExecutionSettings executionSettings)
        {
            string? projectDir = Path.GetDirectoryName(csProjectPath);
            ArgumentNullException.ThrowIfNull(executionSettings, nameof(executionSettings));
            ArgumentNullException.ThrowIfNull(projectDir);

            CsprojPath = csProjectPath ?? throw new ArgumentNullException(nameof(csProjectPath));
            GeneratorFilesDirPath = Path.GetFullPath(Path.Combine(projectDir, executionSettings.GeneratorFilesRelativePath));
            GenerationOutputDirPath = Path.GetFullPath(Path.Combine(GeneratorFilesDirPath, executionSettings.GeneratedFilesOutputDirName));
            ProjectDirPath = projectDir;
            ModsDirPath = Path.GetFullPath(Path.Combine(ProjectDirPath, executionSettings.GeneratorModsRelativePath));
            GenerationInfoPath = Path.Join(GeneratorFilesDirPath, "generation.info");

            Directory.CreateDirectory(GeneratorFilesDirPath);
            Directory.CreateDirectory(GenerationOutputDirPath);
            Directory.CreateDirectory(ModsDirPath);
        }

        // Files:
        public string CsprojPath { get; set; }
        public string GenerationInfoPath { get; set; }

        // Directories:
        public string GeneratorFilesDirPath { get; set; }
        public string GenerationOutputDirPath { get; set; }
        public string ProjectDirPath { get; set; }
        public string ModsDirPath { get; set; }
    }
}
