using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace RedLockNet.SERedis.Configuration
{
	public class RedLockConfiguration
	{
		public RedLockConfiguration(IList<RedLockEndPoint> endPoints, ILoggerFactory loggerFactory = null)
		{
			ConnectionProvider = new InternallyManagedRedLockConnectionProvider(loggerFactory ?? new NullLoggerFactory())
			{
				EndPoints = endPoints
			};
			LoggerFactory = loggerFactory ?? new NullLoggerFactory();
		}

		public RedLockConfiguration(RedLockConnectionProvider connectionProvider, ILoggerFactory loggerFactory = null)
		{
			ConnectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider), "Connection provider must not be null");
			LoggerFactory = loggerFactory ?? new NullLoggerFactory();
		}

		public RedLockConnectionProvider ConnectionProvider { get; }
		public ILoggerFactory LoggerFactory { get; }
		public RedLockRetryConfiguration RetryConfiguration { get; set; }
	}
}