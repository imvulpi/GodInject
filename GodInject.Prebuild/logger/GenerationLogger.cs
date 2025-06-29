using GodInject.Prebuild.API.logging;

namespace GodInject.Prebuild.logger
{
    internal class GenerationLogger : ILogger
    {
        public GenerationLogger(string logsDirPath)
        {
            LogsDirPath = logsDirPath ?? string.Empty;
            logsFilePath = Path.Join(logsDirPath, "logs.txt");
        }

        public string LogsDirPath { get; set; }
        private readonly string logsFilePath;
        public void LogError(string message, Exception exception = null)
        {
            message = $"[ERROR][{DateTime.Now:yyyy.MM.dd HH:mm:ss:f}] {message}";
            if (exception != null)
            {
                message = $"{message}\nError exception:\n{exception}";
            }
            if (!message.EndsWith('\n'))
            {
                message += "\n";
            }
            File.AppendAllText(logsFilePath, message);
        }

        public void LogWarning(string message)
        {
            message = $"[WARNING][{DateTime.Now:yyyy.MM.dd HH:mm:ss:f}] {message}";
            if (!message.EndsWith('\n'))
            {
                message += "\n";
            }
            File.AppendAllText(logsFilePath, message);
        }

        public void LogInfo(string message)
        {
            message = $"[INFO][{DateTime.Now:yyyy.MM.dd HH:mm:ss:f}] {message}";
            if (!message.EndsWith('\n'))
            {
                message += "\n";
            }
            File.AppendAllText(logsFilePath, message);
        }
    }
}
