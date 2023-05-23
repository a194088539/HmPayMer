using HM.Framework;
using HmPMer.Business;
using HmPMer.Entity;
using System.Reflection;

namespace HmPMer.WebAdminUI.Models
{
	public class ModelCommon
	{
		private static string SessionSite;

		private static string StaticVersion;

		static ModelCommon()
		{
			SessionSite = "hmpmer.admin";
			StaticVersion = "";
			SessionSite = Utils.GetAppSetting("SessionSite");
			if (string.IsNullOrEmpty(SessionSite))
			{
				SessionSite = "hmpmer.admin";
			}
			StaticVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
		}

		public static HmAdmin GetUserModel()
		{
			string session = HmSession.GetSession("user");
			if (string.IsNullOrEmpty(session))
			{
				return null;
			}
			return session.FormJson<HmAdmin>();
		}

		public static bool WriteUserModel(HmAdmin model, int min = 3600)
		{
			string val = model.ToJson();
			HmSession.SetSession("user", val, true, model.ID, min);
			return true;
		}

		public static bool RemoveUserModel()
		{
			HmSession.RemoveSession("user");
			return true;
		}

		public static string GetVersion()
		{
			return StaticVersion;
		}

		public static string GetSysConfigVal(string key)
		{
			SysConfigBll sysConfigBll = new SysConfigBll();
			SysConfig forKey = sysConfigBll.GetForKey(key);
			if (forKey != null)
			{
				return forKey.Value;
			}
			return "";
		}
	}
}
