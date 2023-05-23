using HM.Framework;
using HM.Framework.Caching;
using HmPMer.Business;
using HmPMer.Entity;
using HmPMer.MerUI.Fillters;
using HmPMer.MerUI.Models;
using System;
using System.Web.Mvc;

namespace HmPMer.MerUI.Controllers
{
	public class AccountController : Controller
	{
		[Auth(NoLogin = true)]
		public ActionResult Login()
		{
			return View();
		}

		[HttpPost]
		[Auth(NoLogin = true)]
		public ActionResult LoginIn(UserBase model)
		{
			ApiResult<string> failing = ApiResult<string>.Failing;
			if (model == null)
			{
				failing.message = "请输入登录账户名和密码";
				return failing;
			}
			if (string.IsNullOrEmpty(model.MobilePhone))
			{
				failing.message = "请输入登陆手机";
				return failing;
			}

            model.Email = model.MobilePhone;

			if (string.IsNullOrEmpty(model.Pass))
			{
				failing.message = "请输入密码";
				return failing;
			}
			if (string.IsNullOrEmpty(model.Pass2))
			{
				failing.message = "请输入验证码";
				return failing;
			}
			string text = base.Session["hmerpmlogin"] as string;
			if (string.IsNullOrEmpty(text))
			{
				failing.message = "验证码已过期";
				return failing;
			}
			if (!text.ToUpper().Equals(model.Pass2.ToUpper()))
			{
				failing.message = "验证码不正确";
				return failing;
			}
			model.Pass = EncryUtils.MD5(model.Pass);
			model.UserType = 1;
			UserBaseBll userBaseBll = new UserBaseBll();
			UserBase userBase = userBaseBll.LoginIn(model);
			if (userBase != null)
			{
				if (userBase.IsEnabled == 2)
				{
					failing.message = (string.IsNullOrEmpty(new SysConfigBll().GetConfigVaule("LockAccoutText")) ? "你的账户被锁定，不能登录，请联系管理员" : new SysConfigBll().GetConfigVaule("LockAccoutText"));
					return failing;
				}
				ModelCommon.WriteUserModel(userBase);
				model.LastLoginIp = Utils.GetClientIp();
				userBaseBll.UpdateLogin(userBase);
				failing.IsSuccess = true;
				failing.data = userBase.MobilePhone;
				failing.message = "登录成功";
			}
			failing.message = "登录失败，帐户名或者密码不匹配！";
			return failing;
		}

		[Auth(NoLogin = true)]
		public ActionResult LoginOut()
		{
			ModelCommon.RemoveUserModel();
			return new RedirectResult("/account/login");
		}

		[Auth(NoLogin = true)]
		public ActionResult Forget()
		{
			return View();
		}

		[Auth(NoLogin = true)]
		public ActionResult NextGetPwdPhone(string Mobile, string SmsCode, string SmsKey)
		{
			ApiResult<string> failing = ApiResult<string>.Failing;
			if (new UserBaseBll().GetModelForMobile(Mobile, "") == null)
			{
				failing.message = "此手机账号不存在";
				return failing;
			}
			string text = new RedisCache().Get<string>(Mobile + SmsKey);
			if (string.IsNullOrEmpty(text))
			{
				failing.message = "短信验证码不存在或已过期";
				return failing;
			}
			if (!text.ToUpper().Equals(SmsCode.ToUpper()))
			{
				failing.message = "短信验证码不正确";
				return failing;
			}
			string randomStr = Utils.GetRandomStr(20);
			new RedisCache().Add(randomStr, Mobile, DateTime.Now.AddMinutes(15.0));
			failing.IsSuccess = true;
			failing.message = randomStr;
			return failing;
		}

		[Auth(NoLogin = true)]
		public ActionResult RestPwd(string yzCode)
		{
			string text = new RedisCache().Get<string>(yzCode);
			if (string.IsNullOrEmpty(text))
			{
				return Redirect("/Account/ForGet");
			}
			if (new UserBaseBll().GetModelForMobile(text, "") == null)
			{
				return Redirect("/Account/ForGet");
			}
			return View();
		}

		[Auth(NoLogin = true)]
		[HttpPost]
		public ActionResult RestPwdPhone(string Pass, string yzCode)
		{
			ApiResult<string> failing = ApiResult<string>.Failing;
			string text = new RedisCache().Get<string>(yzCode);
			if (string.IsNullOrEmpty(text))
			{
				failing.message = "短信验证码已过期！";
				return Json(failing);
			}
			if (new UserBaseBll().RestPwdPhone(EncryUtils.MD5(Pass), text) > 0)
			{
				failing.IsSuccess = true;
			}
			else
			{
				failing.message = "操作失败！";
			}
			return Json(failing);
		}

		[Auth(NoLogin = true)]
		public ActionResult Reg()
		{
			return View();
		}

		[HttpPost]
		[Auth(NoLogin = true)]
		public ActionResult Regedit(UserBase model)
		{
			ApiResult<bool> failing = ApiResult<bool>.Failing;
			if (new SysConfigBll().GetConfigVaule("RegIsOpen") != "1")
			{
				failing.message = "未开启注册！";
				return failing;
			}
			if (model == null)
			{
				failing.message = "参数错误！";
				return failing;
			}
			string text = new RedisCache().Get<string>(model.MobilePhone + "smshmerpmreg");
			if (string.IsNullOrEmpty(text))
			{
				failing.message = "短信验证码不存在或已过期";
				return failing;
			}
			if (!text.ToUpper().Equals(model.QQ.ToUpper()))
			{
				failing.message = "短信验证码不正确";
				return failing;
			}
			if (string.IsNullOrEmpty(model.MobilePhone))
			{
				failing.message = "请输入电话号码";
				return failing;
			}
			if (string.IsNullOrEmpty(model.Pass))
			{
				failing.message = "请输入密码";
				return failing;
			}
			UserBaseBll userBaseBll = new UserBaseBll();
			if (userBaseBll.GetModelForMobile(model.MobilePhone, "") != null)
			{
				failing.message = "此手机号码已经存在，请更换一个";
				return failing;
			}
			string request = Utils.GetRequest("hAgentId");
			if (!string.IsNullOrEmpty(request))
			{
				model.PromId = request;
			}
			if (userBaseBll.AddBusiness(model, 1) == 0L)
			{
				failing.message = "注册失败，请重新注册！";
				return failing;
			}
			failing.data = true;
			failing.IsSuccess = true;
			return failing;
		}

		[Auth(NoLogin = true)]
		public ActionResult HmAdminIn(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return Content("地址不正确！");
			}
			string[] array = EncryUtils.AesDecryptor(id).Split('_');
			if (array.Length != 2)
			{
				return Content("地址不正确！");
			}
			string userId = array[0];
			string obj = array[1];
			string value = DateTime.Now.ToString("yyyyMMdd");
			if (!obj.Equals(value))
			{
				return Content("地址不正确！");
			}
			UserBaseInfo modelForId = new UserBaseBll().GetModelForId(userId);
			if (modelForId != null)
			{
				ModelCommon.WriteUserModel(modelForId);
				return new RedirectResult("/index/index");
			}
			return Content("此商户不存在！");
		}
	}
}
