using HM.Framework.Logging;
using StackExchange.Redis;
using System;
using System.Web;

namespace HM.Framework
{
	public class HmSession
	{
		private const int db = 0;

		private static string sessionRedisString;

		private static string sessionSite;

		private static ConnectionMultiplexer redisContext;

		private static readonly object _lockObj;

		private const string tokenKey = "湖南哈曼信息技术有限公司haman#hamancom.com(请将#改成@)http://www.hamancom.com/";

		private const string sessionKey = "session.";

		static HmSession()
		{
			sessionRedisString = string.Empty;
			sessionSite = string.Empty;
			_lockObj = new object();
			try
			{
				sessionRedisString = Utils.GetAppSetting("SessionRedis");
				sessionSite = Utils.GetAppSetting("SessionSite");
			}
			catch (Exception exception)
			{
				LogUtil.Error("初始化RedisStackExchangeContext失败！！！", exception);
			}
		}

		private static ConnectionMultiplexer getRedis()
		{
			if (redisContext == null)
			{
				lock (_lockObj)
				{
					if (redisContext != null)
					{
						return redisContext;
					}
					redisContext = ConnectionMultiplexer.Connect(sessionRedisString);
				}
			}
			return redisContext;
		}

		public static void closeRdies()
		{
			lock (_lockObj)
			{
				if (redisContext != null)
				{
					redisContext.Close();
					redisContext = null;
				}
			}
		}

		public static string GetSessionId()
		{
			string text = Utils.GetCookie("HmSessionId");
			if (string.IsNullOrEmpty(text))
			{
				text = EncryUtils.MD5(HttpContext.Current.Session.SessionID + "湖南哈曼信息技术有限公司haman#hamancom.com(请将#改成@)http://www.hamancom.com/");
				Utils.SetCookie("HmSessionId", text);
			}
			return text;
		}

		private static string GetSessionKey(string key)
		{
			return "session." + sessionSite + "." + GetSessionId() + "." + key;
		}

		private static string GetSessionSingleKey(string key)
		{
			return "session." + sessionSite + ".Signle." + key;
		}

		public static bool SetSession(string key, string val, bool isSingle, string singleVal, int min = 60)
		{
			string text = GetSessionKey(key);
			TimeSpan value = DateTime.Now.AddMinutes((double)min) - DateTime.Now;
			try
			{
				IDatabase database = getRedis().GetDatabase(0);
				if (isSingle)
				{
					string sessionSingleKey = GetSessionSingleKey(singleVal);
					string text2 = database.StringGet(sessionSingleKey);
					if (!string.IsNullOrEmpty(text2))
					{
						database.KeyDelete(text2);
					}
					database.StringSet(sessionSingleKey, text, value);
				}
				database.StringSet(text, val, value);
			}
			catch (Exception exception)
			{
				LogUtil.Error($"HmSession.SetSession({key},{val})出错！", exception);
				closeRdies();
				return getRedis().GetDatabase(0).StringSet(text, val, value);
			}
			return true;
		}

		public static string GetSession(string key)
		{
			string empty = string.Empty;
			string key2 = GetSessionKey(key);
			try
			{
				return getRedis().GetDatabase(0).StringGet(key2);
			}
			catch (Exception exception)
			{
				LogUtil.Error($"HmSession.GetSession({key})出错！", exception);
				closeRdies();
				return getRedis().GetDatabase(0).StringGet(key2);
			}
		}

		public static void RemoveSession(string key)
		{
			try
			{
				getRedis().GetDatabase(0).KeyDelete(GetSessionKey(key));
			}
			catch (Exception exception)
			{
				LogUtil.Error($"HmSession.RemoveSession({key})出错！", exception);
				closeRdies();
				getRedis().GetDatabase(0).KeyDelete(key);
			}
		}
	}
}
