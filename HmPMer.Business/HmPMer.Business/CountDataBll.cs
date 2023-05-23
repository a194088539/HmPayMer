using HmPMer.Dal;
using HmPMer.Entity;
using System.Collections.Generic;

namespace HmPMer.Business
{
	public class CountDataBll
	{
		private readonly CountDataDal _dal = new CountDataDal();

		public List<ProfitAnalysis> GetProfitAnalysisList(string date)
		{
			return _dal.GetProfitAnalysisList(date);
		}

		public List<InterfaceAnalysis> GetInterfaceAnalysisList(string date)
		{
			return _dal.GetInterfaceAnalysisList(date);
		}
	}
}
