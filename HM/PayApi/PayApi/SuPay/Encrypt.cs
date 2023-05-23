using System;
using System.Security.Cryptography;
using System.Text;

namespace HM.Framework.PayApi.SuPay
{
	public class Encrypt
	{
		private class HmacMD5
		{
			private uint[] count;

			private uint[] state;

			private byte[] buffer;

			private byte[] Digest;

			private static byte[] pad = new byte[64]
			{
				128,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0
			};

			private const uint S11 = 7u;

			private const uint S12 = 12u;

			private const uint S13 = 17u;

			private const uint S14 = 22u;

			private const uint S21 = 5u;

			private const uint S22 = 9u;

			private const uint S23 = 14u;

			private const uint S24 = 20u;

			private const uint S31 = 4u;

			private const uint S32 = 11u;

			private const uint S33 = 16u;

			private const uint S34 = 23u;

			private const uint S41 = 6u;

			private const uint S42 = 10u;

			private const uint S43 = 15u;

			private const uint S44 = 21u;

			public HmacMD5()
			{
				count = new uint[2];
				state = new uint[4];
				buffer = new byte[64];
				Digest = new byte[16];
				init();
			}

			public void init()
			{
				count[0] = 0u;
				count[1] = 0u;
				state[0] = 1732584193u;
				state[1] = 4023233417u;
				state[2] = 2562383102u;
				state[3] = 271733878u;
			}

			public void update(byte[] data, uint length)
			{
				uint num = length;
				uint num2 = (count[0] >> 3) & 0x3F;
				uint num3 = length << 3;
				uint num4 = 0u;
				if (length != 0)
				{
					count[0] += num3;
					count[1] += length >> 29;
					if (count[0] < num3)
					{
						count[1]++;
					}
					if (num2 != 0)
					{
						uint num5 = (num2 + length > 64) ? (64 - num2) : length;
						Buffer.BlockCopy(data, 0, buffer, (int)num2, (int)num5);
						if (num2 + num5 < 64)
						{
							return;
						}
						transform(buffer);
						num4 += num5;
						num -= num5;
					}
					while (num >= 64)
					{
						Buffer.BlockCopy(data, (int)num4, buffer, 0, 64);
						transform(buffer);
						num4 += 64;
						num -= 64;
					}
					if (num != 0)
					{
						Buffer.BlockCopy(data, (int)num4, buffer, 0, (int)num);
					}
				}
			}

			public byte[] finalize()
			{
				byte[] output = new byte[8];
				encode(ref output, count, 8u);
				uint num = (count[0] >> 3) & 0x3F;
				uint length = (num < 56) ? (56 - num) : (120 - num);
				update(pad, length);
				update(output, 8u);
				encode(ref Digest, state, 16u);
				for (int i = 0; i < 64; i++)
				{
					buffer[i] = 0;
				}
				return Digest;
			}

			public string md5String()
			{
				string text = "";
				for (int i = 0; i < Digest.Length; i++)
				{
					text += Digest[i].ToString("x2");
				}
				return text;
			}

