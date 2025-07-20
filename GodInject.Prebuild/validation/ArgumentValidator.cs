using GodInject.Prebuild.API.logging;
using GodInject.Prebuild.constants;

namespace GodInject.Prebuild.validation
{
    public class ArgumentValidator(ILogger logger) : IArgumentValidator
    {
        private ILogger _logger = logger;
        public bool Validate(string[] args)
        {
            _logger.LogInfo("Checking validity of passed arguments...");
            if (args.Length == 0)
            {
                _logger.LogError(Constants.PROPER_USAGE);
                return false;
            }
            _logger.LogInfo("Arguments are valid");
            return true;
        }
    }
}
