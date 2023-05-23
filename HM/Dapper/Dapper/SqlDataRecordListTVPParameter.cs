using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace HM.Framework.Dapper
{
	internal sealed class SqlDataRecordListTVPParameter : SqlMapper.ICustomQueryParameter
	{
		private readonly IEnumerable<SqlDataRecord> data;

		private readonly string typeName;

		public SqlDataRecordListTVPParameter(IEnumerable<SqlDataRecord> data, string typeName)
		{
			this.data = data;
			this.typeName = typeName;
		}

		void SqlMapper.ICustomQueryParameter.AddParameter(IDbCommand command, string name)
		{
			IDbDataParameter dbDataParameter = command.CreateParameter();
			dbDataParameter.ParameterName = name;
			Set(dbDataParameter, data, typeName);
			command.Parameters.Add(dbDataParameter);
		}

		internal static void Set(IDbDataParameter parameter, IEnumerable<SqlDataRecord> data, string typeName)
		{
			parameter.Value = (((object)data) ?? ((object)DBNull.Value));
			SqlParameter sqlParameter;
			if ((sqlParameter = (parameter as SqlParameter)) != null)
			{
				sqlParameter.SqlDbType = SqlDbType.Structured;
				sqlParameter.TypeName = typeName;
			}
		}
	}
}