			private void transform(byte[] data)
			{
				uint a = state[0];
				uint a2 = state[1];
				uint a3 = state[2];
				uint a4 = state[3];
				uint[] output = new uint[16];
				decode(ref output, data, 64u);
				FF(ref a, a2, a3, a4, output[0], 7u, 3614090360u);
				FF(ref a4, a, a2, a3, output[1], 12u, 3905402710u);
				FF(ref a3, a4, a, a2, output[2], 17u, 606105819u);
				FF(ref a2, a3, a4, a, output[3], 22u, 3250441966u);
				FF(ref a, a2, a3, a4, output[4], 7u, 4118548399u);
				FF(ref a4, a, a2, a3, output[5], 12u, 1200080426u);
				FF(ref a3, a4, a, a2, output[6], 17u, 2821735955u);
				FF(ref a2, a3, a4, a, output[7], 22u, 4249261313u);
				FF(ref a, a2, a3, a4, output[8], 7u, 1770035416u);
				FF(ref a4, a, a2, a3, output[9], 12u, 2336552879u);
				FF(ref a3, a4, a, a2, output[10], 17u, 4294925233u);
				FF(ref a2, a3, a4, a, output[11], 22u, 2304563134u);
				FF(ref a, a2, a3, a4, output[12], 7u, 1804603682u);
				FF(ref a4, a, a2, a3, output[13], 12u, 4254626195u);
				FF(ref a3, a4, a, a2, output[14], 17u, 2792965006u);
				FF(ref a2, a3, a4, a, output[15], 22u, 1236535329u);
				GG(ref a, a2, a3, a4, output[1], 5u, 4129170786u);
				GG(ref a4, a, a2, a3, output[6], 9u, 3225465664u);
				GG(ref a3, a4, a, a2, output[11], 14u, 643717713u);
				GG(ref a2, a3, a4, a, output[0], 20u, 3921069994u);
				GG(ref a, a2, a3, a4, output[5], 5u, 3593408605u);
				GG(ref a4, a, a2, a3, output[10], 9u, 38016083u);
				GG(ref a3, a4, a, a2, output[15], 14u, 3634488961u);
				GG(ref a2, a3, a4, a, output[4], 20u, 3889429448u);
				GG(ref a, a2, a3, a4, output[9], 5u, 568446438u);
				GG(ref a4, a, a2, a3, output[14], 9u, 3275163606u);
				GG(ref a3, a4, a, a2, output[3], 14u, 4107603335u);
				GG(ref a2, a3, a4, a, output[8], 20u, 1163531501u);
				GG(ref a, a2, a3, a4, output[13], 5u, 2850285829u);
				GG(ref a4, a, a2, a3, output[2], 9u, 4243563512u);
				GG(ref a3, a4, a, a2, output[7], 14u, 1735328473u);
				GG(ref a2, a3, a4, a, output[12], 20u, 2368359562u);
				HH(ref a, a2, a3, a4, output[5], 4u, 4294588738u);
				HH(ref a4, a, a2, a3, output[8], 11u, 2272392833u);
				HH(ref a3, a4, a, a2, output[11], 16u, 1839030562u);
				HH(ref a2, a3, a4, a, output[14], 23u, 4259657740u);
				HH(ref a, a2, a3, a4, output[1], 4u, 2763975236u);
				HH(ref a4, a, a2, a3, output[4], 11u, 1272893353u);
				HH(ref a3, a4, a, a2, output[7], 16u, 4139469664u);
				HH(ref a2, a3, a4, a, output[10], 23u, 3200236656u);
				HH(ref a, a2, a3, a4, output[13], 4u, 681279174u);
				HH(ref a4, a, a2, a3, output[0], 11u, 3936430074u);
				HH(ref a3, a4, a, a2, output[3], 16u, 3572445317u);
				HH(ref a2, a3, a4, a, output[6], 23u, 76029189u);
				HH(ref a, a2, a3, a4, output[9], 4u, 3654602809u);
				HH(ref a4, a, a2, a3, output[12], 11u, 3873151461u);
				HH(ref a3, a4, a, a2, output[15], 16u, 530742520u);
				HH(ref a2, a3, a4, a, output[2], 23u, 3299628645u);
				II(ref a, a2, a3, a4, output[0], 6u, 4096336452u);
				II(ref a4, a, a2, a3, output[7], 10u, 1126891415u);
				II(ref a3, a4, a, a2, output[14], 15u, 2878612391u);
				II(ref a2, a3, a4, a, output[5], 21u, 4237533241u);
				II(ref a, a2, a3, a4, output[12], 6u, 1700485571u);
				II(ref a4, a, a2, a3, output[3], 10u, 2399980690u);
				II(ref a3, a4, a, a2, output[10], 15u, 4293915773u);
				II(ref a2, a3, a4, a, output[1], 21u, 2240044497u);
				II(ref a, a2, a3, a4, output[8], 6u, 1873313359u);
				II(ref a4, a, a2, a3, output[15], 10u, 4264355552u);
				II(ref a3, a4, a, a2, output[6], 15u, 2734768916u);
				II(ref a2, a3, a4, a, output[13], 21u, 1309151649u);
				II(ref a, a2, a3, a4, output[4], 6u, 4149444226u);
				II(ref a4, a, a2, a3, output[11], 10u, 3174756917u);
				II(ref a3, a4, a, a2, output[2], 15u, 718787259u);
				II(ref a2, a3, a4, a, output[9], 21u, 3951481745u);
				state[0] += a;
				state[1] += a2;
				state[2] += a3;
				state[3] += a4;
				for (int i = 0; i < 16; i++)
				{
					output[i] = 0u;
				}
			}

			private void encode(ref byte[] output, uint[] input, uint len)
			{
				if (BitConverter.IsLittleEndian)
				{
					uint num = 0u;
					for (uint num2 = 0u; num2 < len; num2 += 4)
					{
						output[num2] = (byte)(input[num] & 0xFF);
						output[num2 + 1] = (byte)((input[num] >> 8) & 0xFF);
						output[num2 + 2] = (byte)((input[num] >> 16) & 0xFF);
						output[num2 + 3] = (byte)((input[num] >> 24) & 0xFF);
						num++;
					}
				}
				else
				{
					uint num = 0u;
					for (uint num2 = 0u; num2 < len; num2 += 4)
					{
						output[num2 + 3] = (byte)(input[num] & 0xFF);
						output[num2 + 2] = (byte)((input[num] >> 8) & 0xFF);
						output[num2 + 1] = (byte)((input[num] >> 16) & 0xFF);
						output[num2] = (byte)((input[num] >> 24) & 0xFF);
						num++;
					}
				}
			}

