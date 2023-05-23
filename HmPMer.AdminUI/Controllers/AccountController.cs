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
	public class AccountController : Controller
	{
		[Auth(FlagStr = "Account")]
		public ActionResult Index()
		{
			return View();
		}

		[Auth(FlagStr = "Account")]
		public ActionResult Add()
		{
			return PartialView();
		}

		[Auth(FlagStr = "Account")]
		public ActionResult UpdatePwd()
		{
			return View();
		}

		[Auth(FlagStr = "Account")]
		public ActionResult Update(string Id)
		{
			HmAdmin hmAdmin = new AccountBll().GetHmAdmin(Id);
			return PartialView(hmAdmin);
		}

		public ActionResult UserInfo()
		{
			HmAdminAmt adminAmt = new AccountBll().GetAdminAmt(ModelCommon.GetUserModel().ID);
			return View(adminAmt);
		}

		public ActionResult AdminList(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			HmAdmin param = new HmAdmin
			{
				AdmUser = Utils.GetRequest("AdmUser")
			};
			ResultPage<HmAdmin> resultPage = new ResultPage<HmAdmin>();
			resultPage.msg = "查询成功";
			resultPage.Item = new AccountBll().LoadAdminPage(param, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		[HttpPost]
		public ActionResult UpIsEnabled(string userId, int IsEnabled)
		{
			int num = new AccountBll().UpIsEnabled(userId, IsEnabled);
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
				behaviorLog.BlName = ((IsEnabled == 1) ? "启用管理员" : "禁用管理员");
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"userId={userId}&IsEnabled={IsEnabled}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		[Auth(FlagStr = "Account")]
		public ActionResult RestPwd(string Id)
		{
			int num = new AccountBll().RestPwd(EncryUtils.MD5("888888"), EncryUtils.MD5("888888"), Id);
			ResultBase resultBase = new ResultBase();
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "操作失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "重置管理员密码";
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"Id={Id}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult UpdatePwd(string oldPwd, string NewPwd)
		{
			ResultBase resultBase = new ResultBase();
			HmAdmin userModel = ModelCommon.GetUserModel();
			if (!userModel.AdmPass.Equals(EncryUtils.MD5(oldPwd)))
			{
				resultBase.Success = false;
				resultBase.Message = "原密码错误！";
				return Json(resultBase);
			}
			int num = new AccountBll().UpdateAdmPass(EncryUtils.MD5(NewPwd), userModel.ID);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "操作失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "修改管理员密码";
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"oldPwd={oldPwd}&NewPwd={NewPwd}&Id={userModel.ID}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		[Auth(FlagStr = "Account")]
		public ActionResult deleteHmAdmin(string Id)
		{
			ResultBase resultBase = new ResultBase();
			HmAdmin userModel = ModelCommon.GetUserModel();
			if (userModel.ID == Id)
			{
				resultBase.Success = false;
				resultBase.Message = "当前登录账号不能删除！";
				return Json(resultBase);
			}
			int num = new AccountBll().deleteHmAdmin(Id);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "操作失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "删除管理员";
				behaviorLog.BlType = 3;
				behaviorLog.parm = $"Id={Id}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		[Auth(FlagStr = "Account")]
		public ActionResult Add(HmAdmin Model)
		{
			ResultBase resultBase = new ResultBase();
			if (new AccountBll().GetHmAdminAdmUser(Model.AdmUser) != null)
			{
				resultBase.Success = false;
				resultBase.Message = "用户名已存在！";
			}
			else
			{
				Model.ID = Guid.NewGuid().ToString();
				Model.IsEnable = 1;
				Model.Rate /= 100m;
				Model.AdmPass = EncryUtils.MD5(Model.AdmPass);
				Model.AdmPass2 = EncryUtils.MD5(Model.AdmPass2);
				Model.AddTime = DateTime.Now;
				Model.LastLoginTime = DateTime.Now;
				Model.LastLoginIp = Utils.GetServerIp();
				Model.RegCode = EncryUtils.MD5(Model.ID + "zqw_no1");
				List<UserRole> list = new List<UserRole>();
				if (!string.IsNullOrEmpty(Model.RoleStr))
				{
					string[] array = Model.RoleStr.Split(',');
					foreach (string roleID in array)
					{
						UserRole userRole = new UserRole();
						userRole.Id = Guid.NewGuid().ToString();
						userRole.userID = Model.ID;
						userRole.roleID = roleID;
						userRole.createTime = DateTime.Now;
						userRole.createUser = ModelCommon.GetUserModel().AdmUser;
						userRole.modifyTime = DateTime.Now;
						userRole.modifyUser = ModelCommon.GetUserModel().AdmUser;
						list.Add(userRole);
					}
				}
				resultBase.Success = new AccountBll().Add(Model, list);
				if (resultBase.Success)
				{
					BehaviorLog behaviorLog = new BehaviorLog();
					behaviorLog.Id = Guid.NewGuid().ToString();
					behaviorLog.BlName = "新增管理员";
					behaviorLog.BlType = 1;
					behaviorLog.parm = Model.ToJson();
					behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
					behaviorLog.addTime = DateTime.Now;
					new SystemBll().InserBehaviorLog(behaviorLog);
				}
			}
			return Json(resultBase);
		}

		[HttpPost]
		[Auth(FlagStr = "Account")]
		public ActionResult UpdaeRate(string NickName, decimal Rate, string Id, string RoleStr)
		{
			List<UserRole> list = new List<UserRole>();
			if (!string.IsNullOrEmpty(RoleStr))
			{
				string[] array = RoleStr.Split(',');
				foreach (string roleID in array)
				{
					UserRole userRole = new UserRole();
					userRole.Id = Guid.NewGuid().ToString();
					userRole.userID = Id;
					userRole.roleID = roleID;
					userRole.createTime = DateTime.Now;
					userRole.createUser = ModelCommon.GetUserModel().AdmUser;
					userRole.modifyTime = DateTime.Now;
					userRole.modifyUser = ModelCommon.GetUserModel().AdmUser;
					list.Add(userRole);
				}
			}
			int num = new AccountBll().UpdaeRate(NickName, Rate / 100m, Id, list);
			ResultBase resultBase = new ResultBase();
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "操作失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "修改管理员";
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"NickName={NickName}&Rate={Rate}&Id={Id}&RoleStr={RoleStr}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}
	}
}
