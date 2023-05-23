using System.Collections.Generic;

namespace HM.Framework.DapperExtensions
{
	public interface IMultipleResultReader
	{
		IEnumerable<T> Read<T>();
	}
}
