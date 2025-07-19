using GodInject.Prebuild.API.IO;
using GodInject.Prebuild.API.logging;

namespace GodInject.Prebuild.logger
{
    internal class GenerationLogger : ILogger
    {
        private const string LOGS_PREFIX = "logs~";
        private const string LOGS_DATE_FORMAT = "yyyy-MM-dd_HH-mm-ss";

        public uint MaxLogFiles { get; set; } = 3;
        public GenerationLogger(string logsDirPath)
        {
            LogsDirPath = logsDirPath ?? string.Empty;
            
            long oldestDate = long.MaxValue;
            string oldestPath = string.Empty;
            int logCount = 0;
            foreach (var metadata in FastFileRetriever.GetFiles(LogsDirPath, "*"))
            {
                if (metadata.Name.StartsWith(LOGS_PREFIX))
                {
                    ReadOnlySpan<char> dateString = metadata.Name.AsSpan(LOGS_PREFIX.Length, LOGS_DATE_FORMAT.Length);
                    if(DateTime.TryParseExact(dateString, LOGS_DATE_FORMAT, null, System.Globalization.DateTimeStyles.NoCurrentDateDefault, out var result))
                    {
                        logCount++;
                        if (oldestDate > result.Ticks)
                        {
                            oldestDate = result.Ticks;
                            oldestPath = Path.Combine(logsDirPath, metadata.Name);
                        }
                    }
                    else
                    {
                        Console.WriteLine("[ERROR] Could not parse logs!");
                    }
                }
            }

            if (logCount >= MaxLogFiles)
            {
                File.Delete(oldestPath);
            }

            logsFilePath = Path.Join(logsDirPath, $"{LOGS_PREFIX}{DateTime.Now.ToString(LOGS_DATE_FORMAT)}.txt");
        }

        public string LogsDirPath { get; set; }
        private readonly string logsFilePath;
        public void LogError(string message, Exception? exception = null)
        {
            message = $"[ERROR][{DateTime.Now:yyyy.MM.dd HH:mm:ss:f}] {message}";
            if (exception != null)
            {
                message = $"{message} Exception of type {exception.GetType().FullName} occurred.\n" +
                          $"Message: {exception.Message}\n" +
                          $"StackTrace:\n{exception.StackTrace}";
            }

            if (!message.EndsWith('\n'))
                message += "\n";
            
            File.AppendAllText(logsFilePath, message);
        }

        public void LogWarning(string message)
        {
            message = $"[WARNING][{DateTime.Now:yyyy.MM.dd HH:mm:ss:f}] {message}";
            if (!message.EndsWith('\n'))
                message += "\n";
            
            File.AppendAllText(logsFilePath, message);
        }

        public void LogInfo(string message)
        {
            message = $"[INFO][{DateTime.Now:yyyy.MM.dd HH:mm:ss:f}] {message}";
            if (!message.EndsWith('\n'))
                message += "\n";
            
            File.AppendAllText(logsFilePath, message);
        }
    }
}
