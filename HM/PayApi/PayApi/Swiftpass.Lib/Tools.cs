using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using System.Xml;

namespace HM.Framework.PayApi.Swiftpass.Lib
{
	public class Tools
	{
		public static string UrlEncode(string instr, string charset)
		{
			if (instr != null && !(instr.Trim() == ""))
			{
				try
				{
					return HttpUtility.UrlEncode(instr, Encoding.GetEncoding(charset));
				}
				catch (Exception value)
				{
					string result = HttpUtility.UrlEncode(instr, Encoding.GetEncoding("GB2312"));
					Console.WriteLine(value);
					return result;
				}
			}
			return "";
		}

		public static string UrlDecode(string instr, string charset)
		{
			if (instr != null && !(instr.Trim() == ""))
			{
				try
				{
					return HttpUtility.UrlDecode(instr, Encoding.GetEncoding(charset));
				}
				catch (Exception value)
				{
					string result = HttpUtility.UrlDecode(instr, Encoding.GetEncoding("GB2312"));
					Console.WriteLine(value);
					return result;
				}
			}
			return "";
		}

		public static uint UnixStamp()
		{
			return Convert.ToUInt32((DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1))).TotalSeconds);
		}

		public static string BuildRandomStr(int length)
		{
			string text = new Random().Next().ToString();
			if (text.Length > length)
			{
				text = text.Substring(0, length);
			}
			else if (text.Length < length)
			{
				for (int num = length - text.Length; num > 0; num--)
				{
					text.Insert(0, "0");
				}
			}
			return text;
		}

		public static string random()
		{
			char[] array = new char[62]
			{
				'0',
				'1',
				'2',
				'3',
				'4',
				'5',
				'6',
				'7',
				'8',
				'9',
				'a',
				'b',
				'c',
				'd',
				'e',
				'f',
				'g',
				'h',
				'i',
				'j',
				'k',
				'l',
				'm',
				'n',
				'o',
				'p',
				'q',
				'r',
				's',
				't',
				'u',
				'v',
				'w',
				'x',
				'y',
				'z',
				'A',
				'B',
				'C',
				'D',
				'E',
				'F',
				'G',
				'H',
				'I',
				'J',
				'K',
				'L',
				'M',
				'N',
				'O',
				'P',
				'Q',
				'R',
				'S',
				'T',
				'U',
				'V',
				'W',
				'X',
				'Y',
				'Z'
			};
			StringBuilder stringBuilder = new StringBuilder(32);
			Random random = new Random();
			for (int i = 0; i < 32; i++)
			{
				stringBuilder.Append(array[random.Next(62)]);
			}
			return stringBuilder.ToString();
		}

		public static string toXml(Hashtable _params)
		{
			StringBuilder stringBuilder = new StringBuilder("<xml>");
			IDictionaryEnumerator enumerator = _params.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					DictionaryEntry dictionaryEntry = (DictionaryEntry)enumerator.Current;
					string value = dictionaryEntry.Key.ToString();
					stringBuilder.Append("<").Append(value).Append("><![CDATA[")
						.Append(dictionaryEntry.Value.ToString())
						.Append("]]></")
						.Append(value)
						.Append(">");
				}
			}
			finally
			{
				(enumerator as IDisposable)?.Dispose();
			}
			return stringBuilder.Append("</xml>").ToString();
		}

		public static string toXml(IDictionary<string, string> _params)
		{
			StringBuilder stringBuilder = new StringBuilder("<xml>");
			foreach (KeyValuePair<string, string> _param in _params)
			{
				string value = _param.Key.ToString();
				stringBuilder.Append("<").Append(value).Append("><![CDATA[")
					.Append(_param.Value.ToString())
					.Append("]]></")
					.Append(value)
					.Append(">");
			}
			return stringBuilder.Append("</xml>").ToString();
		}

		public static Hashtable toHashtable(string content)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(content);
			XmlNodeList childNodes = xmlDocument.SelectSingleNode("xml").ChildNodes;
			Hashtable hashtable = new Hashtable();
			foreach (XmlNode item in childNodes)
			{
				hashtable.Add(item.Name, item.InnerText);
			}
			return hashtable;
		}

		public static Dictionary<string, string> toDictionary(string content)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(content);
			XmlNodeList childNodes = xmlDocument.SelectSingleNode("xml").ChildNodes;
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (XmlNode item in childNodes)
			{
				dictionary.Add(item.Name, item.InnerText);
			}
			return dictionary;
		}

		public static string FormatRequestData(NameValueCollection list)
		{
			if (list == null || list.Count == 0)
			{
				return string.Empty;
			}
			string[] array = new string[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				array[i] = $"{list.Keys[i]}={list[i]}";
			}
			return string.Join("\n\r", array);
		}
	}
}
