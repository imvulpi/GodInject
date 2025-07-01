namespace GodInject.Prebuild.API.data
{
    /// <summary>
    /// Information about generations
    /// </summary>
    public class GenerationInfo
    {
        public long LastRun { get; set; }
        public bool WasSuccessful { get; set; }
        public int DocumentsCheckedCount { get; set; }
    }
}
