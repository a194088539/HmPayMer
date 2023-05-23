using HM.DAL;
using HmPMer.Entity;
using System.Collections.Generic;

namespace HmPMer.Dal
{
	public class CountDataDal
	{
		public List<ProfitAnalysis> GetProfitAnalysisList(string date)
		{
			return DalContext.GetList<ProfitAnalysis>(" select CONVERT(varchar(10),A.PayTime,20) PayTime, B.PayName,sum(OrderAmt) OrderAmt,\r\n                        count(*) CountOrder,sum(MerAmt) MerAmt,sum(PromAmt) PromAmt,sum(AgentAmt) AgentAmt,sum(Profits) Profits\r\n                        from OrderBase A left Join PayType B On A.PayCode=B.PayCode\r\n                        where PayState=1 And  CONVERT(varchar(10),A.PayTime,20)= CONVERT(varchar(10),@date,20)\r\n                        group by CONVERT(varchar(10),A.PayTime,20),B.PayName\r\n                        order by CONVERT(varchar(10),PayTime,20) desc,PayName asc  ", new
			{
				date
			});
		}

		public List<InterfaceAnalysis> GetInterfaceAnalysisList(string date)
		{
			return DalContext.GetList<InterfaceAnalysis>(" select CONVERT(varchar(10),A.PayTime,20) PayTime,B.Name InterfaceName,C.PayName,\r\n                SUM(OrderAmt) CountOrderAmt,SUM(case when PayState=1 then OrderAmt else 0 end ) CountPayOrderAmt,\r\n                Count(*) OrderCount,count(case when PayState=1 then 1 else null end) PayOrderCount\r\n                 from OrderBase A  left join InterfaceBusiness B on A.InterfaceCode=B.Code\r\n                 left Join PayType C On A.PayCode=C.PayCode\r\n                where CONVERT(varchar(10),A.PayTime,20)= CONVERT(varchar(10),@date,20)\r\n                group by CONVERT(varchar(10),A.PayTime,20),B.Name,C.PayName\r\n                order by CONVERT(varchar(10),A.PayTime,20) desc ,B.Name asc ,C.PayName asc ", new
			{
				date
			});
		}
	}
}
