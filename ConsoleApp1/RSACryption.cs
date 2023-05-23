using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HM.Framework.PayApi.DaiFu
{
    public class RSACryption
    {
        public static  String CHARSET = "UTF-8";


        public static string SignatureFormatter(string strPrivateKey, string strContent)
        {
            byte[] btContent = Encoding.UTF8.GetBytes(strContent);
            RSACryptoServiceProvider rsp = new RSACryptoServiceProvider();
            rsp.FromXmlString(strPrivateKey);
            return Convert.ToBase64String(rsp.SignData(btContent, "MD5"));
        }

        /// <summary>  
        /// RSA签名验证  
        /// </summary>  
        /// <param name="strKeyPublic">公钥</param>  
        /// <param name="strHashbyteDeformatter">Hash描述</param>  
        /// <param name="DeformatterData">签名后的结果</param>  
        /// <returns></returns>  
        public static bool SignatureDeformatter(string strKeyPublic, byte[] HashbyteDeformatter, byte[] DeformatterData)
        {
            try
            {
                System.Security.Cryptography.RSACryptoServiceProvider RSA = new System.Security.Cryptography.RSACryptoServiceProvider();
                RSA.FromXmlString(strKeyPublic);
                return RSA.VerifyData(HashbyteDeformatter, "MD5", DeformatterData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// RSA的加密函数  string
        /// </summary>
        /// <param name="publicKey">公钥</param>
        /// <param name="plaintext">明文</param>
        /// <returns></returns>
        public static string Encrypt(string publicKey, string plaintext)
        {
            return Encrypt(publicKey, Encoding.UTF8.GetBytes(plaintext));
        }

        /// <summary>
        /// RSA的加密函数  string
        /// </summary>
        /// <param name="publicKey">公钥</param>
        /// <param name="plainbytes">明文字节数组</param>
        /// <returns></returns>
        public static string Encrypt(string publicKey, byte[] plainbytes)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(publicKey);
                var bufferSize = (rsa.KeySize / 8 - 11);
                byte[] buffer = new byte[bufferSize];//待加密块

                using (MemoryStream msInput = new MemoryStream(plainbytes))
                {
                    using (MemoryStream msOutput = new MemoryStream())
                    {
                        int readLen;
                        while ((readLen = msInput.Read(buffer, 0, bufferSize)) > 0)
                        {
                            byte[] dataToEnc = new byte[readLen];
                            Array.Copy(buffer, 0, dataToEnc, 0, readLen);
                            byte[] encData = rsa.Encrypt(dataToEnc, false);
                            msOutput.Write(encData, 0, encData.Length);
                        }

                        byte[] result = msOutput.ToArray();
                        rsa.Clear();
                        return Convert.ToBase64String(result);
                    }
                }
            }
        }

        /// <summary>
        /// RSA的解密函数  stirng
        /// </summary>
        /// <param name="privateKey">私钥</param>
        /// <param name="ciphertext">密文字符串</param>
        /// <returns></returns>
        public static byte[] Decrypt(string privateKey, string ciphertext)
        {
            return Decrypt(privateKey, Convert.FromBase64String(ciphertext));
        }

        /// <summary>
        /// RSA的解密函数  byte
        /// </summary>
        /// <param name="privateKey">私钥</param>
        /// <param name="cipherbytes">密文字节数组</param>
        /// <returns></returns>
        public static byte[] Decrypt(string privateKey, byte[] cipherbytes)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);
                int keySize = rsa.KeySize / 8;
                byte[] buffer = new byte[keySize];
                using (MemoryStream msInput = new MemoryStream(cipherbytes))
                {
                    using (MemoryStream msOutput = new MemoryStream())
                    {
                        int readLen;

                        while ((readLen = msInput.Read(buffer, 0, keySize)) > 0)
                        {
                            byte[] dataToDec = new byte[readLen];
                            Array.Copy(buffer, 0, dataToDec, 0, readLen);
                            byte[] decData = rsa.Decrypt(dataToDec, false);
                            msOutput.Write(decData, 0, decData.Length);
                        }

                        byte[] result = msOutput.ToArray();
                        rsa.Clear();

                        return result;
                    }
                }
            }
        }
    }
}
