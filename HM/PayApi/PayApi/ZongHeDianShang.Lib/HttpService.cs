using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace HM.Framework.PayApi.ZongHeDianShang.Lib
{
	public class HttpService
	{
		public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
		{
			return true;
		}

		public static string Post(string url, string xml, string contentType, int timeout)
		{
			GC.Collect();
			string result = "";
			bool flag = false;
			HttpWebRequest httpWebRequest = null;
			HttpWebResponse httpWebResponse = null;
			try
			{
				ServicePointManager.DefaultConnectionLimit = 200;
				if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
				{
					ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;
				}
				httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
				httpWebRequest.Method = "POST";
				httpWebRequest.Timeout = timeout * 1000;
				httpWebRequest.ContentType = contentType;
				byte[] bytes = Encoding.UTF8.GetBytes(xml);
				httpWebRequest.ContentLength = bytes.Length;
				Stream requestStream = httpWebRequest.GetRequestStream();
				requestStream.Write(bytes, 0, bytes.Length);
				requestStream.Close();
				httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
				result = streamReader.ReadToEnd().Trim();
				streamReader.Close();
				return result;
			}
			catch (ThreadAbortException)
			{
				Thread.ResetAbort();
				return result;
			}
			catch (WebException)
			{
				return result;
			}
			catch (Exception)
			{
				return result;
			}
			finally
			{
				httpWebResponse?.Close();
				httpWebRequest?.Abort();
			}
		}

		public static string Get(string url)
		{
			GC.Collect();
			string result = "";
			HttpWebRequest httpWebRequest = null;
			HttpWebResponse httpWebResponse = null;
			try
			{
				ServicePointManager.DefaultConnectionLimit = 200;
				if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
				{
					ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;
				}
				httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
				httpWebRequest.Method = "GET";
				httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
				result = streamReader.ReadToEnd().Trim();
				streamReader.Close();
				return result;
			}
			catch (ThreadAbortException)
			{
				return result;
			}
			catch (WebException)
			{
				return result;
			}
			catch (Exception)
			{
				return result;
			}
			finally
			{
				httpWebResponse?.Close();
				httpWebRequest?.Abort();
			}
		}
	}
}
