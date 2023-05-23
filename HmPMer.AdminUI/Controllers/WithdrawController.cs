using HM.Framework;
using HM.Framework.Execl;
using HmPMer.AdminUI.Fillters;
using HmPMer.AdminUI.Models;
using HmPMer.Business;
using HmPMer.Entity;
using HmPMer.WebAdminUI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;

namespace HmPMer.AdminUI.Controllers
{
	public class WithdrawController : Controller
	{
		private WithdrawBll bll = new WithdrawBll();

		[Auth(FlagStr = "WithdrawChannelIndex")]
		public ActionResult WithdrawChannelIndex()
		{
			return View();
		}

		[Auth(FlagStr = "WithdrawChannelIndex")]
		public ActionResult WithdrawChannelAddUc()
		{
			return View();
		}

		[Auth(FlagStr = "WithdrawChannelIndex")]
		public ActionResult WithdrawChannelUpdateUc(string Id)
		{
			WithdrawChannelInfo model = bll.WithdrawChannelGetModel(Id);
			return View(model);
		}

		public ActionResult GetWithdrawChannelPageList(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			WithdrawChannelInfo parm = new WithdrawChannelInfo
			{
				Name = Utils.GetRequest("Name")
			};
			ResultPage<WithdrawChannelInfo> resultPage = new ResultPage<WithdrawChannelInfo>();
			resultPage.msg = "查询成功";
			resultPage.Item = bll.GetWithdrawChannelPageList(parm, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		[HttpPost]
		public ActionResult UpWCIsEnabled(string Id, int IsEnabled)
		{
			int num = bll.UpWCIsEnabled(Id, IsEnabled);
			ResultBase resultBase = new ResultBase();
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "修改失败！";
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult WithdrawChannelAdd(WithdrawChannel Model)
		{
			Model.Id = Guid.NewGuid().ToString();
			long num = bll.WithdrawChannelAdd(Model);
			ResultBase resultBase = new ResultBase();
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "新增失败！";
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult WithdrawChannelUpdate(WithdrawChannel Model)
		{
			bool flag = bll.WithdrawChannelUpdate(Model);
			ResultBase resultBase = new ResultBase();
			if (!flag)
			{
				resultBase.Success = false;
				resultBase.Message = "修改失败！";
			}
			return Json(resultBase);
		}

		[Auth(FlagStr = "WithdrawSchemeIndex")]
		public ActionResult WithdrawSchemeIndex()
		{
			return View();
		}

		[Auth(FlagStr = "WithdrawSchemeIndex")]
		public ActionResult WithdrawSchemeAddUc()
		{
			return View();
		}

		[Auth(FlagStr = "WithdrawSchemeIndex")]
		public ActionResult WithdrawSchemeUpdateUc(string Id)
		{
			WithdrawScheme model = bll.WithdrawSchemeGetModel(Id);
			return View(model);
		}

		public ActionResult GetWithdrawSchemePageList(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			WithdrawSchemeinfo parm = new WithdrawSchemeinfo
			{
				SchemeName = Utils.GetRequest("SchemeName")
			};
			ResultPage<WithdrawSchemeinfo> resultPage = new ResultPage<WithdrawSchemeinfo>();
			resultPage.msg = "查询成功";
			resultPage.Item = bll.GetWithdrawSchemePageList(parm, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		[HttpPost]
		public ActionResult WithdrawSchemeAdd(WithdrawScheme Model)
		{
			Model.Id = Guid.NewGuid().ToString();
			Model.MinAmtSingle *= 100m;
			Model.MaxAmtSingle *= 100m;
			Model.LimitAmtDay *= 100m;
			Model.HandingRateSingle /= 100m;
			Model.MinHandingSingle *= 100m;
			Model.MaxHandingSingle *= 100m;
			long num = bll.WithdrawSchemeAdd(Model);
			ResultBase resultBase = new ResultBase();
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "新增失败！";
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult WithdrawSchemeUpdate(WithdrawScheme Model)
		{
			Model.MinAmtSingle *= 100m;
			Model.MaxAmtSingle *= 100m;
			Model.LimitAmtDay *= 100m;
			Model.HandingRateSingle /= 100m;
			Model.MinHandingSingle *= 100m;
			Model.MaxHandingSingle *= 100m;
			bool flag = bll.WithdrawSchemeUpdate(Model);
			ResultBase resultBase = new ResultBase();
			if (!flag)
			{
				resultBase.Success = false;
				resultBase.Message = "修改失败！";
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult DelWithdrawScheme(string Id)
		{
			ResultBase resultBase = new ResultBase();
			int num = bll.DelWithdrawScheme(Id);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "操作失败！";
			}
			return Json(resultBase);
		}

		public ActionResult WithdrawOrderAuditIndex()
		{
			return View();
		}

		public ActionResult UpdateWithdrawOrder(string OrderId)
		{
			WithdrawOrder withdrawOrderModel = bll.GetWithdrawOrderModel(OrderId);
			return View(withdrawOrderModel);
		}

		public ActionResult WithdrawOrderAuditUc(string OrderId)
		{
			WithdrawOrder withdrawOrderModel = bll.GetWithdrawOrderModel(OrderId);
			return View(withdrawOrderModel);
		}

		public ActionResult GetWithdrawOrderPageList(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			WithdrawOrderQueryParam parm = new WithdrawOrderQueryParam
			{
				OrderType = Utils.GetRequestToInt("OrderType", -1),
				PayState = Utils.GetRequestToInt("PayState", -1),
				UserId = Utils.GetRequestQuery("UserId"),
				InterfaceCode = Utils.GetRequestQuery("InterfaceCode"),
				AddTimeBgin = Utils.StringToDateTime(Utils.GetRequest("BeginTime"), null),
				AddTimeEnd = Utils.StringToDateTime(Utils.GetRequest("EndTime"), null)
			};
			ResultPage<WithdrawOrderInfo> resultPage = new ResultPage<WithdrawOrderInfo>();
			resultPage.msg = "查询成功";
			resultPage.Item = bll.GetWithdrawOrderPageList(parm, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		public void OrderImprotExcel()
		{
			try
			{
				WithdrawOrderQueryParam parm = new WithdrawOrderQueryParam
				{
					OrderType = Utils.GetRequestToInt("OrderType", -1),
					PayState = Utils.GetRequestToInt("PayState", -1),
					UserId = Utils.GetRequestQuery("UserId"),
					InterfaceCode = Utils.GetRequestQuery("InterfaceCode"),
					AddTimeBgin = Utils.StringToDateTime(Utils.GetRequest("BeginTime"), null),
					AddTimeEnd = Utils.StringToDateTime(Utils.GetRequest("EndTime"), null)
				};
				DataTable withdrawOrderTable = bll.GetWithdrawOrderTable(parm);
				ExcelHelper.ExportDataTableToExcel(withdrawOrderTable, "结算订单记录(" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ").xls", "结算订单记录");
			}
			catch (Exception)
			{
			}
		}

		[HttpPost]
		public ActionResult UpdateInterfaceCode(string InterfaceCode, string OrderId, string TarnRemark)
		{
			ResultBase resultBase = new ResultBase();
			int num = bll.UpdateInterfaceCode(InterfaceCode, OrderId);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "操作失败！";
			}
			else
			{
				try
				{
					BehaviorLog behaviorLog = new BehaviorLog();
					behaviorLog.Id = Guid.NewGuid().ToString();
					behaviorLog.BlName = "提现订单付款";
					behaviorLog.BlType = 2;
					behaviorLog.parm = $"InterfaceCode={InterfaceCode}&OrderId={OrderId}";
					behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
					behaviorLog.addTime = DateTime.Now;
					new SystemBll().InserBehaviorLog(behaviorLog);
				}
				catch (Exception)
				{
				}
				WithdrawOrder withdrawOrderModel = bll.GetWithdrawOrderModel(OrderId);
				if (withdrawOrderModel.PayState != 0 && withdrawOrderModel.PayState != 2)
				{
					resultBase.Success = false;
					resultBase.Message = "订单已进行付款操作!";
					return Json(resultBase);
				}
				Tuple<int, string> tuple = bll.WithdrawOrderPayment(withdrawOrderModel, ModelCommon.GetUserModel().ID, TarnRemark);
				if (tuple.Item1 != 0)
				{
					resultBase.Success = false;
					resultBase.Message = "付款失败：" + tuple.Item2;
					return Json(resultBase);
				}
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult PointFailTrue(string OrderId)
		{
			ResultBase resultBase = new ResultBase();
			WithdrawOrder withdrawOrderModel = bll.GetWithdrawOrderModel(OrderId);
			if (withdrawOrderModel.PayState != 2)
			{
				resultBase.Success = false;
				resultBase.Message = "订单不是待确认状态!";
				return Json(resultBase);
			}
			resultBase.Success = bll.PointFailTrue(withdrawOrderModel, ModelCommon.GetUserModel().ID);
			if (resultBase.Success)
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "确认提现订单失败";
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"OrderId={OrderId}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult PointFailReturn(string OrderId)
		{
			ResultBase resultBase = new ResultBase();
			WithdrawOrder withdrawOrderModel = bll.GetWithdrawOrderModel(OrderId);
			if (withdrawOrderModel.PayState != 0)
			{
				resultBase.Success = false;
				resultBase.Message = "订单不是待支付状态!";
				return Json(resultBase);
			}
			resultBase.Success = bll.PointFailReturn(withdrawOrderModel, ModelCommon.GetUserModel().ID);
			if (resultBase.Success)
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "退回提现订单";
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"OrderId={OrderId}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[Auth(FlagStr = "WithdrawOrderPayIndex")]
		public ActionResult WithdrawOrderPayIndex()
		{
			return View();
		}

		[HttpPost]
		public ActionResult PayWithdrawSeartch(string OrderId)
		{
			ResultBase resultBase = new ResultBase();
			WithdrawOrder withdrawOrderModel = bll.GetWithdrawOrderModel(OrderId);
			Tuple<int, string> tuple = bll.WithdrawOrderTarnQuery(withdrawOrderModel, ModelCommon.GetUserModel().ID);
			if (tuple.Item1 != 0)
			{
				resultBase.Success = false;
				resultBase.Message = "查询失败：" + tuple.Item2;
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "查询提现订单";
				behaviorLog.BlType = 4;
				behaviorLog.parm = $"OrderId={OrderId}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[Auth(FlagStr = "WithdrawOrderTarnIndex")]
		public ActionResult WithdrawOrderTarnIndex()
		{
			return View();
		}

		public ActionResult GetWithdrawOrderTarnPageList(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			WithdrawOrderTarn parm = new WithdrawOrderTarn
			{
				UserId = Utils.GetRequestQuery("UserId"),
				ChannelOrderNo = Utils.GetRequestQuery("ChannelOrderNo"),
				FactName = Utils.GetRequestQuery("FactName"),
				BankCode = Utils.GetRequestQuery("BankCode"),
				OrderId = Utils.GetRequestQuery("OrderId"),
				InterfaceCode = Utils.GetRequestQuery("InterfaceCode"),
				TarnState = Utils.GetRequestToInt("TarnState", -1),
				BeginTime = Utils.StringToDateTime(Utils.GetRequest("BeginTime"), null),
				EndTime = Utils.StringToDateTime(Utils.GetRequest("EndTime"), null)
			};
			ResultPage<WithdrawOrderTarn> resultPage = new ResultPage<WithdrawOrderTarn>();
			resultPage.msg = "查询成功";
			resultPage.Item = bll.GetWithdrawOrderTarnPageList(parm, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		public void OrderTarnImprotExcel()
		{
			try
			{
				WithdrawOrderTarn parm = new WithdrawOrderTarn
				{
					UserId = Utils.GetRequestQuery("UserId"),
					ChannelOrderNo = Utils.GetRequestQuery("ChannelOrderNo"),
					FactName = Utils.GetRequestQuery("FactName"),
					BankCode = Utils.GetRequestQuery("BankCode"),
					OrderId = Utils.GetRequestQuery("OrderId"),
					InterfaceCode = Utils.GetRequestQuery("InterfaceCode"),
					TarnState = Utils.GetRequestToInt("TarnState", -1),
					BeginTime = Utils.StringToDateTime(Utils.GetRequest("BeginTime"), null),
					EndTime = Utils.StringToDateTime(Utils.GetRequest("EndTime"), null)
				};
				DataTable withdrawOrderTarnTable = bll.GetWithdrawOrderTarnTable(parm);
				ExcelHelper.ExportDataTableToExcel(withdrawOrderTarnTable, "清算记录(" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ").xls", "清算记录");
			}
			catch (Exception)
			{
			}
		}

		[Auth(FlagStr = "AccountSchemeIndex")]
		public ActionResult AccountSchemeIndex()
		{
			return View();
		}

		[Auth(FlagStr = "AccountSchemeIndex")]
		public ActionResult AddAccountScheme(string Id)
		{
			if (!string.IsNullOrEmpty(Id))
			{
				AccountScheme accountScheme = bll.GetAccountScheme(Id);
				return View(accountScheme);
			}
			return View();
		}

		[Auth(FlagStr = "AccountSchemeIndex")]
		public ActionResult AccountSchemeDetailList(string Id)
		{
			return View();
		}

		[Auth(FlagStr = "AccountSchemeIndex")]
		public ActionResult AddSchemeDetail(string Id)
		{
			long maxSchemeDetailEndtime = bll.GetMaxSchemeDetailEndtime(Id);
			string text = "00:00";
			if (maxSchemeDetailEndtime != 0L)
			{
				text = ((double)maxSchemeDetailEndtime / 100.0).ToString("0.00").Replace(".", ":");
			}
			base.ViewBag.Startimstr = text;
			return View();
		}

		[Auth(FlagStr = "AccountSchemeIndex")]
		public ActionResult SetAccountScheme(string Id)
		{
			List<AccountSchemeDetail> accountSchemeDetail = bll.GetAccountSchemeDetail(Id);
			return View(accountSchemeDetail);
		}

		public ActionResult GetAccountSchemeList(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			AccountScheme parm = new AccountScheme
			{
				name = Utils.GetRequest("name")
			};
			ResultPage<AccountScheme> resultPage = new ResultPage<AccountScheme>();
			resultPage.msg = "查询成功";
			resultPage.Item = bll.GetAccountSchemeList(parm, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		[HttpPost]
		public ActionResult AddAccountSchemeInfo(AccountScheme Model)
		{
			ResultBase resultBase = new ResultBase();
			long num = 0L;
			if (string.IsNullOrEmpty(Model.Id))
			{
				Model.Id = Guid.NewGuid().ToString();
				long maxAccountSchemeId = bll.GetMaxAccountSchemeId();
				if (maxAccountSchemeId == 0L)
				{
					Model.Id = "1000";
				}
				else
				{
					Model.Id = (maxAccountSchemeId + 1).ToString();
				}
				num = bll.AddAccountScheme(Model);
			}
			else
			{
				num = bll.UpdateAccountScheme(Model);
			}
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "操作失败！";
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult DelAccountScheme(string Id)
		{
			ResultBase resultBase = new ResultBase();
			int num = bll.DelAccountScheme(Id);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "操作失败！";
			}
			return Json(resultBase);
		}

		public ActionResult GetAccountSchemeDetailList(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			AccountSchemeDetail parm = new AccountSchemeDetail
			{
				AccountSchemeId = Utils.GetRequest("AccountSchemeId")
			};
			ResultPage<AccountSchemeDetailInfo> resultPage = new ResultPage<AccountSchemeDetailInfo>();
			resultPage.msg = "查询成功";
			resultPage.Item = bll.GetAccountSchemeDetailList(parm, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		[HttpPost]
		public ActionResult AddAccountSchemeDetail(AccountSchemeDetail Model)
		{
			ResultBase resultBase = new ResultBase();
			Model.Id = Guid.NewGuid().ToString();
			long num = bll.AddAccountSchemeDetail(Model);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "操作失败！";
			}
			return Json(resultBase);
		}

		[Auth(FlagStr = "AccountSchemeIndex")]
		public ActionResult UpdateAccountSchemeDetail(string Id)
		{
			AccountSchemeDetailInfo accountSchemeDetailInfo = bll.GetAccountSchemeDetailInfo(Id);
			return View(accountSchemeDetailInfo);
		}

		[HttpPost]
		public ActionResult UpdateAccountSchemeDetailInfo(AccountSchemeDetail Model)
		{
			ResultBase resultBase = new ResultBase();
			long num = bll.UpdateAccountSchemeDetail(Model);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "操作失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "编辑入账方案明细";
				behaviorLog.BlType = 2;
				behaviorLog.parm = Model.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult DelAccountSchemeDetail(string Id)
		{
			ResultBase resultBase = new ResultBase();
			try
			{
				long num = bll.DelAccountSchemeDetail(Id);
				if (num < 0)
				{
					resultBase.Success = false;
					resultBase.Message = "删除失败!";
				}
				else
				{
					BehaviorLog behaviorLog = new BehaviorLog();
					behaviorLog.Id = Guid.NewGuid().ToString();
					behaviorLog.BlName = "删除入账方案明细";
					behaviorLog.BlType = 1;
					behaviorLog.parm = $"Id={Id}";
					behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
					behaviorLog.addTime = DateTime.Now;
					new SystemBll().InserBehaviorLog(behaviorLog);
				}
			}
			catch (Exception ex)
			{
				resultBase.Success = false;
				resultBase.Message = "删除失败:" + ex.Message;
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult SetAccountSchemeDetail(List<AccountSchemeDetail> ListModel, string AccountSchemeId)
		{
			ResultBase resultBase = new ResultBase();
			try
			{
				long num = bll.SetAccountSchemeDetail(AccountSchemeId, ListModel);
				if (num < 0)
				{
					resultBase.Success = false;
					resultBase.Message = "修改失败!";
				}
				else
				{
					BehaviorLog behaviorLog = new BehaviorLog();
					behaviorLog.Id = Guid.NewGuid().ToString();
					behaviorLog.BlName = "设置入账方案明细";
					behaviorLog.BlType = 1;
					behaviorLog.parm = $"ListModel={ListModel.ToJson()}&AccountSchemeId={AccountSchemeId}";
					behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
					behaviorLog.addTime = DateTime.Now;
					new SystemBll().InserBehaviorLog(behaviorLog);
				}
			}
			catch (Exception ex)
			{
				resultBase.Success = false;
				resultBase.Message = "修改失败:" + ex.Message;
			}
			return Json(resultBase);
		}

		[Auth(FlagStr = "WithdrawUserIndex")]
		public ActionResult WithdrawUserIndex()
		{
			return View();
		}

		[HttpPost]
		public ActionResult SettleWithdraw(string idstr)
		{
			ResultBase resultBase = new ResultBase();
			string configVaule = new SysConfigBll().GetConfigVaule("WithdrawMinAmtSingle");
			string configVaule2 = new SysConfigBll().GetConfigVaule("WithdrawMaxAmtSingle");
			string configVaule3 = new SysConfigBll().GetConfigVaule("WithdrawHandingSingle");
			string configVaule4 = new SysConfigBll().GetConfigVaule("WithdrawInterfaceCode");
			decimal d = string.IsNullOrEmpty(configVaule) ? decimal.Zero : (Convert.ToDecimal(configVaule) * 100m);
			decimal num = string.IsNullOrEmpty(configVaule2) ? decimal.Zero : (Convert.ToDecimal(configVaule2) * 100m);
			decimal num2 = string.IsNullOrEmpty(configVaule3) ? decimal.Zero : (Convert.ToDecimal(configVaule3) * 100m);
			string[] array = idstr.Split(',');
			foreach (string userId in array)
			{
				try
				{
					UserBaseInfo modelForId = new UserBaseBll().GetModelForId(userId);
					decimal balance = modelForId.Balance;
					WithdrawOrder withdrawOrder = new WithdrawOrder();
					withdrawOrder.OrderType = 3;
					withdrawOrder.UserId = userId;
					if (!((decimal)(int)balance < d))
					{
						if (num == decimal.Zero)
						{
							num = balance;
						}
						if ((decimal)(int)balance > d && (decimal)(int)balance < num)
						{
							withdrawOrder.Amt = balance;
							withdrawOrder.WithdrawAmt = balance - num2;
							withdrawOrder.Handing = num2;
						}
						if ((decimal)(int)balance >= num)
						{
							withdrawOrder.Amt = num;
							withdrawOrder.WithdrawAmt = num - num2;
							withdrawOrder.Handing = num2;
						}
						UserDetail userDetail = new UserBaseBll().GetUserDetail(userId);
						withdrawOrder.WithdrawChannelCode = userDetail.WithdrawChannelCode;
						withdrawOrder.ProvinceId = userDetail.WithdrawProvinceId;
						withdrawOrder.ProvinceName = userDetail.WithdrawProvince;
						withdrawOrder.CityId = userDetail.WithdrawCityId;
						withdrawOrder.CityName = userDetail.WithdrawCity;
						withdrawOrder.AccountType = userDetail.WithdrawAccountType;
						withdrawOrder.FactName = userDetail.WithdrawFactName;
						withdrawOrder.BankCode = userDetail.WithdrawBankCode;
						withdrawOrder.BankName = userDetail.WithdrawBank;
						withdrawOrder.BankAddress = userDetail.WithdrawBankBranch;
						withdrawOrder.BankLasalleCode = userDetail.WithdrawBankLasalleCode;
						withdrawOrder.ReservedPhone = userDetail.WithdrawReservedPhone;
						withdrawOrder.InterfaceCode = configVaule4;
						withdrawOrder.AddTime = DateTime.Now;
						withdrawOrder.UpdateTime = DateTime.Now;
						withdrawOrder.AuditDesc = "系统人工清算";
						long num3 = bll.WithdrawOrderAdd(withdrawOrder);
						if (num3 > 0)
						{
							Trade trade = new Trade();
							trade.BillNo = withdrawOrder.OrderId;
							trade.TradeId = new UserBaseBll().GetTradeId();
							trade.UserId = withdrawOrder.UserId;
							trade.Type = 0;
							trade.BillType = 5;
							trade.TradeTime = DateTime.Now;
							trade.BeforeAmount = modelForId.Balance;
							trade.Amount = withdrawOrder.Amt;
							trade.Balance = modelForId.Balance - withdrawOrder.Amt;
							trade.AddUser = ModelCommon.GetUserModel().AdmUser;
							trade.Remark = "系统管理员[" + ModelCommon.GetUserModel().AdmUser + "]操作,提现金额：" + withdrawOrder.WithdrawAmt / 100m + " 手续费：" + withdrawOrder.Handing / 100m;
							new UserBaseBll().AddTrade(trade);
							BehaviorLog behaviorLog = new BehaviorLog();
							behaviorLog.Id = Guid.NewGuid().ToString();
							behaviorLog.BlName = "系统余额清算";
							behaviorLog.BlType = 1;
							behaviorLog.parm = $"idstr={idstr}";
							behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
							behaviorLog.addTime = DateTime.Now;
							new SystemBll().InserBehaviorLog(behaviorLog);
						}
					}
				}
				catch (Exception)
				{
				}
			}
			resultBase.Success = true;
			resultBase.Message = "操作成功";
			return Json(resultBase);
		}

		public ActionResult LoadWithdrawUser(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			UserBaseInfo userBaseInfo = new UserBaseInfo
			{
				UserId = Utils.GetRequest("UserId"),
				MerName = Utils.GetRequest("MerName"),
				MobilePhone = Utils.GetRequest("MobilePhone"),
				Email = Utils.GetRequest("Email"),
				AgentId = Utils.GetRequest("AgentId")
			};
			string configVaule = new SysConfigBll().GetConfigVaule("WithdrawMinAmtSingle");
			if (string.IsNullOrEmpty(configVaule))
			{
				userBaseInfo.Balance = decimal.Zero;
			}
			else
			{
				userBaseInfo.Balance = Convert.ToDecimal(configVaule) * 100m;
			}
			ResultPage<WithrawUserBaseInfo> resultPage = new ResultPage<WithrawUserBaseInfo>();
			resultPage.msg = "查询成功";
			resultPage.Item = bll.LoadWithdrawUser(userBaseInfo, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		[Auth(FlagStr = "OrderAccountTarnIndex")]
		public ActionResult OrderAccountTarnIndex()
		{
			return View();
		}

		[Auth(FlagStr = "OrderAccountTarnIndex")]
		public ActionResult OrderAccountIndex()
		{
			return View();
		}

		public ActionResult GetOrderAccountList(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			OrderAccountQueryParam param = new OrderAccountQueryParam
			{
				UserId = Utils.GetRequest("UserId"),
				AddTimeBgin = Utils.StringToDateTime(Utils.GetRequest("BeginTime"), null),
				AddTimeEnd = Utils.StringToDateTime(Utils.GetRequest("EndTime"), null),
				AccountTimeBegin = Utils.StringToDateTime(Utils.GetRequest("AccountTimeBegin"), null),
				AccountTimeEnd = Utils.StringToDateTime(Utils.GetRequest("AccountTimeEnd"), null),
				AccountState = Utils.GetRequestToInt("AccountState", -1)
			};
			ResultPage<OrderAccount> resultPage = new ResultPage<OrderAccount>();
			resultPage.msg = "查询成功";
			resultPage.Item = bll.GetOrderAccountList(param, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		public ActionResult GetOrderAccountTarnList(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			OrderAccountTarn param = new OrderAccountTarn
			{
				UserId = Utils.GetRequest("UserId"),
				AccountDate = DateTime.Now
			};
			List<OrderAccountTarn> orderAccountTarnList = bll.GetOrderAccountTarnList(param, ref paging);
			orderAccountTarnList?.ForEach(delegate(OrderAccountTarn p)
			{
				p.Balance /= 100m;
				p.Amt /= 100m;
				p.UnBalance /= 100m;
			});
			ResultPage<OrderAccountTarn> resultPage = new ResultPage<OrderAccountTarn>();
			resultPage.msg = "查询成功";
			resultPage.Item = orderAccountTarnList;
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		[HttpPost]
		public ActionResult ConfirmOrderAccountTarnBat(List<OrderAccountTarn> list)
		{
			ResultBase resultBase = new ResultBase();
			try
			{
				HmAdmin userModel = ModelCommon.GetUserModel();
				DateTime now = DateTime.Now;
				OrderAccount model = new OrderAccount
				{
					AccountTime = now.Date.AddDays(1.0).AddMilliseconds(-1.0),
					EndTime = now,
					AccountState = 1,
					AdminId = userModel.ID
				};
				if (!(resultBase.Success = bll.ConfirmOrderAccountTarnBat(list, model)))
				{
					resultBase.Message = "入账失败！";
				}
				else
				{
					BehaviorLog behaviorLog = new BehaviorLog();
					behaviorLog.Id = Guid.NewGuid().ToString();
					behaviorLog.BlName = "确认入账";
					behaviorLog.BlType = 2;
					behaviorLog.parm = list.ToJson();
					behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
					behaviorLog.addTime = DateTime.Now;
					new SystemBll().InserBehaviorLog(behaviorLog);
				}
			}
			catch (Exception ex)
			{
				resultBase.Success = false;
				resultBase.Message = "修改失败:" + ex.Message;
			}
			return Json(resultBase);
		}

		public ActionResult GetTxOrder()
		{
			ResultBase resultBase = new ResultBase();
			try
			{
				if (new MenuRoleBll().CheckFlagStr(ModelCommon.GetUserModel().ID, "WithdrawOrderPayIndex"))
				{
					WithdrawOrder txOrder = bll.GetTxOrder();
					if (txOrder != null)
					{
						resultBase.Success = true;
					}
					else
					{
						resultBase.Success = false;
					}
				}
				else
				{
					resultBase.Success = false;
				}
			}
			catch (Exception ex)
			{
				resultBase.Success = false;
				resultBase.Message = "右下角自动提现提醒异常:" + ex.Message;
			}
			return Json(resultBase, JsonRequestBehavior.AllowGet);
		}
	}
}