			private void decode(ref uint[] output, byte[] input, uint len)
			{
				if (BitConverter.IsLittleEndian)
				{
					uint num = 0u;
					for (uint num2 = 0u; num2 < len; num2 += 4)
					{
						output[num] = (uint)(input[num2] | (input[num2 + 1] << 8) | (input[num2 + 2] << 16) | (input[num2 + 3] << 24));
						num++;
					}
				}
				else
				{
					uint num = 0u;
					for (uint num2 = 0u; num2 < len; num2 += 4)
					{
						output[num] = (uint)(input[num2 + 3] | (input[num2 + 2] << 8) | (input[num2 + 1] << 16) | (input[num2] << 24));
						num++;
					}
				}
			}

			private uint rotate_left(uint x, uint n)
			{
				return (x << (int)n) | (x >> (int)(32 - n));
			}

			private uint F(uint x, uint y, uint z)
			{
				return (x & y) | (~x & z);
			}

			private uint G(uint x, uint y, uint z)
			{
				return (x & z) | (y & ~z);
			}

			private uint H(uint x, uint y, uint z)
			{
				return x ^ y ^ z;
			}

			private uint I(uint x, uint y, uint z)
			{
				return y ^ (x | ~z);
			}

			private void FF(ref uint a, uint b, uint c, uint d, uint x, uint s, uint ac)
			{
				a += F(b, c, d) + x + ac;
				a = rotate_left(a, s) + b;
			}

			private void GG(ref uint a, uint b, uint c, uint d, uint x, uint s, uint ac)
			{
				a += G(b, c, d) + x + ac;
				a = rotate_left(a, s) + b;
			}

			private void HH(ref uint a, uint b, uint c, uint d, uint x, uint s, uint ac)
			{
				a += H(b, c, d) + x + ac;
				a = rotate_left(a, s) + b;
			}

			private void II(ref uint a, uint b, uint c, uint d, uint x, uint s, uint ac)
			{
				a += I(b, c, d) + x + ac;
				a = rotate_left(a, s) + b;
			}
		}

		public static string md5(string str)
		{
			byte[] bytes = Encoding.Default.GetBytes(str);
			bytes = new MD5CryptoServiceProvider().ComputeHash(bytes);
			string text = "";
			for (int i = 0; i < bytes.Length; i++)
			{
				text += bytes[i].ToString("x").PadLeft(2, '0');
			}
			return text;
		}

		public static string SHA256(string str)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(str);
			return Convert.ToBase64String(new SHA256Managed().ComputeHash(bytes));
		}

		public static string HmacSign(string aValue)
		{
			return HmacSign(aValue, "");
		}

		public static string HmacSign(string aValue, string aKey)
		{
			byte[] array = new byte[64];
			byte[] array2 = new byte[64];
			byte[] bytes = Encoding.UTF8.GetBytes(aKey);
			byte[] bytes2 = Encoding.UTF8.GetBytes(aValue);
			for (int i = bytes.Length; i < 64; i++)
			{
				array[i] = 54;
			}
			for (int j = bytes.Length; j < 64; j++)
			{
				array2[j] = 92;
			}
			for (int k = 0; k < bytes.Length; k++)
			{
				array[k] = (byte)(bytes[k] ^ 0x36);
				array2[k] = (byte)(bytes[k] ^ 0x5C);
			}
			HmacMD5 hmacMD = new HmacMD5();
			hmacMD.update(array, (uint)array.Length);
			hmacMD.update(bytes2, (uint)bytes2.Length);
			byte[] data = hmacMD.finalize();
			hmacMD.init();
			hmacMD.update(array2, (uint)array2.Length);
			hmacMD.update(data, 16u);
			data = hmacMD.finalize();
			return toHex(data);
		}

		private static string toHex(byte[] input)
		{
			if (input == null)
			{
				return null;
			}
			StringBuilder stringBuilder = new StringBuilder(input.Length * 2);
			for (int i = 0; i < input.Length; i++)
			{
				int num = input[i] & 0xFF;
				if (num < 16)
				{
					stringBuilder.Append("0");
				}
				stringBuilder.Append(num.ToString("x"));
			}
			return stringBuilder.ToString();
		}
	}
}
