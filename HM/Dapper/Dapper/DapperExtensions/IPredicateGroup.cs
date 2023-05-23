using System.Collections.Generic;

namespace HM.Framework.DapperExtensions
{
	public interface IPredicateGroup : IPredicate
	{
		GroupOperator Operator
		{
			get;
			set;
		}

		IList<IPredicate> Predicates
		{
			get;
			set;
		}
	}
}
