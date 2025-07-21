using GodInject.Prebuild.API.contexts;
using GodInject.Prebuild.API.data;
using GodInject.Prebuild.API.generation;
using GodInject.Prebuild.API.logging;
using GodInject.Prebuild.IO;

namespace GodInject.Prebuild.contexts
{
    public class GenerationDataContextBuilder(ILogger logger)
    {
        public async Task<GenerationDataContext> BuildAsync(ExecutionPaths executionPaths, ExecutionSettings executionSettings)
        {
            await logger.LogInfo("Creating generation data context");
            IDataCoupler<GenerationInfo> generationInfoCoupler = new JsonDataCoupler<GenerationInfo>(executionPaths.GenerationInfoPath);
            await logger.LogInfo("Reading generation info");
            GenerationInfo? generationInfo = await generationInfoCoupler.ReadAsync();

            IDataCoupler<StructuresInfo> structuresInfoCoupler = new MemPackDataCoupler<StructuresInfo>(Path.Join(executionPaths.GeneratorFilesDirPath, "structures.bin"));
            await logger.LogInfo("Reading structures info");
            StructuresInfo? structuresInfo = await structuresInfoCoupler.ReadAsync();
            structuresInfo ??= new StructuresInfo();
            string[] absoluteDllPaths = GetAbsolutePaths(executionPaths.ProjectDirPath, executionSettings.ReferencesRelativePaths);

            return new GenerationDataContext(
                generationInfoCoupler, 
                generationInfo, 
                structuresInfoCoupler, 
                structuresInfo, 
                absoluteDllPaths);
        }

        private string[] GetAbsolutePaths(string basePath, string[] paths)
        {
            string[] absoluteDllPaths = new string[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                string dllPath = paths[i];
                absoluteDllPaths[i] = Path.GetFullPath(Path.Join(basePath, dllPath));
            }
            return absoluteDllPaths;
        }
    }
}
