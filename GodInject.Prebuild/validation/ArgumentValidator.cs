using GodInject.Prebuild.API.logging;
using GodInject.Prebuild.constants;

namespace GodInject.Prebuild.validation
{
    public class ArgumentValidator(ILogger logger) : IArgumentValidator
    {
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
