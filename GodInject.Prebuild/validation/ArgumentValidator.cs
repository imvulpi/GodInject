using GodInject.Prebuild.API.logging;
using GodInject.Prebuild.constants;

namespace GodInject.Prebuild.validation
{
    /// <summary>
    /// Argument validator for validation of program arguments.
    /// </summary>
    /// <param name="logger">Logger to be used for logging</param>
    public class ArgumentValidator(ILogger logger) : IArgumentValidator
    {
        /// <summary>
        /// Validates the programs arguments, confirms that args length are bigger than 0.
        /// </summary>
        /// <param name="args">Program arguments</param>
        /// <returns>true if valid; otherwise false</returns>
        public bool Validate(string[] args)
        {
            logger.LogInfo("Checking validity of passed arguments...");
            if (args.Length == 0)
            {
                logger.LogError(Constants.PROPER_USAGE);
                return false;
            }
            logger.LogInfo("Arguments are valid");
            return true;
        }
    }
}
