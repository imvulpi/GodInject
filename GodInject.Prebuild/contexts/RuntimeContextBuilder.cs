using GodInject.Prebuild.API.contexts;
using GodInject.Prebuild.API.data;
using GodInject.Prebuild.API.logging;
using GodInject.Prebuild.constants;
using GodInject.Prebuild.IO;
using GodInject.Prebuild.logger;

public class RuntimeContextBuilder(ILogger logger)
{
    public async Task<RuntimeContext> BuildAsync(string[] args)
    {
        await logger.LogInfo("Creating runtime context");

        var coupler = new TomlDataCoupler<ExecutionSettings>(GetExecutionSettingsPath(args));
        var settings = await GetExecutionSettings(coupler);
        var paths = new ExecutionPaths(args[0], settings);
        logger = SwitchLogger(logger, paths.FrameworkFilesDirPath);

        await logger.LogInfo("Created runtime context");

        return new RuntimeContext(coupler, settings, paths, logger);
    }

    public ILogger SwitchLogger(ILogger logger, string output)
    {
        logger.LogInfo("Switches logger type to a file logger");
        logger = new GenerationLogger(output);
        logger.LogInfo("Successful logger switching.");
        return logger;
    }

    private string GetExecutionSettingsPath(string[] args)
    {
        string projectCsprojPath = args[0];
        string? projectDirPath = Path.GetDirectoryName(projectCsprojPath);

        bool argumentsContainExecSettingsPath = args.Length == 2;
        string path;
        if (argumentsContainExecSettingsPath)
        {
            string execSettingsPath = args[1];

            if (Path.IsPathRooted(execSettingsPath))
            {
                path = execSettingsPath;
            }
            else if (projectDirPath != null)
            {
                string combinedPaths = Path.Combine(projectDirPath, execSettingsPath);
                path = Path.GetFullPath(combinedPaths);
            }
            else
            {
                throw new ApplicationException($"Failed because the program could not figure out the project directory path\n");
            }
        }
        else
        {
            path = Path.Join(projectDirPath,
                ExecutionSettings.GENERATOR_FILES_DEFAULT,
                Constants.EXECUTION_SETTINGS_FILENAME);
        }

        string? directoryPath = Path.GetDirectoryName(path);
        if (directoryPath != null)
        {
            Directory.CreateDirectory(directoryPath);
            return path;
        }
        else
        {
            throw new ApplicationException("Could not get directory path of the exec.conf");
        }
    }

    private async Task<ExecutionSettings> GetExecutionSettings(TomlDataCoupler<ExecutionSettings> coupler)
    {
        ExecutionSettings? executionSettings = await coupler.ReadAsync();
        if (executionSettings == null)
        {
            executionSettings = new();
            await coupler.SaveAsync(executionSettings);
        }
        return executionSettings;
    }
}
