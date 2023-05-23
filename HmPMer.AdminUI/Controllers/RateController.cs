using HM.Framework;
using HmPMer.AdminUI.Fillters;
using HmPMer.AdminUI.Models;
using HmPMer.Business;
using HmPMer.Entity;
using HmPMer.WebAdminUI.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HmPMer.AdminUI.Controllers
{
	public class RateController : Controller
	{
		private RateBll bll = new RateBll();

		public ActionResult Index(string UserId, int Type)
		{
			List<PayRateInfo> list = new List<PayRateInfo>();
			list = ((Type != 3) ? bll.GetPayRateList(UserId, Type) : bll.GetPayRateList(UserId));
			return View(list);
		}

		public ActionResult BusinessRate(string UserId, int Type)
		{
			List<PayRateInfo> list = new List<PayRateInfo>();
			list = ((Type != 3) ? bll.GetPayRateList(UserId, Type) : bll.GetPayRateList(UserId));
			return View(list);
		}

		[HttpPost]
		public ActionResult SetPayRate(List<PayRate> ListModel, string UserId, int Type, int AloneRate)
		{
			foreach (PayRate item in ListModel)
			{
				item.Id = Guid.NewGuid().ToString();
				item.UserId = UserId;
				item.RateType = Type;
				item.Rate /= 100m;
				if (Type != 2 && Type != 4)
				{
					item.IsEnabled = 1;
				}
			}
			long num = bll.SetPayRate(ListModel, UserId, Type);
			ResultBase resultBase = new ResultBase();
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "新增失败！";
			}
			else
			{
				if (Type == 2 || Type == 4)
				{
					new UserBaseBll().UpdateAloneRate(AloneRate, UserId);
				}
				string blName = "";
				if (Type == 1)
				{
					blName = "设置接口商费率";
				}
				if (Type == 2)
				{
					blName = "设置商户费率";
				}
				if (Type == 3)
				{
					blName = "设置等级费率";
				}
				if (Type == 3)
				{
					blName = "设置代理费率";
				}
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = blName;
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"ListModel={ListModel.ToJson()}&UserId={UserId}&Type={Type}&AloneRate={AloneRate}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[Auth(FlagStr = "ListRate")]
		public ActionResult ListRate()
		{
			return View();
		}
	}
}
