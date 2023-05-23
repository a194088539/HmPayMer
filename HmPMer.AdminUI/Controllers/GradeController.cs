using HM.Framework;
using HmPMer.AdminUI.Models;
using HmPMer.Business;
using HmPMer.Entity;
using HmPMer.WebAdminUI.Models;
using System;
using System.Web.Mvc;

namespace HmPMer.AdminUI.Controllers
{
	public class GradeController : Controller
	{
		private GradeBll bll = new GradeBll();

		public ActionResult UserGradeIndex()
		{
			return View();
		}

		public ActionResult UserGradeAdd()
		{
			return View();
		}

		public ActionResult UserGradeUpdate(string Id)
		{
			UserGrade userGradeModel = bll.GetUserGradeModel(Id);
			return View(userGradeModel);
		}

		public ActionResult LoadUserGrade()
		{
			int requestToInt = Utils.GetRequestToInt("UserType", 0);
			ResultPage<UserGrade> resultPage = new ResultPage<UserGrade>();
			resultPage.msg = "查询成功";
			resultPage.Item = bll.GetUserGradeList(requestToInt);
			return resultPage;
		}

		[HttpPost]
		public ActionResult AddUserGradeInfo(string GradeName, int UserType)
		{
			UserGrade userGrade = new UserGrade();
			long maxId = bll.GetMaxId();
			if (maxId == 0L)
			{
				userGrade.Id = "1000";
			}
			else
			{
				userGrade.Id = (maxId + 1).ToString();
			}
			userGrade.GradeName = GradeName;
			userGrade.UserType = UserType;
			long num = bll.AddUserGrade(userGrade);
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
				behaviorLog.BlName = ((UserType == 1) ? "新增商户等级" : "新增代理等级");
				behaviorLog.BlType = 1;
				behaviorLog.parm = userGrade.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult UpdateGradeInfo(string GradeName, string Id)
		{
			int num = num = bll.UpdateUserGrade(GradeName, Id);
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
				behaviorLog.BlName = "修改等级";
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"GradeName={GradeName}&Id={Id}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult DelUserGrade(string Id)
		{
			ResultBase resultBase = new ResultBase();
			int num = bll.DelUserGrade(Id);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "操作失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "删除等级";
				behaviorLog.BlType = 3;
				behaviorLog.parm = $"Id={Id}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}
	}
}
