using HM.Framework.Logging;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

namespace HM.Framework.PayApi.YunFuBao.Lib
{
	public class M2Sdk
	{
		public static string M2ServiceQuery = "aid=AID&api_id=API_ID&signature=SIGNATURE&timestamp=TIMESTAMP&nonce=NONCE";

		public static string M2ServiceQueryAccessToken = "aid=AID&api_id=API_ID&access_token=ACCESS_TOKEN";

		public static string M2CheckTokenQuery = "aid=AID&api_id=API_ID&token=TOKEN ";

		public static string KEY_ERR_CODE = "err_code";

		private IRestResponse InvokeHttp(string url, string httpMethod, string jsonData)
		{
			IRestResponse result = null;
			if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(httpMethod))
			{
				IRestClient restClient = new RestClient();
				restClient.BaseUrl = new Uri(url);
				IRestRequest restRequest = new RestRequest();
				restRequest.RequestFormat = DataFormat.Json;
				if (!string.IsNullOrEmpty(jsonData))
				{
					restRequest.AddParameter("application/json", jsonData, ParameterType.RequestBody);
				}
				if (string.Compare(httpMethod, "post", ignoreCase: true) == 0)
				{
					result = restClient.Post(restRequest);
				}
				if (string.Compare(httpMethod, "get", ignoreCase: true) == 0)
				{
					result = restClient.Get(restRequest);
				}
			}
			return result;
		}

		public IDictionary<string, string> FromJavaMapJsonTo(string javaMapJsonString)
		{
			return new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(javaMapJsonString);
		}

