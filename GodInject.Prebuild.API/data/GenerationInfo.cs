namespace GodInject.Prebuild.API.data
{
    /// <summary>
    /// Stores information about a generation run.
    /// </summary>
    /// <remarks>
    /// Mostly used by dependency collectors to not collect files that were not changed.
    /// </remarks>
    public class GenerationInfo
    {
        /// <summary>
        /// Timestamp of the last generation run as a Windows file time.
        /// </summary>
        /// <remarks>
        /// Represents the number of 100-nanosecond intervals since January 1, 1601 (UTC).
        /// </remarks>
        public long LastRun { get; set; }

        /// <summary>
        /// Indicates whether the last generation run was successful.
        /// </summary>
        public bool WasSuccessful { get; set; }

        /// <summary>
        /// Number of documents checked during the last generation run.
        /// </summary>
        public int DocumentsCheckedCount { get; set; }
    }
}
