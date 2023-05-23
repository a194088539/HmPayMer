using System.Text;
using System.Web;

namespace HM.Framework.PayApi.ShangDe
{
	public class MessageWorker
	{
		public struct trafficMessage
		{
			public string charset;

			public string signType;

			public string data;

			public string sign;

			public string extend;
		}

		private string loggerHeader = "MessageWorker_";

		private Encoding encodeCode = Encoding.UTF8;

		private string pfxFilePath = string.Empty;

		private string pfxPassword = string.Empty;

		private string cerFilePath = string.Empty;

		public string PFXFile
		{
			get
			{
				return pfxFilePath;
			}
			set
			{
				pfxFilePath = value;
			}
		}

		public string PFXPassword
		{
			get
			{
				return pfxPassword;
			}
			set
			{
				pfxPassword = value;
			}
		}

		public string CerFile
		{
			get
			{
				return cerFilePath;
			}
			set
			{
				cerFilePath = value;
			}
		}

		public trafficMessage UrlDecodeMessage(string msgResponse)
		{
			trafficMessage result = default(trafficMessage);
			string[] array = HttpUtility.UrlDecode(msgResponse).Split('&');
			for (int i = 0; i < array.Length; i++)
			{
				switch (array[i].Split('=')[0])
				{
				case "charset":
					result.charset = array[i].Replace("charset=", "").Trim('"');
					break;
				case "signType":
					result.signType = array[i].Replace("signType=", "").Trim('"');
					break;
				case "data":
					result.data = array[i].Replace("data=", "").Trim('"');
					break;
				case "sign":
					result.sign = array[i].Replace("sign=", "").Trim('"');
					break;
				case "extend":
					result.extend = array[i].Replace("extend=", "").Trim('"');
					break;
				}
			}
			return result;
		}

		public string UrlEncodeMessage(trafficMessage msgRequest)
		{
			return "charset=" + HttpUtility.UrlEncode(msgRequest.charset) + "&signType=" + HttpUtility.UrlEncode(msgRequest.signType) + "&data=" + HttpUtility.UrlEncode(msgRequest.data) + "&sign=" + HttpUtility.UrlEncode(msgRequest.sign) + "&extend=" + HttpUtility.UrlEncode(msgRequest.extend);
		}

		public trafficMessage SignMessageBeforePost(trafficMessage msgSource)
		{
			trafficMessage result = default(trafficMessage);
			encodeCode = Encoding.GetEncoding(msgSource.charset);
			result.charset = msgSource.charset;
			result.signType = msgSource.signType;
			result.extend = msgSource.extend;
			result.data = msgSource.data;
			result.sign = CryptUtils.Base64Encoder(CryptUtils.CreateSignWithPrivateKey(CryptUtils.getBytesFromString(msgSource.data, encodeCode), CryptUtils.getPrivateKeyXmlFromPFX(pfxFilePath, pfxPassword)));
			return result;
		}

		public trafficMessage CheckSignMessageAfterResponse(trafficMessage msgEncrypt)
		{
			trafficMessage result = default(trafficMessage);
			encodeCode = Encoding.GetEncoding(msgEncrypt.charset);
			result.charset = msgEncrypt.charset;
			result.signType = msgEncrypt.signType;
			result.extend = msgEncrypt.extend;
			result.data = msgEncrypt.data;
			result.sign = CryptUtils.VerifySignWithPublicKey(CryptUtils.getBytesFromString(msgEncrypt.data, encodeCode), CryptUtils.getPublicKeyXmlFromCer(cerFilePath), CryptUtils.Base64Decoder(msgEncrypt.sign)).ToString();
			return result;
		}

		public trafficMessage postMessage(string serverUrl, trafficMessage requestSourceMessage)
		{
			string paramData = UrlEncodeMessage(SignMessageBeforePost(requestSourceMessage));
			string msgResponse = HttpUtils.HttpPost(serverUrl, paramData, encodeCode);
			return CheckSignMessageAfterResponse(UrlDecodeMessage(msgResponse));
		}
	}
}
