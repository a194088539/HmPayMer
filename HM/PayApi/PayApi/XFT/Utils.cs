using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Security;

namespace PayApi.XFT
{
    public class Utils
    {
        /**
         * 商户订单号
         */
        public String orderCreate()
        {
            Random ran = new Random();
            return "PX" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ran.Next(1000, 9999).ToString();
        }

        /**
         * 代付订单号
         */
        public String agentCreate()
        {
            Random ran = new Random();
            return "TX" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ran.Next(1000, 9999).ToString();
        }

        public static String GetMD5Hash(String MD5String)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider x =
                new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] MD5Temp = System.Text.Encoding.UTF8.GetBytes(MD5String);
            MD5Temp = x.ComputeHash(MD5Temp);
            System.Text.StringBuilder StrTemp = new System.Text.StringBuilder();
            foreach (byte Res in MD5Temp)
            {
                StrTemp.Append(Res.ToString("x2").ToLower());
            }

            String password = StrTemp.ToString();
            return password;
        }


        public static String GetMd5str(Dictionary<String, String> Parms, String KeyStr)
        {
            List<String> SortStr = new List<String>(Parms.Keys);
            SortStr.Sort();
            String Md5Str = CreateLinkstring(Parms, SortStr);
            //报验签错误，此处需打印MD5str,进行校核,KEY会包含在最后面，注意KEY安全！
            //Console.WriteLine(Md5Str); 
            return GetMD5Hash(Md5Str + KeyStr);
        }

        /**
         * 判断值是否为空 FALSE 为不空  TRUE 为空
         * @param Temp
         * @return
         */
        public static bool StrEmpty(String Temp)
        {
            if (null == Temp || String.IsNullOrEmpty(Temp))
            {
                return true;
            }

            return false;
        }

        /**
         * 拼接报文
         * @param Parm
         * @param SortStr
         * @return
         */
        public static String CreateLinkstring(Dictionary<String, String> Parms, List<String> SortStr)
        {
            String LinkStr = "";
            for (int i = 0; i < SortStr.Count; i++)
            {
                if (!StrEmpty(Parms[SortStr[i].ToString()]))
                {
                    LinkStr += SortStr[i] + "=" + Parms[SortStr[i].ToString()];
                    if ((i + 1) < SortStr.Count)
                    {
                        LinkStr += "&";
                    }
                }
            }

            return LinkStr;
        }

        /// SHA加密
        public static String GetSHAstr(Dictionary<String, String> Parms, String KeyStr)
        {
            List<String> SortStr = new List<String>(Parms.Keys);
            SortStr.Sort();
            String SHAStr = CreateLinkstring(Parms, SortStr);
            //报验签错误，此处需打印SHAstr,进行校核,KEY会包含在最后面，注意KEY安全！
            //Console.WriteLine(SHAStr); 
            return FormsAuthentication.HashPasswordForStoringInConfigFile(SHAStr + KeyStr, "SHA1").ToUpper();
        }

        /**
         *  post模拟form方法
         */
        public static String HtmlPost(String Url, Dictionary<String, String> Params)
        {
            String FormString =
                "<body onLoad=\"document.actform.submit()\"><form  id=\"actform\" name=\"actform\" method=\"post\" action=\"" +
                Url + "\">";

            foreach (string key in Params.Keys)
            {
                FormString += "<input name=\"" + key + "\" type=\"hidden\" value='" + Params[key] + "'>";

            }

            FormString += "</form></body>";
            return FormString;
        }

        /**
         *  get模拟form方法
         */
        public static String HtmlGet(String Url, Dictionary<String, String> Params)
        {
            String FormString =
                "<body onLoad=\"document.actform.submit()\"><form  id=\"actform\" name=\"actform\" method=\"get\" action=\"" +
                Url + "\">";

            foreach (string key in Params.Keys)
            {
                FormString += "<input name=\"" + key + "\" type=\"hidden\" value='" + Params[key] + "'>";

            }

            FormString += "</form></body>";
            return FormString;
        }

        /// <summary>  
        /// 发送Get请求  
        /// </summary>  
        /// <param name="url">地址</param>  
        /// <param name="dic">请求参数定义</param>  
        public static string Get(string url, Dictionary<string, string> dic)
        {
            string result = "";
            StringBuilder builder = new StringBuilder();
            builder.Append(url);
            if (dic.Count > 0)
            {
                builder.Append("?");
                int i = 0;
                foreach (var item in dic)
                {
                    if (i > 0)
                        builder.Append("&");
                    builder.AppendFormat("{0}={1}", item.Key, item.Value);
                    i++;
                }
            }

            HttpWebRequest req = (HttpWebRequest) WebRequest.Create(builder.ToString());
            //添加参数  
            HttpWebResponse resp = (HttpWebResponse) req.GetResponse();
            Stream stream = resp.GetResponseStream();
            try
            {
                //获取内容  
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }
            finally
            {
                stream.Close();
            }

            return result;
        }

        /// <summary>  
        /// 指定Post地址使用Get 方式获取全部字符串  
        /// </summary>  
        /// <param name="url">请求后台地址</param>  
        /// <returns></returns>  
        public static string Post(string url, Dictionary<string, string> dic)
        {
            string result = "";
            HttpWebRequest req = (HttpWebRequest) WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            #region 添加Post 参数  

            StringBuilder builder = new StringBuilder();
            int i = 0;
            foreach (var item in dic)
            {
                if (i > 0)
                    builder.Append("&");
                builder.AppendFormat("{0}={1}", item.Key, item.Value);
                i++;
            }

            byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
            req.ContentLength = data.Length;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }

            #endregion

            HttpWebResponse resp = (HttpWebResponse) req.GetResponse();
            Stream stream = resp.GetResponseStream();
            //获取响应内容  
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }
    }
}