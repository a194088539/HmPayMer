using HM.Framework;
using HM.Framework.Caching;
using HmPMer.Business;
using HmPMer.Entity;
using HmPMer.MerUI.Fillters;
using HmPMer.MerUI.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;

namespace HmPMer.MerUI.Controllers
{
	public class CenterController : Controller
	{
		private AccountBll bll = new AccountBll();

		private UserBaseBll ubll = new UserBaseBll();

		private UserBase user = ModelCommon.GetUserModel();

		public ActionResult Index()
		{
			UserBaseInfo modelForId = ubll.GetModelForId(user.UserId);
			new PayBll();
			List<PayRateInfo> list = new List<PayRateInfo>();
			if (modelForId.AloneRate == 1)
			{
				list = new RateBll().GetPayRateList(user.UserId, 2);
			}
			else if (modelForId.AloneRate == 0 && !string.IsNullOrEmpty(modelForId.GradeId))
			{
				list = new RateBll().GetUserGradRateList(user.UserId, modelForId.GradeId);
			}
			foreach (PayRateInfo item in list)
			{
				if (!string.IsNullOrEmpty(item.AccountScheme))
				{
					AccountScheme accountScheme = new WithdrawBll().GetAccountScheme(item.AccountScheme);
					if (accountScheme != null)
					{
						item.AccountSchemeName = accountScheme.name;
					}
				}
			}
			base.ViewBag.RateList = list;
			return View(modelForId);
		}

		public ActionResult Addamt()
		{
			UserBaseInfo modelForId = ubll.GetModelForId(user.UserId);
			return View(modelForId);
		}

		public ActionResult Safety()
		{
			UserBaseInfo modelForId = ubll.GetModelForId(user.UserId);
			return View(modelForId);
		}

		public ActionResult EditPass()
		{
			UserBaseInfo modelForId = ubll.GetModelForId(user.UserId);
			return View(modelForId);
		}

		public ActionResult EditPass2()
		{
			UserBaseInfo modelForId = ubll.GetModelForId(user.UserId);
			return View(modelForId);
		}

		[HttpPost]
		public ActionResult UpdatePass(string pwd, string newPwd)
		{
			ApiResult<string> failing = ApiResult<string>.Failing;
			UserBase userModel = ModelCommon.GetUserModel();
			if (string.IsNullOrEmpty(pwd))
			{
				failing.message = "原密码不能为空！";
				return failing;
			}
			if (string.IsNullOrEmpty(newPwd))
			{
				failing.message = "新密码不能为空！";
				return failing;
			}
			pwd = EncryUtils.MD5(pwd);
			newPwd = EncryUtils.MD5(newPwd);
			userModel.Pass = pwd;
			userModel.UserType = 1;
			if (ubll.LoginIn(userModel) == null)
			{
				failing.message = "旧密码错误！";
				return failing;
			}
			failing.IsSuccess = (ubll.RestPwd1(newPwd, userModel.UserId) > 0);
			if (failing.IsSuccess)
			{
				failing.IsSuccess = true;
				failing.message = "密码修改成功！";
				ModelCommon.RemoveUserModel();
			}
			else
			{
				failing.message = "密码修改失败！";
			}
			return failing;
		}

		[HttpPost]
		public ActionResult UpdatePass2(string PhoneCode, string SmsKey, string newPwd)
		{
			ApiResult<string> failing = ApiResult<string>.Failing;
			UserBase userModel = ModelCommon.GetUserModel();
			string text = new RedisCache().Get<string>(userModel.MobilePhone + SmsKey);
			if (string.IsNullOrEmpty(text))
			{
				failing.message = "短信验证码不存在或已过期";
				return failing;
			}
			if (!text.ToUpper().Equals(PhoneCode.ToUpper()))
			{
				failing.message = "短信验证码不正确";
				return failing;
			}
			if (string.IsNullOrEmpty(newPwd))
			{
				failing.message = "二级密码不能为空！";
				return failing;
			}
			newPwd = EncryUtils.MD5(newPwd);
			failing.IsSuccess = (ubll.RestPwd2(newPwd, userModel.UserId) > 0);
			if (failing.IsSuccess)
			{
				failing.IsSuccess = true;
				failing.message = "二级密码修改成功！";
			}
			else
			{
				failing.message = "二级密码修改失败！";
			}
			return failing;
		}

		public ActionResult AuthPhone()
		{
			UserBase userModel = ModelCommon.GetUserModel();
			return View(userModel);
		}

		public ActionResult AuthPhoneIn(string Mobile, string SmsCode, string SmsKey)
		{
			ApiResult<string> failing = ApiResult<string>.Failing;
			string text = new RedisCache().Get<string>(user.MobilePhone + SmsKey);
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
			UserBase userModel = ModelCommon.GetUserModel();
			userModel.IsMobilePhone = 1;
			userModel.MobilePhone = Mobile;
			bool flag = ubll.AuthMobile(userModel) > 0;
			if (flag)
			{
				failing.IsSuccess = flag;
				failing.message = "手机认证成功！";
				ModelCommon.WriteUserModel(userModel);
			}
			else
			{
				failing.message = "手机认证失败！";
			}
			return failing;
		}

