using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodInject.Prebuild.logger
{
    public interface ILogger
    {
        public void LogError(string message, Exception exception = null);
        public void LogWarning(string message);
        public void LogInfo(string message);
    }
}
