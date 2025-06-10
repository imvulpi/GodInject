using GodInject.Prebuild.generation;
using GodInject.Prebuild.generation.generators;
using GodInject.Prebuild.logger;
using Microsoft.Build.Locator;
using Tomlet;

namespace GodInject.Prebuild
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            ILogger logger = new SimpleLogger();
            if (args.Length == 0)
            {
                logger.LogError("Usage: dotnet GodInject.Prebuild.dll <path-to-projectFilePath.csproj> (optional: <path-to-settings>)");
                return 1;
            }

            ExecutionSettings generationSettings = GetGenerationSettings(args);
            GenerationPaths generationPaths = GetGenerationPaths(args[0], generationSettings);
            MSBuildLocator.RegisterDefaults();

            Generator generator = new(generationSettings, generationPaths);
            await generator.Generate();

            return 0;
        }

        private static GenerationPaths GetGenerationPaths(string csProjectPath, ExecutionSettings generationSettings)
        {
            return new GenerationPaths(csProjectPath, generationSettings);
        }

        private static ExecutionSettings GetGenerationSettings(string[] args)
        {
            string projectPath = args[0];
            string settingsPath = null;
            string projectDir = Path.GetDirectoryName(projectPath);
            ExecutionSettings generationSettings = new() { };
            if (args.Length == 2)
            {
                if (Path.IsPathRooted(args[1]))
                {
                    settingsPath = args[1];
                }
                else
                {
                    if (projectDir != null)
                    {
                        string combinedPaths = Path.Combine(projectDir, args[1]);
                        settingsPath = Path.GetFullPath(combinedPaths);
                    }
                }
            }
            else
            {
                settingsPath = Path.Join(projectDir, generationSettings.RelativeGeneratorPath, "exec.conf"); // consts later
            }

            if (File.Exists(settingsPath))
            {
                string settingsString = File.ReadAllText(settingsPath);
                generationSettings = TomletMain.To<ExecutionSettings>(settingsString);
            }
            else if (settingsPath != null)
            {
                string dirPath = Path.GetDirectoryName(settingsPath);
                if (dirPath != null)
                {
                    Directory.CreateDirectory(dirPath);
                    string name = Path.GetFileName(settingsPath);
                    File.WriteAllText(Path.Combine(dirPath, name), TomletMain.TomlStringFrom(generationSettings));
                }
            }

            Directory.CreateDirectory(generationSettings.OutputDirName);
            return generationSettings;
        }
    }
}
