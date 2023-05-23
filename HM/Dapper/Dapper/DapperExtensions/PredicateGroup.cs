using HM.Framework.DapperExtensions.Sql;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HM.Framework.DapperExtensions
{
	public class PredicateGroup : IPredicateGroup, IPredicate
	{
		public GroupOperator Operator
		{
			get;
			set;
		}

		public IList<IPredicate> Predicates
		{
			get;
			set;
		}

		public string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters)
		{
			string seperator = (Operator == GroupOperator.And) ? " AND " : " OR ";
			return "(" + Predicates.Aggregate(new StringBuilder(), (StringBuilder sb, IPredicate p) => ((sb.Length == 0) ? sb : sb.Append(seperator)).Append(p.GetSql(sqlGenerator, parameters)), delegate(StringBuilder sb)
			{
				string text = sb.ToString();
				if (text.Length == 0)
				{
					return sqlGenerator.Configuration.Dialect.EmptyExpression;
				}
				return text;
			}) + ")";
		}
	}
}
