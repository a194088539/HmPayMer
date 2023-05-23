using HmPMer.Dal.System;
using HmPMer.Entity;
using System.Collections.Generic;

namespace HmPMer.Business
{
	public class DistrictBll
	{
		private readonly DistrictDal _dal = new DistrictDal();

		public List<District> LoadAll()
		{
			return _dal.LoadAll();
		}

		public List<District> LoadLevel(int Level)
		{
			return _dal.LoadLevel(Level);
		}

		public List<District> LoadParentId(int ParentId)
		{
			return _dal.LoadParentId(ParentId);
		}
	}
}
