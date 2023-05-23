using HM.Framework;
using HM.Framework.Caching;
using HM.Framework.Image;
using HmPMer.Business;
using HmPMer.Entity;
using HmPMer.MerUI.Fillters;
using HmPMer.MerUI.Models;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace HM.WebAdminUI.Controllers
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
                base.Response.OutputStream.Write(memoryStream.GetBuffer(), 0, (int) memoryStream.Length);
                base.Response.End();
            }

            return null;
        }

        [Auth(NoLogin = true)]
        public ActionResult GetValidateImg(string codeKey)
        {
            int width = 95;
            int height = 40;
            int fontSize = 17;
            string Code = string.Empty;
            byte[] fileContents =
                HmPMer.MerUI.Models.ValidateCode.CreateValidateGraphic(out Code, 4, width, height, fontSize);
            base.Session[codeKey] = Code;
            return File(fileContents, "image/jpeg");
        }

        [Auth(NoLogin = true)]
        public ActionResult SendMobileCode(string Mobile, string SmsKey, string ImgCode, string ImgCodeKey,
            string SmsCode = "SmsCode", int type = 1)
        {
            ApiResult<string> failing = ApiResult<string>.Failing;
            try
            {
                string text = base.Session[ImgCodeKey] as string;
                if (string.IsNullOrEmpty(text))
                {
                    failing.message = "图片验证码已过期";
                    return failing;
                }

                if (!text.ToUpper().Equals(ImgCode.ToUpper()))
                {
                    failing.message = "图片验证码不正确";
                    return failing;
                }

                UserBase modelForMobile = new UserBaseBll().GetModelForMobile(Mobile, "");
                if (type != 1 && modelForMobile == null)
                {
                    failing.message = "此手机账号不存在";
                    return failing;
                }

                string text2 = new Random().Next(100000, 999999).ToString();
                if (!new SystemBll().SendSms(SmsCode, Mobile, text2,
                    (modelForMobile == null) ? Mobile : modelForMobile.UserId, decimal.Zero))
                {
                    failing.message = " 发送失败";
                    return failing;
                }

                failing.IsSuccess = true;
                new RedisCache().Add(Mobile + SmsKey, text2, DateTime.Now.AddMinutes(3.0));
                return failing;
            }
            catch (Exception ex)
            {
                failing.IsSuccess = false;
                failing.message = ex.Message;
                return failing;
            }
        }

        public ActionResult SendMobile(string Mobile, string SmsKey, string SmsCode = "SmsCode")
        {
            ApiResult<string> failing = ApiResult<string>.Failing;
            try
            {

                UserBase modelForMobile = new UserBaseBll().GetModelForMobile(Mobile, "");
                if (modelForMobile == null)
                {
                    failing.message = "此手机账号不存在";
                    return failing;
                }

                string text2 = new Random().Next(100000, 999999).ToString();
                if (!new SystemBll().SendSms(SmsCode, Mobile, text2,
                    (modelForMobile == null) ? Mobile : modelForMobile.UserId, decimal.Zero))
                {
                    failing.message = " 发送失败";
                    return failing;
                }

                failing.IsSuccess = true;
                new RedisCache().Add(Mobile + SmsKey, text2, DateTime.Now.AddMinutes(3.0));
                return failing;
            }
            catch (Exception ex)
            {
                failing.IsSuccess = false;
                failing.message = ex.Message;
                return failing;
            }
        }
    }
}