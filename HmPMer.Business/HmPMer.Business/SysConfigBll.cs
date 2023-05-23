using HmPMer.Entity;
using System.Collections.Generic;

namespace HmPMer.Business
{
	public class SysConfigBll
	{
		private readonly SysConfigDal _dal = new SysConfigDal();

		public SysConfig GetForKey(string Key)
		{
			return _dal.GetForKey(Key);
		}

		public string GetConfigVaule(string Key)
		{
			SysConfig forKey = _dal.GetForKey(Key);
			if (forKey != null)
			{
				return forKey.Value;
			}
			return "";
		}

		public List<SysConfig> GetSysConfig(int Type)
		{
			return _dal.GetSysConfig(Type);
		}

		public long SetSysConfig(List<SysConfig> ListModel, int Type)
		{
			return _dal.SetSysConfig(ListModel, Type);
		}
	}
}
