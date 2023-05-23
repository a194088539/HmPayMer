using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace HM.Framework.Image
{
	public class ValidateCode
	{
		public static byte[] CreateValidateGraphic(out string Code, int CodeLength, int Width, int Height, int FontSize)
		{
			string text = string.Empty;
			Color[] array = new Color[8]
			{
				Color.Black,
				Color.Red,
				Color.Blue,
				Color.Green,
				Color.Orange,
				Color.Brown,
				Color.Brown,
				Color.DarkBlue
			};
			string[] array2 = new string[6]
			{
				"Times New Roman",
				"MS Mincho",
				"Book Antiqua",
				"Gungsuh",
				"PMingLiU",
				"Impact"
			};
			char[] array3 = new char[27]
			{
				'2',
				'3',
				'4',
				'5',
				'6',
				'8',
				'9',
				'A',
				'B',
				'C',
				'D',
				'E',
				'F',
				'G',
				'H',
				'J',
				'K',
				'L',
				'M',
				'N',
				'P',
				'R',
				'S',
				'T',
				'W',
				'X',
				'Y'
			};
			Random random = new Random();
			Bitmap bitmap = null;
			Graphics graphics = null;
			int num = 0;
			Point pt = default(Point);
			Point pt2 = default(Point);
			Font font = null;
			Color color = default(Color);
			for (num = 0; num <= CodeLength - 1; num++)
			{
				text += array3[random.Next(array3.Length)].ToString();
			}
			bitmap = new Bitmap(Width, Height);
			graphics = Graphics.FromImage(bitmap);
			graphics.Clear(Color.White);
			try
			{
				for (num = 0; num <= 4; num++)
				{
					pt.X = random.Next(Width);
					pt.Y = random.Next(Height);
					pt2.X = random.Next(Width);
					pt2.Y = random.Next(Height);
					color = array[random.Next(array.Length)];
					graphics.DrawLine(new Pen(color), pt, pt2);
				}
				float num2 = 0f;
				float num3 = 0f;
				float num4 = 0f;
				if (CodeLength != 0)
				{
					num2 = (float)((Width - FontSize * CodeLength - 10) / CodeLength);
				}
				for (num = 0; num <= text.Length - 1; num++)
				{
					font = new Font(array2[random.Next(array2.Length)], (float)FontSize, FontStyle.Italic);
					color = array[random.Next(array.Length)];
					num4 = (float)((Height - font.Height) / 2 + 2);
					num3 = Convert.ToSingle(num) * (float)FontSize + (float)(num + 1) * num2;
					graphics.DrawString(text[num].ToString(), font, new SolidBrush(color), num3, num4);
				}
				for (int i = 0; i <= 30; i++)
				{
					int x = random.Next(bitmap.Width);
					int y = random.Next(bitmap.Height);
					Color color2 = array[random.Next(array.Length)];
					bitmap.SetPixel(x, y, color2);
				}
				Code = text;
				MemoryStream memoryStream = new MemoryStream();
				bitmap.Save(memoryStream, ImageFormat.Jpeg);
				return memoryStream.ToArray();
			}
			finally
			{
				graphics.Dispose();
			}
		}
	}
}
