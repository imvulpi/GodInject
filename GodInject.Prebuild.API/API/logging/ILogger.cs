namespace GodInject.Prebuild.API.logging
{
    public interface ILogger
    {
        public void LogError(string message, Exception exception = null);
        public void LogWarning(string message);
        public void LogInfo(string message);
    }
}
