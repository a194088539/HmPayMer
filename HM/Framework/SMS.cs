using System.Text;
using System.Web;

namespace HM.Framework
{
	public class SMS
	{
		private string apiUri = "http://api.smsbao.com/sms";

		public string Sn
		{
			get;
			set;
		}

		public string Key
		{
			get;
			set;
		}

		public string Ext
		{
			get;
			set;
		}

		public bool IsVerify
		{
			get;
			set;
		}

		public SMS()
		{
			string appSetting = Utils.GetAppSetting("SMSSN");
			string appSetting2 = Utils.GetAppSetting("SMSKEY");
			string appSetting3 = Utils.GetAppSetting("SMEXT");
			Sn = appSetting;
			Key = appSetting2;
			Ext = appSetting3;
		}

		public SMS(string sn, string key)
			: this(sn, key, "")
		{
		}

		public SMS(string sn, string key, string ext)
		{
			Sn = sn;
			Key = key;
			Ext = ext;
		}

		public SMSState Send(string mobile, string content)
		{
			Key = EncryUtils.MD5(Key);
			SMSState result = SMSState.发送失败;
			HttpUtility.UrlEncode(content, Encoding.GetEncoding("utf-8"));
			string para = $"?u={Sn}&p={Key}&m={mobile}&c={content}&ext=0{Ext}";
			if (HttpUtils.SendRequest(apiUri, para, "GET", "UTF-8").Equals("0"))
			{
				result = SMSState.发送成功;
			}
			return result;
		}
	}
}
