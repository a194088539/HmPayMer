using HM.DAL;
using HM.Framework.Dapper;
using HmPMer.Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace HmPMer.Dal
{
	public class SystemDal
	{
		public EmailSet GetEmailSetForCode(string EmialCode)
		{
			return DalContext.GetModel<EmailSet>(" select top 1 * from EmailSet where EmialCode=@EmialCode ", new
			{
				EmialCode
			});
		}

		public bool UpdateEmailSet(EmailSet Model)
		{
			return DalContext.Update(Model);
		}

		public SmsSet GetSmsSetForCode(string SmsCode)
		{
			return DalContext.GetModel<SmsSet>(" select top 1 * from SmsSet where SmsCode=@SmsCode ", new
			{
				SmsCode
			});
		}

		public bool UpdateSmsSet(SmsSet Model)
		{
			return DalContext.Update(Model);
		}

		public List<SmsModel> GetSmsModelList()
		{
			return DalContext.GetList<SmsModel>(" select * from SmsModel ");
		}

		public bool UpdateSmsModel(List<SmsModel> Model)
		{
			bool result = false;
			foreach (SmsModel item in Model)
			{
				result = DalContext.Update(item);
			}
			return result;
		}

		public SmsModel GetSmsModel(string code)
		{
			return DalContext.GetModel<SmsModel>(" SELECT * FROM [SmsModel] WHERE Code=@Code ", new
			{
				Code = code
			});
		}

		public long AddSmsTrans(SmsTrans Model)
		{
			return DalContext.Insert(Model);
		}

		public List<SmsTrans> GetSmsTransPageList(SmsTrans parm, ref Paging paging)
		{
			string str = " select count(*) from SmsTrans A WHERE 1=1  ";
			string text = "";
			if (!string.IsNullOrEmpty(parm.Mobile))
			{
				text = text + " And A.Mobile ='" + parm.Mobile + "'";
			}
			string sql = " select * from SmsTrans A Where 1=1  " + text;
			str += text;
			return DalContext.GetPage<SmsTrans>(sql, str, " * ", " Addtime desc ", ref paging);
		}

		public int BackDataBase(string Path)
		{
			return DalContext.ExecuteSql(" backup database HmPayMerchant to disk=@Path ", new
			{
				Path
			});
		}

		public List<PayTypeQuota> GetPayTypeQuotaList()
		{
			return DalContext.GetList<PayTypeQuota>(" select A.PayCode, B.minMoney,B.maxMoney,A.PayName from PayType A Left Join PayTypeQuota B On A.PayCode=B.PayCode ");
		}

		public long SetPayTypeQuota(List<PayTypeQuota> ListModel)
		{
			int num = DalContext.ExecuteSql(" delete PayTypeQuota  ");
			if (ListModel != null && ListModel.Count > 0)
			{
				foreach (PayTypeQuota item in ListModel)
				{
					item.minMoney *= 100m;
					item.maxMoney *= 100m;
					item.addtime = DateTime.Now;
				}
				return DalContext.InsertBat(ListModel);
			}
			return num;
		}

		public PayTypeQuota GetPayTypeQuotaForPayCode(string payCode)
		{
			return DalContext.GetModel<PayTypeQuota>("select * from PayTypeQuota where PayCode = @PayCode", new
			{
				PayCode = payCode
			});
		}

		public DateTime GetTDate(DateTime StarTime, int Day)
		{
			DynamicParameters dynamicParameters = new DynamicParameters();
			dynamicParameters.Add("@TimeNow", StarTime);
			dynamicParameters.Add("@DayCount", Day);
			dynamicParameters.Add("@NestTime", null, DbType.DateTime, ParameterDirection.Output);
			DalContext.ExecuteProce("proc_GetNextWorkDay", dynamicParameters);
			return dynamicParameters.Get<DateTime>("@NestTime");
		}

		public List<Notice> GetNoticePageList(Notice parm, ref Paging paging)
		{
			string str = " select count(*) from Notice A WHERE 1=1  ";
			string text = "";
            parm.Title = DalContext.EscapeString(parm.Title);
            if (!string.IsNullOrEmpty(parm.Title))
			{
				text = text + " And ( A.Title like '%" + parm.Title + "%')";
			}
			string sql = " select * from Notice A Where 1=1  " + text;
			str += text;
			return DalContext.GetPage<Notice>(sql, str, " * ", " Addtime desc ", ref paging);
		}

		public List<NoticeInfo> GetNoticeInfoPageList(NoticeInfo parm, ref Paging paging)
		{
			string str = " select count(*) from Notice A WHERE 1=1 And IsRelease=1 ";
			string text = "";
            parm.Title = DalContext.EscapeString(parm.Title);
            if (!string.IsNullOrEmpty(parm.Title))
			{
				text = text + " And ( A.Title like '%" + parm.Title + "%')";
			}
            parm.NoticeType = DalContext.EscapeString(parm.NoticeType);
            if (!string.IsNullOrEmpty(parm.NoticeType))
			{
				text = text + " And A.NoticeType='" + parm.NoticeType + "'";
			}
			string sql = " select * from Notice A Where 1=1 And IsRelease=1 " + text;
			str += text;
			return DalContext.GetPage<NoticeInfo>(sql, str, " * ", " Addtime desc ", ref paging);
		}

		public Notice GetNoticeModel(string Id)
		{
			return DalContext.GetModel<Notice>(" select * from [Notice] where Id=@Id ", new
			{
				Id
			});
		}

		public NoticeInfo GetNoticeInfoModel(string Id)
		{
			return DalContext.GetModel<NoticeInfo>(" select * from [Notice] where Id=@Id ", new
			{
				Id
			});
		}

		public ReadNotice GetReadNotice(string NoticeId, string UserId)
		{
			return DalContext.GetModel<ReadNotice>(" select * from ReadNotice where UserId=@UserId And NoticeId=@NoticeId ", new
			{
				UserId,
				NoticeId
			});
		}

		public long AddReadNotice(ReadNotice Model)
		{
			return DalContext.Insert(Model);
		}

		public long AddNotice(Notice Model)
		{
			return DalContext.Insert(Model);
		}

		public int UpdateNotice(Notice Model)
		{
			return DalContext.ExecuteSql(" update Notice set Content=@Content,Title=@Title where Id=@Id ", Model);
		}

		public int ReleaseNotice(Notice Model)
		{
			return DalContext.ExecuteSql(" update Notice set IsRelease=@IsRelease where Id=@Id ", Model);
		}

		public int DelNotice(string Id)
		{
			return DalContext.ExecuteSql(" delete Notice where Id=@Id ", new
			{
				Id
			});
		}

		public NoticeInfo GetNewNoticeInfo(int NoticeType)
		{
			return DalContext.GetModel<NoticeInfo>(" select top 1 * from Notice where NoticeType=@NoticeType And IsRelease=1 order by Addtime desc ", new
			{
				NoticeType
			});
		}

		public List<NoticeInfo> GetNewListNoticeInfo(int NoticeType)
		{
			return DalContext.GetList<NoticeInfo>(" select top 7 * from Notice where NoticeType=@NoticeType And IsRelease=1 order by Addtime desc ", new
			{
				NoticeType
			});
		}

		public BankLasalle GetBankLasalleCode(string BankLasalleCode)
		{
			return DalContext.GetModel<BankLasalle>(" select * from BankLasalle where BankLasalleCode=@BankLasalleCode ", new
			{
				BankLasalleCode
			});
		}

		public List<BankLasalle> GetListBankLasalle(string BankCode, int Proid, int Cityid)
		{
			return DalContext.GetList<BankLasalle>(" select * from BankLasalle where BankCode=@BankCode And Proid=@Proid And Cityid=@Cityid ", new
			{
				BankCode,
				Proid,
				Cityid
			});
		}

		public List<BankLasalleInfo> GetBankLasalleList(BankLasalleInfo parm, ref Paging paging)
		{
			string str = " select count(*) from  BankLasalle A \r\n                left join District B On A.proid = B.id\r\n                left join District C On A.Cityid = C.id Where 1 = 1  ";
			string text = "";
            parm.BankLasalleName = DalContext.EscapeString(parm.BankLasalleName);
            if (!string.IsNullOrEmpty(parm.BankLasalleName))
			{
				text = text + " And A.BankLasalleName like '%" + parm.BankLasalleName + "%'";
			}
			if (parm.Proid > -1)
			{
				text = text + " And A.Proid=" + parm.Proid;
			}
			if (parm.Cityid > -1)
			{
				text = text + " And A.Cityid=" + parm.Cityid;
			}
			string sql = " select A.*,B.Name ProName,C.Name CityName from  BankLasalle A \r\n                left join District B On A.proid=B.id\r\n                left join District C On A.Cityid=C.id Where 1=1  " + text;
			str += text;
			return DalContext.GetPage<BankLasalleInfo>(sql, str, " * ", " BankLasalleCode asc ", ref paging);
		}

		public int UpdateBankLasalle(string BankLasalleCode, int ProvinceId, int CityId)
		{
			return DalContext.ExecuteSql(" update BankLasalle set  proid=@ProvinceId , Cityid=@CityId where BankLasalleCode in(" + BankLasalleCode + ") ", new
			{
				ProvinceId,
				CityId
			});
		}

		public long InserBehaviorLog(BehaviorLog Model)
		{
			return DalContext.Insert(Model);
		}

		public List<BehaviorLog> GetBehaviorLogList(BehaviorLog param, ref Paging paging)
		{
			string str = " select count(*) from BehaviorLog A where 1=1 ";
			string text = "";
            param.BlName = DalContext.EscapeString(param.BlName);
            param.createUser = DalContext.EscapeString(param.createUser);
            if (!string.IsNullOrEmpty(param.BlName))
			{
				text = text + " And  A.BlName='" + param.BlName + "'";
			}
			if (!string.IsNullOrEmpty(param.createUser))
			{
				text = text + " And  A.createUser='" + param.createUser + "'";
			}
			if (param.BlType > -1)
			{
				text = text + " And  A.BlType=" + param.BlType;
			}
			if (param.BeginTime.HasValue)
			{
				text += $" AND A.[addTime]>='{param.BeginTime.Value:yyyy-MM-dd HH:mm:ss}' ";
			}
			if (param.EndTime.HasValue)
			{
				text += $" AND A.[addTime]<='{param.EndTime.Value:yyyy-MM-dd HH:mm:ss}' ";
			}
			string sql = " select * from BehaviorLog A where 1=1 " + text;
			str += text;
			return DalContext.GetPage<BehaviorLog>(sql, str, " * ", " addTime desc ", ref paging);
		}
	}
}
