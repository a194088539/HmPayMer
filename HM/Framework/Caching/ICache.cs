using System;
using System.Collections.Generic;

namespace HM.Framework.Caching
{
	public interface ICache
	{
		bool ExistsKey(string key);

		long Increment(string key, uint amount);

		long Decrement(string key, uint amount);

		T Get<T>(string key);

		T Get<T>(string key, string hashId);

		bool Remove(string key);

		void RemoveAll(IEnumerable<string> keys);

		bool Add<T>(string key, T value);

		bool Add<T>(string key, T value, DateTime expiresAt);

		bool Add<T>(string key, T value, TimeSpan expiresIn);

		bool Add<T>(string hashId, IDictionary<string, T> dic);

		bool Add<T>(string hashId, IDictionary<string, T> dic, DateTime expiresAt);

		bool Add<T>(string hashId, IDictionary<string, T> dic, TimeSpan expiresIn);

		void FlushAll();
	}
}
