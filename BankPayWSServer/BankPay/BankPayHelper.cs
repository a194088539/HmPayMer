using AooFu.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using BankPayWSServer.BankPay.Models;

namespace BankPayWSServer.BankPay
{
    public class BankPayHelper
    {
        private static readonly string DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";


        public static string Login(string deviceId, string password)
        {
            string sign = getSign("",new object[]
            {
                "username",
                deviceId,
                "userpassword",
                password,
                "client",
                3
            });
            string apiAddress = "https://bkapi.payweipan.com//api/User/Login";
            string configurationValue = AooFu.Tools.Config.GetConfigurationValue("hostIPMiddleware");
            if (!string.IsNullOrEmpty(configurationValue))
            {
                apiAddress = configurationValue + "/api/User/Login";
            }
            string json = CallAPIByPost<CasherInfo>(new CasherInfo
            {
                sign = sign,
                username = deviceId,
                userpassword = password,
                deviceid = "",
                client = 3,
                version = 0f
            }, apiAddress);
            return json;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payType">1微信，2支付宝</param>
        /// <param name="num">金额，元</param>
        /// <param name="orderId"></param>
        /// <param name="cashId"></param>
        /// <param name="localIp"></param>
        /// <param name="cashierName"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string QrcodePay(int payType, double num, ref string orderId,
            int cashId, string localIp, string cashierName, string key)
        {
            if (num <= 0.0)
            {
                throw new Exception("请输入正确的收款金额");
            }
            string apiAddress = string.Empty;
            string sign = string.Empty;
            string json = string.Empty;
            string configurationValue = AooFu.Tools.Config.GetConfigurationValue("hostIPMiddleware");
            if (string.IsNullOrEmpty(configurationValue))
            {
                apiAddress = "https://bkapi.payweipan.com/" + ((payType == 1) ? "/api/Bank/wxpay_precreate" : "/api/Bank/alipay_precreate");
            }
            else
            {
                apiAddress = configurationValue + ((payType == 1) ? "/api/Bank/wxpay_precreate" : "/api/Bank/alipay_precreate");
            }
            PABPay pabpay = new PABPay();
            SortedDictionary<string, object> sortedDictionary;
            if (payType == 1)
            {
                pabpay = new PABPay
                {
                    cashid = cashId,
                    spbill_create_ip = localIp,
                    sign = sign,
                    notify_url = WebConfigHelper.GetConfig("PAB_WXNotifyUrl"),
                    total_fee = Convert.ToDecimal(num),
                    body = cashierName,
                    sub_appid = "",
                    subject = cashierName
                };
                sortedDictionary = new SortedDictionary<string, object>();
                if (pabpay.cashid > 0)
                {
                    sortedDictionary.Add("cashid", pabpay.cashid);
                }
                if (pabpay.version > 0f)
                {
                    sortedDictionary.Add("version", pabpay.version);
                }
                if (pabpay.client > 0)
                {
                    sortedDictionary.Add("client", pabpay.client);
                }
                if (!string.IsNullOrEmpty(pabpay.body))
                {
                    sortedDictionary.Add("body", pabpay.body);
                }
                if (pabpay.total_fee > 0m)
                {
                    sortedDictionary.Add("total_fee", pabpay.total_fee.ConvertDecimal());
                }
                if (!string.IsNullOrEmpty(pabpay.spbill_create_ip))
                {
                    sortedDictionary.Add("spbill_create_ip", pabpay.spbill_create_ip);
                }
                if (!string.IsNullOrEmpty(pabpay.notify_url))
                {
                    sortedDictionary.Add("notify_url", pabpay.notify_url);
                }
                if (!string.IsNullOrEmpty(pabpay.sub_appid))
                {
                    sortedDictionary.Add("sub_appid", pabpay.sub_appid);
                }
            }
            else
            {
                pabpay = new PABPay
                {
                    cashid = cashId,
                    spbill_create_ip = localIp,
                    sign = sign,
                    total_amount = Convert.ToDecimal(num),
                    subject = cashierName,
                    notify_url = WebConfigHelper.GetConfig("PAB_AliNotifyUrl"),
                    undiscountable_amount = 0m,
                    body = cashierName
                };
                sortedDictionary = new SortedDictionary<string, object>();
                if (pabpay.cashid > 0)
                {
                    sortedDictionary.Add("cashid", pabpay.cashid);
                }
                if (pabpay.version > 0f)
                {
                    sortedDictionary.Add("version", pabpay.version);
                }
                if (pabpay.client > 0)
                {
                    sortedDictionary.Add("client", pabpay.client);
                }
                if (!string.IsNullOrEmpty(pabpay.notify_url))
                {
                    sortedDictionary.Add("notify_url", pabpay.notify_url);
                }
                if (pabpay.total_amount > 0m)
                {
                    sortedDictionary.Add("total_amount", pabpay.total_amount.ConvertDecimal());
                }
                if (pabpay.undiscountable_amount > 0m)
                {
                    sortedDictionary.Add("undiscountable_amount", pabpay.undiscountable_amount.ConvertDecimal());
                }
                if (!string.IsNullOrEmpty(pabpay.subject))
                {
                    sortedDictionary.Add("subject", pabpay.subject);
                }
                if (!string.IsNullOrEmpty(pabpay.body))
                {
                    sortedDictionary.Add("body", pabpay.body);
                }
            }
            sign = getSign(sortedDictionary, key);
            pabpay.sign = sign;
            json = CallAPIByPost<PABPay>(pabpay, apiAddress);
            return json;
        }

