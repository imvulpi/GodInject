using GodInject.Prebuild.API.IO;

namespace GodInject.Prebuild.API.generation
{
    public interface IStructureInfoValidator
    {
        public StructuresInfo StructuresInfo { get; }
        public void Switch(StructuresInfo structureInfo);
        public void ProcessStructureInfo(IEnumerable<(string path, FileMetaRef metadata)> files);
        public void ValidateFile(string path, FileMetaRef fileMetadata);
    }
}
