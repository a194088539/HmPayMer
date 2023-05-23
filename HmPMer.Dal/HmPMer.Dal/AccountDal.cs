using HM.DAL;
using HmPMer.Entity;
using System.Collections.Generic;

namespace HmPMer.Dal
{
	public class AccountDal
	{
		public HmAdmin AdminLoginIn(HmAdmin model)
		{
			return DalContext.GetModel<HmAdmin>(" select * from HmAdmin where AdmUser=@AdmUser and AdmPass=@AdmPass ", model);
		}

		public HmAdmin GetHmAdmin(string Id)
		{
			return DalContext.GetModel<HmAdmin>(" select * from HmAdmin where Id=@Id ", new
			{
				Id
			});
		}

		public HmAdmin GetHmAdminAdmUser(string AdmUser)
		{
			return DalContext.GetModel<HmAdmin>(" select * from HmAdmin where AdmUser=@AdmUser ", new
			{
				AdmUser
			});
		}

		public List<HmAdmin> LoadAdminPage(HmAdmin param, ref Paging paging)
		{
			string str = " select count(*) from HmAdmin WHERE 1=1  ";
			string text = "";
            param.AdmUser = DalContext.EscapeString(param.AdmUser);
			if (!string.IsNullOrEmpty(param.AdmUser))
			{
				text += string.Format(" AND (AdmUser LIKE '%{0}%' or NickName LIKE '%{0}%') ", param.AdmUser);
			}
			string sql = " select * from HmAdmin where 1=1 " + text;
			str += text;
			return DalContext.GetPage<HmAdmin>(sql, str, " * ", " AddTime DESC ", ref paging);
		}

		public int UpdateLastLoginIp(string LastLoginIp, string AdmUser)
		{
			return DalContext.ExecuteSql(" update HmAdmin set LastLoginTime=GETDATE(),LastLoginIp=@LastLoginIp  where AdmUser=@AdmUser ", new
			{
				LastLoginIp,
				AdmUser
			});
		}

		public int UpIsEnabled(string userId, int IsEnabled)
		{
			return DalContext.ExecuteSql($" update HmAdmin set IsEnable={IsEnabled} where Id in ({userId}) ");
		}

		public int RestPwd(string AdmPass, string AdmPass2, string Id)
		{
			return DalContext.ExecuteSql(" update HmAdmin set AdmPass=@AdmPass ,AdmPass2=@AdmPass2 where id =@Id ", new
			{
				AdmPass,
				AdmPass2,
				Id
			});
		}

		public int UpdateAdmPass(string AdmPass, string Id)
		{
			return DalContext.ExecuteSql(" update HmAdmin set AdmPass=@AdmPass where id =@Id ", new
			{
				AdmPass,
				Id
			});
		}

		public int deleteHmAdmin(string Id)
		{
			return DalContext.ExecuteSql(" delete  HmAdmin where id =@Id;delete [UserRole] where UserId=@Id; ", new
			{
				Id
			});
		}

		public bool Add(HmAdmin Model, List<UserRole> userrole)
		{
			long num = DalContext.Insert(Model);
			if (num > 0 && userrole.Count > 0)
			{
				DalContext.ExecuteSql(" delete UserRole where userID=@userID ", new
				{
					userID = Model.ID
				});
				DalContext.InsertBat(userrole);
			}
			return num > 0;
		}

		public bool AddAdminAmt(HmAdminAmt Model)
		{
			return DalContext.Insert(Model) > 0;
		}

		public int UpdaeRate(string NickName, decimal Rate, string Id, List<UserRole> userrole)
		{
			int num = DalContext.ExecuteSql(" update HmAdmin set NickName=@NickName ,Rate=@Rate where id =@Id ", new
			{
				NickName,
				Rate,
				Id
			});
			if (num > 0 && userrole.Count > 0)
			{
				DalContext.ExecuteSql(" delete UserRole where userID=@userID ", new
				{
					userID = Id
				});
				DalContext.InsertBat(userrole);
			}
			return num;
		}

		public HmAdminAmt GetAdminAmt(string id)
		{
			return DalContext.GetModel<HmAdminAmt>(" select * from HmAdminAmt where AdminId=@AdminId ", new
			{
				AdminId = id
			});
		}

		public List<HmAdmin> GetAllHmAdmin()
		{
			return DalContext.GetList<HmAdmin>(" select * from HmAdmin where IsEnable=1 order by AddTime asc ");
		}
	}
}
