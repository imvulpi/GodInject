using GodInject.Prebuild.API.contexts;
using GodInject.Prebuild.API.data;
using GodInject.Prebuild.API.generation;
using GodInject.Prebuild.API.logging;
using GodInject.Prebuild.IO;

namespace GodInject.Prebuild.contexts
{
    /// <summary>
    /// Builds the <see cref="GenerationDataContext"/> using <see cref="BuildAsync(ExecutionPaths, ExecutionSettings)"/>
    /// </summary>
    /// <param name="logger">Logger to be used for logging</param>
    public class GenerationDataContextBuilder(ILogger logger)
    {
        /// <summary>
        /// Creates <see cref="GenerationDataContext"/> using <paramref name="executionPaths"/> and <paramref name="executionSettings"/> 
        /// </summary>
        /// <param name="executionPaths">Paths to be used in creation of instances</param>
        /// <param name="executionSettings">Settings to be used in creation of instances</param>
        /// <returns>A <see cref="GenerationDataContext"/> with filled dependencies</returns>
        public async Task<GenerationDataContext> BuildAsync(ExecutionPaths executionPaths, ExecutionSettings executionSettings)
        {
            await logger.LogInfo("Creating generation data context");
            IDataCoupler<GenerationInfo> generationInfoCoupler = new JsonDataCoupler<GenerationInfo>(executionPaths.GenerationInfoPath);
            await logger.LogInfo("Reading generation info");
            GenerationInfo? generationInfo = await generationInfoCoupler.ReadAsync();

            IDataCoupler<StructuresInfo> structuresInfoCoupler = new MemPackDataCoupler<StructuresInfo>(Path.Join(executionPaths.FrameworkFilesDirPath, "structures.bin"));
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

        /// <summary>
        /// Turns relative paths to absolute paths using <paramref name="basePath"/>
        /// </summary>
        /// <param name="basePath">Base path to be applied on <paramref name="relativePaths"/></param>
        /// <param name="relativePaths">Relative paths</param>
        /// <returns>Absolute paths of <paramref name="basePath"/> and <paramref name="relativePaths"/></returns>
        private string[] GetAbsolutePaths(string basePath, string[] relativePaths)
        {
            string[] absoluteDllPaths = new string[relativePaths.Length];
            for (int i = 0; i < relativePaths.Length; i++)
            {
                string dllPath = relativePaths[i];
                absoluteDllPaths[i] = Path.GetFullPath(Path.Join(basePath, dllPath));
            }
            return absoluteDllPaths;
        }
    }
}
