namespace GodInject.Prebuild.generation
{
    public class GenerationPaths
    {
        public GenerationPaths(string csProjectPath, ExecutionSettings generationSettings)
        {
            ArgumentNullException.ThrowIfNull(generationSettings, nameof(generationSettings));

            CSProjectPath = csProjectPath ?? throw new ArgumentNullException(nameof(csProjectPath));
            ProjectDirPath = Path.GetDirectoryName(csProjectPath);
            GeneratorFilesPath = Path.GetFullPath(Path.Combine(ProjectDirPath, generationSettings.RelativeGeneratorPath));
            GeneratedFilesOutputPath = Path.GetFullPath(Path.Combine(GeneratorFilesPath, generationSettings.OutputDirName));

            Directory.CreateDirectory(GeneratorFilesPath);
            Directory.CreateDirectory(GeneratedFilesOutputPath);
        }
        public string GeneratorFilesPath { get; set; }
        public string GeneratedFilesOutputPath { get; set; }
        public string CSProjectPath { get; set; }
        public string ProjectDirPath { get; set; }
    }
}
