using HmPMer.Dal;
using HmPMer.Entity;
using System.Collections.Generic;

namespace HmPMer.Business
{
	public class MenuRoleBll
	{
		private readonly MenuRoleDal _dal = new MenuRoleDal();

		public List<Menu> GetMenuPageList(Menu menu, ref Paging paging)
		{
			return _dal.GetMenuPageList(menu, ref paging);
		}

		public List<Menu> GetMuListPid(string parentID)
		{
			return _dal.GetMuListPid(parentID);
		}

		public long GetMaxId(string parentID)
		{
			return _dal.GetMaxId(parentID);
		}

		public Menu GetModel(string Id)
		{
			return _dal.GetModel(Id);
		}

		public long AddMenu(Menu Model)
		{
			return _dal.AddMenu(Model);
		}

		public int UpdateMenu1(Menu Model)
		{
			return _dal.UpdateMenu1(Model);
		}

		public int UpdateMenu2(Menu Model)
		{
			return _dal.UpdateMenu2(Model);
		}

		public int UpdateIsEnabled(int IsEnabled, string Id)
		{
			return _dal.UpdateIsEnabled(IsEnabled, Id);
		}

		public List<Menu> GetUserMenu(string UserId)
		{
			return _dal.GetUserMenu(UserId);
		}

		public List<MenuJson> GetUserMenu(string UserId, string parentID)
		{
			return _dal.GetUserMenu(UserId, parentID);
		}

		public List<Menu> GetUserMenuIndex(string UserId)
		{
			return _dal.GetUserMenuIndex(UserId);
		}

		public List<MenuJson> GetUserMenuIndex(string UserId, string parentID)
		{
			return _dal.GetUserMenuIndex(UserId, parentID);
		}

		public List<Role> GetRoleList(Role role, ref Paging paging)
		{
			return _dal.GetRoleList(role, ref paging);
		}

		public List<Menu> GetRoleList(string parentID = "-1", int IsEnabled = -1)
		{
			return _dal.GetRoleList(parentID, IsEnabled);
		}

		public List<Menu> GetRoleMenuList(string roleID)
		{
			return _dal.GetRoleMenuList(roleID);
		}

		public List<Role> GetRoleList()
		{
			return _dal.GetRoleList();
		}

		public List<Role> GetRoleList(string userId)
		{
			return _dal.GetRoleList(userId);
		}

		public long AddRole(Role role)
		{
			return _dal.AddRole(role);
		}

		public long SetRoleMenu(string roleID, List<RoleMenu> listModel)
		{
			return _dal.SetRoleMenu(roleID, listModel);
		}

		public int DelRole(string roleID)
		{
			return _dal.DelRole(roleID);
		}

		public bool CheckFlag(string userid, int menuid)
		{
			return _dal.CheckFlag(userid, menuid) > 0;
		}

		public bool CheckFlagStr(string userid, string FlagStr)
		{
			return _dal.CheckFlagStr(userid, FlagStr) > 0;
		}
	}
}
