using System;

namespace HM.Framework.DapperExtensions.Mapper
{
	public class AutoClassMapper<T> : ClassMapper<T> where T : class
	{
		public AutoClassMapper()
		{
			Type typeFromHandle = typeof(T);
			Table(typeFromHandle.Name);
			AutoMap();
		}
	}
}
