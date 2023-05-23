using HmPMer.Dal;
using HmPMer.Entity;
using System.Collections.Generic;

namespace HmPMer.Business
{
	public class UserBankBll
	{
		private readonly UserBankDal _dal = new UserBankDal();

		public List<UsersPayBank> LoadUserBankPage(UsersPayBank param, ref Paging paging)
		{
			return _dal.LoadUserBankPage(param, ref paging);
		}

		public UsersPayBank GetBankForId(string Id)
		{
			return _dal.GetBankForId(Id);
		}

		public UsersPayBank GetBankForCode(string BankCode)
		{
			return _dal.GetBankForCode(BankCode);
		}

		public List<UsersPayBank> GetBankList(string UserId)
		{
			return _dal.GetBankList(UserId);
		}

		public long AddUserBank(UsersPayBank Model)
		{
			return _dal.AddUserBank(Model);
		}

		public bool UpdateUserBank(UsersPayBank Model)
		{
			return _dal.UpdateUserBank(Model);
		}

		public bool RemoveUserBank(string Id)
		{
			return _dal.RemoveUserBank(Id);
		}
	}
}
