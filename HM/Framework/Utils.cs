using System;
using System.Configuration;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace HM.Framework
{
	public static class Utils
	{
		public const int MerchantCodeLength = 10;

		public static int MachineCode;

		public static string RechargeCode;

		static Utils()
		{
			MachineCode = 1;
			RechargeCode = "rc";
			MachineCode = StringToInt(GetAppSetting("MachineCode"), 1);
			RechargeCode = (string.IsNullOrEmpty(GetAppSetting("RechargeCode")) ? "rc" : GetAppSetting("RechargeCode"));
		}

		public static string GetRequest(string name)
		{
			string text = GetRequestForm(name);
			if (string.IsNullOrEmpty(text))
			{
				text = GetRequestQuery(name);
			}
			return text;
		}

		public static int GetRequestToInt(string name, int defVal = 0)
		{
			return StringToInt(GetRequest(name), defVal);
		}

		public static float GetRequestFloat(string name, float defVal = 0f)
		{
			return StringToFloat(GetRequest(name), defVal);
		}

		public static decimal GetRequestToDecimal(string name, decimal defVal = 0m)
		{
			return StringToDecimal(GetRequest(name), defVal);
		}

		public static DateTime? GetRequestToInt(string name, DateTime? defVal = default(DateTime?))
		{
			return StringToDateTime(GetRequest(name), defVal);
		}

		public static string GetRequestForm(string name)
		{
			return HttpContext.Current.Request.Form.Get(name);
		}

		public static int GetRequestFormToInt(string name, int defVal = 0)
		{
			return StringToInt(GetRequestForm(name), defVal);
		}

		public static float GetRequestFormFloat(string name, float defVal = 0f)
		{
			return StringToFloat(GetRequestForm(name), defVal);
		}

		public static decimal GetRequestFormToInt(string name, decimal defVal = 0m)
		{
			return StringToDecimal(GetRequestForm(name), defVal);
		}

		public static DateTime? GetRequestFormToInt(string name, DateTime? defVal = default(DateTime?))
		{
			return StringToDateTime(GetRequestForm(name), defVal);
		}

		public static string GetRequestQuery(string name)
		{
			return HttpContext.Current.Request.QueryString.Get(name);
		}

		public static int GetRequestQueryToInt(string name, int defVal = 0)
		{
			return StringToInt(GetRequestQuery(name), defVal);
		}

		public static float GetRequestQueryFloat(string name, float defVal = 0f)
		{
			return StringToFloat(GetRequestQuery(name), defVal);
		}

		public static decimal GetRequestQueryToDecimal(string name, decimal defVal = 0m)
		{
			return StringToDecimal(GetRequestQuery(name), defVal);
		}

		public static DateTime? GetRequestQueryToInt(string name, DateTime? defVal = default(DateTime?))
		{
			return StringToDateTime(GetRequestQuery(name), defVal);
		}

		public static int StringToInt(string str, int defValue)
		{
			if (string.IsNullOrEmpty(str) || str.Trim().Length >= 11 || !Regex.IsMatch(str.Trim(), "^([-]|[0-9])[0-9]*(\\.\\w*)?$"))
			{
				return defValue;
			}
			if (int.TryParse(str, out int result))
			{
				return result;
			}
			return defValue;
		}

		public static long StringToLong(string str, long defValue)
		{
			if (string.IsNullOrEmpty(str) || !Regex.IsMatch(str.Trim(), "^([-]|[0-9])[0-9]*(\\.\\w*)?$"))
			{
				return defValue;
			}
			if (long.TryParse(str, out long result))
			{
				return result;
			}
			return defValue;
		}

		public static float StringToFloat(string str, float defValue)
		{
			if (str == null || str.Length > 10)
			{
				return defValue;
			}
			float result = defValue;
			if (str != null && Regex.IsMatch(str, "^([-]|[0-9])[0-9]*(\\.\\w*)?$"))
			{
				float.TryParse(str, out result);
			}
			return result;
		}

		public static DateTime? StringToDateTime(string str, DateTime? defValue)
		{
			if (!string.IsNullOrEmpty(str) && DateTime.TryParse(str, out DateTime result))
			{
				return result;
			}
			return defValue;
		}

		public static decimal StringToDecimal(string str, decimal defValue)
		{
			if (!string.IsNullOrEmpty(str) && Regex.IsMatch(str, "^([-]|[0-9])[0-9]*(\\.\\w*)?$") && decimal.TryParse(str, out decimal result))
			{
				return result;
			}
			return defValue;
		}

		public static string GetAppSetting(string key)
		{
			try
			{
				return ConfigurationManager.AppSettings.Get(key);
			}
			catch (Exception)
			{
				return "";
			}
		}

		public static string GetConnectionStrings(string key)
		{
			return EncryUtils.RijndaelDecrypt(ConfigurationManager.ConnectionStrings[key].ToString());
		}

		public static bool QuickValidate(string _express, string _value)
		{
			Regex regex = new Regex(_express);
			if (_value.Length == 0)
			{
				return false;
			}
			return regex.IsMatch(_value);
		}

		public static bool IsNumeric(string _value)
		{
			return QuickValidate("^[1-9]*[0-9]*$", _value);
		}

		public static bool IsMobileNum(string _value)
		{
			return new Regex("^1\\d{10}$", RegexOptions.IgnoreCase).Match(_value).Success;
		}

		public static bool IsNumber(string _value)
		{
			return QuickValidate("^(0|([1-9]+[0-9]*))(.[0-9][0-9])?$", _value);
		}

		public static bool IsUrl(string source)
		{
			return Regex.IsMatch(source, "^((h|H)(t|T)(t|T)(p|P)|(f|F)(t|T)(p|P)|(f|F)(i|I)(l|L)(e|E)|(t|T)(e|E)(l|L)(n|N)(e|E)(t|T)|(g|G)(o|O)(p|P)(h|H)(e|E)(r|R)|(h|H)(t|T)(t|T)(p|P)(s|S)|(m|M)(a|A)(i|I)(l|L)(t|T)(o|O)|(n|N)(e|E)(w|W)(s|S)|(w|W)(a|A)(i|I)(s|S))://([\\w-]+(\\.)?)+[\\w-]+(:\\d+)?(/[\\w- ./?%&=]*)?$", RegexOptions.IgnoreCase);
		}

		public static bool IsNotifyUrlOk(string url)
		{
			if (IsUrl(url))
			{
				if (!url.Contains("?"))
				{
					return !url.Contains("&");
				}
				return false;
			}
			return false;
		}

		public static bool IsIPSect(string ip)
		{
			if (string.IsNullOrEmpty(ip))
			{
				return false;
			}
			return Regex.IsMatch(ip, "^((2[0-4]\\d|25[0-5]|[01]?\\d\\d?)\\.){2}((2[0-4]\\d|25[0-5]|[01]?\\d\\d?|\\*)\\.)(2[0-4]\\d|25[0-5]|[01]?\\d\\d?|\\*)$");
		}

		public static string CreateOrderId()
		{
			string text = Math.Abs(GetRandomSeed()).ToString();
			if (text.Length > 5)
			{
				text = text.Substring(0, 5);
			}
			return $"{DateTime.Now:yyMMddHHmmssfff}{text:00000}{MachineCode:000}";
		}

		public static string CreateRechargeOrderNo()
		{
			string text = Math.Abs(GetRandomSeed()).ToString();
			if (text.Length > 5)
			{
				text = text.Substring(0, 5);
			}
			return $"{RechargeCode:000}{DateTime.Now:yyMMddHHmmssfff}{text:00000}";
		}

		public static string CreateOrderNo(string prefix, string id)
		{
			return $"{prefix}{DateTime.Now:yyMMddHHmmss}{id}";
		}

		public static int GetRandomSeed()
		{
			byte[] array = new byte[4];
			new RNGCryptoServiceProvider().GetBytes(array);
			return BitConverter.ToInt32(array, 0);
		}

		public static decimal DateTimeToTimestamp()
		{
			return DateTimeToTimestamp(DateTime.Now);
		}

		public static decimal DateTimeToTimestamp(DateTime dateTime)
		{
			DateTime d = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
			return (long)(dateTime - d).TotalMilliseconds;
		}

		public static DateTime TimestampToDateTime(decimal timestamp)
		{
			return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddMilliseconds((double)(long)timestamp);
		}

		public static string GetHost()
		{
			return HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority;
		}

		public static string GetClientIp()
		{
			string empty = string.Empty;
			HttpRequest request = HttpContext.Current.Request;
			empty = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
			if (!string.IsNullOrEmpty(empty))
			{
				string[] array = empty.Split(',');
				foreach (string text in array)
				{
					if (!string.IsNullOrEmpty(text) && IsIPSect(text))
					{
						return text;
					}
				}
			}
			IPAddress[] hostAddresses = Dns.GetHostAddresses(request.UserHostAddress);
			foreach (IPAddress iPAddress in hostAddresses)
			{
				if (iPAddress.AddressFamily.ToString() == "InterNetwork")
				{
					empty = iPAddress.ToString();
					break;
				}
			}
			if (!string.IsNullOrEmpty(empty))
			{
				return empty;
			}
			hostAddresses = Dns.GetHostAddresses(Dns.GetHostName());
			foreach (IPAddress iPAddress2 in hostAddresses)
			{
				if (iPAddress2.AddressFamily.ToString() == "InterNetwork")
				{
					empty = iPAddress2.ToString();
					break;
				}
			}
			if (string.IsNullOrEmpty(empty))
			{
				empty = "127.0.0.1";
			}
			return empty;
		}

		public static string GetServerIp()
		{
			return Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString();
		}

		public static bool SetCookie(string key, string value, int? min = default(int?), string domain = null)
		{
			try
			{
				HttpCookie httpCookie = new HttpCookie(key);
				if (string.IsNullOrEmpty(domain))
				{
					httpCookie.Domain = domain;
				}
				if (min.HasValue)
				{
					httpCookie.Expires = DateTime.Now.AddMinutes((double)min.Value);
				}
				httpCookie.Value = value;
				HttpContext.Current.Response.Cookies.Add(httpCookie);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static string GetCookie(string key)
		{
			return HttpContext.Current.Request.Cookies[key]?.Value.ToString();
		}

		public static bool DelCookie(string key, string domain = null)
		{
			try
			{
				HttpCookie httpCookie = new HttpCookie(key);
				if (!string.IsNullOrEmpty(domain))
				{
					httpCookie.Domain = domain;
				}
				httpCookie.Expires = DateTime.Now.AddDays(-1.0);
				HttpContext.Current.Response.Cookies.Add(httpCookie);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static string GetHeader(string key)
		{
			return HttpContext.Current.Request.Headers.Get(key);
		}

		public static string StarCharShow(int program_StarLen, string program_StarChar)
		{
			string text = "";
			for (int i = 0; i < program_StarLen; i++)
			{
				text += program_StarChar;
			}
			return text;
		}

		public static string StrReplaceByStar(string str, int program_starBegin, int program_starEnd, string program_StarChar)
		{
			string text = "";
			string str2 = "";
			int num = program_starEnd - program_starBegin + 1;
			if (num <= 0)
			{
				return str;
			}
			string str3 = StarCharShow(num, program_StarChar);
			if (str.Length <= program_starEnd)
			{
				text = ((str.Length >= program_starBegin) ? str.Substring(0, program_starBegin - 1) : str);
			}
			else
			{
				text = str.Substring(0, program_starBegin - 1);
				str2 = str.Substring(program_starEnd, str.Length - program_starEnd);
			}
			return text + str3 + str2;
		}

		public static string MaskMobilePhone(string phone)
		{
			if (string.IsNullOrEmpty(phone))
			{
				return phone;
			}
			if (phone.Length != 11)
			{
				return phone;
			}
			return StrReplaceByStar(phone, 4, 7, "*");
		}

		public static string MaskIdCard(string idCard)
		{
			int length = idCard.Length;
			switch (length)
			{
			case 15:
				return StrReplaceByStar(idCard, length - 6, length, "*");
			case 18:
				return StrReplaceByStar(idCard, length - 6, length, "*");
			default:
				return idCard;
			}
		}

		public static string MaskEmail(string email)
		{
			int num = email.LastIndexOf('@');
			if (num < 2)
			{
				return email;
			}
			return StrReplaceByStar(email, num - 2, num, "*");
		}

		public static string MaskBankCode(string bankCode)
		{
			int length = bankCode.Length;
			if (length > 12)
			{
				return StrReplaceByStar(bankCode, 6, length - 4, "*");
			}
			return bankCode;
		}

		public static string DelNum0(string arraylist)
		{
			if (string.IsNullOrEmpty(arraylist))
			{
				return "";
			}
			try
			{
				if (Convert.ToDecimal(arraylist) == decimal.Zero)
				{
					return "0";
				}
			}
			catch (Exception)
			{
				return arraylist;
			}
			int num = 0;
			string text = "";
			for (int num2 = arraylist.Length - 1; num2 >= 0; num2--)
			{
				if (arraylist[num2].ToString() != "0")
				{
					num = num2;
					break;
				}
			}
			for (int i = 0; i <= num; i++)
			{
				if (i != num || !(arraylist[i].ToString() == "."))
				{
					text += arraylist[i].ToString();
				}
			}
			return text;
		}

		public static string GetRandomStr(int n)
		{
			string text = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
			StringBuilder stringBuilder = new StringBuilder();
			Random random = new Random();
			for (int i = 0; i < n; i++)
			{
				stringBuilder.Append(text.Substring(random.Next(0, text.Length), 1));
			}
			return stringBuilder.ToString();
		}
	}
}
