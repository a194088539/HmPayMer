using HM.Framework;
using HM.Framework.Caching;
using HmPMer.AgentUI.Models;
using HmPMer.Business;
using HmPMer.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace HmPMer.AgentUI.Controllers
{
	public class CenterController : Controller
	{
		private AccountBll bll = new AccountBll();

		private UserBaseBll ubll = new UserBaseBll();

		private UserBase user = ModelCommon.GetUserModel();

		public ActionResult Index()
		{
			UserBaseInfo modelForId = ubll.GetModelForId(user.UserId);
			PayBll payBll = new PayBll();
			List<PayRateInfo> list = new RateBll().GetPayRateList(user.UserId, 4).FindAll((PayRateInfo p) => p.IsEnabled == 1);
			List<PayRateInfo> list2 = new RateBll().GetPayRateList(user.GradeId).FindAll((PayRateInfo p) => p.IsEnabled == 1);
			List<PayType> interfaceType = payBll.GetInterfaceType(user.UserId, 4);
			Dictionary<string, PayRateInfo> dictionary = new Dictionary<string, PayRateInfo>();
			foreach (PayRateInfo item3 in list2)
			{
				if (interfaceType.FindIndex((PayType p) => p.PayCode.Equals(item3.PayCode)) >= 0)
				{
					dictionary[item3.PayCode] = item3;
				}
			}
			foreach (PayRateInfo item4 in list)
			{
				if (interfaceType.FindIndex((PayType p) => p.PayCode.Equals(item4.PayCode)) >= 0)
				{
					dictionary[item4.PayCode] = item4;
				}
			}
			base.ViewBag.RateList = dictionary.Values.ToList();
			return View(modelForId);
		}

		public ActionResult UpdateInfo()
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
			if (ubll.LoginIn(userModel) == null)
			{
				failing.message = "旧密码错误！";
				return failing;
			}
			failing.IsSuccess = (ubll.RestPwd1(newPwd, userModel.UserId) > 0);
			if (failing.IsSuccess)
			{
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
			UserBaseInfo modelForId = ubll.GetModelForId(user.UserId);
			return View(modelForId);
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
			new AccountBll();
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
	}
}
