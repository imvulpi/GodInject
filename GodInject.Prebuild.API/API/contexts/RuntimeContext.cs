using GodInject.Prebuild.API.data;
using GodInject.Prebuild.API.logging;

namespace GodInject.Prebuild.API.contexts
{
    public class RuntimeContext
    {
        public RuntimeContext(IDataCoupler<ExecutionSettings> executionSettingsCoupler, ExecutionSettings executionSettings, ExecutionPaths executionPaths, ILogger logger)
        {
            ExecutionSettingsCoupler = executionSettingsCoupler;
            ExecutionSettings = executionSettings;
            ExecutionPaths = executionPaths;
            Logger = logger;
        }

        public IDataCoupler<ExecutionSettings> ExecutionSettingsCoupler { get; set; }
        public ExecutionSettings ExecutionSettings { get; set; }
        public ExecutionPaths ExecutionPaths { get; set; }
        public ILogger Logger { get; set; }
    }
}
