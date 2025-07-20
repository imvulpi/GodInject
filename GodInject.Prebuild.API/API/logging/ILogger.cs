namespace GodInject.Prebuild.API.logging
{
    /// <summary>
    /// Interface for logging information, warnings and errors
    /// </summary>
    public interface ILogger
    {
        public Task LogError(string message, Exception? exception = null);
        public Task LogWarning(string message);
        public Task LogInfo(string message);
    }
}
