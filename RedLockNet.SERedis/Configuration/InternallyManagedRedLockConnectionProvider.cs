using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using RedLockNet.SERedis.Internal;
using RedLockNet.SERedis.Util;
using StackExchange.Redis;

namespace RedLockNet.SERedis.Configuration
{
	/// <summary>
	/// A connection provider that manages its own connections to Redis
	/// </summary>
	public class InternallyManagedRedLockConnectionProvider : RedLockConnectionProvider
	{
		private readonly ILoggerFactory _loggerFactory;
		public IList<RedLockEndPoint> EndPoints { get; set; }

		private ICollection<RedisConnection> _connections;

		private const int DefaultConnectionTimeout = 100;
		private const int DefaultSyncTimeout = 1000;
		private const int DefaultConfigCheckSeconds = 10;

		public InternallyManagedRedLockConnectionProvider(ILoggerFactory loggerFactory = null)
		{
			_loggerFactory = loggerFactory ?? new NullLoggerFactory();

			EndPoints = new List<RedLockEndPoint>();
		}

		internal override ICollection<RedisConnection> CreateRedisConnections()
		{
			if (EndPoints == null || !	EndPoints.Any())
			{
				throw new ArgumentException("No endpoints specified");
			}

			var logger = _loggerFactory.CreateLogger<InternallyManagedRedLockConnectionProvider>();

			_connections = new List<RedisConnection>(EndPoints.Count);

			foreach (var endPoint in EndPoints)
			{
				var redisConfig = new ConfigurationOptions
				{
					AbortOnConnectFail = false,
					ConnectTimeout = endPoint.ConnectionTimeout ?? DefaultConnectionTimeout,
					SyncTimeout = endPoint.SyncTimeout ?? DefaultSyncTimeout,
					Ssl = endPoint.Ssl,
					SslProtocols = endPoint.SslProtocols,
					Password = endPoint.Password,
					ConfigCheckSeconds = endPoint.ConfigCheckSeconds ?? DefaultConfigCheckSeconds
				};

				foreach (var e in endPoint.EndPoints)
				{
					redisConfig.EndPoints.Add(e);
				}

				var redisConnection = new RedisConnection
				{
					ConnectionMultiplexer = ConnectionMultiplexer.Connect(redisConfig),
					RedisDatabase = endPoint.RedisDatabase ?? DefaultRedisDatabase,
					RedisKeyFormat = string.IsNullOrEmpty(endPoint.RedisKeyFormat) ? DefaultRedisKeyFormat : endPoint.RedisKeyFormat
				};

				redisConnection.ConnectionMultiplexer.ConnectionFailed += (sender, args) =>
				{
					logger.LogWarning($"ConnectionFailed: {args.EndPoint.GetFriendlyName()} ConnectionType: {args.ConnectionType} FailureType: {args.FailureType}");
				};

				redisConnection.ConnectionMultiplexer.ConnectionRestored += (sender, args) =>
				{
					logger.LogWarning($"ConnectionRestored: {args.EndPoint.GetFriendlyName()} ConnectionType: {args.ConnectionType} FailureType: {args.FailureType}");
				};

				redisConnection.ConnectionMultiplexer.ConfigurationChanged += (sender, args) =>
				{
					logger.LogDebug($"ConfigurationChanged: {args.EndPoint.GetFriendlyName()}");
				};

				redisConnection.ConnectionMultiplexer.ConfigurationChangedBroadcast += (sender, args) =>
				{
					logger.LogDebug($"ConfigurationChangedBroadcast: {args.EndPoint.GetFriendlyName()}");
				};

				redisConnection.ConnectionMultiplexer.ErrorMessage += (sender, args) =>
				{
					logger.LogWarning($"ErrorMessage: {args.EndPoint.GetFriendlyName()} Message: {args.Message}");
				};

				_connections.Add(redisConnection);
			}

			return _connections;
		}

		internal override void DisposeConnections()
		{
			foreach (var connection in _connections)
			{
				connection.ConnectionMultiplexer.Dispose();
			}
		}
	}
}