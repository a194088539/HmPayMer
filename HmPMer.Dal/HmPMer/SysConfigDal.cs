using HM.DAL;
using HmPMer.Entity;
using System.Collections.Generic;

namespace HmPMer
{
	public class SysConfigDal
	{
		public SysConfig GetForKey(string Key)
		{
			return DalContext.GetModel<SysConfig>(" select * from SysConfig Where [Key]=@Key ", new
			{
				Key
			});
		}

		public List<SysConfig> GetSysConfig(int Type)
		{
			return DalContext.GetList<SysConfig>(" select * from SysConfig Where Type=@Type ", new
			{
				Type
			});
		}

		public long SetSysConfig(List<SysConfig> ListModel, int Type)
		{
			int num = DalContext.ExecuteSql(" delete SysConfig where Type=@Type ", new
			{
				Type
			});
			if (ListModel != null && ListModel.Count > 0)
			{
				return DalContext.InsertBat(ListModel);
			}
			return num;
		}
	}
}
