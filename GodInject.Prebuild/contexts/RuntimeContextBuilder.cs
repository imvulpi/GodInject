using GodInject.Prebuild.API.contexts;
using GodInject.Prebuild.API.data;
using GodInject.Prebuild.API.logging;
using GodInject.Prebuild.constants;
using GodInject.Prebuild.IO;
using GodInject.Prebuild.logger;

/// <summary>
/// Builds the <see cref="RuntimeContext"/> using <see cref="BuildAsync(string[])"/>
/// </summary>
/// <param name="logger">Logger to be used for logging</param>
public class RuntimeContextBuilder(ILogger logger)
{
    /// <summary>
    /// Creates the <see cref="RuntimeContext"/> using <paramref name="args"/>
    /// </summary>
    /// <param name="args">Program arguments (from <see cref="GodInject.Prebuild.Program.Main(string[])"/>)</param>
    /// <returns>A task with <see cref="RuntimeContext"/> value</returns>
    public async Task<RuntimeContext> BuildAsync(string[] args)
    {
        await logger.LogInfo("Creating runtime context");

        var coupler = new TomlDataCoupler<ExecutionSettings>(GetExecutionSettingsPath(args));
        var settings = await GetExecutionSettings(coupler);
        var paths = new ExecutionPaths(args[0], settings);
        SwitchLogger(ref logger, paths.FrameworkFilesDirPath);

        await logger.LogInfo("Created runtime context");

        return new RuntimeContext(coupler, settings, paths, logger);
    }

    /// <summary>
    /// Switched the logger instance to a <see cref="GenerationLogger"/>
    /// </summary>
    /// <remarks>
    /// This is mostly used for logging :P
    /// </remarks>
    /// <param name="logger">Logger to switch</param>
    /// <param name="output">Output directory of the new logger</param>
    /// <returns>Newly created logger that the <paramref name="logger"/> was switched to</returns>
    public ILogger SwitchLogger(ref ILogger logger, string output)
    {
        logger.LogInfo("Switches logger type to a file logger");
        logger = new GenerationLogger(output);
        logger.LogInfo("Successful logger switching.");
        return logger;
    }

    /// <summary>
    /// Processes <paramref name="args"/> to retrieve the passed by user path of <see cref="ExecutionSettings"/> Or in case
    /// of no path it locates and creates a default path for the settings.
    /// </summary>
    /// <param name="args">Program arguments (from <see cref="GodInject.Prebuild.Program.Main(string[])"/></param>
    /// <returns>A path to <see cref="ExecutionSettings"/> file</returns>
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

    /// <summary>
    /// Gets the <see cref="ExecutionSettings"/> using a provided <paramref name="coupler"/>
    /// </summary>
    /// <param name="coupler">Coupler to be used to retrieve the settings</param>
    /// <returns>A Task with <see cref="ExecutionSettings"/> value</returns>
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
