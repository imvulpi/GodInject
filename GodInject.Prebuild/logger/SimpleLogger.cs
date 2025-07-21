using GodInject.Prebuild.API.logging;

namespace GodInject.Prebuild.logger
{
    /// <summary>
    /// Basic logger used in the early phases to log into the console.
    /// </summary>
    internal class SimpleLogger : ILogger
    {
        public Task LogError(string message, Exception? exception = null)
        {
            if (exception == null)
            {
                Console.Write($"[ERROR] {message}");
                return Task.CompletedTask;
            }

            string logMessage =
                $"[ERROR] Exception of type {exception.GetType().FullName} occurred.\n" +
                $"Message: {exception.Message}\n" +
                $"StackTrace:\n{exception.StackTrace}";
            message = CheckEndLine(message);

            Console.Write(logMessage);
            return Task.CompletedTask;
        }

        public Task LogInfo(string message)
        {
            message = $"[INFO] {message}";
            message = CheckEndLine(message);

            Console.Write(message);
            return Task.CompletedTask;
        }

        public Task LogWarning(string message)
        {
            message = $"[WARNING] {message}";
            message = CheckEndLine(message);

            Console.Write(message);
            return Task.CompletedTask;
        }

        private string CheckEndLine(string message)
        {
            if (!message.EndsWith('\n'))
                message += "\n";
            return message;
        }
    }
}
