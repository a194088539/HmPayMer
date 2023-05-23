using HM.Framework.DapperExtensions.Sql;
using System.Collections.Generic;

namespace HM.Framework.DapperExtensions
{
	public class PropertyPredicate<T, T2> : ComparePredicate, IPropertyPredicate, IComparePredicate, IBasePredicate, IPredicate where T : class where T2 : class
	{
		public string PropertyName2
		{
			get;
			set;
		}

		public override string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters)
		{
			string columnName = GetColumnName(typeof(T), sqlGenerator, base.PropertyName);
			string columnName2 = GetColumnName(typeof(T2), sqlGenerator, PropertyName2);
			return $"({columnName} {GetOperatorString()} {columnName2})";
		}
	}
}
