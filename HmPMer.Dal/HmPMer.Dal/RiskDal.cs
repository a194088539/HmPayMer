using HM.DAL;
using HmPMer.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace HmPMer.Dal
{
	public class RiskDal
	{
		public long AddRiskScheme(RiskScheme model)
		{
			return DalContext.Insert(model);
		}

		public bool UpdateRiskScheme(RiskScheme model)
		{
			return DalContext.Update(model);
		}

		public bool DeleteRiskScheme(string id)
		{
			return DalContext.ExecuteSql(" DELETE FROM RiskScheme WHERE RiskSchemeId=@RiskSchemeId ", new
			{
				RiskSchemeId = id
			}) > 0;
		}

		public RiskScheme GetRiskSchemeModel(string id)
		{
			return DalContext.GetModel<RiskScheme>(" SELECT * FROM RiskScheme WHERE RiskSchemeId=@RiskSchemeId ", new
			{
				RiskSchemeId = id
			});
		}

		public RiskScheme GetRiskSchemeModel(int riskType, string id)
		{
			return DalContext.GetModel<RiskScheme>("  SELECT TOP 1 * FROM RiskScheme WHERE RiskSchemeId IN (SELECT RiskSchemeId FROM RiskSetting WHERE RiskSettingType=@TYPEID AND TargetId=@ID) ", new
			{
				TYPEID = riskType,
				ID = id
			});
		}

		public List<RiskScheme> GetRiskSchemeList(RiskScheme param, ref Paging paging)
		{
			string str = " select count(*) from RiskScheme A WHERE 1=1  ";
			string text = " ";
			if (param.RiskSchemeTaype > -1)
			{
				text += $" AND A.RiskSchemeTaype={param.RiskSchemeTaype} ";
			}
			string sql = " SELECT * FROM RiskScheme AS A WHERE 1=1 " + text;
			str += text;
			return DalContext.GetPage<RiskScheme>(sql, str, " * ", " Sort asc ", ref paging, param);
		}

		public List<RiskScheme> GetRiskSchemeList(int riskSchemeType, string userId)
		{
			string text = " SELECT * FROM dbo.RiskScheme WHERE RiskSchemeTaype=@RiskSchemeTaype  ";
			if (riskSchemeType == 1 && !string.IsNullOrEmpty(userId))
			{
				text += " AND UserId=@UserId  ";
			}
			return DalContext.GetList<RiskScheme>(text, new
			{
				RiskSchemeTaype = riskSchemeType,
				UserId = userId
			});
		}

		public RiskSetting GetRiskSetting(int riskType, string targetId)
		{
			return DalContext.GetModel<RiskSetting>(" SELECT * FROM dbo.RiskSetting WHERE RiskSettingType=@RiskSettingType AND TargetId=@TargetId ", new
			{
				RiskSettingType = riskType,
				TargetId = targetId
			});
		}

		public bool SetRiskSetting(List<RiskSetting> list, string riskSchemeId, int riskType, int aloneRisk)
		{
			int count = list.Count;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat(" DELETE FROM dbo.RiskSetting WHERE RiskSettingType={0} ", riskType);
			stringBuilder.Append(" AND TargetId IN( ");
			for (int i = 0; i < count; i++)
			{
				RiskSetting riskSetting = list[i];
				if (i > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.AppendFormat("'{0}'", riskSetting.TargetId);
			}
			stringBuilder.Append(" ) ");
			DalContext.ExecuteSql(stringBuilder.ToString());
			if (aloneRisk == 1 && !string.IsNullOrEmpty(riskSchemeId))
			{
				list.ForEach(delegate(RiskSetting p)
				{
					p.RiskSettingId = Guid.NewGuid().ToString();
					p.RiskSchemeId = riskSchemeId;
					p.RiskSettingType = riskType;
				});
				DalContext.InsertBat(list);
			}
			return true;
		}

		public bool AddRiskTarnRecord(RiskTarnRecord record)
		{
			BuildId(record);
			return DalContext.ExecuteSql("\r\n\t     IF EXISTS(SELECT 1 FROM dbo.RiskTarnRecord WHERE TarnId=@TarnId)\r\n\t\t BEGIN\r\n\t\t\tUPDATE dbo.RiskTarnRecord SET TarnAmt=TarnAmt+@TarnAmt,TarnCount=TarnCount+@TarnCount WHERE TarnId=@TarnId\r\n\t\t END\r\n\t\t ELSE\r\n\t\t\tINSERT INTO dbo.RiskTarnRecord\r\n\t\t\t(\r\n\t\t\t    TarnId,\r\n\t\t\t    AccountType,\r\n\t\t\t    AccountId,\r\n\t\t\t    TarnDate,\r\n\t\t\t    TarnCount,\r\n\t\t\t    TarnAmt\r\n\t\t\t)\r\n\t\t\tVALUES\r\n\t\t\t(   @TarnId,      \r\n\t\t\t    @AccountType, \r\n\t\t\t    @AccountId,   \r\n\t\t\t    @TarnDate,    \r\n\t\t\t    1,            \r\n\t\t\t   @TarnAmt      \r\n\t\t\t    )", record) > 0;
		}

		public RiskTarnRecord GetRiskTarn(RiskTarnRecord record)
		{
			BuildId(record);
			return DalContext.GetModel<RiskTarnRecord>(" SELECT * FROM dbo.RiskTarnRecord WHERE TarnId=@TarnId ", record);
		}

		private void BuildId(RiskTarnRecord record)
		{
			record.TarnId = $"{record.TarnDate:yyyyMMdd}:{record.AccountType}:{record.AccountId}";
		}

		public bool AddRiskLimt(RiskLimit model)
		{
			bool num = DalContext.Insert(model) > 0;
			if (num)
			{
				DalContext.ExecuteSql(" UPDATE InterfaceAccount set IsEnabled=2 where Id=@ID ", new
				{
					ID = model.TargetId
				});
			}
			return num;
		}

		public int RemoveRiskLimt(RiskType riskType, DateTime time)
		{
			int num = DalContext.ExecuteSql(" UPDATE InterfaceAccount SET IsEnabled=1 WHERE IsEnabled=2 AND ID IN (SELECT TargetId FROM RiskLimit WHERE State=1 AND RiskType=@TYPE AND EndTime<=@TimeNow) ", new
			{
				TYPE = (int)riskType,
				TimeNow = time
			});
			if (num > 0)
			{
				DalContext.ExecuteSql("UPDATE RiskLimit SET State=2 WHERE State=1 AND RiskType=@TYPE AND EndTime<=@TimeNow", new
				{
					TYPE = (int)riskType,
					TimeNow = time
				});
			}
			return num;
		}

		public int DelRiskScheme(string RiskSchemeId)
		{
			return DalContext.ExecuteSql("delete RiskScheme where RiskSchemeId=@RiskSchemeId;\r\n                 delete RiskSetting where RiskSchemeId = @RiskSchemeId; ", new
			{
				RiskSchemeId
			});
		}
	}
}
