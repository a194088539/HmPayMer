using System;
using System.Data;

namespace HM.Framework.Dapper
{
	public interface IWrappedDataReader : IDataReader, IDisposable, IDataRecord
	{
		IDataReader Reader
		{
			get;
		}

		IDbCommand Command
		{
			get;
		}
	}
}
