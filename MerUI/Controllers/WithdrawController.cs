using HM.Framework;
using HM.Framework.Caching;
using HM.Framework.Execl;
using HmPMer.Business;
using HmPMer.Entity;
using HmPMer.MerUI.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HmPMer.MerUI.Controllers
{
	public class WithdrawController : Controller
	{
		private AccountBll bll = new AccountBll();

		private UserBaseBll ubll = new UserBaseBll();

		private UserBase user = ModelCommon.GetUserModel();

		private WithdrawBll withdrawBll = new WithdrawBll();

		private UserBankBll userbankBll = new UserBankBll();

		public PartialViewResult SettleApplyUi()
		{
			return PartialView();
		}

		public ActionResult WithdrawApply()
        {
            UserBaseInfo modelForId = ubll.GetModelForId(user.UserId);
			if (modelForId.AgentPay == 0)
			{
				return View();
			}
			return View(modelForId);
		}

		public ActionResult SettleApply()
		{
			UserBaseInfo modelForId = ubll.GetModelForId(user.UserId);
			return View(modelForId);
		}

		public ApiResult<decimal> GetWithdrawHanding(decimal amount)
		{
			ApiResult<decimal> failing = ApiResult<decimal>.Failing;
			failing.IsSuccess = true;
			if (amount == decimal.Zero)
			{
				failing.data = decimal.Zero;
				return failing;
			}
			string userId = ModelCommon.GetUserModel().UserId;
			UserBaseInfo modelForId = ubll.GetModelForId(userId);
			if (string.IsNullOrEmpty(modelForId.WithdrawSchemeId))
			{
				failing.IsSuccess = false;
				failing.message = "商户未设置提现方案，无法提现。";
				return failing;
			}
			amount *= 100m;
			WithdrawScheme withdrawScheme = new WithdrawBll().WithdrawSchemeGetModel(modelForId.WithdrawSchemeId);
			if (withdrawScheme == null)
			{
				failing.IsSuccess = false;
				failing.message = "结算方案不存在！";
				return failing;
			}
			decimal d = amount * withdrawScheme.HandingRateSingle;
			if (withdrawScheme.IsMinHandingSingle > 0 && d < withdrawScheme.MinHandingSingle)
			{
				d = withdrawScheme.MinHandingSingle;
			}
			if (withdrawScheme.IsMaxHandingSingle > 0 && d > withdrawScheme.MaxHandingSingle)
			{
				d = withdrawScheme.MaxHandingSingle;
			}
			failing.data = d / 100m;
			return failing;
		}

		public ActionResult SettleApplyIn(string Pass2, string PhoneCode, WithdrawOrder model)
		{
			ApiResult<string> failing = ApiResult<string>.Failing;
			if (model == null)
			{
				failing.message = "数据错误";
				return failing;
			}
			if (string.IsNullOrEmpty(Pass2))
			{
				failing.message = "提现密码不能为空!";
				return failing;
			}
			//if (string.IsNullOrEmpty(PhoneCode))
			//{
			//	failing.message = "短信验证码不能为空!";
			//	return failing;
			//}
			if (model.WithdrawAmt <= decimal.Zero)
			{
				failing.message = "请填写提现金额";
				return failing;
			}
			UserBaseInfo modelForId = ubll.GetModelForId(user.UserId);
			if (string.IsNullOrEmpty(modelForId.Pass2))
			{
				failing.message = "未设置二级密码，不能提现";
				return failing;
			}
			if (!modelForId.Pass2.Equals(EncryUtils.MD5(Pass2)))
			{
				failing.message = "提现密码不正确";
				return failing;
			}
			//string text = new RedisCache().Get<string>(modelForId.MobilePhone + "smsagentsettleapply");
			//if (string.IsNullOrEmpty(text))
			//{
			//	failing.message = "短信验证码不存在或已过期";
			//	return failing;
			//}
			//if (!text.ToUpper().Equals(PhoneCode.ToUpper()))
			//{
			//	failing.message = "短信验证码不正确";
			//	return failing;
			//}
			model.UserId = user.UserId;
			model.OrderType = 1;
			UserDetail userDetail = ubll.GetUserDetail(user.UserId);
			model.WithdrawChannelCode = userDetail.WithdrawChannelCode;
			model.ProvinceId = userDetail.WithdrawProvinceId;
			model.ProvinceName = userDetail.WithdrawProvince;
			model.CityId = userDetail.WithdrawCityId;
			model.CityName = userDetail.WithdrawCity;
			model.AccountType = userDetail.WithdrawAccountType;
			model.FactName = userDetail.WithdrawFactName;
			model.BankCode = userDetail.WithdrawBankCode;
			model.BankName = userDetail.WithdrawBank;
			model.BankAddress = userDetail.WithdrawBankBranch;
			model.BankLasalleCode = userDetail.WithdrawBankLasalleCode;
			model.ReservedPhone = userDetail.WithdrawReservedPhone;
			model.WithdrawAmt *= 100m;
			model.Handing *= 100m;
			model.Amt = model.WithdrawAmt + model.Handing;
			model.AddTime = DateTime.Now;
			model.UpdateTime = DateTime.Now;
			Tuple<int, string> tuple = withdrawBll.WithdrawApply(modelForId.WithdrawSchemeId, model);
			if (tuple.Item1 != 0)
			{
				failing.message = tuple.Item2;
			}
			else
			{
				failing.IsSuccess = true;
				failing.message = tuple.Item2;
			}
			return failing;
		}

		public ActionResult WithdrawApplyIn(string Pass2, string PhoneCode, WithdrawOrder model)
		{
			ApiResult<string> failing = ApiResult<string>.Failing;
			if (model == null)
			{
				failing.message = "数据错误";
				return failing;
			}
			if (string.IsNullOrEmpty(Pass2))
			{
				failing.message = "提现密码不能为空!";
				return failing;
			}
			//if (string.IsNullOrEmpty(PhoneCode))
			//{
			//	failing.message = "短信验证码不能为空!";
			//	return failing;
			//}
			if (string.IsNullOrEmpty(model.WithdrawChannelCode))
			{
				failing.message = "请选择支付账户";
				return failing;
			}
			if (string.IsNullOrEmpty(model.BankCode))
			{
				failing.message = "收款账户不能为空";
				return failing;
			}
			if (string.IsNullOrEmpty(model.FactName))
			{
				failing.message = "收款人不能为空";
				return failing;
			}
			if (model.WithdrawAmt <= decimal.Zero)
			{
				failing.message = "请填写提现金额";
				return failing;
			}
			string userId = ModelCommon.GetUserModel().UserId;
			UserBaseInfo modelForId = ubll.GetModelForId(userId);
			if (string.IsNullOrEmpty(modelForId.Pass2))
			{
				failing.message = "未设置二级密码，不能提现";
				return failing;
			}
			if (!modelForId.Pass2.Equals(EncryUtils.MD5(Pass2)))
			{
				failing.message = "提现密码不正确";
				return failing;
			}

            var isSmsCheck = new SysConfigBll().GetForKey("IsSmsCheck");
            if (isSmsCheck.Value.Equals("1"))
            {
                string text = new RedisCache().Get<string>(modelForId.MobilePhone + "smsagentwithdrawapply");
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
            }

            //string text = new RedisCache().Get<string>(modelForId.MobilePhone + "smsagentwithdrawapply");
            //if (string.IsNullOrEmpty(text))
            //{
            //	failing.message = "短信验证码不存在或已过期";
            //	return failing;
            //}
            //if (!text.ToUpper().Equals(PhoneCode.ToUpper()))
            //{
            //	failing.message = "短信验证码不正确";
            //	return failing;
            //}
            model.UserId = modelForId.UserId;
			model.OrderType = 2;
			model.WithdrawAmt *= 100m;
			model.Handing *= 100m;
			model.Amt = model.WithdrawAmt + model.Handing;
			model.AddTime = DateTime.Now;
			model.UpdateTime = DateTime.Now;
			Tuple<int, string> tuple = withdrawBll.WithdrawApply(modelForId.WithdrawSchemeId, model);
			if (tuple.Item1 != 0)
			{
				failing.message = tuple.Item2;
			}
			else
			{
				failing.IsSuccess = true;
				failing.message = tuple.Item2;
				UsersPayBank usersPayBank = new UsersPayBank();
				usersPayBank.UserId = ModelCommon.GetUserModel().UserId;
				usersPayBank.AccountType = model.AccountType;
				usersPayBank.BankCode = model.BankCode;
				usersPayBank.FactName = model.FactName;
				usersPayBank.WithdrawChannelCode = model.WithdrawChannelCode;
				usersPayBank.BankName = model.BankName;
				usersPayBank.ProvinceId = model.ProvinceId;
				usersPayBank.ProvinceName = model.ProvinceName;
				usersPayBank.CityId = model.CityId;
				usersPayBank.CityName = model.CityName;
				usersPayBank.BankLasalleCode = model.BankLasalleCode;
				usersPayBank.ReservedPhone = model.ReservedPhone;
				usersPayBank.BankAddress = model.BankAddress;
				usersPayBank.Addtime = DateTime.Now;
				UsersPayBank bankForCode = userbankBll.GetBankForCode(model.BankCode);
				if (bankForCode == null)
				{
					usersPayBank.Id = Guid.NewGuid().ToString();
					userbankBll.AddUserBank(usersPayBank);
				}
				else
				{
					usersPayBank.Id = bankForCode.Id;
					userbankBll.UpdateUserBank(usersPayBank);
				}
			}
			return failing;
		}

		public ActionResult UserBankIndex()
		{
			UserBaseInfo modelForId = ubll.GetModelForId(user.UserId);
			return View(modelForId);
		}

		public ActionResult UserBankList(int? pageIndex, int? pageSize, UsersPayBank param)
		{
			Paging paging = new Paging
			{
				PageIndex = ((!pageIndex.HasValue) ? 1 : pageIndex.Value),
				PageSize = (pageSize.HasValue ? pageSize.Value : 15)
			};
			param.UserId = user.UserId;
			new WithdrawBll();
			List<UsersPayBank> model = userbankBll.LoadUserBankPage(param, ref paging);
			base.ViewData["PageSize"] = paging.PageSize;
			base.ViewData["TotalCount"] = paging.TotalCount;
			base.ViewData["PageCount"] = paging.PageCount;
			base.ViewData["page"] = new PageInfo().createAjaxPageControl("page", paging.PageSize, paging.PageIndex, paging.TotalCount);
			return View(model);
		}

		[HttpPost]
		public ApiResult<string> RemoveUserBank(string id)
		{
			ApiResult<string> failing = ApiResult<string>.Failing;
			if (string.IsNullOrEmpty(id))
			{
				failing.message = "账户不正确!";
			}
			failing.IsSuccess = userbankBll.RemoveUserBank(id);
			if (failing.IsSuccess)
			{
				failing.message = "删除成功";
			}
			else
			{
				failing.message = "删除失败";
			}
			return failing;
		}

		public ActionResult AddUserBank()
		{
			return View();
		}

		public ActionResult GetBankForCode(string BankCode)
		{
			UsersPayBank bankForCode = userbankBll.GetBankForCode(BankCode);
			return Json(bankForCode);
		}

		public ActionResult OrderIndex()
		{
			UserBaseInfo modelForId = ubll.GetModelForId(user.UserId);
			return View(modelForId);
		}

		public ActionResult OrderList(int? pageIndex, int? pageSize, WithdrawOrderQueryParam param)
		{
			Paging paging = new Paging
			{
				PageIndex = ((!pageIndex.HasValue) ? 1 : pageIndex.Value),
				PageSize = (pageSize.HasValue ? pageSize.Value : 15)
			};
			param.UserId = user.UserId;
			List<WithdrawOrderInfo> withdrawOrderPageListUi = new WithdrawBll().GetWithdrawOrderPageListUi(param, ref paging);
			if (withdrawOrderPageListUi != null && withdrawOrderPageListUi.Count > 0)
			{
				withdrawOrderPageListUi.ForEach(delegate(WithdrawOrderInfo p)
				{
					p.BankCode = Utils.MaskBankCode(p.BankCode);
				});
			}
			base.ViewData["PageSize"] = paging.PageSize;
			base.ViewData["TotalCount"] = paging.TotalCount;
			base.ViewData["PageCount"] = paging.PageCount;
			base.ViewData["page"] = new PageInfo().createAjaxPageControl("page", paging.PageSize, paging.PageIndex, paging.TotalCount);
			return View(withdrawOrderPageListUi);
		}

		public void OrderTarnImprotExcel(WithdrawOrderQueryParam param)
		{
			try
			{
				WithdrawBll obj = new WithdrawBll();
				param.UserId = user.UserId;
				ExcelHelper.ExportDataTableToExcel(obj.GetWithdrawOrderUiTable(param), "清算记录(" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ").xls", "清算记录");
			}
			catch (Exception)
			{
			}
		}
	}
}
