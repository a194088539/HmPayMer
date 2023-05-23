using System.Collections.Generic;

namespace HM.Framework.PayApi.XingFuTianXia.Lib
{
	public class XfData
	{
		public static Dictionary<string, string> FormString(string str)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string[] array = str.Split('&');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Trim().Split('=');
				if (array2.Length == 2)
				{
					string key = array2[0].Trim();
					string value = array2[1].Trim();
					if (!dictionary.ContainsKey(key))
					{
						dictionary.Add(key, value);
					}
				}
			}
			return dictionary;
		}
	}
}
