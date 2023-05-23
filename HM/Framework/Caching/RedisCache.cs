using HM.Framework.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace HM.Framework.Caching
{
	public class RedisCache : ICache
	{
		private int db;

		public long Increment(string key, uint amount)
		{
			try
			{
				return RedisStackExchangeContext.WriteRedis.GetDatabase().StringIncrement(key, amount);
			}
			catch (Exception exception)
			{
				LogUtil.Error($"RedisCache.Get({key})", exception);
				return -1L;
			}
		}

		public long Decrement(string key, uint amount)
		{
			try
			{
				return RedisStackExchangeContext.WriteRedis.GetDatabase().StringDecrement(key, amount);
			}
			catch (Exception exception)
			{
				LogUtil.Error($"RedisCache.Get({key})", exception);
				return -1L;
			}
		}

		public bool ExistsKey(string key)
		{
			try
			{
				return RedisStackExchangeContext.ReadRedis.GetDatabase().KeyExists(key);
			}
			catch (Exception exception)
			{
				LogUtil.Error($"RedisCache.ExistsKey({key})", exception);
				return false;
			}
		}

		public T Get<T>(string key)
		{
			try
			{
				RedisValue redisValue = RedisStackExchangeContext.ReadRedis.GetDatabase().StringGet(key);
				if (redisValue.IsNullOrEmpty)
				{
					return default(T);
				}
				return redisValue.ToString().FormJson<T>();
			}
			catch (Exception exception)
			{
				LogUtil.Error($"RedisCache.Get({key})", exception);
				return default(T);
			}
		}

		public T Get<T>(string key, string hashId)
		{
			try
			{
				RedisValue redisValue = RedisStackExchangeContext.ReadRedis.GetDatabase().HashGet(key, hashId);
				if (redisValue.IsNullOrEmpty)
				{
					return default(T);
				}
				return redisValue.ToString().FormJson<T>();
			}
			catch (Exception exception)
			{
				LogUtil.Error($"RedisCache.Get({key})", exception);
				return default(T);
			}
		}

		public bool Remove(string key)
		{
			try
			{
				return RedisStackExchangeContext.WriteRedis.GetDatabase().KeyDelete(key);
			}
			catch (Exception exception)
			{
				LogUtil.Error($"RedisCache.Get({key})", exception);
				return false;
			}
		}

		public void RemoveAll(IEnumerable<string> keys)
		{
			try
			{
				IDatabase database = RedisStackExchangeContext.ReadRedis.GetDatabase();
				foreach (string key in keys)
				{
					database.KeyDelete(key);
				}
			}
			catch (Exception exception)
			{
				LogUtil.Error("RemoveAll", exception);
			}
		}

		public bool Add<T>(string key, T value)
		{
			try
			{
				return RedisStackExchangeContext.WriteRedis.GetDatabase().StringSet(key, value.ToJson());
			}
			catch (Exception exception)
			{
				LogUtil.Error($"RedisCache.Add({key})", exception);
				return false;
			}
		}

		public bool Add<T>(string key, T value, TimeSpan expiresIn)
		{
			try
			{
				return RedisStackExchangeContext.WriteRedis.GetDatabase().StringSet(key, value.ToJson(), expiresIn);
			}
			catch (Exception exception)
			{
				LogUtil.Error($"RedisCache.Add({key})", exception);
				return false;
			}
		}

		public bool Add<T>(string key, T value, DateTime expiresAt)
		{
			TimeSpan expiresIn = expiresAt - DateTime.Now;
			return Add(key, value, expiresIn);
		}

		public bool Add<T>(string key, T value, int min)
		{
			return Add(key, value, DateTime.Now.AddMinutes((double)min));
		}

		public bool Add<T>(string key, IDictionary<string, T> dic)
		{
			try
			{
				IDatabase database = RedisStackExchangeContext.WriteRedis.GetDatabase();
				HashEntry[] array = new HashEntry[dic.Count];
				int num = 0;
				foreach (KeyValuePair<string, T> item in dic)
				{
					HashEntry hashEntry = new HashEntry(item.Key, item.Value.ToJson());
					array.SetValue(hashEntry, num++);
				}
				database.HashSet(key, array);
				return true;
			}
			catch (Exception exception)
			{
				LogUtil.Error($"RedisCache.Add({key})", exception);
				return false;
			}
		}

		public bool Add<T>(string key, IDictionary<string, T> dic, DateTime expiresAt)
		{
			try
			{
				IDatabase database = RedisStackExchangeContext.WriteRedis.GetDatabase();
				HashEntry[] array = new HashEntry[dic.Count];
				int num = 0;
				foreach (KeyValuePair<string, T> item in dic)
				{
					HashEntry hashEntry = new HashEntry(item.Key, item.Value.ToJson());
					array.SetValue(hashEntry, num++);
				}
				database.HashSet(key, array);
				database.KeyExpire(key, expiresAt);
				return true;
			}
			catch (Exception exception)
			{
				LogUtil.Error($"RedisCache.Add({key})", exception);
				return false;
			}
		}

		public bool Add<T>(string key, IDictionary<string, T> dic, TimeSpan expiresIn)
		{
			try
			{
				IDatabase database = RedisStackExchangeContext.WriteRedis.GetDatabase();
				HashEntry[] array = new HashEntry[dic.Count];
				int num = 0;
				foreach (KeyValuePair<string, T> item in dic)
				{
					HashEntry hashEntry = new HashEntry(item.Key, item.Value.ToJson());
					array.SetValue(hashEntry, num++);
				}
				database.HashSet(key, array);
				database.KeyExpire(key, expiresIn);
				return true;
			}
			catch (Exception exception)
			{
				LogUtil.Error($"RedisCache.Add({key})", exception);
				return false;
			}
		}

		public void FlushAll()
		{
		}
	}
}
