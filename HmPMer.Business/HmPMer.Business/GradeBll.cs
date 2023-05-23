using HmPMer.Dal;
using HmPMer.Entity;
using System.Collections.Generic;

namespace HmPMer.Business
{
	public class GradeBll
	{
		private readonly GradeDal _dal = new GradeDal();

		public List<UserGrade> GetUserGradeList(int UserType)
		{
			return _dal.GetUserGradeList(UserType);
		}

		public long AddUserGrade(UserGrade Model)
		{
			return _dal.AddUserGrade(Model);
		}

		public UserGrade GetUserGradeModel(string Id)
		{
			return _dal.GetUserGradeModel(Id);
		}

		public int UpdateUserGrade(string GradeName, string Id)
		{
			return _dal.UpdateUserGrade(GradeName, Id);
		}

		public long GetMaxId()
		{
			return _dal.GetMaxId();
		}

		public List<UserGrade> GetAllUserGrade(int UserType)
		{
			return _dal.GetAllUserGrade(UserType);
		}

		public int DelUserGrade(string Id)
		{
			return _dal.DelUserGrade(Id);
		}
	}
}
