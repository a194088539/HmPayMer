using HM.Framework;
using HM.Framework.Caching;
using HM.Framework.Execl;
using HmPMer.AgentUI.Models;
using HmPMer.Business;
using HmPMer.Entity;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HmPMer.AgentUI.Controllers
{
	public class WithdrawController : Controller
	{
		private AccountBll bll = new AccountBll();

		private UserBaseBll ubll = new UserBaseBll();

		private UserBase user = ModelCommon.GetUserModel();

		private WithdrawBll withdrawBll = new WithdrawBll();

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
			amount *= 100m;
			string userId = ModelCommon.GetUserModel().UserId;
			UserBaseInfo modelForId = ubll.GetModelForId(userId);
			if (string.IsNullOrEmpty(modelForId.WithdrawSchemeId))
			{
				failing.IsSuccess = false;
				failing.message = "商户未设置提现方案，无法提现。";
				return failing;
			}
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
			if (string.IsNullOrEmpty(Pass2))
			{
				failing.message = "提现密码不能为空!";
				return failing;
			}
			if (model == null)
			{
				failing.message = "";
			}
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

		public ActionResult OrderIndex()
		{
			return View();
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