        public static PABPay QrcodePayData(int payType, double num, ref string orderId,
            int cashId, string localIp, string cashierName, string key)
        {
            if (num <= 0.0)
            {
                throw new Exception("请输入正确的收款金额");
            }
            
            string sign = string.Empty;
            string json = string.Empty;
            
            PABPay pabpay = new PABPay();
            SortedDictionary<string, object> sortedDictionary;
            if (payType == 1)
            {
                pabpay = new PABPay
                {
                    cashid = cashId,
                    spbill_create_ip = localIp,
                    sign = sign,
                    notify_url = WebConfigHelper.GetConfig("PAB_WXNotifyUrl"),
                    total_fee = Convert.ToDecimal(num),
                    body = cashierName,
                    sub_appid = "",
                    subject = cashierName
                };
                sortedDictionary = new SortedDictionary<string, object>();
                if (pabpay.cashid > 0)
                {
                    sortedDictionary.Add("cashid", pabpay.cashid);
                }
                if (pabpay.version > 0f)
                {
                    sortedDictionary.Add("version", pabpay.version);
                }
                if (pabpay.client > 0)
                {
                    sortedDictionary.Add("client", pabpay.client);
                }
                if (!string.IsNullOrEmpty(pabpay.body))
                {
                    sortedDictionary.Add("body", pabpay.body);
                }
                if (pabpay.total_fee > 0m)
                {
                    sortedDictionary.Add("total_fee", pabpay.total_fee.ConvertDecimal());
                }
                if (!string.IsNullOrEmpty(pabpay.spbill_create_ip))
                {
                    sortedDictionary.Add("spbill_create_ip", pabpay.spbill_create_ip);
                }
                if (!string.IsNullOrEmpty(pabpay.notify_url))
                {
                    sortedDictionary.Add("notify_url", pabpay.notify_url);
                }
                if (!string.IsNullOrEmpty(pabpay.sub_appid))
                {
                    sortedDictionary.Add("sub_appid", pabpay.sub_appid);
                }
            }
            else
            {
                pabpay = new PABPay
                {
                    cashid = cashId,
                    spbill_create_ip = localIp,
                    sign = sign,
                    total_amount = Convert.ToDecimal(num),
                    subject = cashierName,
                    notify_url = WebConfigHelper.GetConfig("PAB_AliNotifyUrl"),
                    undiscountable_amount = 0m,
                    body = cashierName
                };
                sortedDictionary = new SortedDictionary<string, object>();
                if (pabpay.cashid > 0)
                {
                    sortedDictionary.Add("cashid", pabpay.cashid);
                }
                if (pabpay.version > 0f)
                {
                    sortedDictionary.Add("version", pabpay.version);
                }
                if (pabpay.client > 0)
                {
                    sortedDictionary.Add("client", pabpay.client);
                }
                if (!string.IsNullOrEmpty(pabpay.notify_url))
                {
                    sortedDictionary.Add("notify_url", pabpay.notify_url);
                }
                if (pabpay.total_amount > 0m)
                {
                    sortedDictionary.Add("total_amount", pabpay.total_amount.ConvertDecimal());
                }
                if (pabpay.undiscountable_amount > 0m)
                {
                    sortedDictionary.Add("undiscountable_amount", pabpay.undiscountable_amount.ConvertDecimal());
                }
                if (!string.IsNullOrEmpty(pabpay.subject))
                {
                    sortedDictionary.Add("subject", pabpay.subject);
                }
                if (!string.IsNullOrEmpty(pabpay.body))
                {
                    sortedDictionary.Add("body", pabpay.body);
                }
            }
            sign = getSign(sortedDictionary, key);
            pabpay.sign = sign;
            return pabpay;
        }

