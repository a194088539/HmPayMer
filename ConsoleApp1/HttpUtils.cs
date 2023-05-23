using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace HM.Framework
{
	public abstract class HttpUtils
	{
		public HttpUtils()
		{
		}

		public static string SendRequest(string url, string para, string method, string encoding = "gb2312")
		{
			return SendRequest(url, para, method, encoding, null);
		}

		public static string SendRequest(string url, string para, string method, string encoding, string contentType)
		{
			string text = "";
			if (url == null || url == "")
			{
				return null;
			}
			if (method == null || method == "")
			{
				method = "GET";
			}
			if (string.IsNullOrEmpty(contentType))
			{
				contentType = "application/x-www-form-urlencoded";
			}
			if (method.ToUpper() == "GET")
			{
				try
				{
					WebRequest webRequest = WebRequest.Create(url + para);
					webRequest.Method = "GET";
					if (url.StartsWith("https://"))
					{
						ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
					}
					text = new StreamReader(webRequest.GetResponse().GetResponseStream(), Encoding.GetEncoding(encoding)).ReadToEnd();
				}
				catch (Exception ex)
				{
					return ex.Message;
				}
			}
			if (method.ToUpper() == "POST")
			{
				if (para.Length > 0 && para.IndexOf('?') == 0)
				{
					para = para.Substring(1);
				}
				WebRequest webRequest2 = WebRequest.Create(url);
				webRequest2.Method = "POST";
				webRequest2.ContentType = contentType;
				if (url.StartsWith("https://"))
				{
					ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
				}
				StringBuilder stringBuilder = new StringBuilder();
				char[] anyOf = new char[3]
				{
					'?',
					'=',
					'&'
				};
				byte[] array = null;
				if (para != null)
				{
					int num2;
					for (int num = 0; num < para.Length; num = num2 + 1)
					{
						num2 = para.IndexOfAny(anyOf, num);
						if (num2 == -1)
						{
							stringBuilder.Append(HttpUtility.UrlEncode(para.Substring(num, para.Length - num), Encoding.GetEncoding(encoding)));
							break;
						}
						stringBuilder.Append(HttpUtility.UrlEncode(para.Substring(num, num2 - num), Encoding.GetEncoding(encoding)));
						stringBuilder.Append(para.Substring(num2, 1));
					}
					array = Encoding.Default.GetBytes(stringBuilder.ToString());
					webRequest2.ContentLength = array.Length;
					Stream requestStream = webRequest2.GetRequestStream();
					requestStream.Write(array, 0, array.Length);
					requestStream.Close();
				}
				else
				{
					webRequest2.ContentLength = 0L;
				}
				try
				{
					Stream responseStream = webRequest2.GetResponse().GetResponseStream();
					byte[] array2 = new byte[512];
					for (int num3 = responseStream.Read(array2, 0, 512); num3 > 0; num3 = responseStream.Read(array2, 0, 512))
					{
						Encoding encoding2 = Encoding.GetEncoding(encoding);
						text += encoding2.GetString(array2, 0, num3);
					}
					return text;
				}
				catch (Exception ex2)
				{
					return ex2.Message;
				}
			}
			return text;
		}


        public static string SendRequest(string url, IDictionary<string, string> parameters, string method, string encoding, string contentType)
        {
            string text = "";
            if (url == null || url == "")
            {
                return null;
            }
            if (method == null || method == "")
            {
                method = "GET";
            }
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = "application/x-www-form-urlencoded";
            }

            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<string,string> p in parameters)
            {
                stringBuilder.Append(p.Key);
                stringBuilder.Append("=");
                stringBuilder.Append(HttpUtility.UrlEncode(p.Value, Encoding.GetEncoding(encoding)));
                stringBuilder.Append("&");
            }
            if(stringBuilder.Length > 0)
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            if (method.ToUpper() == "GET")
            {
                try
                {
                    WebRequest webRequest = WebRequest.Create(url + stringBuilder.ToString());
                    webRequest.Method = "GET";
                    if (url.StartsWith("https://"))
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                    }
                    text = new StreamReader(webRequest.GetResponse().GetResponseStream(), Encoding.GetEncoding(encoding)).ReadToEnd();
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            if (method.ToUpper() == "POST")
            {
                WebRequest webRequest2 = WebRequest.Create(url);
                webRequest2.Method = "POST";
                webRequest2.ContentType = contentType;
                if (url.StartsWith("https://"))
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                }
                byte[] array = null;
                if (stringBuilder.Length > 0)
                {
                    array = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                    webRequest2.ContentLength = array.Length;
                    Stream requestStream = webRequest2.GetRequestStream();
                    requestStream.Write(array, 0, array.Length);
                    requestStream.Close();
                }
                else
                {
                    webRequest2.ContentLength = 0L;
                }
                try
                {
                    System.IO.StreamReader sr = new System.IO.StreamReader(webRequest2.GetResponse().GetResponseStream(), System.Text.Encoding.GetEncoding(encoding));
                    text = sr.ReadToEnd();

                    return text;
                }
                catch (Exception ex2)
                {
                    return ex2.Message;
                }
            }
            return text;
        }

        public static string SendRequest(string url, string para)
		{
			return SendRequest(url, para, "GET");
		}
	}
}
