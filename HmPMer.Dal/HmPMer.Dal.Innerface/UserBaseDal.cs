using HM.DAL;
using HM.Framework;
using HM.Framework.Dapper;
using HmPMer.Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace HmPMer.Dal.Innerface
{
	public class UserBaseDal
	{
		public int GetNewUserSeedVal()
		{
			DynamicParameters dynamicParameters = new DynamicParameters();
			dynamicParameters.Add("@SeedKey", "USERID");
			dynamicParameters.Add("@SeedVal", 0, DbType.Int32, ParameterDirection.Output);
			DalContext.ExecuteProce("proc_GetUserSeedVal", dynamicParameters);
			return dynamicParameters.Get<int>("@SeedVal");
		}

		public List<UserBaseInfo> LoadUserPage(UserBaseInfo param, ref Paging paging)
		{
			string str = " SELECT COUNT(*) FROM UserBase A WHERE 1=1  ";
			string text = "";
            param.UserId = DalContext.EscapeString(param.UserId);
            param.MerName = DalContext.EscapeString(param.MerName);
            param.MobilePhone = DalContext.EscapeString(param.MobilePhone);
            param.Email = DalContext.EscapeString(param.Email);
            param.AgentId = DalContext.EscapeString(param.AgentId);
            param.PromId = DalContext.EscapeString(param.PromId);
            if (param.UserType > -1)
			{
				text += $" AND A.UserType = {param.UserType} ";
			}
			if (!string.IsNullOrEmpty(param.UserId))
			{
				text += $" AND A.UserId = '{param.UserId}' ";
			}
			if (!string.IsNullOrEmpty(param.MerName))
			{
				text += $" AND A.MerName LIKE '%{param.MerName}%' ";
			}
			if (!string.IsNullOrEmpty(param.MobilePhone))
			{
				text += $" AND A.MobilePhone LIKE '%{param.MobilePhone}%' ";
			}
			if (!string.IsNullOrEmpty(param.Email))
			{
				text += $" AND A.Email LIKE '%{param.Email}%' ";
			}
			if (!string.IsNullOrEmpty(param.AgentId))
			{
				text += $" AND A.AgentId = '{param.AgentId}' ";
			}
			if (!string.IsNullOrEmpty(param.PromId))
			{
				text += $" AND A.PromId = '{param.PromId}' ";
			}
			if (param.AloneRate > -1)
			{
				text += $" AND A.AloneRate = {param.AloneRate} ";
			}
			if (param.IsEnabled > -1)
			{
				text += $" AND A.IsEnabled = {param.IsEnabled} ";
			}
			if (param.Type == 1)
			{
				text += $" And A.IsEnabled=2 ";
			}
			if (param.Type == 2)
			{
				text += $" And A.IsEnabled=0 ";
			}
			string sql = "SELECT\r\n                        A.*\r\n                        , B.OrderAmt,B.Freeze,B.UnBalance,B.Balance\r\n                        , D.RiskSchemeId\r\n                        , D.SchemeName as [RiskSchemeName]\r\n                        from UserBase AS A  \r\n                        JOIN UserAmt  AS B ON A.UserId=B.UserId\r\n                        LEFT JOIN RiskSetting AS C ON A.UserId=C.TargetId AND C.RiskSettingType=2\r\n                        LEFT JOIN RiskScheme  AS D ON C.RiskSchemeId=D.RiskSchemeId\r\n                        Where 1=1  " + text;
			str += text;
			return DalContext.GetPage<UserBaseInfo>(sql, str, " * ", " RegTime DESC ", ref paging);
		}

		public List<UserBase> GetUserBaseForType(int UserType)
		{
			return DalContext.GetList<UserBase>(" select * from UserBase Where UserType=@UserType ", new
			{
				Usertype = UserType
			});
		}

		public UserBase GetModelForMobile(string MobilePhone, string UserId)
		{
			string text = " select * from UserBase Where MobilePhone=@MobilePhone ";

            UserId = DalContext.EscapeString(UserId);
            if (!string.IsNullOrEmpty(UserId))
			{
				text = text + " And UserId!='" + UserId + "' ";
			}
			return DalContext.GetModel<UserBase>(text, new
			{
				MobilePhone
			});
		}

		public UserBase GetModelForEmail(string Email, string UserId)
		{
			string text = " select * from UserBase Where Email=@Email ";
            UserId = DalContext.EscapeString(UserId);
            if (!string.IsNullOrEmpty(UserId))
			{
				text = text + " And UserId!='" + UserId + "' ";
			}
			return DalContext.GetModel<UserBase>(text, new
			{
				Email
			});
		}

		public UserBaseInfo GetModelForId(string UserId)
		{
			return DalContext.GetModel<UserBaseInfo>(" select A.*,B.SchemeName,C.GradeName,D.Balance,D.Freeze,D.OrderAmt,D.UnBalance\r\n                    from UserBase A Left Join WithdrawScheme B On A.WithdrawSchemeId=B.Id\r\n                    Left Join UserGrade C On A.GradeId=C.Id\r\n                    Left Join UserAmt D On A.UserId=D.UserId\r\n                    Where A.UserId=@UserId ", new
			{
				UserId
			});
		}

		public UserDetail GetUserDetail(string UserId)
		{
			return DalContext.GetModel<UserDetail>(" select * from UserDetail where UserId=@UserId ", new
			{
				UserId
			});
		}

		public UserAmt GetUserAmt(string userId)
		{
			return DalContext.GetModel<UserAmt>(" select top 1 * from UserAmt where UserId=@UserId ", new
			{
				UserId = userId
			});
		}

		public int UpIsEnabled(string userId, int IsEnabled)
		{
			return DalContext.ExecuteSql(" update UserBase set IsEnabled=@IsEnabled where userId=@userId ", new
			{
				userId,
				IsEnabled
			});
		}

		public int RestPwd1(string Pass, string UserId)
		{
			return DalContext.ExecuteSql(" update userbase set Pass=@Pass where UserId=@UserId ", new
			{
				Pass,
				UserId
			});
		}

		public int RestPwd2(string Pass2, string UserId)
		{
			return DalContext.ExecuteSql(" update userbase set Pass2=@Pass2 where UserId=@UserId ", new
			{
				Pass2,
				UserId
			});
		}

		public int RestPwdPhone(string Pass, string MobilePhone)
		{
			return DalContext.ExecuteSql(" update userbase set Pass=@Pass  where MobilePhone=@MobilePhone ", new
			{
				Pass,
				MobilePhone
			});
		}

		public long AddBusiness(UserBase Model)
		{
			return DalContext.Insert(Model);
		}

		public bool CreateUserAmt(UserAmt model)
		{
			return DalContext.Insert(model) > 0;
		}

		public int UpModelInfo(UserBase Model)
		{
			int num = DalContext.ExecuteSql(" Update UserBase Set MerName=@MerName,PromId=@PromId,GradeId=@GradeId,MobilePhone=@MobilePhone,AgentId=@AgentId,\r\n                            QQ=@QQ,Email=@Email,IsMobilePhone=@IsMobilePhone,ApiKey=@ApiKey,IsEmail=@IsEmail,WithdrawSchemeId=@WithdrawSchemeId Where UserId=@UserId ", Model);
			if (num > 0)
			{
				if (!string.IsNullOrEmpty(Model.Pass))
				{
					RestPwd1(EncryUtils.MD5(Model.Pass), Model.UserId);
				}
				if (!string.IsNullOrEmpty(Model.Pass2))
				{
					RestPwd2(EncryUtils.MD5(Model.Pass2), Model.UserId);
				}
			}
			return num;
		}

		public int UpdateAloneRate(int AloneRate, string UserId)
		{
			return DalContext.ExecuteSql(" update UserBase set AloneRate=@AloneRate where UserId=@UserId ", new
			{
				AloneRate,
				UserId
			});
		}

		public int UpBusinessAgentPay(string UserId, int AgentPay)
		{
			return DalContext.ExecuteSql(" update UserBase set AgentPay=@AgentPay where UserId=@UserId ", new
			{
				UserId,
				AgentPay
			});
		}

		public int DelBusiness(string Userid, string DeleteUser)
		{
			return DalContext.ExecuteSqlTransaction(new List<Tuple<string, object>>
			{
				new Tuple<string, object>(" Insert into UserBaseBack\r\n                select A.*, Balance, UnBalance, SettleBalance, Freeze, OrderAmt, CompanyName, LicenseId, FactName, IdCard, CompanyProId,\r\n                CompanyProName, CompanyCityId, CompanyCityName, CompanyDicId, CompanyDicName, [Address], CustTel, IdCardImg1, IdCardImg2, LicenseImg,\r\n                WithdrawAccountType, WithdrawChannelCode, WithdrawBank, WithdrawFactName, WithdrawBankCode, WithdrawProvinceId, WithdrawProvince, WithdrawCityId, WithdrawCity,\r\n                WithdrawBankBranch, WithdrawBankLasalleCode, WithdrawReservedPhone, WithdrawData1, WithdrawData2, DeleteTime = GetDate(), DeleteUser =@DeleteUser\r\n                from UserBase A Inner Join UserAmt B on A.userId = B.userId\r\n                Inner Join UserDetail C On A.UserId = C.UserID where A.Userid = @Userid ", new
				{
					Userid,
					DeleteUser
				}),
				new Tuple<string, object>("delete UserBase where UserId=@Userid ", new
				{
					Userid
				}),
				new Tuple<string, object>("delete UserAmt where UserId=@Userid ", new
				{
					Userid
				}),
				new Tuple<string, object>("delete UserDetail where UserId=@Userid ", new
				{
					Userid
				})
			});
		}

		public int AgentUpdate(UserBase Model)
		{
			return DalContext.ExecuteSql(" update UserBase set MobilePhone=@MobilePhone,Email=@Email,QQ=@QQ,GradeId=@GradeId,\r\n                MerName=@MerName,WithdrawSchemeId=@WithdrawSchemeId,AccountType=@AccountType\r\n                where UserId=@UserId ", Model);
		}

		public UserBase LoginIn(UserBase Model)
		{
			return DalContext.GetModel<UserBase>(" select A.* from UserBase A where (A.MobilePhone=@MobilePhone or A.Email=@Email) and A.Pass=@Pass AND A.UserType=@UserType ", Model);
		}

		public int UpdateLogin(UserBase user)
		{
			return DalContext.ExecuteSql(" update UserBase set LastLoginTime=GETDATE(),LastLoginIp=@LastLoginIp where UserId=@UserId ", user);
		}

		public bool AuthIdCard(UserBase model)
		{
			return DalContext.ExecuteSql(" UPDATE UserBase SET FactName=@FactName, IdCard=@IdCard, IsIdCard=@IsIdCard WHERE UserId=@UserId ", model) > 0;
		}

		public int AuthEmail(UserBase model)
		{
			return DalContext.ExecuteSql(" Update UserBase Set IsEmail=@IsEmail Where UserId=@UserId And Email=@Email ", model);
		}

		public int AuthMobile(UserBase model)
		{
			return DalContext.ExecuteSql(" Update UserBase Set IsMobilePhone=@IsMobilePhone,MobilePhone=@MobilePhone Where UserId=@UserId ", model);
		}

		public void InsertUserDetail(UserDetail model)
		{
			DalContext.Insert(model);
		}

		public int UpUserWithdrawInfo(UserDetail model)
		{
			int num = DalContext.ExecuteSql(" update UserDetail set WithdrawAccountType=@WithdrawAccountType,WithdrawChannelCode=@WithdrawChannelCode,\r\n            WithdrawBank=@WithdrawBank,WithdrawFactName=@WithdrawFactName,WithdrawBankCode=@WithdrawBankCode,WithdrawProvinceId=@WithdrawProvinceId,\r\n            WithdrawProvince=@WithdrawProvince,WithdrawCityId=@WithdrawCityId,WithdrawCity=@WithdrawCity,WithdrawBankBranch=@WithdrawBankBranch,\r\n            WithdrawBankLasalleCode=@WithdrawBankLasalleCode,WithdrawReservedPhone=@WithdrawReservedPhone,WithdrawData1=@WithdrawData1,WithdrawData2=@WithdrawData2\r\n            where UserId=@UserId  ", model);
			if (num > 0)
			{
				DalContext.ExecuteSql(" update userbase set WithdrawStatus=3,WithdrawTime=null,WithdrawAuditDes='' where UserId=@UserId ", new
				{
					model.UserId
				});
			}
			return num;
		}

		public int AuditUserWithdraw(int WithdrawStatus, string WithdrawAuditDes, string UserId)
		{
			return DalContext.ExecuteSql(" update userbase set WithdrawStatus=@WithdrawStatus,WithdrawTime=getdate(),WithdrawAuditDes=@WithdrawAuditDes where UserId=@UserId ", new
			{
				WithdrawStatus,
				WithdrawAuditDes,
				UserId
			});
		}

		public List<UserDetailInfo> GetUserDetailList(UserDetailInfo param, ref Paging paging)
		{
			string str = " select count(*) from UserDetail A Inner Join UserBase B On A.UserId = B.UserId Where 1 = 1  ";
			string str2 = "";
            param.UserId = DalContext.EscapeString(param.UserId);

            if (!string.IsNullOrEmpty(param.UserId))
			{
				str2 += $" AND A.UserId = '{param.UserId}' ";
			}
			if (param.UserType > -1)
			{
				str2 += $" AND B.AccountType = {param.UserType} ";
			}
			str2 = ((param.WithdrawStatus <= -1) ? (str2 + $" AND B.WithdrawStatus != 0 ") : (str2 + $" AND B.WithdrawStatus = {param.WithdrawStatus} "));
			string sql = " select A.*,B.AccountType,B.WithdrawStatus,B.WithdrawTime,B.WithdrawAuditDes\r\n from UserDetail A Inner Join UserBase B On A.UserId=B.UserId Where 1=1  " + str2;
			str += str2;
			return DalContext.GetPage<UserDetailInfo>(sql, str, " * ", " WithdrawTime DESC ", ref paging);
		}

		public int UpIdcardInfo(UserDetail model)
		{
			int num = DalContext.ExecuteSql(" update UserDetail set  FactName=@FactName,IdCard=@IdCard,IdCardImg1=@IdCardImg1,IdCardImg2=@IdCardImg2 where UserId=@UserId  ", model);
			if (num > 0)
			{
				DalContext.ExecuteSql(" update userbase set IdCardStatus=3,IdCardTime=null,IdCardAuditDes='' where UserId=@UserId ", new
				{
					model.UserId
				});
			}
			return num;
		}

		public int UpIdcardCommpanyInfo(UserDetail model)
		{
			int num = DalContext.ExecuteSql(" update UserDetail set CompanyName=@CompanyName,CompanyProId=@CompanyProId,CompanyProName=@CompanyProName,\r\n                            CompanyCityId=@CompanyCityId,CompanyCityName=@CompanyCityName,CompanyDicId=@CompanyDicId,CompanyDicName=@CompanyDicName,\r\n                            LicenseId=@LicenseId,Address=@Address,CustTel=@CustTel,IdCardImg1=@IdCardImg1,IdCardImg2=@IdCardImg2,LicenseImg=@LicenseImg,\r\n                       FactName=@FactName,IdCard=@IdCard where UserId=@UserId  ", model);
			if (num > 0)
			{
				DalContext.ExecuteSql(" update userbase set IdCardStatus=3,IdCardTime=null,IdCardAuditDes='' where UserId=@UserId ", new
				{
					model.UserId
				});
			}
			return num;
		}

		public int AuditIdCard(int IdCardStatus, string IdCardAuditDes, string UserId)
		{
			return DalContext.ExecuteSql(" update userbase set IdCardStatus=@IdCardStatus,IdCardTime=getdate(),IdCardAuditDes=@IdCardAuditDes where UserId=@UserId ", new
			{
				IdCardStatus,
				IdCardAuditDes,
				UserId
			});
		}

		public List<UserDetailInfo> GetUserIdCardDetailList(UserDetailInfo param, ref Paging paging)
		{
			string str = " select count(*) from UserDetail A Inner Join UserBase B On A.UserId = B.UserId Where 1 = 1  ";
			string str2 = "";
            param.UserId = DalContext.EscapeString(param.UserId);
            if (!string.IsNullOrEmpty(param.UserId))
			{
				str2 += $" AND A.UserId = '{param.UserId}' ";
			}
			if (param.UserType > -1)
			{
				str2 += $" AND B.UserType = {param.UserType} ";
			}
			str2 = ((param.IdCardStatus <= -1) ? (str2 + $" AND B.IdCardStatus != 0 ") : (str2 + $" AND B.IdCardStatus = {param.IdCardStatus} "));
			string sql = " select A.*,B.AccountType,B.IdCardStatus,B.IdCardTime,B.IdCardAuditDes\r\n from UserDetail A Inner Join UserBase B On A.UserId=B.UserId Where 1=1  " + str2;
			str += str2;
			return DalContext.GetPage<UserDetailInfo>(sql, str, " * ", " IdCardTime DESC ", ref paging);
		}

		public int AddBalanceUser(decimal Balance, string UserId)
		{
			return DalContext.ExecuteSql(" update UserAmt set Balance=Balance + @Balance  Where UserId=@UserId  ", new
			{
				Balance,
				UserId
			});
		}

		public List<Trade> GetTradeList(Trade param, ref Paging paging)
		{
			string str = " select count(*) FROM trade A where 1=1  ";
			string text = "";
            param.UserId = DalContext.EscapeString(param.UserId);
            if (param.BillType > -1)
			{
				text = text + " And  A.BillType=" + param.BillType;
			}
            if (!string.IsNullOrEmpty(param.UserId))
			{
				text = text + " And  A.UserId='" + param.UserId + "'";
			}
			if (param.BeginTime.HasValue)
			{
				text += $" AND A.[TradeTime]>='{param.BeginTime.Value:yyyy-MM-dd HH:mm:ss}' ";
			}
			if (param.EndTime.HasValue)
			{
				text += $" AND A.[TradeTime]<='{param.EndTime.Value:yyyy-MM-dd HH:mm:ss}' ";
			}
			string sql = " select * from trade A where 1=1 " + text;
			str += text;
			return DalContext.GetPage<Trade>(sql, str, " * ", "  TradeTime Desc ", ref paging, param);
		}

		public long AddTrade(Trade Model)
		{
			return DalContext.Insert(Model);
		}

		public long GetMaxTradeId()
		{
			return DalContext.GetSingVal<long>(" select MAX(TradeId) from Trade where CONVERT( varchar(10),TradeTime,20) = CONVERT(varchar(10), getdate(),20)  ");
		}
	}
}
