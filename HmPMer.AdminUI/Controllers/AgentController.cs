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
	public class AgentController : Controller
	{
		private UserBaseBll bll = new UserBaseBll();

		[Auth(FlagStr = "Agent")]
		public ActionResult Index()
		{
			return View();
		}

		[Auth(FlagStr = "Agent")]
		public ActionResult AddUc()
		{
			return View();
		}

		[Auth(FlagStr = "Agent")]
		public ActionResult UpdateUc(string UserID)
		{
			UserBaseInfo modelForId = new UserBaseBll().GetModelForId(UserID);
			return View(modelForId);
		}

		public ActionResult GetAgentList(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			UserBaseInfo param = new UserBaseInfo
			{
				UserId = Utils.GetRequest("UserId"),
				MerName = Utils.GetRequest("MerName"),
				MobilePhone = Utils.GetRequest("MobilePhone"),
				Email = Utils.GetRequest("Email"),
				UserType = 2,
				AloneRate = Utils.GetRequestQueryToInt("AloneRate", -1),
				IsEnabled = Utils.GetRequestQueryToInt("IsEnabled", -1),
				AgentId = Utils.GetRequest("AgentId"),
				Type = Utils.GetRequestQueryToInt("Type", 0)
			};
			ResultPage<UserBaseInfo> resultPage = new ResultPage<UserBaseInfo>();
			resultPage.msg = "查询成功";
			resultPage.Item = new UserBaseBll().LoadUserPage(param, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		[HttpPost]
		public ActionResult AgentAdd(UserBase Model)
		{
			ResultBase resultBase = new ResultBase();
			if (string.IsNullOrEmpty(Model.MobilePhone))
			{
				resultBase.Success = false;
				resultBase.Message = "手机号码不能为空!";
				return Json(resultBase);
			}
			if (bll.GetModelForMobile(Model.MobilePhone, "") != null)
			{
				resultBase.Success = false;
				resultBase.Message = "此手机号码已经存在，请更换一个";
				return Json(resultBase);
			}
			Model.UserType = 2;
			Model.RegTime = DateTime.Now;
			Model.IsEnabled = 1;
			Model.AgentCode = EncryUtils.MD5(Guid.NewGuid().ToString());
			Model.RegIp = Utils.GetClientIp();
			Model.PromId = ModelCommon.GetUserModel().AdmUser;
			Model.LastLoginTime = DateTime.Now;
			Model.LastLoginIp = Utils.GetClientIp();
			long num = bll.AddAgent(Model);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "新增失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "新增代理";
				behaviorLog.BlType = 1;
				behaviorLog.parm = Model.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult AgentUpdate(UserBase Model)
		{
			ResultBase resultBase = new ResultBase();
			if (string.IsNullOrEmpty(Model.MobilePhone))
			{
				resultBase.Success = false;
				resultBase.Message = "手机号码不能为空!";
				return Json(resultBase);
			}
			if (bll.GetModelForMobile(Model.MobilePhone, Model.UserId) != null)
			{
				resultBase.Success = false;
				resultBase.Message = "此手机号码已经存在，请更换一个";
				return Json(resultBase);
			}
			int num = bll.AgentUpdate(Model);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "修改失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "修改代理";
				behaviorLog.BlType = 2;
				behaviorLog.parm = Model.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult UpIsEnabled(int IsEnabled, string Id)
		{
			int num = bll.UpIsEnabled(Id, IsEnabled);
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
				behaviorLog.BlName = "修改代理";
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"IsEnabled={IsEnabled}&Id={Id}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult DelBusiness(string UserId)
		{
			ResultBase resultBase = new ResultBase();
			int num = bll.DelBusiness(UserId, ModelCommon.GetUserModel().AdmUser);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "操作失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "删除代理";
				behaviorLog.BlType = 3;
				behaviorLog.parm = $"UserId={UserId}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}
	}
}
