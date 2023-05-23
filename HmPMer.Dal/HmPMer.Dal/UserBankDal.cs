using HM.DAL;
using HmPMer.Entity;
using System.Collections.Generic;

namespace HmPMer.Dal
{
	public class UserBankDal
	{
		public List<UsersPayBank> LoadUserBankPage(UsersPayBank param, ref Paging paging)
		{
			string str = " SELECT COUNT(*) FROM UsersPayBank WHERE 1=1  ";
			string text = "";
            param.UserId = DalContext.EscapeString(param.UserId);
            if (!string.IsNullOrEmpty(param.UserId))
			{
				text += $" And UserId='{param.UserId}' ";
			}
			string sql = " select * from UsersPayBank Where 1=1  " + text;
			str += text;
			return DalContext.GetPage<UsersPayBank>(sql, str, " * ", " AddTime desc ", ref paging);
		}

		public UsersPayBank GetBankForId(string Id)
		{
			return DalContext.GetModel<UsersPayBank>(" select * from UsersPayBank Where Id=@Id  ", new
			{
				Id
			});
		}

		public UsersPayBank GetBankForCode(string BankCode)
		{
			return DalContext.GetModel<UsersPayBank>(" select * from UsersPayBank Where BankCode=@BankCode  ", new
			{
				BankCode
			});
		}

		public List<UsersPayBank> GetBankList(string UserId)
		{
			return DalContext.GetList<UsersPayBank>(" select * from UsersPayBank Where UserId=@UserId  ", new
			{
				UserId
			});
		}

		public long AddUserBank(UsersPayBank Model)
		{
			return DalContext.Insert(Model);
		}

		public bool UpdateUserBank(UsersPayBank Model)
		{
			return DalContext.Update(Model);
		}

		public bool RemoveUserBank(string Id)
		{
			return DalContext.ExecuteSql(" delete UsersPayBank Where Id=@Id  ", new
			{
				Id
			}) > 0;
		}
	}
}
