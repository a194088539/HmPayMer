using HM.Framework;
using HM.Framework.Image;
using HmPMer.AdminUI.Fillters;
using HmPMer.AdminUI.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace HmPMer.AdminUI.Controllers
{
	public class DrawingApiController : Controller
	{
		[Auth(NoLogin = true)]
		public ActionResult QrCode()
		{
			string text = Utils.GetRequest("data");
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

		[Auth(NoLogin = true)]
		public ActionResult GetValidateImg()
		{
			int width = 80;
			int height = 32;
			int fontSize = 15;
			string Code = string.Empty;
			byte[] fileContents = HmPMer.AdminUI.Models.ValidateCode.CreateValidateGraphic(out Code, 4, width, height, fontSize);
			base.Session["code"] = Code;
			return File(fileContents, "image/jpeg");
		}
	}
}
