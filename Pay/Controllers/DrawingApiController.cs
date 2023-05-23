using HM.Framework;
using HM.Framework.Image;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace HmPMer.Pay.Controllers
{
	public class DrawingApiController : Controller
	{
		public ActionResult QrCode()
		{
			string text = Utils.GetRequest("d");
			if (!string.IsNullOrEmpty(text))
			{
				text = HttpUtility.UrlDecode(text);
			}
			Bitmap bitmap = QRCode.CreateImage(text);
			using (MemoryStream memoryStream = new MemoryStream())
			{
				bitmap.Save(memoryStream, ImageFormat.Png);
				base.Response.ContentType = "image/Png";
				base.Response.OutputStream.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
				base.Response.End();
			}
			return null;
		}
	}
}
