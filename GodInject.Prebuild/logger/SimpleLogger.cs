using GodInject.Prebuild.API.logging;

namespace GodInject.Prebuild.logger
{
    internal class SimpleLogger : ILogger
    {
        public void LogError(string message, Exception? exception = null)
        {
            if(exception == null)
            {
                Console.Write($"[ERROR] {message}");
                return;
            }

            string logMessage =
                $"[ERROR] Exception of type {exception.GetType().FullName} occurred.\n" +
                $"Message: {exception.Message}\n" +
                $"StackTrace:\n{exception.StackTrace}";
            if (!logMessage.EndsWith('\n'))
            {
                logMessage += "\n";
            }
            Console.Write(logMessage);
        }

        public void LogInfo(string message)
        {
            message = $"[INFO] {message}";
            if (!message.EndsWith('\n'))
            {
                message += "\n";
            }
            Console.Write(message);
        }

        public void LogWarning(string message)
        {
            message = $"[WARNING] {message}";
            if (!message.EndsWith('\n'))
            {
                message += "\n";
            }
            Console.Write(message);
        }
    }
}
