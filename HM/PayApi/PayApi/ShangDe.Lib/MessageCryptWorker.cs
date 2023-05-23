using HM.Framework.Logging;
using System;
using System.Net;
using System.Text;
using System.Web;

namespace HM.Framework.PayApi.ShangDe.Lib
{
	public class MessageCryptWorker
	{
		public struct trafficMessage
		{
			public string transCode;

			public string merId;

			public string encryptKey;

			public string encryptData;

			public string sign;

			public string extend;

			public string plId;

			public string accessType;
		}

		private Encoding encodeCode = Encoding.UTF8;

		private string pfxFilePath = string.Empty;

		private string pfxPassword = string.Empty;

		private string cerFilePath = string.Empty;

		public Encoding EncodeCode
		{
			get
			{
				return encodeCode;
			}
			set
			{
				encodeCode = value;
			}
		}

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

		private trafficMessage EncryptMessageBeforePost(trafficMessage msgSource)
		{
			trafficMessage trafficMessage = default(trafficMessage);
			trafficMessage.transCode = msgSource.transCode;
			trafficMessage.merId = msgSource.merId;
			trafficMessage.extend = msgSource.extend;
			trafficMessage.plId = msgSource.plId;
			trafficMessage.accessType = msgSource.accessType;
			msgSource.encryptKey = CryptUtils.GuidTo16String();
			trafficMessage.encryptKey = CryptUtils.Base64Encoder(CryptUtils.RSAEncrypt(CryptUtils.getPublicKeyXmlFromCer(cerFilePath).PublicKey.Key.ToXmlString(includePrivateParameters: false), CryptUtils.getBytesFromString(msgSource.encryptKey, encodeCode)));
			LogUtil.Debug("encryptKey[" + msgSource.encryptKey + "][" + trafficMessage.encryptKey + "]");
			trafficMessage.encryptData = CryptUtils.Base64Encoder(CryptUtils.AESEncrypt(CryptUtils.getBytesFromString(msgSource.encryptData, encodeCode), msgSource.encryptKey));
			LogUtil.Debug("encryptData[" + msgSource.encryptData + "][" + trafficMessage.encryptData + "]");
			trafficMessage.sign = CryptUtils.Base64Encoder(CryptUtils.CreateSignWithPrivateKey(CryptUtils.getBytesFromString(msgSource.encryptData, encodeCode), CryptUtils.getPrivateKeyXmlFromPFX(pfxFilePath, pfxPassword)));
			LogUtil.Debug("sign[" + trafficMessage.sign + "]");
			return trafficMessage;
		}

		public trafficMessage DecryptMessageAfterResponse(trafficMessage msgEncrypt)
		{
			trafficMessage trafficMessage = default(trafficMessage);
			trafficMessage.transCode = msgEncrypt.transCode;
			trafficMessage.merId = msgEncrypt.merId;
			trafficMessage.extend = msgEncrypt.extend;
			trafficMessage.plId = msgEncrypt.plId;
			trafficMessage.accessType = msgEncrypt.accessType;
			trafficMessage.encryptKey = CryptUtils.getStringFromBytes(CryptUtils.RSADecrypt(CryptUtils.getPrivateKeyXmlFromPFX(pfxFilePath, pfxPassword).PrivateKey.ToXmlString(includePrivateParameters: true), CryptUtils.Base64Decoder(msgEncrypt.encryptKey)), encodeCode);
			LogUtil.Debug("Decrypted remote AESkey [" + trafficMessage.encryptKey + "]");
			LogUtil.Debug("decryptKey[" + trafficMessage.encryptKey + "]");
			byte[] array = CryptUtils.AESDecrypt(CryptUtils.Base64Decoder(msgEncrypt.encryptData), trafficMessage.encryptKey);
			trafficMessage.encryptData = CryptUtils.getStringFromBytes(array, encodeCode);
			LogUtil.Debug("decryptData[" + trafficMessage.encryptData + "][" + msgEncrypt.encryptData + "]");
			trafficMessage.sign = CryptUtils.VerifySignWithPublicKey(array, CryptUtils.getPublicKeyXmlFromCer(cerFilePath), CryptUtils.Base64Decoder(msgEncrypt.sign)).ToString();
			LogUtil.Debug("sign[" + trafficMessage.sign + "][" + msgEncrypt.sign + "]");
			return trafficMessage;
		}

		private trafficMessage UrlDecodeMessage(string msgResponse)
		{
			trafficMessage result = default(trafficMessage);
			string[] array = msgResponse.Split('&');
			for (int i = 0; i < array.Length; i++)
			{
				switch (array[i].Split('=')[0])
				{
				case "plId":
					result.plId = HttpUtility.UrlDecode(array[i].Replace("plId=", "").Trim('"'));
					break;
				case "accessType":
					result.accessType = HttpUtility.UrlDecode(array[i].Replace("accessType=", "").Trim('"'));
					break;
				case "transCode":
					result.transCode = HttpUtility.UrlDecode(array[i].Replace("transCode=", "").Trim('"'));
					break;
				case "merId":
					result.merId = HttpUtility.UrlDecode(array[i].Replace("merId=", "").Trim('"'));
					break;
				case "encryptKey":
					result.encryptKey = HttpUtility.UrlDecode(array[i].Replace("encryptKey=", "").Trim('"'));
					break;
				case "encryptData":
					result.encryptData = HttpUtility.UrlDecode(array[i].Replace("encryptData=", "").Trim('"'));
					break;
				case "sign":
					result.sign = HttpUtility.UrlDecode(array[i].Replace("sign=", "").Trim('"'));
					break;
				case "extend":
					result.extend = HttpUtility.UrlDecode(array[i].Replace("extend=", "").Trim('"'));
					break;
				}
			}
			return result;
		}

		private string UrlEncodeMessage(trafficMessage msgRequest)
		{
			return "transCode=" + HttpUtility.UrlEncode(msgRequest.transCode) + "&plId=" + HttpUtility.UrlEncode(msgRequest.plId) + "&accessType=" + HttpUtility.UrlEncode(msgRequest.accessType) + "&merId=" + HttpUtility.UrlEncode(msgRequest.merId) + "&encryptKey=" + HttpUtility.UrlEncode(msgRequest.encryptKey) + "&encryptData=" + HttpUtility.UrlEncode(msgRequest.encryptData) + "&sign=" + HttpUtility.UrlEncode(msgRequest.sign) + "&extend=" + HttpUtility.UrlEncode(msgRequest.extend);
		}

		public trafficMessage postMessage(string serverUrl, trafficMessage requestSourceMessage, CookieContainer cookies = null)
		{
			trafficMessage result = default(trafficMessage);
			try
			{
				string text = UrlEncodeMessage(EncryptMessageBeforePost(requestSourceMessage));
				string text2 = HttpUtils.HttpPost(serverUrl, text, encodeCode);
				LogUtil.Debug("serverUrl <==[" + serverUrl + "]");
				LogUtil.Debug("requestString <==[" + text + "]");
				LogUtil.Debug("response <==[" + text2 + "]");
				result = DecryptMessageAfterResponse(UrlDecodeMessage(text2));
				return result;
			}
			catch (Exception ex)
			{
				LogUtil.Debug(ex.ToString());
				result.extend = ex.ToString();
				return result;
			}
		}
	}
}
