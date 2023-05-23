using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace HM.Framework.PayApi.ShangDe
{
	public class CryptUtils
	{
		public static byte[] AESEncrypt(byte[] Data, string Key)
		{
			return new RijndaelManaged
			{
				Key = Encoding.UTF8.GetBytes(Key),
				Mode = CipherMode.ECB,
				Padding = PaddingMode.PKCS7
			}.CreateEncryptor().TransformFinalBlock(Data, 0, Data.Length);
		}

		public static byte[] AESDecrypt(byte[] Data, string Key)
		{
			return new RijndaelManaged
			{
				Key = Encoding.UTF8.GetBytes(Key),
				Mode = CipherMode.ECB,
				Padding = PaddingMode.PKCS7
			}.CreateDecryptor().TransformFinalBlock(Data, 0, Data.Length);
		}

		public static string GuidTo16String()
		{
			string text = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
			int length = text.Length;
			long num = 1L;
			byte[] array = Guid.NewGuid().ToByteArray();
			foreach (byte b in array)
			{
				num *= b + 1;
			}
			string text2 = string.Empty;
			Random random = new Random((int)(num & uint.MaxValue) | (int)(num >> 32));
			for (int j = 0; j < 16; j++)
			{
				text2 += text[random.Next() % length].ToString();
			}
			return text2;
		}

		public static string getStringFromBytes(byte[] hexbytes, Encoding enc)
		{
			return enc.GetString(hexbytes);
		}

		public static byte[] getBytesFromString(string str, Encoding enc)
		{
			return enc.GetBytes(str);
		}

		public static byte[] asc2hex(string hexString)
		{
			byte[] array = new byte[hexString.Length / 2];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
			}
			return array;
		}

		public static string hex2asc(byte[] hexbytes)
		{
			return BitConverter.ToString(hexbytes).Replace("-", string.Empty);
		}

		public static byte[] RSADecrypt(string xmlPrivateKey, byte[] EncryptedBytes)
		{
			using (RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider())
			{
				rSACryptoServiceProvider.FromXmlString(xmlPrivateKey);
				int num = rSACryptoServiceProvider.KeySize / 8;
				if (EncryptedBytes.Length > num)
				{
					using (MemoryStream memoryStream = new MemoryStream(EncryptedBytes))
					{
						using (MemoryStream memoryStream2 = new MemoryStream())
						{
							byte[] array = new byte[num];
							for (int num2 = memoryStream.Read(array, 0, num); num2 > 0; num2 = memoryStream.Read(array, 0, num))
							{
								byte[] array2 = new byte[num2];
								Array.Copy(array, 0, array2, 0, num2);
								byte[] array3 = rSACryptoServiceProvider.Decrypt(array2, fOAEP: false);
								memoryStream2.Write(array3, 0, array3.Length);
							}
							return memoryStream2.ToArray();
						}
					}
				}
				return rSACryptoServiceProvider.Decrypt(EncryptedBytes, fOAEP: false);
			}
		}

		public static byte[] RSAEncrypt(string xmlPublicKey, byte[] SourceBytes)
		{
			using (RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider())
			{
				rSACryptoServiceProvider.FromXmlString(xmlPublicKey);
				int num = rSACryptoServiceProvider.KeySize / 8 - 11;
				if (SourceBytes.Length > num)
				{
					using (MemoryStream memoryStream = new MemoryStream(SourceBytes))
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
							return memoryStream2.ToArray();
						}
					}
				}
				return rSACryptoServiceProvider.Encrypt(SourceBytes, fOAEP: false);
			}
		}

		public static X509Certificate2 getPrivateKeyXmlFromPFX(string PFX_file, string password)
		{
			return new X509Certificate2(PFX_file, password, X509KeyStorageFlags.Exportable);
		}

		public static X509Certificate2 getPublicKeyXmlFromCer(string Cer_file)
		{
			return new X509Certificate2(Cer_file);
		}

		public static byte[] CreateSignWithPrivateKey(byte[] msgin, X509Certificate2 certEncrypt)
		{
			byte[] rgbHash = HashAlgorithm.Create("SHA1").ComputeHash(msgin);
			using (RSACryptoServiceProvider rSACryptoServiceProvider = (RSACryptoServiceProvider)certEncrypt.PrivateKey)
			{
				RSAPKCS1SignatureFormatter rSAPKCS1SignatureFormatter = new RSAPKCS1SignatureFormatter(rSACryptoServiceProvider);
				rSAPKCS1SignatureFormatter.SetHashAlgorithm("SHA1");
				byte[] result = rSAPKCS1SignatureFormatter.CreateSignature(rgbHash);
				rSACryptoServiceProvider.Dispose();
				return result;
			}
		}

		public static bool VerifySignWithPublicKey(byte[] msgin, X509Certificate2 cerDecrypt, byte[] signedHashBytes)
		{
			byte[] rgbHash = HashAlgorithm.Create("SHA1").ComputeHash(msgin);
			bool flag = false;
			using (RSACryptoServiceProvider rSACryptoServiceProvider = (RSACryptoServiceProvider)cerDecrypt.PublicKey.Key)
			{
				RSAPKCS1SignatureDeformatter rSAPKCS1SignatureDeformatter = new RSAPKCS1SignatureDeformatter(rSACryptoServiceProvider);
				rSAPKCS1SignatureDeformatter.SetHashAlgorithm("SHA1");
				flag = rSAPKCS1SignatureDeformatter.VerifySignature(rgbHash, signedHashBytes);
				rSACryptoServiceProvider.Dispose();
				return flag;
			}
		}

		public static string Base64Encoder(byte[] b)
		{
			return Convert.ToBase64String(b);
		}

		public static byte[] Base64Decoder(string base64String)
		{
			return Convert.FromBase64String(base64String);
		}
	}
}
