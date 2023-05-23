using HM.Framework.Dapper;
using System.Collections.Generic;

namespace HM.Framework.DapperExtensions
{
	public class SequenceReaderResultReader : IMultipleResultReader
	{
		private readonly Queue<SqlMapper.GridReader> _items;

		public SequenceReaderResultReader(IEnumerable<SqlMapper.GridReader> items)
		{
			_items = new Queue<SqlMapper.GridReader>(items);
		}

		public IEnumerable<T> Read<T>()
		{
			return _items.Dequeue().Read<T>();
		}
	}
}
