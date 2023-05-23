using HmPMer.Dal;
using HmPMer.Entity;
using System;
using System.Collections.Generic;

namespace HmPMer.Business
{
	public class RiskBll
	{
		private readonly RiskDal _dal = new RiskDal();

		public long AddRiskScheme(RiskScheme model)
		{
			return _dal.AddRiskScheme(model);
		}

		public bool UpdateRiskScheme(RiskScheme model)
		{
			return _dal.UpdateRiskScheme(model);
		}

		public bool DeleteRiskScheme(string id)
		{
			return _dal.DeleteRiskScheme(id);
		}

		public RiskScheme GetRiskSchemeModel(string id)
		{
			return _dal.GetRiskSchemeModel(id);
		}

		public RiskScheme GetRiskSchemeModel(int riskType, string id)
		{
			return _dal.GetRiskSchemeModel(riskType, id);
		}

		public List<RiskScheme> GetRiskSchemeList(RiskScheme param, ref Paging paging)
		{
			return _dal.GetRiskSchemeList(param, ref paging);
		}

		public List<RiskScheme> GetRiskSchemeList(int riskSchemeType, string userId)
		{
			return _dal.GetRiskSchemeList(riskSchemeType, userId);
		}

		public RiskSetting GetRiskSetting(int riskType, string targetId)
		{
			return _dal.GetRiskSetting(riskType, targetId);
		}

		public bool SetRiskSetting(List<RiskSetting> list, string riskSchemeId, int riskType, int aloneRisk)
		{
			return _dal.SetRiskSetting(list, riskSchemeId, riskType, aloneRisk);
		}

		public bool AddRiskTarnRecord(RiskTarnRecord record)
		{
			return _dal.AddRiskTarnRecord(record);
		}

		public RiskTarnRecord GetRiskTarn(RiskTarnRecord record)
		{
			return _dal.GetRiskTarn(record);
		}

		public bool AddRiskLimt(RiskLimit model)
		{
			return _dal.AddRiskLimt(model);
		}

		public int RemoveRiskLimt(RiskType riskType, DateTime time)
		{
			return _dal.RemoveRiskLimt(riskType, time);
		}

		public int DelRiskScheme(string RiskSchemeId)
		{
			return _dal.DelRiskScheme(RiskSchemeId);
		}
	}
}
