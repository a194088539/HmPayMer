using HM.Framework.Dapper;
using System.Collections.Generic;

namespace HM.Framework.DapperExtensions
{
	public class GridReaderResultReader : IMultipleResultReader
	{
		private readonly SqlMapper.GridReader _reader;

		public GridReaderResultReader(SqlMapper.GridReader reader)
		{
			_reader = reader;
		}

		public IEnumerable<T> Read<T>()
		{
			return _reader.Read<T>();
		}
	}
}
