namespace GodInject.Prebuild.validation
{
    /// <summary>
    /// Interface for argument validation
    /// </summary>
    public interface IArgumentValidator
    {
        /// <summary>
        /// Validates arguments based on some criteria
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <returns>true if valid; otherwise false</returns>
        bool Validate(string[] args);
    }
}
