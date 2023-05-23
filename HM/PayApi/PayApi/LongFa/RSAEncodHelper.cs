using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace PayApi.LongFa
{
    public class RSAEncodHelper
    {
        //public static string RSAPublicKeyJava2DotNet(string publicKey)
        //{
        //    RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
        //    return string.Format(
        //        "<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
        //        Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
        //        Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned())
        //    );
        //}
        /// <summary>
        /// 公钥格式转换
        /// </summary>
        /// <param name="publicKey">公钥</param>
        /// <returns></returns>
        public static string RSAPublicKeyJava2DotNet(string publicKey)
        {
            RsaKeyParameters publicKeyParam =
                (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
            return string.Format(
                "<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned())
            );
        }

        public static byte[] RSAPublicKeySignByte(string signStr, string publicKey)
        {
            try
            {
                using (var rsaProvider = new RSACryptoServiceProvider())
                {
                    var inputBytes = Encoding.UTF8.GetBytes(signStr); //有含义的字符串转化为字节流
                    rsaProvider.FromXmlString(publicKey); //载入公钥
                    int bufferSize = (rsaProvider.KeySize / 8) - 11; //单块最大长度
                    var buffer = new byte[bufferSize];
                    using (MemoryStream inputStream = new MemoryStream(inputBytes),
                        outputStream = new MemoryStream())
                    {
                        while (true)
                        {
                            //分段加密
                            int readSize = inputStream.Read(buffer, 0, bufferSize);
                            if (readSize <= 0)
                            {
                                break;
                            }

                            var temp = new byte[readSize];
                            Array.Copy(buffer, 0, temp, 0, readSize);
                            var encryptedBytes = rsaProvider.Encrypt(temp, false);
                            outputStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                        }

                        return outputStream.ToArray(); //转化为字节流方便传输
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static string SortDictionaryCharCaseToString(Dictionary<string, string> dic)
        {
            string[] sary = dic.Keys.ToArray();
            Array.Sort(sary, (a, b) => string.CompareOrdinal(a, b));
            StringBuilder sb = new StringBuilder();
            foreach (string pair in sary)
            {
                if (!string.IsNullOrEmpty(pair) && dic.ContainsKey(pair) && !string.IsNullOrEmpty(dic[pair]))
                {
                    sb.AppendFormat("{0}={1}&", pair, dic[pair]);
                }
            }

            return sb.ToString();
        }
    }
}