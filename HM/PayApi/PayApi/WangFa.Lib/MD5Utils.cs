using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HM.Framework.PayApi.WangFa.Lib
{
	public class MD5Utils
	{
		public static string GetMd5(string input)
		{
			if (input == null)
			{
				return null;
			}
			byte[] array = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(input));
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("x2"));
			}
			return stringBuilder.ToString();
		}

		public static string getSign(Dictionary<string, object> paramDic, string paySecret)
		{
			paramDic = (from k in paramDic
			orderby k.Key
			select k).ToDictionary((KeyValuePair<string, object> k) => k.Key, (KeyValuePair<string, object> p) => p.Value);
			string text = "";
			foreach (KeyValuePair<string, object> item in paramDic)
			{
				text = text + item.Key + "=" + item.Value + "&";
			}
			text = text + "secret_key=" + paySecret;
			return GetMd5(text).ToUpper();
		}
	}
}
