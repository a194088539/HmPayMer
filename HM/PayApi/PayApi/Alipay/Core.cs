using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace HM.Framework.PayApi.Alipay
{
	public class Core
	{
		public static Dictionary<string, string> FilterPara(SortedDictionary<string, string> dicArrayPre)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (KeyValuePair<string, string> item in dicArrayPre)
			{
				if (item.Key.ToLower() != "sign" && item.Key.ToLower() != "sign_type" && item.Value != "" && item.Value != null)
				{
					dictionary.Add(item.Key, item.Value);
				}
			}
			return dictionary;
		}

		public static string CreateLinkString(Dictionary<string, string> dicArray)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, string> item in dicArray)
			{
				stringBuilder.Append(item.Key + "=" + item.Value + "&");
			}
			int length = stringBuilder.Length;
			stringBuilder.Remove(length - 1, 1);
			return stringBuilder.ToString();
		}

        public static string CreateLinkString(SortedDictionary<string, string> dicArray)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> item in dicArray)
            {
                stringBuilder.Append(item.Key + "=" + item.Value + "&");
            }
            int length = stringBuilder.Length;
            stringBuilder.Remove(length - 1, 1);
            return stringBuilder.ToString();
        }

        public static string CreateLinkStringUrlencode(Dictionary<string, string> dicArray, Encoding code)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, string> item in dicArray)
			{
				stringBuilder.Append(item.Key + "=" + HttpUtility.UrlEncode(item.Value, code) + "&");
			}
			int length = stringBuilder.Length;
			stringBuilder.Remove(length - 1, 1);
			return stringBuilder.ToString();
		}

		public static string GetAbstractToMD5(Stream sFile)
		{
			byte[] array = new MD5CryptoServiceProvider().ComputeHash(sFile);
			StringBuilder stringBuilder = new StringBuilder(32);
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("x").PadLeft(2, '0'));
			}
			return stringBuilder.ToString();
		}

		public static string GetAbstractToMD5(byte[] dataFile)
		{
			byte[] array = new MD5CryptoServiceProvider().ComputeHash(dataFile);
			StringBuilder stringBuilder = new StringBuilder(32);
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("x").PadLeft(2, '0'));
			}
			return stringBuilder.ToString();
		}
	}
}
