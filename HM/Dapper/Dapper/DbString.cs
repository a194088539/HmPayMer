using System;
using System.Data;

namespace HM.Framework.Dapper
{
	public sealed class DbString : SqlMapper.ICustomQueryParameter
	{
		public const int DefaultLength = 4000;

		public static bool IsAnsiDefault
		{
			get;
			set;
		}

		public bool IsAnsi
		{
			get;
			set;
		}

		public bool IsFixedLength
		{
			get;
			set;
		}

		public int Length
		{
			get;
			set;
		}

		public string Value
		{
			get;
			set;
		}

		public DbString()
		{
			Length = -1;
			IsAnsi = IsAnsiDefault;
		}

		public void AddParameter(IDbCommand command, string name)
		{
			if (IsFixedLength && Length == -1)
			{
				throw new InvalidOperationException("If specifying IsFixedLength,  a Length must also be specified");
			}
			bool num = !command.Parameters.Contains(name);
			IDbDataParameter dbDataParameter;
			if (num)
			{
				dbDataParameter = command.CreateParameter();
				dbDataParameter.ParameterName = name;
			}
			else
			{
				dbDataParameter = (IDbDataParameter)command.Parameters[name];
			}
			dbDataParameter.Value = SqlMapper.SanitizeParameterValue(Value);
			if (Length == -1 && Value != null && Value.Length <= 4000)
			{
				dbDataParameter.Size = 4000;
			}
			else
			{
				dbDataParameter.Size = Length;
			}
			dbDataParameter.DbType = ((!IsAnsi) ? (IsFixedLength ? DbType.StringFixedLength : DbType.String) : (IsFixedLength ? DbType.AnsiStringFixedLength : DbType.AnsiString));
			if (num)
			{
				command.Parameters.Add(dbDataParameter);
			}
		}
	}
}
