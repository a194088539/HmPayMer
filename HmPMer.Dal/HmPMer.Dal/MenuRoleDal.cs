using HM.DAL;
using HmPMer.Entity;
using System.Collections.Generic;

namespace HmPMer.Dal
{
	public class MenuRoleDal
	{
		public List<Menu> GetMenuPageList(Menu menu, ref Paging paging)
		{
			string str = " select * from [Menu] A Where 1=1  ";
			string str2 = " select count(*) from [Menu] A WHERE 1=1  ";
			string text = "";
            menu.menuName = DalContext.EscapeString(menu.menuName);
            if (!string.IsNullOrEmpty(menu.menuName))
			{
				text = text + " And ( A.menuName like '%" + menu.menuName + "%' or A.Id like '" + menu.menuName + "%'  )";
			}
			str += text;
			str2 += text;
			return DalContext.GetPage<Menu>(str, str2, " * ", " id asc ", ref paging);
		}

		public long GetMaxId(string parentID)
		{
			return DalContext.GetSingVal<long>(" select MAX(id) from Menu where parentID = @parentID ", new
			{
				parentID
			});
		}

		public List<Menu> GetMuListPid(string parentID)
		{
			return DalContext.GetList<Menu>(" select * from [Menu] where parentID=@parentID ", new
			{
				parentID
			});
		}

		public Menu GetModel(string Id)
		{
			return DalContext.GetModel<Menu>(" select * from [Menu] where Id=@Id ", new
			{
				Id
			});
		}

		public long AddMenu(Menu Model)
		{
			return DalContext.Insert(Model);
		}

		public int UpdateMenu1(Menu Model)
		{
			return DalContext.ExecuteSql(" update Menu set menuName=@menuName,menuUrl=@menuUrl,orderNo=@orderNo,icon=@icon,FlagStr=@FlagStr,createTime=GETDATE() where Id=@Id ", new
			{
				Model.menuName,
				Model.menuUrl,
				Model.orderNo,
				Model.icon,
				Model.FlagStr,
				Model.Id
			});
		}

		public int UpdateMenu2(Menu Model)
		{
			return DalContext.ExecuteSql(" update Menu set Id=@newId, menuName=@menuName,menuUrl=@menuUrl,menuLeval=@menuLeval,parentID=@parentID,orderNo=@orderNo,icon=@icon,FlagStr=@FlagStr,createTime=GETDATE() where Id=@Id ", new
			{
				newId = Model.NewId,
				menuName = Model.menuName,
				menuLeval = Model.menuLeval,
				parentID = Model.parentID,
				menuUrl = Model.menuUrl,
				orderNo = Model.orderNo,
				icon = Model.icon,
				FlagStr = Model.FlagStr,
				Id = Model.Id
			});
		}

		public int UpdateIsEnabled(int IsEnabled, string Id)
		{
			return DalContext.ExecuteSql(" update Menu set IsEnabled=@IsEnabled where Id=@Id ", new
			{
				IsEnabled,
				Id
			});
		}

		public List<Menu> GetUserMenu(string UserId)
		{
			return DalContext.GetList<Menu>("select distinct A.* from Menu A LEFT JOIN  RoleMenu B on A.Id=B.menuID \r\n            Left join UserRole C on B.roleID=C.roleID\r\n            where A.parentID=0 and IsEnabled=1 and UserId=@UserId  order by A.orderNo desc ", new
			{
				UserId
			});
		}

		public List<MenuJson> GetUserMenu(string UserId, string parentID)
		{
			return DalContext.GetList<MenuJson>(" select distinct A.menuName as title,A.icon , A.menuUrl href,A.orderNo from Menu A LEFT JOIN  RoleMenu B on A.Id=B.menuID \r\n                            Left join UserRole C on B.roleID=C.roleID\r\n                            where A.parentID=@parentID and UserId=@UserId and IsEnabled=1 order by A.orderNo desc ", new
			{
				parentID,
				UserId
			});
		}

		public List<Menu> GetUserMenuIndex(string UserId)
		{
			return DalContext.GetList<Menu>(" select distinct A.* from Menu A  LEFT JOIN  RoleMenu B on A.Id=B.menuID  \r\n            Left join UserRole C on B.roleID=C.roleID \r\n            where A.parentID=0 and B.RoleId='indexadmin' \r\n            And A.Id in( select distinct A.Id from Menu A LEFT JOIN  RoleMenu B on A.Id=B.menuID            \r\n            Left join UserRole C on B.roleID=C.roleID             \r\n            where A.parentID=0 and IsEnabled=1 And C.UserId=@UserId )\r\n            order by A.orderNo desc \r\n             ", new
			{
				UserId
			});
		}

