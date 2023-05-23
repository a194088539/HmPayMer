using HM.Framework;
using HmPMer.Dal.Innerface;
using HmPMer.Entity;
using System;
using System.Collections.Generic;

namespace HmPMer.Business
{
	public class UserBaseBll
	{
		private readonly UserBaseDal _dal = new UserBaseDal();

		public List<UserBaseInfo> LoadUserPage(UserBaseInfo param, ref Paging paging)
		{
			return _dal.LoadUserPage(param, ref paging);
		}

		public List<UserBase> GetUserBaseForType(int UserType)
		{
			return _dal.GetUserBaseForType(UserType);
		}

		public UserBase GetModelForMobile(string MobilePhone, string UserId)
		{
			return _dal.GetModelForMobile(MobilePhone, UserId);
		}

		public UserBase GetModelForEmail(string Email, string UserId)
		{
			return _dal.GetModelForEmail(Email, UserId);
		}

		public UserBaseInfo GetModelForId(string UserId)
		{
			return _dal.GetModelForId(UserId);
		}

		public UserDetail GetUserDetail(string UserId)
		{
			return _dal.GetUserDetail(UserId);
		}

		public UserAmt GetUserAmt(string userId)
		{
			return _dal.GetUserAmt(userId);
		}

		public int UpIsEnabled(string userId, int IsEnabled)
		{
			return _dal.UpIsEnabled(userId, IsEnabled);
		}

		public int RestPwd1(string Pass, string UserId)
		{
			return _dal.RestPwd1(Pass, UserId);
		}

		public int RestPwd2(string Pass2, string UserId)
		{
			return _dal.RestPwd2(Pass2, UserId);
		}

		public int RestPwdPhone(string Pass, string MobilePhone)
		{
			return _dal.RestPwdPhone(Pass, MobilePhone);
		}

		public long AddBusiness(UserBase model, int Type = 0)
		{
			model.UserId = $"{DateTime.Now:yyMMdd}{_dal.GetNewUserSeedVal():0000}";
			model.UserType = 1;
			if (!string.IsNullOrEmpty(model.Pass))
			{
				model.Pass = EncryUtils.MD5(model.Pass);
			}
			if (!string.IsNullOrEmpty(model.Pass2))
			{
				model.Pass2 = EncryUtils.MD5(model.Pass2);
			}
			model.RegTime = DateTime.Now;
			model.RegIp = Utils.GetClientIp();
			model.ApiKey = EncryUtils.MD5(Guid.NewGuid().ToString());
			if (Type == 1)
			{
				model.GradeId = new SysConfigBll().GetConfigVaule("RegGradeId");
				model.WithdrawSchemeId = new SysConfigBll().GetConfigVaule("RegWithdrawSchemeId");
				model.IsEnabled = Convert.ToInt32(new SysConfigBll().GetConfigVaule("RegIsEnabled"));
			}
			model.LastLoginIp = Utils.GetClientIp();
			model.LastLoginTime = DateTime.Now;
			long num = _dal.AddBusiness(model);
			if (num > 0)
			{
				UserAmt model2 = new UserAmt
				{
					UserId = model.UserId,
					Balance = decimal.Zero,
					Freeze = decimal.Zero,
					OrderAmt = decimal.Zero
				};
				_dal.CreateUserAmt(model2);
				UserDetail model3 = new UserDetail
				{
					UserId = model.UserId
				};
				_dal.InsertUserDetail(model3);
			}
			return num;
		}

		public int UpModelInfo(UserBase Model)
		{
			return _dal.UpModelInfo(Model);
		}

		public bool AuthIdCard(UserBase model)
		{
			return _dal.AuthIdCard(model);
		}

		public int AuthEmail(UserBase model)
		{
			return _dal.AuthEmail(model);
		}

		public int AuthMobile(UserBase model)
		{
			return _dal.AuthMobile(model);
		}

