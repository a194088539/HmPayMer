using HM.Framework.Caching;
using HmPMer.Dal;
using HmPMer.Entity;
using System.Collections.Generic;

namespace HmPMer.Business
{
	public class AccountBll
	{
		private readonly AccountDal _dal = new AccountDal();

		public HmAdmin AdminLoginIn(HmAdmin model)
		{
			return _dal.AdminLoginIn(model);
		}

		public HmAdmin GetHmAdmin(string Id)
		{
			ICache caching = CachingFactory.GetCaching();
			string key = CachingKey.CreateHmAdmin(Id);
			HmAdmin hmAdmin = caching.Get<HmAdmin>(key);
			if (hmAdmin == null)
			{
				hmAdmin = _dal.GetHmAdmin(Id);
				if (hmAdmin != null)
				{
					caching.Add(key, hmAdmin);
				}
			}
			return hmAdmin;
		}

		public HmAdmin GetHmAdminAdmUser(string AdmUser)
		{
			return _dal.GetHmAdminAdmUser(AdmUser);
		}

		public List<HmAdmin> LoadAdminPage(HmAdmin param, ref Paging paging)
		{
			return _dal.LoadAdminPage(param, ref paging);
		}

		public int UpdateLastLoginIp(string LastLoginIp, string AdmUser)
		{
			return _dal.UpdateLastLoginIp(LastLoginIp, AdmUser);
		}

		public int UpIsEnabled(string userId, int IsEnabled)
		{
			return _dal.UpIsEnabled(userId, IsEnabled);
		}

		public int RestPwd(string AdmPass, string AdmPass2, string Id)
		{
			return _dal.RestPwd(AdmPass, AdmPass2, Id);
		}

		public int UpdateAdmPass(string AdmPass, string Id)
		{
			return _dal.UpdateAdmPass(AdmPass, Id);
		}

		public int deleteHmAdmin(string Id)
		{
			return _dal.deleteHmAdmin(Id);
		}

		public bool Add(HmAdmin Model, List<UserRole> userrole)
		{
			bool num = _dal.Add(Model, userrole);
			if (num)
			{
				_dal.AddAdminAmt(new HmAdminAmt
				{
					AdminId = Model.ID,
					Balance = decimal.Zero,
					TotalBalance = decimal.Zero,
					OrderNum = 0,
					OrderAmt = decimal.Zero
				});
				ICache caching = CachingFactory.GetCaching();
				string key = CachingKey.CreateHmAdmin(Model.ID);
				caching.Remove(key);
			}
			return num;
		}

		public int UpdaeRate(string NickName, decimal Rate, string Id, List<UserRole> userrole)
		{
			int num = _dal.UpdaeRate(NickName, Rate, Id, userrole);
			if (num > 0)
			{
				ICache caching = CachingFactory.GetCaching();
				string key = CachingKey.CreateHmAdmin(Id);
				caching.Remove(key);
			}
			return num;
		}

		public HmAdminAmt GetAdminAmt(string id)
		{
			return _dal.GetAdminAmt(id);
		}

		public List<HmAdmin> GetAllHmAdmin()
		{
			return _dal.GetAllHmAdmin();
		}
	}
}
