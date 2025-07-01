namespace GodInject.Prebuild.API.logging
{
    /// <summary>
    /// Interface for logging information, warnings and errors
    /// </summary>
    public interface ILogger
    {
        public void LogError(string message, Exception? exception = null);
        public void LogWarning(string message);
        public void LogInfo(string message);
    }
}
