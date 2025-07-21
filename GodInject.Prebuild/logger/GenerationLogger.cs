using GodInject.Prebuild.API.IO;
using GodInject.Prebuild.API.logging;

namespace GodInject.Prebuild.logger
{
    /// <summary>
    /// Interface for logging messages at various severity levels to a file in a specific file format.
    /// </summary>
    /// <remarks>
    /// Restricts logs amount to <see cref="MaxLogFiles"/>
    /// </remarks>
    internal class GenerationLogger : ILogger
    {
        private const string LOGS_PREFIX = "logs~";
        private const string LOGS_DATE_FORMAT = "yyyy-MM-dd_HH-mm-ss-ffff";

        public uint MaxLogFiles { get; set; } = 3;
        public GenerationLogger(string logsDirPath)
        {
            LogsDirPath = logsDirPath ?? string.Empty;
            DeleteOlderLogs(LogsDirPath);
            logsFilePath = Path.Join(logsDirPath, $"{LOGS_PREFIX}{DateTime.Now.ToString(LOGS_DATE_FORMAT)}.txt");
        }

        public string LogsDirPath { get; set; }
        private readonly string logsFilePath;
        public async Task LogError(string message, Exception? exception = null)
        {
            message = $"[ERROR][{DateTime.Now:yyyy.MM.dd HH:mm:ss:ffff}] {message}";
            if (exception != null)
            {
                message = $"{message} Exception of type {exception.GetType().FullName} occurred.\n" +
                          $"Message: {exception.Message}\n" +
                          $"StackTrace:\n{exception.StackTrace}";
            }
            message = CheckEndLine(message);

            await TryLogging(logsFilePath, message);
        }

        public async Task LogWarning(string message)
        {
            message = $"[WARNING][{DateTime.Now:yyyy.MM.dd HH:mm:ss:ffff}] {message}";
            message = CheckEndLine(message);

            await TryLogging(logsFilePath, message);
        }

        public async Task LogInfo(string message)
        {
            message = $"[INFO][{DateTime.Now:yyyy.MM.dd HH:mm:ss:ffff}] {message}";
            message = CheckEndLine(message);

            await TryLogging(logsFilePath, message);
        }

        private string CheckEndLine(string message)
        {
            if (!message.EndsWith('\n'))
                message += "\n";
            return message;
        }

        private async Task TryLogging(string path, string message)
        {
            await using var _ = await FileLockManager.WaitAsync(path);
            using var stream = new FileStream(logsFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            using var writer = new StreamWriter(stream);
            await writer.WriteAsync(message);
        }

        private void DeleteOlderLogs(string path)
        {
            long oldestDate = long.MaxValue;
            string oldestPath = string.Empty;
            int logCount = 0;
            foreach (var metadata in FastFileRetriever.GetFiles(path, "*"))
            {
                if (metadata.Name.StartsWith(LOGS_PREFIX))
                {
                    ReadOnlySpan<char> dateString = metadata.Name.AsSpan(LOGS_PREFIX.Length, LOGS_DATE_FORMAT.Length);
                    if (DateTime.TryParseExact(dateString, LOGS_DATE_FORMAT, null, System.Globalization.DateTimeStyles.NoCurrentDateDefault, out var result))
                    {
                        logCount++;
                        if (oldestDate > result.Ticks)
                        {
                            oldestDate = result.Ticks;
                            oldestPath = Path.Join(path, metadata.Name);
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
        }
    }
}
