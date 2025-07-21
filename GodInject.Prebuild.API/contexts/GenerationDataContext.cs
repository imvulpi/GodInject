using GodInject.Prebuild.API.data;
using GodInject.Prebuild.API.generation;

namespace GodInject.Prebuild.API.contexts
{
    /// <summary>
    /// Contains data and data couplers used during the generation phase.
    /// </summary>
    /// <remarks>
    /// Provides access to generation data and the couplers responsible for reading and writing it.
    /// </remarks>
    public class GenerationDataContext
    {
        public GenerationDataContext(IDataCoupler<GenerationInfo> generationInfoCoupler, GenerationInfo? generationInfo, IDataCoupler<StructuresInfo> structuresInfoCoupler, StructuresInfo structuresInfo, string[] absoluteDllPaths)
        {
            GenerationInfoCoupler = generationInfoCoupler;
            GenerationInfo = generationInfo;
            StructuresInfoCoupler = structuresInfoCoupler;
            StructuresInfo = structuresInfo;
            AbsoluteDllPaths = absoluteDllPaths;
        }

        /// <summary>
        /// Coupler for reading or saving <see cref="GenerationInfo"/>.
        /// </summary>
        public IDataCoupler<GenerationInfo> GenerationInfoCoupler { get; set; }

        /// <summary>
        /// The current generation info, typically loaded via the coupler.
        /// </summary>
        public GenerationInfo? GenerationInfo { get; set; }

        /// <summary>
        /// Coupler for reading or saving <see cref="StructuresInfo"/>.
        /// </summary>
        public IDataCoupler<StructuresInfo> StructuresInfoCoupler { get; set; }

        /// <summary>
        /// The current structures info used during generation
        /// </summary>
        public StructuresInfo StructuresInfo { get; set; }

        /// <summary>
        /// Absolute paths to all loaded DLLs used in the generation process.
        /// </summary>
        public string[] AbsoluteDllPaths { get; set; }

    }
}