		public string InvokeM2(string m2ServiceAddressWithPath, string applicationId, string apiId, string applicationKey, string requestBodyData, bool isDebug = false)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			if (m2ServiceAddressWithPath.EndsWith("mode=1"))
			{
				return "";
			}
			string arg = Guid.NewGuid().ToString();
			string result = string.Empty;
			string text = ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds).ToString();
			string text2 = new Random().Next().ToString();
			if (!string.IsNullOrEmpty(m2ServiceAddressWithPath) && !string.IsNullOrEmpty(applicationId) && !string.IsNullOrEmpty(apiId) && !string.IsNullOrEmpty(applicationKey))
			{
				string text3 = DigestM2(applicationId, apiId, applicationKey, text, text2);
				if (!string.IsNullOrEmpty(text3))
				{
					string str = M2ServiceQuery.Replace("AID", applicationId).Replace("API_ID", apiId).Replace("SIGNATURE", text3)
						.Replace("TIMESTAMP", text)
						.Replace("NONCE", text2);
					str = m2ServiceAddressWithPath + "?" + str;
					if (isDebug)
					{
						str += "&debug=true";
					}
					try
					{
						LogUtil.Info("美付宝代付接口Post text5:" + str + "  requestBodyData:" + requestBodyData);
						IRestResponse restResponse = InvokeHttp(str, "POST", requestBodyData);
						LogUtil.Info($"美付宝代付M2ClientTranCode:{arg}:请求M2耗时:{stopwatch.ElapsedMilliseconds}");
						if (restResponse != null)
						{
							string empty = string.Empty;
							empty = ((restResponse.StatusCode != HttpStatusCode.OK) ? JsonConvert.SerializeObject(new
							{
								error_msg = restResponse.Content
							}) : restResponse.Content);
							result = empty;
						}
					}
					catch (Exception ex)
					{
						result = string.Format("M2ClientTranCode:{0}:请求M2异常:{1};请求数据:{2}", arg, ex.Message + ";" + ex.StackTrace, requestBodyData);
					}
				}
			}
			return result;
		}

		public string DigestM2(string applicationId, string apiId, string applicationKey, string timeStamp, string nonce)
		{
			string result = null;
			if (!string.IsNullOrEmpty(applicationId) && !string.IsNullOrEmpty(apiId) && !string.IsNullOrEmpty(applicationKey) && !string.IsNullOrEmpty(timeStamp) && !string.IsNullOrEmpty(nonce))
			{
				IOrderedEnumerable<string> orderedEnumerable = from c in new List<string>
				{
					applicationId,
					apiId,
					applicationKey,
					timeStamp,
					nonce
				}
				orderby c
				select c;
				string text = string.Empty;
				foreach (string item in orderedEnumerable)
				{
					text += item;
				}
				result = BitConverter.ToString(new SHA1CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", string.Empty).ToUpper();
			}
			return result;
		}

		public string InvokeM2(string url, string requestBodyData)
		{
			string empty = string.Empty;
			try
			{
				IRestResponse restResponse = InvokeHttp(url, "POST", requestBodyData);
				if (restResponse == null)
				{
					return empty;
				}
				if (restResponse.StatusCode == HttpStatusCode.OK)
				{
					string content = restResponse.Content;
					if (!string.IsNullOrEmpty(content))
					{
						if (!content.Contains(KEY_ERR_CODE))
						{
							return content;
						}
						return content;
					}
					return content;
				}
				return empty;
			}
			catch (Exception ex)
			{
				return ex.Message + "--" + ex.StackTrace + "--M2SDK";
			}
		}

		public static string getNonce()
		{
			Random random = new Random();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(random.Next(10)).Append(random.Next(10)).Append(random.Next(10))
				.Append(random.Next(10))
				.Append(random.Next(10))
				.Append(random.Next(10));
			return stringBuilder.ToString();
		}

		public string getSignature(string[] args)
		{
			if (args == null || args.Length == 0)
			{
				return null;
			}
			Array.Sort(args);
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < args.Length; i++)
			{
				stringBuilder.Append(args[i]);
			}
			return BitConverter.ToString(new SHA1CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(stringBuilder.ToString()))).Replace("-", string.Empty).ToUpper();
		}

		public string getSignature(string aid, string tid, string api_id, string key, string timestamp, string nonce, string data_sign, string forward, string error_url)
		{
			if (aid == null)
			{
				aid = "";
			}
			if (tid == null)
			{
				tid = "";
			}
			if (api_id == null)
			{
				api_id = "";
			}
			if (key == null)
			{
				key = "";
			}
			if (timestamp == null)
			{
				timestamp = "";
			}
			if (nonce == null)
			{
				nonce = "";
			}
			if (data_sign == null)
			{
				data_sign = "";
			}
			if (forward == null)
			{
				forward = "";
			}
			if (error_url == null)
			{
				error_url = "";
			}
			string[] args = new string[9]
			{
				aid,
				tid,
				api_id,
				key,
				timestamp,
				nonce,
				data_sign,
				forward,
				error_url
			};
			return getSignature(args);
		}

		public string create_get_access_token_url(string url, string aid, string tid, string keys, bool is_debug)
		{
			try
			{
				string api_id = "";
				string text = ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds).ToString();
				string nonce = getNonce();
				string signature = getSignature(aid, "", api_id, keys, text, nonce, null, null, null);
				if (aid != null)
				{
					url = url.Replace("AID", aid);
				}
				if (tid != null && !tid.Equals(""))
				{
					url = ((!url.Contains("TID")) ? (url + "&tid=" + tid) : url.Replace("TID", tid));
				}
				url = url.Replace("SIGNATURE", signature).Replace("TIMESTAMP", text).Replace("NONCE", nonce);
				if (!is_debug)
				{
					return url;
				}
				if (!url.Contains("debug="))
				{
					url += "&debug=true";
					return url;
				}
				url = url.Replace("debug=", "debug=" + is_debug.ToString());
				return url;
			}
			catch (Exception)
			{
				return url;
			}
		}

		public string Register_Token(string m2ServiceAddressWithPath, string applicationId, string apiId, string applicationKey, string requestBodyData, bool isDebug = false)
		{
			string empty = string.Empty;
			((long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds).ToString();
			new Random().Next().ToString();
			if (!string.IsNullOrEmpty(m2ServiceAddressWithPath) && !string.IsNullOrEmpty(applicationId) && !string.IsNullOrEmpty(apiId) && !string.IsNullOrEmpty(applicationKey))
			{
				string url = m2ServiceAddressWithPath + "?" + M2ServiceQuery;
				string url2 = create_get_access_token_url(url, applicationId, "", applicationKey, isDebug);
				try
				{
					IRestResponse restResponse = InvokeHttp(url2, "POST", requestBodyData);
					if (restResponse == null)
					{
						return empty;
					}
					if (restResponse.StatusCode == HttpStatusCode.OK)
					{
						string content = restResponse.Content;
						if (!string.IsNullOrEmpty(content))
						{
							if (!content.Contains(KEY_ERR_CODE))
							{
								return content;
							}
							return content + "--M2SDK";
						}
						return content + "--M2SDK";
					}
					return empty;
				}
				catch (Exception ex)
				{
					return ex.Message + "--" + ex.StackTrace + "--M2SDK";
				}
			}
			return empty;
		}
	}
}
