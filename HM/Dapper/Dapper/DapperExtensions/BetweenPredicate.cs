using HM.Framework.DapperExtensions.Sql;
using System.Collections.Generic;

namespace HM.Framework.DapperExtensions
{
	public class BetweenPredicate<T> : BasePredicate, IBetweenPredicate, IPredicate where T : class
	{
		public BetweenValues Value
		{
			get;
			set;
		}

		public bool Not
		{
			get;
			set;
		}

		public override string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters)
		{
			string columnName = GetColumnName(typeof(T), sqlGenerator, base.PropertyName);
			string text = parameters.SetParameterName(base.PropertyName, Value.Value1, sqlGenerator.Configuration.Dialect.ParameterPrefix);
			string text2 = parameters.SetParameterName(base.PropertyName, Value.Value2, sqlGenerator.Configuration.Dialect.ParameterPrefix);
			return string.Format("({0} {1}BETWEEN {2} AND {3})", columnName, Not ? "NOT " : string.Empty, text, text2);
		}
	}
}
