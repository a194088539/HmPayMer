using HM.Framework;
using HmPMer.AdminUI.Fillters;
using HmPMer.AdminUI.Models;
using HmPMer.Business;
using HmPMer.Entity;
using HmPMer.WebAdminUI.Models;
using System;
using System.Web.Mvc;

namespace HmPMer.AdminUI.Controllers
{
	public class DataManageController : Controller
	{
		private UserBaseBll bll = new UserBaseBll();

		[Auth(FlagStr = "BalanceIndex")]
		public ActionResult AddBalance()
		{
			return View();
		}

		[Auth(FlagStr = "BalanceIndex")]
		public ActionResult BalanceIndex()
		{
			return View();
		}

		[Auth(FlagStr = "TradeList")]
		public ActionResult TradeList()
		{
			return View();
		}

		public ActionResult BalanceTradeList()
		{
			return View();
		}

		[HttpPost]
		public ActionResult AddBalanceUser(decimal Balance, string UserId, int Type)
		{
			ResultBase resultBase = new ResultBase();
			UserBaseInfo modelForId = bll.GetModelForId(UserId);
			if (modelForId == null)
			{
				resultBase.Success = false;
				resultBase.Message = "商户不存在！";
				return Json(resultBase);
			}
			if (Type == 0)
			{
				if (Balance * 100m > modelForId.Balance)
				{
					resultBase.Success = false;
					resultBase.Message = "原有金额小于所减金额！";
					return Json(resultBase);
				}
				Balance *= decimal.MinusOne;
			}
			int num = bll.AddBalanceUser(Balance * 100m, UserId);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "修改失败！";
			}
			else
			{
				Trade trade = new Trade();
				trade.TradeId = bll.GetTradeId();
				trade.UserId = modelForId.UserId;
				trade.Type = Type;
				trade.BillType = 1;
				trade.TradeTime = DateTime.Now;
				trade.BeforeAmount = modelForId.Balance;
				trade.Amount = ((Balance > decimal.Zero) ? (Balance * 100m) : (Balance * -100m));
				trade.Balance = modelForId.Balance + Balance * 100m;
				trade.AddUser = ModelCommon.GetUserModel().AdmUser;
				trade.Remark = "系统管理员[" + ModelCommon.GetUserModel().AdmUser + "]操作";
				bll.AddTrade(trade);
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = ((Balance > decimal.Zero) ? "增加商户余额" : "减少商户余额");
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"Balance={Balance}&UserId={UserId}&Type={Type}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		public ActionResult GetTradeList(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			Trade param = new Trade
			{
				UserId = Utils.GetRequest("UserId"),
				BillType = Utils.GetRequestQueryToInt("BillType", -1),
				BeginTime = Utils.StringToDateTime(Utils.GetRequest("BeginTime"), null),
				EndTime = Utils.StringToDateTime(Utils.GetRequest("EndTime"), null)
			};
			ResultPage<Trade> resultPage = new ResultPage<Trade>();
			resultPage.msg = "查询成功";
			resultPage.Item = bll.GetTradeList(param, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		[Auth(FlagStr = "BehaviorLog")]
		public ActionResult BehaviorLog()
		{
			return View();
		}

		public ActionResult GetBehaviorLogList(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			BehaviorLog parm = new BehaviorLog
			{
				BlName = Utils.GetRequest("BlName"),
				createUser = Utils.GetRequest("createUser"),
				BlType = Utils.GetRequestToInt("BlType", -1),
				BeginTime = Utils.StringToDateTime(Utils.GetRequest("BeginTime"), null),
				EndTime = Utils.StringToDateTime(Utils.GetRequest("EndTime"), null)
			};
			ResultPage<BehaviorLog> resultPage = new ResultPage<BehaviorLog>();
			resultPage.msg = "查询成功";
			resultPage.Item = new SystemBll().GetBehaviorLogList(parm, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}
	}
}
