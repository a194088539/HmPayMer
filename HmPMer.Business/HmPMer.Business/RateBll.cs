using HmPMer.Dal;
using HmPMer.Entity;
using System.Collections.Generic;
using System.Data;

namespace HmPMer.Business
{
	public class RateBll
	{
		private RateDal _dal = new RateDal();

		public List<PayRateInfo> GetPayRateList(string UserId, int Type)
		{
			if (string.IsNullOrEmpty(UserId))
			{
				return new List<PayRateInfo>();
			}
			return _dal.GetPayRateList(UserId, Type);
		}

		public List<PayRateInfo> GetUserGradRateList(string UserId, string GradId)
		{
			return _dal.GetUserGradRateList(UserId, GradId);
		}

		public List<PayRateInfo> GetPayRateList(string UserId)
		{
			return _dal.GetPayRateList(UserId);
		}

		public PayRateInfo GetPayRate(string payCode, string userId, string gradeId)
		{
			return _dal.GetPayRate(payCode, userId, gradeId);
		}

		public long SetPayRate(List<PayRate> ListModel, string UserId, int Type)
		{
			return _dal.SetPayRate(ListModel, UserId, Type);
		}

		public PayRateInfo GetPayRate(int rateType, string payCode, string userId, string gradeId)
		{
			return _dal.GetPayRate(rateType, payCode, userId, gradeId);
		}

		public PayRate GetPayRate(int rateType, string payCode, string id)
		{
			return _dal.GetPayRate(rateType, payCode, id);
		}

		public DataTable GetInterfaceListRate()
		{
			return _dal.GetInterfaceListRate();
		}

		public DataTable GetUserGradeListRate(string UserType)
		{
			return _dal.GetUserGradeListRate(UserType);
		}
	}
}