		public List<MenuJson> GetUserMenuIndex(string UserId, string parentID)
		{
			return DalContext.GetList<MenuJson>(" select distinct A.menuName as title,A.icon , A.menuUrl href,A.orderNo from Menu A\r\n                         LEFT JOIN  RoleMenu B on A.Id=B.menuID \r\n                        Left join UserRole C on B.roleID=C.roleID\r\n                        where A.parentID=@parentID And IsEnabled=1 And B.RoleId='indexadmin'\r\n                        And A.id in(\r\n                        select distinct A.Id from Menu A LEFT JOIN  RoleMenu B on A.Id=B.menuID \r\n                        Left join UserRole C on B.roleID=C.roleID\r\n                        where A.parentID=@parentID and UserId=@UserId and IsEnabled=1\r\n                        )\r\n                         order by A.orderNo desc ", new
			{
				parentID,
				UserId
			});
		}

		public List<Role> GetRoleList(Role role, ref Paging paging)
		{
			string str = " select count(*) from [Role] A WHERE id!='indexadmin'  ";
			string text = "";
			if (!string.IsNullOrEmpty(role.roleName))
			{
				text = text + " And A.roleName like '%" + role.roleName + "%' ";
			}
			string sql = " select * from [Role] A Where id!='indexadmin'  " + text;
			str += text;
			return DalContext.GetPage<Role>(sql, str, " * ", " createTime DESC ", ref paging);
		}

		public List<Menu> GetRoleList(string parentID, int IsEnabled)
		{
			string text = "  select * from Menu Where 1=1 ";
            parentID = DalContext.EscapeString(parentID);
            if (parentID != "-1")
			{
				text = text + " And parentID='" + parentID + "' ";
			}
			if (IsEnabled != -1)
			{
				text = text + " And IsEnabled=" + IsEnabled + " ";
			}
			return DalContext.GetList<Menu>(text);
		}

		public List<Menu> GetRoleMenuList(string roleID)
		{
			return DalContext.GetList<Menu>("  select A.* from Menu A inner join RoleMenu B on A.Id=B.menuID where B.roleID=@roleID And A.IsEnabled=1 order by A.orderNo desc ", new
			{
				roleID
			});
		}

		public List<Role> GetRoleList()
		{
			return DalContext.GetList<Role>("  select * from [Role] where id!='indexadmin'  ");
		}

		public List<Role> GetRoleList(string userId)
		{
			return DalContext.GetList<Role>("  select A.* from Role A inner join UserRole B on A.Id= B.roleID where B.userId=@userId ", new
			{
				userId
			});
		}

		public long AddRole(Role role)
		{
			return DalContext.Insert(role);
		}

		public long SetRoleMenu(string roleID, List<RoleMenu> listModel)
		{
			int num = DalContext.ExecuteSql(" delete RoleMenu where roleID=@roleID ", new
			{
				roleID
			});
			if (listModel != null && listModel.Count > 0)
			{
				return DalContext.InsertBat(listModel);
			}
			return num;
		}

		public int DelRole(string roleID)
		{
			return DalContext.ExecuteSql(" delete [Role] where Id=@Id; delete [RoleMenu] where roleId=@Id;delete [UserRole] where roleId=@Id; ", new
			{
				Id = roleID
			});
		}

		public long CheckFlag(string userid, int menuid)
		{
			return DalContext.GetSingVal<long>(" select count(*) from [dbo].[RoleMenu] A inner join [dbo].[UserRole] B On A.RoleId=B.RoleId\r\n                             where B.userid =@userid And A.menuid = @menuid ", new
			{
				userid,
				menuid
			});
		}

		public long CheckFlagStr(string userid, string FlagStr)
		{
			return DalContext.GetSingVal<long>(" \r\n                select count(*) from [dbo].[RoleMenu] A \r\n                inner join [dbo].[UserRole] B On A.RoleId=B.RoleId\r\n                inner join Menu C on A.[menuID]=C.Id\r\n                where B.userid =@userid And C.FlagStr = @FlagStr ", new
			{
				userid,
				FlagStr
			});
		}
	}
}
