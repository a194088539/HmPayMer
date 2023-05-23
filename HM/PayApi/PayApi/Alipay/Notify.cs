using HM.Framework.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace HM.Framework.PayApi.Alipay
{
	public class Notify
	{
		public string _sign_type = "MD5";

		private string Https_veryfy_url = "https://mapi.alipay.com/gateway.do?service=notify_verify&";

		public string _partner
		{
			get;
			set;
		}

		public string _key
		{
			get;
			set;
		}

		public string _input_charset
		{
			get;
			set;
		}

		public static string getPublicKeyStr(string Path)
		{
			StreamReader streamReader = new StreamReader(Path);
			string text = streamReader.ReadToEnd();
			streamReader.Close();
			if (text != null)
			{
				text = text.Replace("-----BEGIN PUBLIC KEY-----", "");
				text = text.Replace("-----END PUBLIC KEY-----", "");
				text = text.Replace("\r", "");
				text = text.Replace("\n", "");
			}
			return text;
		}

		public bool Verify(SortedDictionary<string, string> inputPara, string notify_id, string sign)
		{
			bool signVeryfy = GetSignVeryfy(inputPara, sign);
			string a = "false";
			if (notify_id != null && notify_id != "")
			{
				a = GetResponseTxt(notify_id);
			}
			if (a == "true" && signVeryfy)
			{
				return true;
			}
			return false;
		}

		private string GetPreSignStr(SortedDictionary<string, string> inputPara)
		{
			new Dictionary<string, string>();
			return Core.CreateLinkString(Core.FilterPara(inputPara));
		}

		private bool GetSignVeryfy(SortedDictionary<string, string> inputPara, string sign)
		{
			new Dictionary<string, string>();
			string prestr = Core.CreateLinkString(Core.FilterPara(inputPara));
			bool result = false;
			if (sign != null && sign != "")
			{
				string sign_type = _sign_type;
				if (sign_type == "MD5")
				{
					result = AlipayMD5.Verify(prestr, sign, _key, _input_charset);
				}
			}
			return result;
		}

		private string GetResponseTxt(string notify_id)
		{
			string strUrl = Https_veryfy_url + "partner=" + _partner + "&notify_id=" + notify_id;
			return Get_Http(strUrl, 120000);
		}

		private string Get_Http(string strUrl, int timeout)
		{
			string text;
			try
			{
				HttpWebRequest obj = (HttpWebRequest)WebRequest.Create(strUrl);
				obj.Timeout = timeout;
				StreamReader streamReader = new StreamReader(((HttpWebResponse)obj.GetResponse()).GetResponseStream(), Encoding.Default);
				StringBuilder stringBuilder = new StringBuilder();
				while (-1 != streamReader.Peek())
				{
					stringBuilder.Append(streamReader.ReadLine());
				}
				text = stringBuilder.ToString();
			}
			catch (Exception ex)
			{
				text = "错误：" + ex.Message;
			}
			LogUtil.DebugFormat("支付宝扫码支付回调:{0}", text);
			return text;
		}
	}
}
