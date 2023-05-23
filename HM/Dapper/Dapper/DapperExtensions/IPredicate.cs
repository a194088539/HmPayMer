using HM.Framework.DapperExtensions.Sql;
using System.Collections.Generic;

namespace HM.Framework.DapperExtensions
{
	public interface IPredicate
	{
		string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters);
	}
}
