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
	public class PayController : Controller
	{
		private PayBll bll = new PayBll();

		[Auth(FlagStr = "PayTypeIndex")]
		public ActionResult PayTypeIndex()
		{
			return View();
		}

		[Auth(FlagStr = "PayTypeIndex")]
		public ActionResult PayTypeAdd()
		{
			return View();
		}

		[Auth(FlagStr = "PayTypeIndex")]
		public ActionResult PayTypeUpdate(string PayCode)
		{
			PayType payTypeModel = bll.GetPayTypeModel(PayCode);
			return View(payTypeModel);
		}

		public ActionResult LoadRechargePayType(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			PayType param = new PayType
			{
				PayName = Utils.GetRequest("PayName")
			};
			ResultPage<PayType> resultPage = new ResultPage<PayType>();
			resultPage.msg = "查询成功";
			resultPage.Item = bll.LoadRechargePayType(param, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		[HttpPost]
		public ActionResult PayTypeAddinfo(PayType Model)
		{
			long num = bll.AddPayType(Model);
			ResultBase resultBase = new ResultBase();
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "新增失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "新增支付类型";
				behaviorLog.BlType = 1;
				behaviorLog.parm = Model.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult PayTypeUpdateinfo(PayType Model)
		{
			long num = bll.UpdatePayType(Model);
			ResultBase resultBase = new ResultBase();
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "修改失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "修改支付类型";
				behaviorLog.BlType = 2;
				behaviorLog.parm = Model.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult UpPayTypeEnabled(string PayCode, int IsEnabled)
		{
			int num = bll.UpPayTypeEnabled(PayCode, IsEnabled);
			ResultBase resultBase = new ResultBase();
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "修改失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = ((IsEnabled == 1) ? "启用支付类型" : "禁用支付类型");
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"PayCode={PayCode}&IsEnabled={IsEnabled}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult DelPayType(string PayCode)
		{
			ResultBase resultBase = new ResultBase();
			int num = bll.DelPayType(PayCode);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "操作失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "删除支付类型";
				behaviorLog.BlType = 3;
				behaviorLog.parm = $"PayCode={PayCode}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		public ActionResult InterfaceType(string Code, int Type)
		{
			List<PayType> interfaceType = bll.GetInterfaceType(Code, Type);
			return View(interfaceType);
		}

		public ActionResult UserInterfaceType(string Code, int Type)
		{
			List<PayType> interfaceType = bll.GetInterfaceType(Code, Type);
			return View(interfaceType);
		}

		[HttpPost]
		public ActionResult SetInterfaceType(string InterfaceCode, string PayCode, int Type)
		{
			List<InterfaceType> list = new List<InterfaceType>();
			if (!string.IsNullOrEmpty(PayCode))
			{
				string[] array = PayCode.Split(',');
				foreach (string text in array)
				{
					InterfaceType interfaceType = new InterfaceType();
					interfaceType.InterfaceCode = InterfaceCode;
					interfaceType.PayCode = text.Split('#')[0];
					interfaceType.DefaulInfaceCode = ((text.Split('#').Length > 1) ? text.Split('#')[1] : "");
					interfaceType.AccountScheme = ((text.Split('#').Length > 2) ? text.Split('#')[2] : "");
					interfaceType.Type = Type;
					list.Add(interfaceType);
				}
			}
			long num = bll.SetInterfaceType(list, InterfaceCode);
			ResultBase resultBase = new ResultBase();
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "设置失败！";
			}
			else
			{
				string blName = "";
				if (Type == 1)
				{
					blName = "设置接口商支付类型";
				}
				if (Type == 2)
				{
					blName = "设置商户支付类型";
				}
				if (Type == 3)
				{
					blName = "设置等级支付类型";
				}
				if (Type == 3)
				{
					blName = "设置代理支付类型";
				}
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = blName;
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"InterfaceCode={InterfaceCode}&PayCode={PayCode}&Type={Type}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[Auth(FlagStr = "PayChannelIndex")]
		public ActionResult PayChannelIndex()
		{
			return View();
		}

		[Auth(FlagStr = "PayChannelIndex")]
		public ActionResult PayChannelAdd()
		{
			return View();
		}

		[Auth(FlagStr = "PayChannelIndex")]
		public ActionResult PayChannelUpdate(string Code)
		{
			PayChannel payChannelModel = bll.GetPayChannelModel(Code);
			return View(payChannelModel);
		}

		public ActionResult LoadRechargePayChannel(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			PayChannelInfo param = new PayChannelInfo
			{
				ChannelName = Utils.GetRequest("ChannelName"),
				PayCode = Utils.GetRequest("PayCode")
			};
			ResultPage<PayChannelInfo> resultPage = new ResultPage<PayChannelInfo>();
			resultPage.msg = "查询成功";
			resultPage.Item = bll.LoadRechargePayChannel(param, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		[HttpPost]
		public ActionResult PayChannelAddinfo(PayChannel Model)
		{
			ResultBase resultBase = new ResultBase();
			try
			{
				long num = bll.AddPayChannel(Model);
				if (num <= 0)
				{
					resultBase.Success = false;
					resultBase.Message = "新增失败！";
				}
				else
				{
					BehaviorLog behaviorLog = new BehaviorLog();
					behaviorLog.Id = Guid.NewGuid().ToString();
					behaviorLog.BlName = "新增通道";
					behaviorLog.BlType = 2;
					behaviorLog.parm = Model.ToJson();
					behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
					behaviorLog.addTime = DateTime.Now;
					new SystemBll().InserBehaviorLog(behaviorLog);
				}
			}
			catch (Exception)
			{
				resultBase.Success = false;
				resultBase.Message = "编号重复！";
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult PayChannelUpdateinfo(PayChannel Model)
		{
			long num = bll.UpdatePayChannel(Model);
			ResultBase resultBase = new ResultBase();
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "修改失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "修改通道";
				behaviorLog.BlType = 2;
				behaviorLog.parm = Model.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult UpPayChannelEnabled(string Code, int IsEnabled)
		{
			int num = bll.UpPayChannelEnabled(Code, IsEnabled);
			ResultBase resultBase = new ResultBase();
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "修改失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = ((IsEnabled == 1) ? "启用通道" : "禁用通道");
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"Code={Code}&IsEnabled={IsEnabled}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult DelChannel(string Code)
		{
			ResultBase resultBase = new ResultBase();
			int num = bll.DelChannel(Code);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "操作失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "删除通道";
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"Code={Code}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}
	}
}
