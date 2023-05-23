using System.Drawing;
using ThoughtWorks.QRCode.Codec;

namespace HM.Framework.Image
{
	public class QRCode
	{
		public static Bitmap CreateImage(string codeUrl, int widhtHeight = 300)
		{
			return new QRCodeEncoder
			{
				QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE,
				QRCodeScale = 4,
				QRCodeVersion = 0,
				QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.L
			}.Encode(codeUrl);
		}
	}
}
