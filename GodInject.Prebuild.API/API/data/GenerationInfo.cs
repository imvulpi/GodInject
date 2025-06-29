namespace GodInject.Prebuild.API.data
{
    public class GenerationInfo
    {
        public long LastRun { get; set; }
        public bool WasSuccessful { get; set; }
        public int FilesCheckedCount { get; set; }
        public int FilesGeneratedCount { get; set; }
        public int FilesIgnoredCount { get; set; }
    }
}
