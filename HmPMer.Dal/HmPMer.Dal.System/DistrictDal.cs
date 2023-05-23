using HM.DAL;
using HmPMer.Entity;
using System.Collections.Generic;

namespace HmPMer.Dal.System
{
	public class DistrictDal
	{
		public List<District> LoadAll()
		{
			return DalContext.GetList<District>($" SELECT * FROM District  ");
		}

		public List<District> LoadLevel(int Level)
		{
			return DalContext.GetList<District>("  SELECT * FROM District WHERE Level=@Level  ", new
			{
				Level
			});
		}

		public List<District> LoadParentId(int ParentId)
		{
			return DalContext.GetList<District>("  SELECT * FROM District WHERE ParentId=@ParentId  ", new
			{
				ParentId
			});
		}
	}
}
