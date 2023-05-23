using HM.DAL;
using HmPMer.Entity;
using System.Collections.Generic;
using System.Data;

namespace HmPMer.Dal
{
	public class RateDal
	{
		public List<PayRateInfo> GetPayRateList(string UserId, int Type)
		{
			return DalContext.GetList<PayRateInfo>(" select B.PayCode,B.PayName,A.AccountScheme,C.* from InterfaceType A \r\n            inner join PayType B On A.PayCode=B.PayCode \r\n            left join PayRate C On C.ChannelId=B.PayCode And A.InterfaceCode=C.UserId\r\n            where B.IsEnable=1 And A.InterfaceCode=@UserId And A.Type=@Type order by B.PaySort desc ", new
			{
				UserId,
				Type
			});
		}

		public List<PayRateInfo> GetUserGradRateList(string UserId, string GradId)
		{
			return DalContext.GetList<PayRateInfo>(" select B.PayCode,B.PayName,A.AccountScheme,c.* from InterfaceType A \r\n                    inner join PayType B On A.PayCode=B.PayCode  \r\n                    Left join  PayRate C on B.PayCode=C.ChannelId  and c.RateType=3 and UserId=@GradId \r\n                    where B.IsEnable=1 And A.InterfaceCode=@UserId And A.[Type]=2  order by B.PaySort desc ", new
			{
				UserId,
				GradId
			});
		}

		public List<PayRateInfo> GetPayRateList(string UserId)
		{
			return DalContext.GetList<PayRateInfo>(" select B.PayCode,B.PayName,c.* from PayType B Left join  \r\n              PayRate C on B.PayCode=C.ChannelId  and c.RateType=3 and UserId=@UserId  where B.IsEnable=1 ", new
			{
				UserId
			});
		}

		public PayRateInfo GetPayRate(string payCode, string userId, string gradeId)
		{
			List<PayRateInfo> list = DalContext.GetList<PayRateInfo>("  \r\n            SELECT * FROM PayRate(nolock) WHERE RateType=@UserRate AND ChannelId=@PayCode AND UserId=@UserId AND IsEnabled=1\r\n            UNION ALL \r\n            SELECT * FROM PayRate(nolock) WHERE RateType=@GradeType AND ChannelId=@PayCode AND UserId=@GradeId  ", new
			{
				PayCode = payCode,
				UserRate = 2,
				UserId = userId,
				GradeType = 3,
				GradeId = gradeId
			});
			PayRateInfo result = null;
			if (list.Count == 1)
			{
				result = list[0];
			}
			else if (list.Count > 1)
			{
				result = list.Find((PayRateInfo p) => p.RateType == 3);
				result = list.Find((PayRateInfo p) => p.RateType == 2);
			}
			return result;
		}

		public PayRateInfo GetPayRate(int type, string payCode, string id)
		{
			return DalContext.GetModel<PayRateInfo>(" SELECT TOP 1 * FROM PayRate(nolock) WHERE RateType=@RateType AND ChannelId=@PayCode AND UserId=@Id AND IsEnabled=1 ", new
			{
				RateType = type,
				PayCode = payCode,
				Id = id
			});
		}

		public long SetPayRate(List<PayRate> ListModel, string UserId, int Type)
		{
			int num = DalContext.ExecuteSql(" delete PayRate where UserId=@UserId and RateType=@Type ", new
			{
				UserId,
				Type
			});
			if (ListModel != null && ListModel.Count > 0)
			{
				return DalContext.InsertBat(ListModel);
			}
			return num;
		}

		public PayRateInfo GetPayRate(int rateType, string payCode, string userId, string gradeId)
		{
			List<PayRateInfo> list = DalContext.GetList<PayRateInfo>("  \r\n            SELECT * FROM PayRate(nolock) WHERE RateType=@UserRate AND ChannelId=@PayCode AND UserId=@UserId AND IsEnabled=1\r\n            UNION ALL \r\n            SELECT * FROM PayRate(nolock) WHERE RateType=@GradeType AND ChannelId=@PayCode AND UserId=@GradeId  ", new
			{
				PayCode = payCode,
				UserRate = rateType,
				UserId = userId,
				GradeType = 3,
				GradeId = gradeId
			});
			PayRateInfo result = null;
			if (list.Count == 1)
			{
				result = list[0];
			}
			else if (list.Count > 1)
			{
				result = list.Find((PayRateInfo p) => p.RateType == 3);
				result = list.Find((PayRateInfo p) => p.RateType == rateType);
			}
			return result;
		}

		public DataTable GetInterfaceListRate()
		{
			return DalContext.GetDataTable(" if object_id('tempdb..#TempTable1') is not null Begin drop table #TempTable1 End;\r\n            WITH mycte \r\n            AS (\r\n            select B.Name,A.PayName,isnull(c.Rate*100,0) Rate,A.PaySort from paytype as A \r\n            cross join InterfaceBusiness as B\r\n            left join payrate C on a.PayCode=C.ChannelId and C.UserId=b.Code and c.RateType=1\r\n            where A.IsEnable=1\r\n            )\r\n            select Name,PayName,Rate INTO #TempTable1 from mycte order by name asc ,PaySort desc;\r\n            DECLARE @sql_str VARCHAR(8000)\r\n            DECLARE @sql_col VARCHAR(8000)\r\n            SELECT @sql_col = ISNULL(@sql_col + ',','') + QUOTENAME(PayName) FROM #TempTable1 GROUP BY PayName\r\n            SET @sql_str = '\r\n            SELECT * FROM (\r\n                SELECT [PayName],[Name] 接口商名称,[Rate] FROM [#TempTable1]) p PIVOT \r\n                (SUM([Rate]) FOR [PayName] IN ( '+ @sql_col +') ) AS pvt \r\n               '\r\n            PRINT (@sql_str);\r\n            EXEC (@sql_str);\r\n            DROP TABLE #TempTable1;\r\n            ");
		}

		public DataTable GetUserGradeListRate(string UserType)
		{
			return DalContext.GetDataTable($" if object_id('tempdb..#TempTable') is not null Begin drop table #TempTable End;\r\n            WITH mycte \r\n            AS (\r\n            select B.GradeName,A.PayName,isnull(c.Rate*100,0) Rate,A.PaySort from paytype as A \r\n            cross join UserGrade as B\r\n            left join payrate C on a.PayCode=C.ChannelId and C.UserId=b.Id and c.RateType=3\r\n            where A.IsEnable=1 And B.UserType={UserType}\r\n            )\r\n            select GradeName,PayName,Rate INTO #TempTable from mycte order by GradeName asc ,PaySort desc;\r\n            DECLARE @sql_str VARCHAR(8000)\r\n            DECLARE @sql_col VARCHAR(8000)\r\n            SELECT @sql_col = ISNULL(@sql_col + ',','') + QUOTENAME(PayName) FROM #TempTable GROUP BY PayName\r\n            SET @sql_str = '\r\n            SELECT * FROM (\r\n            SELECT [PayName],[GradeName] 等级名称,[Rate] FROM [#TempTable]) p PIVOT \r\n            (SUM([Rate]) FOR [PayName] IN ( '+ @sql_col +') ) AS pvt \r\n            '\r\n            PRINT (@sql_str);\r\n            EXEC (@sql_str);\r\n            DROP TABLE #TempTable\r\n            ");
		}
	}
}
