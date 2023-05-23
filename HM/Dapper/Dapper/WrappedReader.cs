using System;
using System.Data;

namespace HM.Framework.Dapper
{
	internal class WrappedReader : IWrappedDataReader, IDataReader, IDisposable, IDataRecord
	{
		private IDataReader reader;

		private IDbCommand cmd;

		public IDataReader Reader
		{
			get
			{
				IDataReader dataReader = reader;
				if (dataReader == null)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				return dataReader;
			}
		}

		IDbCommand IWrappedDataReader.Command
		{
			get
			{
				IDbCommand dbCommand = cmd;
				if (dbCommand == null)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				return dbCommand;
			}
		}

		int IDataReader.Depth
		{
			get
			{
				return Reader.Depth;
			}
		}

		bool IDataReader.IsClosed
		{
			get
			{
				return reader?.IsClosed ?? true;
			}
		}

		int IDataReader.RecordsAffected
		{
			get
			{
				return Reader.RecordsAffected;
			}
		}

		int IDataRecord.FieldCount
		{
			get
			{
				return Reader.FieldCount;
			}
		}

		object IDataRecord.this[string name]
		{
			get
			{
				return Reader[name];
			}
		}

		object IDataRecord.this[int i]
		{
			get
			{
				return Reader[i];
			}
		}

		public WrappedReader(IDbCommand cmd, IDataReader reader)
		{
			this.cmd = cmd;
			this.reader = reader;
		}

		void IDataReader.Close()
		{
			reader?.Close();
		}

		DataTable IDataReader.GetSchemaTable()
		{
			return Reader.GetSchemaTable();
		}

		bool IDataReader.NextResult()
		{
			return Reader.NextResult();
		}

		bool IDataReader.Read()
		{
			return Reader.Read();
		}

		void IDisposable.Dispose()
		{
			reader?.Close();
			reader?.Dispose();
			reader = null;
			cmd?.Dispose();
			cmd = null;
		}

		bool IDataRecord.GetBoolean(int i)
		{
			return Reader.GetBoolean(i);
		}

		byte IDataRecord.GetByte(int i)
		{
			return Reader.GetByte(i);
		}

		long IDataRecord.GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			return Reader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
		}

		char IDataRecord.GetChar(int i)
		{
			return Reader.GetChar(i);
		}

		long IDataRecord.GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			return Reader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
		}

		IDataReader IDataRecord.GetData(int i)
		{
			return Reader.GetData(i);
		}

		string IDataRecord.GetDataTypeName(int i)
		{
			return Reader.GetDataTypeName(i);
		}

		DateTime IDataRecord.GetDateTime(int i)
		{
			return Reader.GetDateTime(i);
		}

		decimal IDataRecord.GetDecimal(int i)
		{
			return Reader.GetDecimal(i);
		}

		double IDataRecord.GetDouble(int i)
		{
			return Reader.GetDouble(i);
		}

		Type IDataRecord.GetFieldType(int i)
		{
			return Reader.GetFieldType(i);
		}

		float IDataRecord.GetFloat(int i)
		{
			return Reader.GetFloat(i);
		}

		Guid IDataRecord.GetGuid(int i)
		{
			return Reader.GetGuid(i);
		}

		short IDataRecord.GetInt16(int i)
		{
			return Reader.GetInt16(i);
		}

		int IDataRecord.GetInt32(int i)
		{
			return Reader.GetInt32(i);
		}

		long IDataRecord.GetInt64(int i)
		{
			return Reader.GetInt64(i);
		}

		string IDataRecord.GetName(int i)
		{
			return Reader.GetName(i);
		}

		int IDataRecord.GetOrdinal(string name)
		{
			return Reader.GetOrdinal(name);
		}

		string IDataRecord.GetString(int i)
		{
			return Reader.GetString(i);
		}

		object IDataRecord.GetValue(int i)
		{
			return Reader.GetValue(i);
		}

		int IDataRecord.GetValues(object[] values)
		{
			return Reader.GetValues(values);
		}

		bool IDataRecord.IsDBNull(int i)
		{
			return Reader.IsDBNull(i);
		}
	}
}