        public static string OrderStateData(int payType, string orderId, int cashId, string key)
        {
            string text = "https://bkapi.payweipan.com/";
            string configurationValue = AooFu.Tools.Config.GetConfigurationValue("hostIPMiddleware");
            if (!string.IsNullOrEmpty(configurationValue))
            {
                text = configurationValue;
            }
            string url = string.Empty;
            string text2 = string.Empty;
            string json = string.Empty;
            text2 = getSign(key, new object[]
            {
                "cashid",
                cashId,
                "paytype",
                payType,
                "out_trade_no",
                orderId
            });
            url = string.Concat(new object[]
            {
                text,
                "/api/Bank/pay_query?client=0&version=0&cashid=",
                cashId,
                "&sign=",
                text2,
                "&paytype=",
                payType,
                "&out_trade_no=",
                orderId,
                "&trade_no="
            });
            
            return url;
        }

        public string OrderState(int payType, string orderId,int cashId, string key)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                throw new Exception("订单ID缺失");
            }
            string text = "https://bkapi.payweipan.com/";
            string configurationValue = AooFu.Tools.Config.GetConfigurationValue("hostIPMiddleware");
            if (!string.IsNullOrEmpty(configurationValue))
            {
                text = configurationValue;
            }
            string url = string.Empty;
            string text2 = string.Empty;
            string json = string.Empty;
            text2 = getSign(key,new object[]
            {
                "cashid",
                cashId,
                "paytype",
                payType,
                "out_trade_no",
                orderId
            });
            url = string.Concat(new object[]
            {
                text,
                "/api/Bank/pay_query?client=0&version=0&cashid=",
                cashId,
                "&sign=",
                text2,
                "&paytype=",
                payType,
                "&out_trade_no=",
                orderId,
                "&trade_no="
            });
            json = this.getGetData(url);
            return json;
        }

        public static string getSign(string key, params object[] datas)
		{
			SortedDictionary<string, object> sortedDictionary = new SortedDictionary<string, object>();
			string result = string.Empty;
			for (int i = 0; i < datas.Length; i++)
			{
				sortedDictionary.Add(datas[i].ToString(), datas[i + 1]);
				i++;
			}
			if (key == "")
			{
				result = WXMD5.MakeSign(sortedDictionary);
			}
			else
			{
				result = WXMD5.MakeSign(sortedDictionary, key);
			}
			return result;
		}

        public static string getSign(SortedDictionary<string, object> m_values, string key = "")
        {
            string result = string.Empty;
            bool flag = key == "";
            if (flag)
            {
                result = WXMD5.MakeSign(m_values);
            }
            else
            {
                result = WXMD5.MakeSign(m_values, key);
            }
            return result;
        }
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        public static Task<HttpResponseMessage> CreatePost(string url, IDictionary<string, object> parameters, string userAgent = "")
        {
            StringBuilder stringBuilder = new StringBuilder();
            int num = 0;
            foreach (string arg in parameters.Keys)
            {
                if (num > 0)
                {
                    stringBuilder.AppendFormat("&{0}={1}", arg, parameters[arg]);
                }
                else
                {
                    stringBuilder.AppendFormat("{0}={1}", arg, parameters[arg]);
                }
                num++;
            }
            return CreatePost(url, stringBuilder.ToString(), "application/x-www-form-urlencoded", userAgent);
        }
        public static Task<HttpResponseMessage> CreatePost(string url, string inputDto, string contentType, string userAgent = "")
        {
            RemoteCertificateValidationCallback remoteCertificateValidationCallback = null;
            if (remoteCertificateValidationCallback == null)
            {
                remoteCertificateValidationCallback = ((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true);
            }
            ServicePointManager.ServerCertificateValidationCallback = remoteCertificateValidationCallback;
            HttpContent httpContent = new StringContent(inputDto);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            HttpClient httpClient = new HttpClient();
            if (!string.IsNullOrWhiteSpace(userAgent))
            {
                httpClient.DefaultRequestHeaders.Add("user-agent", "User-Agent    " + userAgent);
            }
            Task<HttpResponseMessage> task = httpClient.PostAsync(url, httpContent);
            return task;
        }

        public static HttpWebResponse CreatePostHttpResponse(string url, IDictionary<string, object> parameters)
        {
            HttpWebRequest httpWebRequest = null;
            Encoding utf = Encoding.UTF8;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            httpWebRequest = (System.Net.WebRequest.Create(url) as HttpWebRequest);
            httpWebRequest.ProtocolVersion = HttpVersion.Version10;
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.UserAgent = DefaultUserAgent;
            if (parameters != null && parameters.Count != 0)
            {
                StringBuilder stringBuilder = new StringBuilder();
                int num = 0;
                foreach (string arg in parameters.Keys)
                {
                    if (num > 0)
                    {
                        stringBuilder.AppendFormat("&{0}={1}", arg, parameters[arg]);
                    }
                    else
                    {
                        stringBuilder.AppendFormat("{0}={1}", arg, parameters[arg]);
                    }
                    num++;
                }
                byte[] bytes = utf.GetBytes(stringBuilder.ToString());
                using (Stream requestStream = httpWebRequest.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                }
            }
            return httpWebRequest.GetResponse() as HttpWebResponse;
        }

        // Token: 0x06000137 RID: 311 RVA: 0x00012218 File Offset: 0x00010418
        public static HttpWebResponse CreateGetHttpResponse(string url)
        {
            HttpWebRequest httpWebRequest;
            if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                httpWebRequest = (System.Net.WebRequest.Create(url) as HttpWebRequest);
                httpWebRequest.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                httpWebRequest = (System.Net.WebRequest.Create(url) as HttpWebRequest);
            }
            httpWebRequest.Method = "GET";
            return httpWebRequest.GetResponse() as HttpWebResponse;
        }

        // Token: 0x06000138 RID: 312 RVA: 0x00012284 File Offset: 0x00010484
        public string getGetData(string url)
        {
            HttpWebResponse httpWebResponse = CreateGetHttpResponse(url);
            Stream responseStream = httpWebResponse.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream);
            string text = streamReader.ReadToEnd();
            return text.Replace("\\", string.Empty).Replace("\"[", "[").Replace("]\"", "]").Replace("\"{", "{").Replace("}\"", "}").TrimStart(new char[]
            {
                '"'
            }).TrimEnd(new char[]
            {
                '"'
            });
        }

        // Token: 0x06000139 RID: 313 RVA: 0x00012324 File Offset: 0x00010524
        public static string getPostData(string url, params object[] datas)
        {
            IDictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("client", 3);
            dictionary.Add("version", 0);
            for (int i = 0; i < datas.Length; i++)
            {
                dictionary.Add(datas[i].ToString(), datas[i + 1]);
                i++;
            }
            HttpWebResponse httpWebResponse = CreatePostHttpResponse(url, dictionary);
            Stream responseStream = httpWebResponse.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream);
            return streamReader.ReadToEnd();
        }

        // Token: 0x0600013A RID: 314 RVA: 0x000123A4 File Offset: 0x000105A4
        public static string CallAPIByPost<T>(T inputDto, string apiAddress)
        {
            RemoteCertificateValidationCallback remoteCertificateValidationCallback = null;
            string text = string.Empty;
            try
            {
                if (remoteCertificateValidationCallback == null)
                {
                    remoteCertificateValidationCallback = ((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true);
                }
                ServicePointManager.ServerCertificateValidationCallback = remoteCertificateValidationCallback;
                HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(inputDto));
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpClient httpClient = new HttpClient();
                text = httpClient.PostAsync(apiAddress, httpContent).Result.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            bool flag = text != null;
            if (flag)
            {
                text = text.Replace("\\", string.Empty).Replace(":\"[", ":[").Replace("]\",", "],").Replace("\"{", "{").Replace("}\"", "}").TrimStart(new char[]
                {
                    '"'
                }).TrimEnd(new char[]
                {
                    '"'
                });
            }
            return text;
        }


    }

}
