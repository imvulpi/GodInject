using GodInject.Prebuild.API.data;
using GodInject.Prebuild.API.generation;

namespace GodInject.Prebuild.API.contexts
{
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

        public IDataCoupler<GenerationInfo> GenerationInfoCoupler { get; set; }
        public GenerationInfo? GenerationInfo { get; set; }
        public IDataCoupler<StructuresInfo> StructuresInfoCoupler { get; set; }
        public StructuresInfo StructuresInfo { get; set; }
        public string[] AbsoluteDllPaths { get; set; }
    }
}
