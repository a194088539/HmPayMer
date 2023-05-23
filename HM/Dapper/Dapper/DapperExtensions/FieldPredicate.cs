using HM.Framework.DapperExtensions.Sql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HM.Framework.DapperExtensions
{
	public class FieldPredicate<T> : ComparePredicate, IFieldPredicate, IComparePredicate, IBasePredicate, IPredicate where T : class
	{
		public object Value
		{
			get;
			set;
		}

		public override string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters)
		{
			string columnName = GetColumnName(typeof(T), sqlGenerator, base.PropertyName);
			if (Value == null)
			{
				return string.Format("({0} IS {1}NULL)", columnName, base.Not ? "NOT " : string.Empty);
			}
			if (Value is IEnumerable && !(Value is string))
			{
				if (base.Operator != 0)
				{
					throw new ArgumentException("Operator must be set to Eq for Enumerable types");
				}
				List<string> list = new List<string>();
				foreach (object item2 in (IEnumerable)Value)
				{
					string item = parameters.SetParameterName(base.PropertyName, item2, sqlGenerator.Configuration.Dialect.ParameterPrefix);
					list.Add(item);
				}
				string arg = list.Aggregate(new StringBuilder(), (StringBuilder sb, string s) => sb.Append(((sb.Length != 0) ? ", " : string.Empty) + s), (StringBuilder sb) => sb.ToString());
				return string.Format("({0} {1}IN ({2}))", columnName, base.Not ? "NOT " : string.Empty, arg);
			}
			string arg2 = parameters.SetParameterName(base.PropertyName, Value, sqlGenerator.Configuration.Dialect.ParameterPrefix);
			return $"({columnName} {GetOperatorString()} {arg2})";
		}
	}
}
