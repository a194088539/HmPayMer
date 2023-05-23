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
	public class MenuRoleController : Controller
	{
		[Auth(FlagStr = "MenuRole")]
		public ActionResult Index()
		{
			return View();
		}

		[Auth(FlagStr = "MenuRole")]
		public ActionResult AddRole()
		{
			return View();
		}

		[Auth(FlagStr = "MenuRole")]
		public ActionResult SetRoleFlag(string RoleId)
		{
			List<Menu> roleMenuList = new MenuRoleBll().GetRoleMenuList(RoleId);
			return View(roleMenuList);
		}

		[Auth(FlagStr = "Menu")]
		public ActionResult MenuList()
		{
			return View();
		}

		[Auth(FlagStr = "Menu")]
		public ActionResult AddMenu()
		{
			return View();
		}

		[Auth(FlagStr = "Menu")]
		public ActionResult UpdateMenu(string Id)
		{
			Menu model = new MenuRoleBll().GetModel(Id);
			return View(model);
		}

		public ActionResult GetMenuPageList(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			Menu menu = new Menu
			{
				menuName = Utils.GetRequest("menuName")
			};
			ResultPage<Menu> resultPage = new ResultPage<Menu>();
			resultPage.msg = "查询成功";
			resultPage.Item = new MenuRoleBll().GetMenuPageList(menu, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		[HttpPost]
		public ActionResult AddMenuInfo(Menu Model)
		{
			if (!string.IsNullOrEmpty(Model.icon))
			{
				Model.icon = "&" + Model.icon.Trim();
			}
			long maxId = new MenuRoleBll().GetMaxId(Model.parentID);
			if (maxId == 0L)
			{
				Model.Id = Model.parentID + "001";
			}
			else
			{
				Model.Id = (maxId + 1).ToString();
			}
			Model.createTime = DateTime.Now;
			Model.IsEnabled = 1;
			Model.menuLeval = ((Model.parentID == "0") ? 1 : 2);
			long num = new MenuRoleBll().AddMenu(Model);
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
				behaviorLog.BlName = "新增菜单";
				behaviorLog.BlType = 1;
				behaviorLog.parm = Model.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult UpdateMenuInfo(Menu Model)
		{
			int num = 0;
			if (!string.IsNullOrEmpty(Model.icon))
			{
				Model.icon = "&" + Model.icon.Trim();
			}
			if (string.IsNullOrEmpty(Model.NewId))
			{
				num = new MenuRoleBll().UpdateMenu1(Model);
			}
			else
			{
				long maxId = new MenuRoleBll().GetMaxId(Model.parentID);
				if (maxId == 0L)
				{
					Model.NewId = Model.parentID + "001";
				}
				else
				{
					Model.NewId = (maxId + 1).ToString();
				}
				Model.menuLeval = ((Model.parentID == "0") ? 1 : 2);
				num = new MenuRoleBll().UpdateMenu2(Model);
			}
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
				behaviorLog.BlName = "修改菜单";
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
			int num = new MenuRoleBll().UpdateIsEnabled(IsEnabled, Id);
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
				behaviorLog.BlName = ((IsEnabled == 1) ? "启用菜单" : "禁用菜单");
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"IsEnabled={IsEnabled}&Id={Id}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		public ActionResult GetUserMenu(string parentID)
		{
			List<MenuJson> userMenu = new MenuRoleBll().GetUserMenu(ModelCommon.GetUserModel().ID, parentID);
			return Json(userMenu);
		}

		public ActionResult LoadRechargeRole(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			Role role = new Role
			{
				roleName = Utils.GetRequest("roleName")
			};
			ResultPage<Role> resultPage = new ResultPage<Role>();
			resultPage.msg = "查询成功";
			resultPage.Item = new MenuRoleBll().GetRoleList(role, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		[HttpPost]
		public ActionResult AddRoleInfo(string roleName, string describe)
		{
			Role role = new Role();
			role.Id = Guid.NewGuid().ToString();
			role.roleName = roleName;
			role.describe = describe;
			role.createTime = DateTime.Now;
			role.modifyTime = DateTime.Now;
			role.createUser = ModelCommon.GetUserModel().AdmUser;
			role.modifyUser = ModelCommon.GetUserModel().AdmUser;
			long num = new MenuRoleBll().AddRole(role);
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
				behaviorLog.BlName = "新增角色";
				behaviorLog.BlType = 2;
				behaviorLog.parm = role.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult SetRoleMenu(string roleID, string MenuId)
		{
			List<RoleMenu> list = new List<RoleMenu>();
			if (!string.IsNullOrEmpty(MenuId))
			{
				string[] array = MenuId.Split(',');
				foreach (string menuID in array)
				{
					RoleMenu roleMenu = new RoleMenu();
					roleMenu.Id = Guid.NewGuid().ToString();
					roleMenu.roleID = roleID;
					roleMenu.menuID = menuID;
					roleMenu.createTime = DateTime.Now;
					roleMenu.createUser = ModelCommon.GetUserModel().AdmUser;
					roleMenu.modifyTime = DateTime.Now;
					roleMenu.modifyUser = ModelCommon.GetUserModel().AdmUser;
					list.Add(roleMenu);
				}
			}
			long num = new MenuRoleBll().SetRoleMenu(roleID, list);
			ResultBase resultBase = new ResultBase();
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "分配失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "分配角色权限";
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"roleID={roleID}&MenuId={MenuId}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult DelRole(string Id)
		{
			ResultBase resultBase = new ResultBase();
			int num = new MenuRoleBll().DelRole(Id);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "操作失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "删除角色";
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