		public int UpdateAloneRate(int AloneRate, string UserId)
		{
			return _dal.UpdateAloneRate(AloneRate, UserId);
		}

		public int UpBusinessAgentPay(string UserId, int AgentPay)
		{
			return _dal.UpBusinessAgentPay(UserId, AgentPay);
		}

		public int DelBusiness(string Userid, string DeleteUser)
		{
			return _dal.DelBusiness(Userid, DeleteUser);
		}

		public long AddAgent(UserBase model)
		{
			model.UserId = "DL" + $"{DateTime.Now:yyMMdd}{_dal.GetNewUserSeedVal():0000}";
			model.UserType = 2;
			if (!string.IsNullOrEmpty(model.Pass))
			{
				model.Pass = EncryUtils.MD5(model.Pass);
			}
			if (!string.IsNullOrEmpty(model.Pass2))
			{
				model.Pass2 = EncryUtils.MD5(model.Pass2);
			}
			long num = _dal.AddBusiness(model);
			if (num > 0)
			{
				UserAmt model2 = new UserAmt
				{
					UserId = model.UserId,
					Balance = decimal.Zero,
					Freeze = decimal.Zero,
					OrderAmt = decimal.Zero
				};
				_dal.CreateUserAmt(model2);
				UserDetail model3 = new UserDetail
				{
					UserId = model.UserId
				};
				_dal.InsertUserDetail(model3);
			}
			return num;
		}

		public int AgentUpdate(UserBase Model)
		{
			int num = _dal.AgentUpdate(Model);
			if (num > 0)
			{
				if (!string.IsNullOrEmpty(Model.Pass))
				{
					new UserBaseDal().RestPwd1(EncryUtils.MD5(Model.Pass), Model.UserId);
				}
				if (!string.IsNullOrEmpty(Model.Pass2))
				{
					new UserBaseDal().RestPwd2(EncryUtils.MD5(Model.Pass2), Model.UserId);
				}
			}
			return num;
		}

		public UserBase LoginIn(UserBase user)
		{
			return _dal.LoginIn(user);
		}

		public int UpdateLogin(UserBase user)
		{
			return _dal.UpdateLogin(user);
		}

		public int UpUserWithdrawInfo(UserDetail model)
		{
			return _dal.UpUserWithdrawInfo(model);
		}

		public int AuditUserWithdraw(int WithdrawStatus, string WithdrawAuditDes, string UserId)
		{
			return _dal.AuditUserWithdraw(WithdrawStatus, WithdrawAuditDes, UserId);
		}

		public List<UserDetailInfo> GetUserDetailList(UserDetailInfo param, ref Paging paging)
		{
			return _dal.GetUserDetailList(param, ref paging);
		}

		public int UpIdcardInfo(UserDetail model)
		{
			return _dal.UpIdcardInfo(model);
		}

		public int UpIdcardCommpanyInfo(UserDetail model)
		{
			return _dal.UpIdcardCommpanyInfo(model);
		}

		public int AuditIdCard(int IdCardStatus, string IdCardAuditDes, string UserId)
		{
			return _dal.AuditIdCard(IdCardStatus, IdCardAuditDes, UserId);
		}

		public List<UserDetailInfo> GetUserIdCardDetailList(UserDetailInfo param, ref Paging paging)
		{
			return _dal.GetUserIdCardDetailList(param, ref paging);
		}

		public int AddBalanceUser(decimal Balance, string UserId)
		{
			return _dal.AddBalanceUser(Balance, UserId);
		}

		public List<Trade> GetTradeList(Trade param, ref Paging paging)
		{
			return _dal.GetTradeList(param, ref paging);
		}

		public long AddTrade(Trade Model)
		{
			return _dal.AddTrade(Model);
		}

		public string GetTradeId()
		{
			return Guid.NewGuid().ToString();
		}

		public long GetMaxTradeId()
		{
			return _dal.GetMaxTradeId();
		}
	}
}
