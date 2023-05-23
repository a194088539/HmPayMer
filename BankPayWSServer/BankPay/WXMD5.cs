using AooFu.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BankPayWSServer.BankPay
{
    public class WXMD5
    {
        // Token: 0x060003DC RID: 988 RVA: 0x000498EC File Offset: 0x00047AEC
        public static string MakeSign(SortedDictionary<string, object> m_values)
        {
            string text = WXMD5.ToUrl(m_values);
            text = text + "&key=" + WebConfigHelper.GetConfig("Bank_Appselect");
            MD5 md = MD5.Create();
            byte[] array = md.ComputeHash(Encoding.UTF8.GetBytes(text));
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in array)
            {
                stringBuilder.Append(b.ToString("x2"));
            }
            return stringBuilder.ToString().ToUpper();
        }

        // Token: 0x060003DD RID: 989 RVA: 0x00049970 File Offset: 0x00047B70
        public static string MakeSign(SortedDictionary<string, object> m_values, string workkey)
        {
            string text = WXMD5.ToUrl(m_values);
            text = text + "&key=" + workkey;
            MD5 md = MD5.Create();
            byte[] array = md.ComputeHash(Encoding.UTF8.GetBytes(text));
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in array)
            {
                stringBuilder.Append(b.ToString("x2"));
            }
            return stringBuilder.ToString().ToUpper();
        }

        // Token: 0x060003DE RID: 990 RVA: 0x000499EC File Offset: 0x00047BEC
        public static string ToUrl(SortedDictionary<string, object> m_values)
        {
            string text = "";
            foreach (KeyValuePair<string, object> keyValuePair in m_values)
            {
                if (keyValuePair.Value == null)
                {
                    Log.Error("QQPayData内部含有值为null的字段!");
                    throw new Exception("QQPayData内部含有值为null的字段!");
                }
                if (keyValuePair.Key != "sign" && keyValuePair.Value.ToString() != "")
                {
                    object obj = text;
                    text = string.Concat(new object[]
                    {
                        obj,
                        keyValuePair.Key,
                        "=",
                        keyValuePair.Value,
                        "&"
                    });
                }
            }
            text = text.Trim(new char[]
            {
                '&'
            });
            return text;
        }

        // Token: 0x04000509 RID: 1289
        private SortedDictionary<string, object> m_values = new SortedDictionary<string, object>();
    }
}
