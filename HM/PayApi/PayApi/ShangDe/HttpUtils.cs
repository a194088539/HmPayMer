using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace HM.Framework.PayApi.ShangDe
{
	public class HttpUtils
	{
		public static string HttpPost(string postUrl, string paramData, Encoding EncodingName)
		{
			byte[] bytes = EncodingName.GetBytes(paramData);
			HttpWebRequest httpWebRequest = null;
			if (postUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase))
			{
				ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;
				httpWebRequest = (WebRequest.Create(postUrl) as HttpWebRequest);
				httpWebRequest.ProtocolVersion = HttpVersion.Version10;
			}
			else
			{
				httpWebRequest = (WebRequest.Create(postUrl) as HttpWebRequest);
			}
			httpWebRequest.Method = "POST";
			httpWebRequest.ContentType = "application/x-www-form-urlencoded";
			Stream requestStream = httpWebRequest.GetRequestStream();
			requestStream.Write(bytes, 0, bytes.Length);
			requestStream.Close();
			Stream responseStream = ((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream();
			StreamReader streamReader = new StreamReader(responseStream, EncodingName);
			string result = streamReader.ReadToEnd();
			streamReader.Close();
			responseStream.Close();
			return result;
		}

		private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
		{
			return true;
		}
	}
}
