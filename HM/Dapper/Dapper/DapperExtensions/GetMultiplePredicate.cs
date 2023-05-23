using System;
using System.Collections.Generic;

namespace HM.Framework.DapperExtensions
{
	public class GetMultiplePredicate
	{
		public class GetMultiplePredicateItem
		{
			public object Value
			{
				get;
				set;
			}

			public Type Type
			{
				get;
				set;
			}

			public IList<ISort> Sort
			{
				get;
				set;
			}
		}

		private readonly List<GetMultiplePredicateItem> _items;

		public IEnumerable<GetMultiplePredicateItem> Items => _items.AsReadOnly();

		public GetMultiplePredicate()
		{
			_items = new List<GetMultiplePredicateItem>();
		}

		public void Add<T>(IPredicate predicate, IList<ISort> sort = null) where T : class
		{
			_items.Add(new GetMultiplePredicateItem
			{
				Value = predicate,
				Type = typeof(T),
				Sort = sort
			});
		}

		public void Add<T>(object id) where T : class
		{
			_items.Add(new GetMultiplePredicateItem
			{
				Value = id,
				Type = typeof(T)
			});
		}
	}
}
