namespace GodInject.Prebuild.API.logging
{
    /// <summary>
    /// Interface for logging messages at various severity levels.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs an error message, optionally including an exception.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        /// <param name="exception">An optional exception related to the error.</param>
        /// <returns>A task representing the asynchronous logging operation.</returns>
        Task LogError(string message, Exception? exception = null);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The warning message to log.</param>
        /// <returns>A task representing the asynchronous logging operation.</returns>
        Task LogWarning(string message);

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The informational message to log.</param>
        /// <returns>A task representing the asynchronous logging operation.</returns>
        Task LogInfo(string message);
    }
}
