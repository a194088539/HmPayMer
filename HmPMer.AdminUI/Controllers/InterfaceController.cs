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
	public class InterfaceController : Controller
	{
		private InterfaceBll bll = new InterfaceBll();

		[Auth(FlagStr = "InterfaceBusinessIndex")]
		public ActionResult InterfaceBusinessIndex()
		{
			return View();
		}

		[Auth(FlagStr = "InterfaceBusinessIndex")]
		public ActionResult InterfaceBusinessAdd()
		{
			return View();
		}

		[Auth(FlagStr = "InterfaceBusinessIndex")]
		public ActionResult InterfaceBusinessUpdate(string Code)
		{
			InterfaceBusiness interfaceBusinessModel = bll.GetInterfaceBusinessModel(Code);
			return View(interfaceBusinessModel);
		}

		[Auth(FlagStr = "InterfaceBusinessIndex")]
		public ActionResult InterfaceBusinessDetail(string Code)
		{
			InterfaceBusiness interfaceBusinessModel = bll.GetInterfaceBusinessModel(Code);
			return View(interfaceBusinessModel);
		}

		public ActionResult LoadInterfaceBusiness(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			InterfaceBusiness param = new InterfaceBusiness
			{
				Name = Utils.GetRequest("Name")
			};
			ResultPage<InterfaceBusiness> resultPage = new ResultPage<InterfaceBusiness>();
			resultPage.msg = "查询成功";
			resultPage.Item = bll.LoadInterfaceBusiness(param, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		[HttpPost]
		public ActionResult InterfaceBusinessAddInfo(InterfaceBusiness Model)
		{
			InterfaceBusiness interfaceBusinessModel = bll.GetInterfaceBusinessModel(Model.Code);
			ResultBase resultBase = new ResultBase();
			if (interfaceBusinessModel != null)
			{
				resultBase.Success = false;
				resultBase.Message = "编号已经存在！";
				return Json(resultBase);
			}
			long num = bll.AddInterfaceBusiness(Model);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "新增失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "新增接口商";
				behaviorLog.BlType = 1;
				behaviorLog.parm = Model.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult UpInterfaceBusinessEnabled(string Code, int IsEnabled)
		{
			int num = bll.UpInterfaceBusinessEnabled(Code, IsEnabled);
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
				behaviorLog.BlName = ((IsEnabled == 1) ? "启用接口商" : "禁用接口商");
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"Code={Code}&IsEnabled={IsEnabled}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult UpInterfaceBusinessAgentPay(string Code, int AgentPay)
		{
			int num = bll.UpInterfaceBusinessAgentPay(Code, AgentPay);
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
				behaviorLog.BlName = ((AgentPay == 1) ? "启用接口商代付" : "禁用接口商代付");
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"Code={Code}&AgentPay={AgentPay}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult InterfaceBusinessUpdateinfo(InterfaceBusiness Model)
		{
			ResultBase resultBase = new ResultBase();
			resultBase.Success = bll.UpdateInterfaceBusiness(Model);
			if (!resultBase.Success)
			{
				resultBase.Message = "修改失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "修改接口商";
				behaviorLog.BlType = 2;
				behaviorLog.parm = Model.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult DelInterface(string Code)
		{
			ResultBase resultBase = new ResultBase();
			int num = bll.DelInterface(Code);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "操作失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "删除接口商";
				behaviorLog.BlType = 3;
				behaviorLog.parm = $"Code={Code}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[Auth(FlagStr = "InterfaceBusinessIndex")]
		public ActionResult InterfaceAccountIndex(string Code)
		{
			return View();
		}

		[Auth(FlagStr = "InterfaceBusinessIndex")]
		public ActionResult InterfaceAccountAdd(string Code)
		{
			return View();
		}

		[Auth(FlagStr = "InterfaceBusinessIndex")]
		public ActionResult InterfaceAccountUpdate(string Id)
		{
			InterfaceAccount interfaceAccountModel = bll.GetInterfaceAccountModel(Id);
			return View(interfaceAccountModel);
		}

		public ActionResult LoadInterfaceAccountDetail(string Id)
		{
			InterfaceAccount interfaceAccountModel = bll.GetInterfaceAccountModel(Id);
			return View(interfaceAccountModel);
		}

		public ActionResult LoadInterfaceAccount(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			InterfaceAccount param = new InterfaceAccount
			{
				Account = Utils.GetRequest("Account"),
				Code = Utils.GetRequest("Code"),
				IsEnabled = Utils.GetRequestToInt("IsEnabled", -1)
			};
			ResultPage<InterfaceAccount> resultPage = new ResultPage<InterfaceAccount>();
			resultPage.msg = "查询成功";
			resultPage.Item = bll.LoadInterfaceAccount(param, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		public ActionResult InterfaceAccountAddInfo(InterfaceAccount Model)
		{
			ResultBase resultBase = new ResultBase();
			Model.Id = Guid.NewGuid().ToString();
			long num = bll.AddInterfaceAccount(Model);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "新增失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "新增接口商账户";
				behaviorLog.BlType = 1;
				behaviorLog.parm = Model.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult UpInterfaceAccountEnabled(string Id, int IsEnabled)
		{
			int num = bll.UpInterfaceAccountEnabled(Id, IsEnabled);
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
				behaviorLog.BlName = ((IsEnabled == 1) ? "启用接口商账户" : "禁用接口商账户");
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"Id={Id}&IsEnabled={IsEnabled}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult UpdateInterfaceAccountInfo(InterfaceAccount Model)
		{
			ResultBase resultBase = new ResultBase();
			resultBase.Success = bll.UpdateInterfaceAccount(Model);
			if (!resultBase.Success)
			{
				resultBase.Message = "修改失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "修改接口商账户";
				behaviorLog.BlType = 2;
				behaviorLog.parm = Model.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult DelInterfaceAccountBat(List<string> idlist)
		{
			ApiResult<string> failing = ApiResult<string>.Failing;
			if (idlist == null || idlist.Count == 0)
			{
				failing.message = "请选择要删除的账户";
				return failing;
			}
			failing.IsSuccess = bll.DelInterfaceAccountBat(idlist);
			if (!failing.IsSuccess)
			{
				failing.message = "删除失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "删除接口商账户";
				behaviorLog.BlType = 3;
				behaviorLog.parm = $"idlist={idlist}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return failing;
		}

		public ActionResult InterfaceListRate()
		{
			return View();
		}
	}
}
