using GodInject.Prebuild.API.data;
using GodInject.Prebuild.API.logging;

namespace GodInject.Prebuild.API.contexts
{
    /// <summary>
    /// Encapsulates runtime-related dependencies, configuration, and services used during application execution.
    /// </summary>
    /// <remarks>
    /// Provides access to execution configuration, project paths, and logging functionality.
    /// </remarks>
    public class RuntimeContext
    {
        public RuntimeContext(IDataCoupler<ExecutionSettings> executionSettingsCoupler, ExecutionSettings executionSettings, ExecutionPaths executionPaths, ILogger logger)
        {
            ExecutionSettingsCoupler = executionSettingsCoupler;
            ExecutionSettings = executionSettings;
            ExecutionPaths = executionPaths;
            Logger = logger;
        }

        /// <summary>
        /// Coupler used to load or save <see cref="ExecutionSettings"/>.
        /// </summary>
        public IDataCoupler<ExecutionSettings> ExecutionSettingsCoupler { get; set; }

        /// <summary>
        /// The current execution settings used by the application.
        /// </summary>
        public ExecutionSettings ExecutionSettings { get; set; }

        /// <summary>
        /// Provides access to relevant runtime and generation paths.
        /// </summary>
        public ExecutionPaths ExecutionPaths { get; set; }

        /// <summary>
        /// The logger instance used for outputting runtime messages.
        /// </summary>
        public ILogger Logger { get; set; }
    }
}
