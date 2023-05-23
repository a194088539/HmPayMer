using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HM.Framework
{
	public static class EncryUtils
	{
		private static byte[] RijndaelIV => MD5StrToByte("郑州科技有限公司 | mooqqqppp");

		private static byte[] RijndaelKey
		{
			get
			{
				byte[] array = new byte[32];
				Array.Copy(MD5StrToByte("郑州科技有限公司"), 0, array, 0, 16);
				Array.Copy(MD5ByteToByte(MD5StrToByte("mooqqqppp")), 0, array, 16, 16);
				return array;
			}
		}

		public static byte[] MD5StrToByte(string strToEncrypt)
		{
			return MD5ByteToByte(Encoding.UTF8.GetBytes(strToEncrypt));
		}

		private static byte[] MD5ByteToByte(byte[] bytesToEncrypt)
		{
			return ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(bytesToEncrypt);
		}

		public static string RijndaelEncryptor(string strToEncryptor)
		{
			byte[] rijndaelKey = RijndaelKey;
			byte[] rijndaelIV = RijndaelIV;
			byte[] bytes = Encoding.UTF8.GetBytes(strToEncryptor);
			byte[] array = new byte[bytes.Length];
			MemoryStream memoryStream = new MemoryStream(bytes);
			RijndaelManaged rijndaelManaged = new RijndaelManaged();
			CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndaelManaged.CreateEncryptor(rijndaelKey, rijndaelIV), CryptoStreamMode.Read);
			try
			{
				cryptoStream.Read(array, 0, array.Length);
			}
			catch (Exception ex)
			{
				memoryStream.Close();
				cryptoStream.Close();
				throw ex;
			}
			return Convert.ToBase64String(array);
		}

		public static string RijndaelDecrypt(string strToDecrypt)
		{
			byte[] rijndaelKey = RijndaelKey;
			byte[] rijndaelIV = RijndaelIV;
			byte[] array = Convert.FromBase64String(strToDecrypt);
			byte[] array2 = new byte[array.Length];
			MemoryStream memoryStream = new MemoryStream(array);
			RijndaelManaged rijndaelManaged = new RijndaelManaged();
			CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndaelManaged.CreateDecryptor(rijndaelKey, rijndaelIV), CryptoStreamMode.Read);
			try
			{
				cryptoStream.Read(array2, 0, array2.Length);
			}
			catch (Exception)
			{
				memoryStream.Close();
				cryptoStream.Close();
				return "";
			}
			return Encoding.UTF8.GetString(array2);
		}

		public static string Encrypt(string pToEncrypt, string sKey)
		{
			using (DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider())
			{
				byte[] bytes = Encoding.UTF8.GetBytes(pToEncrypt);
				dESCryptoServiceProvider.Key = Encoding.ASCII.GetBytes(sKey);
				dESCryptoServiceProvider.IV = Encoding.ASCII.GetBytes(sKey);
				MemoryStream memoryStream = new MemoryStream();
				using (CryptoStream cryptoStream = new CryptoStream(memoryStream, dESCryptoServiceProvider.CreateEncryptor(), CryptoStreamMode.Write))
				{
					cryptoStream.Write(bytes, 0, bytes.Length);
					cryptoStream.FlushFinalBlock();
					cryptoStream.Close();
				}
				string result = Convert.ToBase64String(memoryStream.ToArray());
				memoryStream.Close();
				return result;
			}
		}

		public static string Decrypt(string pToDecrypt, string sKey)
		{
			byte[] array = Convert.FromBase64String(pToDecrypt);
			using (DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider())
			{
				dESCryptoServiceProvider.Key = Encoding.ASCII.GetBytes(sKey);
				dESCryptoServiceProvider.IV = Encoding.ASCII.GetBytes(sKey);
				MemoryStream memoryStream = new MemoryStream();
				using (CryptoStream cryptoStream = new CryptoStream(memoryStream, dESCryptoServiceProvider.CreateDecryptor(), CryptoStreamMode.Write))
				{
					cryptoStream.Write(array, 0, array.Length);
					cryptoStream.FlushFinalBlock();
					cryptoStream.Close();
				}
				string @string = Encoding.UTF8.GetString(memoryStream.ToArray());
				memoryStream.Close();
				return @string;
			}
		}

		public static string GenerateKey()
		{
			DESCryptoServiceProvider dESCryptoServiceProvider = (DESCryptoServiceProvider)DES.Create();
			return Encoding.ASCII.GetString(dESCryptoServiceProvider.Key);
		}

		public static string EncryptString(string sInputString)
		{
			byte[] bytes = Encoding.GetEncoding("GBK").GetBytes(sInputString);
			return BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(bytes)).Replace("-", "");
		}

		public static string MD5(string encypStr, string charset = "UTF-8")
		{
			MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
			byte[] bytes;
			try
			{
				bytes = Encoding.GetEncoding(charset).GetBytes(encypStr);
			}
			catch (Exception)
			{
				bytes = Encoding.GetEncoding(charset).GetBytes(encypStr);
			}
			return BitConverter.ToString(mD5CryptoServiceProvider.ComputeHash(bytes)).Replace("-", "").ToUpper();
		}

		public static string MD5(string strToEncrypt)
		{
			byte[] bytes = Encoding.GetEncoding("GB2312").GetBytes(strToEncrypt);
			bytes = new MD5CryptoServiceProvider().ComputeHash(bytes);
			string text = "";
			for (int i = 0; i < bytes.Length; i++)
			{
				text += bytes[i].ToString("x").PadLeft(2, '0');
			}
			return text;
		}

		public static string MD5_16(string encypStr)
		{
			MD5 mD = System.Security.Cryptography.MD5.Create();
			byte[] array = new byte[16];
			ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
			return Convert.ToBase64String(mD.ComputeHash(aSCIIEncoding.GetBytes(encypStr)));
		}

		public static string Base64(this string value)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(value);
			return Convert.ToBase64String(bytes, 0, bytes.Length);
		}

		public static string UnBase64(this string value)
		{
			byte[] bytes = Convert.FromBase64String(value);
			return Encoding.UTF8.GetString(bytes);
		}

		private static byte[] FormatByte(this string strVal, Encoding encoding)
		{
			return encoding.GetBytes(strVal.Base64().Substring(0, 16).ToUpper());
		}

		public static string AesEncrypt(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return string.Empty;
			}
			Encoding uTF = Encoding.UTF8;
			byte[] rijndaelKey = RijndaelKey;
			byte[] rijndaelIV = RijndaelIV;
			byte[] bytes = uTF.GetBytes(value);
			Rijndael rijndael = Rijndael.Create();
			string result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndael.CreateEncryptor(rijndaelKey, rijndaelIV), CryptoStreamMode.Write))
				{
					cryptoStream.Write(bytes, 0, bytes.Length);
					cryptoStream.FlushFinalBlock();
					result = Convert.ToBase64String(memoryStream.ToArray());
				}
			}
			rijndael.Clear();
			return result;
		}

		public static string AesDecryptor(string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				Encoding uTF = Encoding.UTF8;
				byte[] rijndaelKey = RijndaelKey;
				byte[] rijndaelIV = RijndaelIV;
				try
				{
					byte[] array = Convert.FromBase64String(value);
					Rijndael rijndael = Rijndael.Create();
					string @string;
					using (MemoryStream memoryStream = new MemoryStream())
					{
						using (CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndael.CreateDecryptor(rijndaelKey, rijndaelIV), CryptoStreamMode.Write))
						{
							cryptoStream.Write(array, 0, array.Length);
							cryptoStream.FlushFinalBlock();
							@string = uTF.GetString(memoryStream.ToArray());
						}
					}
					rijndael.Clear();
					return @string;
				}
				catch (Exception)
				{
					return string.Empty;
				}
			}
			return string.Empty;
		}

		public static string RSAPrivateKeyJava2DotNet(string privateKey)
		{
			if (!string.IsNullOrEmpty(privateKey))
			{
				privateKey = privateKey.Trim().Replace(" ", "");
			}
			RsaPrivateCrtKeyParameters rsaPrivateCrtKeyParameters = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKey));
			return $"<RSAKeyValue><Modulus>{Convert.ToBase64String(rsaPrivateCrtKeyParameters.Modulus.ToByteArrayUnsigned())}</Modulus><Exponent>{Convert.ToBase64String(rsaPrivateCrtKeyParameters.PublicExponent.ToByteArrayUnsigned())}</Exponent><P>{Convert.ToBase64String(rsaPrivateCrtKeyParameters.P.ToByteArrayUnsigned())}</P><Q>{Convert.ToBase64String(rsaPrivateCrtKeyParameters.Q.ToByteArrayUnsigned())}</Q><DP>{Convert.ToBase64String(rsaPrivateCrtKeyParameters.DP.ToByteArrayUnsigned())}</DP><DQ>{Convert.ToBase64String(rsaPrivateCrtKeyParameters.DQ.ToByteArrayUnsigned())}</DQ><InverseQ>{Convert.ToBase64String(rsaPrivateCrtKeyParameters.QInv.ToByteArrayUnsigned())}</InverseQ><D>{Convert.ToBase64String(rsaPrivateCrtKeyParameters.Exponent.ToByteArrayUnsigned())}</D></RSAKeyValue>";
		}

		public static string RSAPublicKeyJava2DotNet(string publicKey)
		{
			if (!string.IsNullOrEmpty(publicKey))
			{
				publicKey = publicKey.Trim().Replace(" ", "");
			}
			RsaKeyParameters rsaKeyParameters = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
			return $"<RSAKeyValue><Modulus>{Convert.ToBase64String(rsaKeyParameters.Modulus.ToByteArrayUnsigned())}</Modulus><Exponent>{Convert.ToBase64String(rsaKeyParameters.Exponent.ToByteArrayUnsigned())}</Exponent></RSAKeyValue>";
		}

		public static string RSAEncryptByPublicKey(string message, string pubilcKey)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(message);
			RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
			rSACryptoServiceProvider.FromXmlString(pubilcKey);
			int num = rSACryptoServiceProvider.KeySize / 8 - 11;
			if (bytes.Length <= num)
			{
				return Convert.ToBase64String(rSACryptoServiceProvider.Encrypt(bytes, fOAEP: false));
			}
			using (MemoryStream memoryStream = new MemoryStream(bytes))
			{
				using (MemoryStream memoryStream2 = new MemoryStream())
				{
					byte[] array = new byte[num];
					for (int num2 = memoryStream.Read(array, 0, num); num2 > 0; num2 = memoryStream.Read(array, 0, num))
					{
						byte[] array2 = new byte[num2];
						Array.Copy(array, 0, array2, 0, num2);
						byte[] array3 = rSACryptoServiceProvider.Encrypt(array2, fOAEP: false);
						memoryStream2.Write(array3, 0, array3.Length);
					}
					return Convert.ToBase64String(memoryStream2.ToArray(), Base64FormattingOptions.None);
				}
			}
		}

		public static string DecryptPublicKeyJava(string publicKeyJava, string data, string encoding = "UTF-8")
		{
			if (string.IsNullOrEmpty(publicKeyJava))
			{
				return string.Empty;
			}
			if (string.IsNullOrEmpty(data))
			{
				return string.Empty;
			}
			RsaKeyParameters parameters = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKeyJava));
			byte[] array = Convert.FromBase64String(data);
			IAsymmetricBlockCipher cipher = new RsaEngine();
			cipher = new Pkcs1Encoding(cipher);
			cipher.Init(forEncryption: false, parameters);
			string text = "";
			for (int i = 0; i < array.Length / 128; i++)
			{
				byte[] array2 = new byte[128];
				for (int j = 0; j < 128; j++)
				{
					array2[j] = array[j + 128 * i];
				}
				array2 = cipher.ProcessBlock(array2, 0, array2.Length);
				char[] array3 = new char[Encoding.GetEncoding(encoding).GetCharCount(array2, 0, array2.Length)];
				Encoding.GetEncoding(encoding).GetChars(array2, 0, array2.Length, array3, 0);
				text += new string(array3);
			}
			return text;
		}

		public static string RSADecryptByPublicKey(string strDecryptString, string pubilcKey, string input_charset = "UTF-8")
		{
			if (string.IsNullOrEmpty(strDecryptString))
			{
				return string.Empty;
			}
			if (string.IsNullOrEmpty(pubilcKey))
			{
				return string.Empty;
			}
			byte[] array = Convert.FromBase64String(strDecryptString);
			string text = "";
			for (int i = 0; i < array.Length / 128; i++)
			{
				byte[] array2 = new byte[128];
				for (int j = 0; j < 128; j++)
				{
					array2[j] = array[j + 128 * i];
				}
				text += decryptByPubilcKey(array2, pubilcKey, input_charset);
			}
			return text;
		}

		private static string decryptByPubilcKey(byte[] data, string pubilcKey, string input_charset)
		{
			RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
			rSACryptoServiceProvider.FromXmlString(pubilcKey);
			byte[] array = rSACryptoServiceProvider.Decrypt(data, fOAEP: false);
			char[] array2 = new char[Encoding.GetEncoding(input_charset).GetCharCount(array, 0, array.Length)];
			Encoding.GetEncoding(input_charset).GetChars(array, 0, array.Length, array2, 0);
			return new string(array2);
		}

		public static string RSASignByPrivateKey(string content, string privateKey, string halg = "SHA1")
		{
			RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
			rSACryptoServiceProvider.FromXmlString(privateKey);
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			return Convert.ToBase64String(rSACryptoServiceProvider.SignData(bytes, halg));
		}

		public static bool RsaVerifyByPublicKey(string content, string publicKey, string sign, string halg = "SHA1")
		{
			RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
			rSACryptoServiceProvider.FromXmlString(publicKey);
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			byte[] signature = Convert.FromBase64String(sign);
			return rSACryptoServiceProvider.VerifyData(bytes, halg, signature);
		}
	}
}
