using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HM.Framework.PayApi._669U
{
    /// <summary>
    /// 加密类基类
    /// </summary>
    public class CryptoBase
    {
        /// <summary>
        /// 编码转换器，用于字节码和字符串之间的转换，默认为本机编码
        /// </summary>
        static public Encoding Encode = Encoding.Default;
        public enum EncoderMode { Base64Encoder, HexEncoder };

        /// <summary>
        /// 带编码模式的字符串加密
        /// </summary>
        /// <param name="data">要加密的数据</param>
        /// <param name="pass">密码</param>
        /// <param name="em">编码模式</param>
        /// <returns>加密后经过编码的字符串</returns>
        public String Encrypt(String data, String pass, CryptoBase.EncoderMode em)
        {
            if (data == null || pass == null) return null;
            if (em == EncoderMode.Base64Encoder)
                return Convert.ToBase64String(EncryptEx(Encode.GetBytes(data), pass));
            else
                return ByteToHex(EncryptEx(Encode.GetBytes(data), pass));
        }

        /// <summary>
        /// 带编码模式的字符串解密
        /// </summary>
        /// <param name="data">要解密的数据</param>
        /// <param name="pass">密码</param>
        /// <param name="em">编码模式</param>
        /// <returns>明文</returns>
        public String Decrypt(String data, String pass, CryptoBase.EncoderMode em)
        {
            if (data == null || pass == null) return null;
            if (em == EncoderMode.Base64Encoder)
                return Encode.GetString(DecryptEx(Convert.FromBase64String(data), pass));
            else
                return Encode.GetString(DecryptEx(HexToByte(data), pass));
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="data">要加密的数据</param>
        /// <param name="pass">密码</param>
        /// <returns>加密后经过默认编码的字符串</returns>
        public String Encrypt(String data, String pass)
        {
            return Encrypt(data, pass, EncoderMode.Base64Encoder);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="data">要解密的经过编码的数据</param>
        /// <param name="pass">密码</param>
        /// <returns>明文</returns>
        public String Decrypt(String data, String pass)
        {
            return Decrypt(data, pass, EncoderMode.Base64Encoder);
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="data">要加密的数据</param>
        /// <param name="pass">密钥</param>
        /// <returns>密文</returns>
        virtual public Byte[] EncryptEx(Byte[] data, String pass) { return null; }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="data">要解密的数据</param>
        /// <param name="pass">密码</param>
        /// <returns>明文</returns>
        virtual public Byte[] DecryptEx(Byte[] data, String pass) { return null; }

        static public Byte[] HexToByte(String szHex)
        {
            // 两个十六进制代表一个字节
            Int32 iLen = szHex.Length;
            if (iLen <= 0 || 0 != iLen % 2)
            {
                return null;
            }
            Int32 dwCount = iLen / 2;
            UInt32 tmp1, tmp2;
            Byte[] pbBuffer = new Byte[dwCount];
            for (Int32 i = 0; i < dwCount; i++)
            {
                tmp1 = (UInt32)szHex[i * 2] - (((UInt32)szHex[i * 2] >= (UInt32)'A') ? (UInt32)'A' - 10 : (UInt32)'0');
                if (tmp1 >= 16) return null;
                tmp2 = (UInt32)szHex[i * 2 + 1] - (((UInt32)szHex[i * 2 + 1] >= (UInt32)'A') ? (UInt32)'A' - 10 : (UInt32)'0');
                if (tmp2 >= 16) return null;
                pbBuffer[i] = (Byte)(tmp1 * 16 + tmp2);
            }
            return pbBuffer;
        }

        static public String ByteToHex(Byte[] vByte)
        {
            if (vByte == null || vByte.Length < 1) return null;
            StringBuilder sb = new StringBuilder(vByte.Length * 2);
            for (int i = 0; i < vByte.Length; i++)
            {
                if ((UInt32)vByte[i] < 0) return null;
                UInt32 k = (UInt32)vByte[i] / 16;
                sb.Append((Char)(k + ((k > 9) ? 'A' - 10 : '0')));
                k = (UInt32)vByte[i] % 16;
                sb.Append((Char)(k + ((k > 9) ? 'A' - 10 : '0')));
            }
            return sb.ToString();
        }
    }

    interface ICrypto
    {
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="data">要加密的数据</param>
        /// <param name="pass">密钥</param>
        /// <returns>密文</returns>
        Byte[] EncryptEx(Byte[] data, String pass);

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="data">要解密的数据</param>
        /// <param name="pass">密码</param>
        /// <returns>明文</returns>
        Byte[] DecryptEx(Byte[] data, String pass);
    }
}
