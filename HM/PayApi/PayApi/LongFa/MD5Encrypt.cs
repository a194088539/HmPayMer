using System;
using System.Security.Cryptography;
using System.Text;

namespace PayApi.LongFa
{
    /// <summary>
    /// MD5加密
    /// </summary>
    public static class MD5Encrypt
    {
        /// <summary>
        /// MD5函数
        /// </summary>
        /// <param name="str">原始字符串</param>
        /// <param name="half">为真则为16位,否则32位</param>
        /// <returns>MD5结果</returns>
        public static string MD5(string str, bool half)
        {
            byte[] b = Encoding.UTF8.GetBytes(str);
            b = new MD5CryptoServiceProvider().ComputeHash(b);
            string ret = "";
            for (int i = 0; i < b.Length; i++)
            {
                ret += b[i].ToString("x").PadLeft(2, '0');
            }

            if (half)
            {
                ret = ret.Substring(8, 16);
            }

            return ret;
        }

        /// <summary>
        /// 华势MD5
        /// </summary>
        /// <param name="MD5String"></param>
        /// <returns></returns>
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

    }
}