		public ActionResult AuthCompany()
		{
			UserBaseInfo modelForId = ubll.GetModelForId(user.UserId);
			return View(modelForId);
		}

		public ActionResult AuthIdCard()
		{
			UserBaseInfo modelForId = ubll.GetModelForId(user.UserId);
			return View(modelForId);
		}

		public ActionResult ApiDown()
		{
			UserBaseInfo modelForId = ubll.GetModelForId(user.UserId);
			return View(modelForId);
		}

		public ActionResult UserWithdrawIndex()
		{
			UserBaseInfo modelForId = ubll.GetModelForId(user.UserId);
			return View(modelForId);
		}

		public ActionResult UpUserWithdrawInfo(UserDetail model)
		{
			ApiResult<string> failing = ApiResult<string>.Failing;
			UserBaseInfo modelForId = ubll.GetModelForId(user.UserId);
			if (modelForId.IdCardStatus != 1)
			{
				failing.IsSuccess = false;
				failing.message = "请先进行资质认证！";
				return failing;
			}
			UserDetail userDetail = ubll.GetUserDetail(user.UserId);
			model.UserId = user.UserId;
			model.WithdrawAccountType = modelForId.AccountType;
			model.WithdrawFactName = ((modelForId.AccountType == 0) ? userDetail.FactName : userDetail.CompanyName);
			if (ubll.UpUserWithdrawInfo(model) > 0)
			{
				failing.IsSuccess = true;
				failing.message = "提交成功！";
			}
			else
			{
				failing.IsSuccess = false;
				failing.message = "提交失败！";
			}
			return failing;
		}

		public ActionResult UpIdcardInfo(UserDetail model)
		{
			ApiResult<string> failing = ApiResult<string>.Failing;
			model.UserId = user.UserId;
			if (ubll.UpIdcardCommpanyInfo(model) > 0)
			{
				failing.IsSuccess = true;
				failing.message = "提交成功！";
			}
			else
			{
				failing.IsSuccess = false;
				failing.message = "提交失败！";
			}
			return failing;
		}

		public ActionResult UpIdcardCommpanyInfo(UserDetail model)
		{
			ApiResult<string> failing = ApiResult<string>.Failing;
			model.UserId = user.UserId;
			if (ubll.UpIdcardCommpanyInfo(model) > 0)
			{
				failing.IsSuccess = true;
				failing.message = "提交成功！";
			}
			else
			{
				failing.IsSuccess = false;
				failing.message = "提交失败！";
			}
			return failing;
		}

		public ActionResult PayAmt()
		{
			ApiResult<string> failing = ApiResult<string>.Failing;
			UserBaseInfo modelForId = ubll.GetModelForId(user.UserId);
			string text = "V2.0";
			string str = new SysConfigBll().GetConfigVaule("ApiUrl") + "/pay/gateway";
			string userId = modelForId.UserId;
			string apiKey = modelForId.ApiKey;
			decimal requestToDecimal = Utils.GetRequestToDecimal("total_amount");
			string text2 = Guid.NewGuid().ToString().Substring(0, 20)
				.Replace("-", "");
			string request = Utils.GetRequest("trade_type");
			string text3 = "余额充值";
			string text4 = new SysConfigBll().GetConfigVaule("WebUrl") + "/center/payreturn";
			string text5 = new SysConfigBll().GetConfigVaule("WebUrl") + "/center/paynotity";
			string clientIp = Utils.GetClientIp();
			requestToDecimal *= 100m;
			SortedDictionary<string, string> obj = new SortedDictionary<string, string>
			{
				{
					"app_id",
					userId
				},
				{
					"trade_type",
					request
				},
				{
					"total_amount",
					requestToDecimal.ToString("0")
				},
				{
					"out_trade_no",
					text2
				},
				{
					"notify_url",
					text5
				}
			};
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, string> item in obj)
			{
				stringBuilder.AppendFormat("&{0}={1}", item.Key, item.Value);
			}
			stringBuilder.Append(apiKey);
			stringBuilder.Remove(0, 1);
			string text6 = EncryUtils.MD5(stringBuilder.ToString()).ToLower();
			string text7 = string.Format(str + "?app_id={0}&trade_type={1}&total_amount={2}&out_trade_no={3}&notify_url={4}&return_url={5}&client_ip={6}&extra_return_param={7}&sign={8}&interface_version={9}", userId, request, requestToDecimal.ToString("0"), text2, text5, text4, clientIp, text3, text6, text);
			failing.IsSuccess = true;
			failing.data = text7;
			return new RedirectResult(text7);
		}

		[Auth(NoLogin = true)]
		public ActionResult PayNotity()
		{
			return Content("SUCCESS");
		}

		[Auth(NoLogin = true)]
		public ActionResult PayReturn()
		{
			return Content("支付成功");
		}
	}
}
