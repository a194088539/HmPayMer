using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace PayApi.AoYou
{
    public class HttpPost
    {
        public static string SendPost(string url, string postData, int timeout = 5000,
            string contentType = "application/x-www-form-urlencoded")
        {
            string reqInfo = "HttpPost1 POST：" + url + "?" + postData ?? string.Empty;
            Trace.WriteLine(reqInfo);

            HttpWebRequest request;
            if (url.Contains("https"))
            {
                ServicePointManager.ServerCertificateValidationCallback =
                    new RemoteCertificateValidationCallback(CheckValidationResult);
                request = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
            }
            else
            {
                request = (HttpWebRequest)WebRequest.Create(url);
            }

            //HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            //request.ContentType = "application/x-www-form-urlencoded";
            request.ContentType = contentType;
            //request.ContentLength = postData.Length;
            request.Timeout = timeout;

            HttpWebResponse response = null;

            try
            {
                StreamWriter swRequestWriter = new StreamWriter(request.GetRequestStream());
                swRequestWriter.Write(postData);

                if (swRequestWriter != null)
                    swRequestWriter.Close();

                response = (HttpWebResponse)request.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                //NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                return ex.Message;
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors errors)
        {
            //直接确认，否则打不开
            return true;
        }

        internal static object SendPost(string postUri, string parameters, Encoding encoding)
        {
            throw new NotImplementedException();
        }
    }
}