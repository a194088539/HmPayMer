using HM.Framework.Logging;
using StackExchange.Redis;
using System;

namespace HM.Framework.Caching
{
	public static class RedisStackExchangeContext
	{
		private static ConnectionMultiplexer redisReadContext;

		private static ConnectionMultiplexer redisWriteContext;

		private static readonly object _lockReadObj;

		private static readonly object _lockWriteObj;

		private static string redisReadConnectionString;

		private static string redisWriteConnectionString;

		public static ConnectionMultiplexer ReadRedis
		{
			get
			{
				if (redisReadContext == null)
				{
					lock (_lockReadObj)
					{
						if (redisReadContext != null)
						{
							return redisReadContext;
						}
						redisReadContext = GetReadManager();
					}
				}
				return redisReadContext;
			}
		}

		public static ConnectionMultiplexer WriteRedis
		{
			get
			{
				if (redisWriteContext == null)
				{
					lock (_lockWriteObj)
					{
						if (redisWriteContext != null)
						{
							return redisWriteContext;
						}
						redisWriteContext = GetWriteManager();
					}
				}
				return redisWriteContext;
			}
		}

		static RedisStackExchangeContext()
		{
			_lockReadObj = new object();
			_lockWriteObj = new object();
			redisReadConnectionString = string.Empty;
			redisWriteConnectionString = string.Empty;
			try
			{
				redisReadConnectionString = Utils.GetAppSetting("RedisRead");
				redisWriteConnectionString = Utils.GetAppSetting("RedisWrite");
			}
			catch (Exception exception)
			{
				LogUtil.Error("初始化RedisStackExchangeContext失败！！！", exception);
			}
		}

		private static ConnectionMultiplexer GetReadManager()
		{
			return ConnectionMultiplexer.Connect(redisReadConnectionString);
		}

		private static ConnectionMultiplexer GetWriteManager()
		{
			return ConnectionMultiplexer.Connect(redisWriteConnectionString);
		}
	}
}
