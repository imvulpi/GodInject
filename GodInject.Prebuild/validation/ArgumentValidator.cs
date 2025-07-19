using GodInject.Prebuild.API.logging;
using GodInject.Prebuild.constants;

namespace GodInject.Prebuild.validation
{
    public class ArgumentValidator : IArgumentValidator
    {
        public ArgumentValidator(ILogger logger)
        {
            _logger = logger;
        }

        private ILogger _logger;
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
