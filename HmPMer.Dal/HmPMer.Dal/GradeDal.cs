using HM.DAL;
using HmPMer.Entity;
using System.Collections.Generic;

namespace HmPMer.Dal
{
	public class GradeDal
	{
		public List<UserGrade> GetUserGradeList(int UserType)
		{
			return DalContext.GetList<UserGrade>(" select * from  UserGrade where UserType=@UserType", new
			{
				UserType
			});
		}

		public long AddUserGrade(UserGrade Model)
		{
			return DalContext.Insert(Model);
		}

		public UserGrade GetUserGradeModel(string Id)
		{
			return DalContext.GetModel<UserGrade>(" select * from UserGrade where Id=@Id ", new
			{
				Id
			});
		}

		public int UpdateUserGrade(string GradeName, string Id)
		{
			return DalContext.ExecuteSql(" update UserGrade set GradeName=@GradeName where Id=@Id ", new
			{
				GradeName,
				Id
			});
		}

		public long GetMaxId()
		{
			return DalContext.GetSingVal<long>(" select MAX(id) from UserGrade  ");
		}

		public List<UserGrade> GetAllUserGrade(int UserType)
		{
			return DalContext.GetList<UserGrade>(" select * from UserGrade where UserType=@UserType ", new
			{
				UserType
			});
		}

		public int DelUserGrade(string Id)
		{
			return DalContext.ExecuteSql(" delete UserGrade where Id=@Id;\r\n                    delete PayRate where RateType=3 And UserId=@Id;\r\n                     ", new
			{
				Id
			});
		}
	}
}
