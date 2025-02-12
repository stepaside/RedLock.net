using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace RedLockNet.SERedis
{
    internal class NullLoggerFactory : ILoggerFactory
	{
        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
			return NullLogger.Instance;
        }

        public void Dispose()
        {
        }
    }
}