using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace HM.Framework.Dapper
{
	internal sealed class TableValuedParameter : SqlMapper.ICustomQueryParameter
	{
		private readonly DataTable table;

		private readonly string typeName;

		private static readonly Action<SqlParameter, string> setTypeName;

		public TableValuedParameter(DataTable table)
			: this(table, null)
		{
		}

		public TableValuedParameter(DataTable table, string typeName)
		{
			this.table = table;
			this.typeName = typeName;
		}

		static TableValuedParameter()
		{
			PropertyInfo property = typeof(SqlParameter).GetProperty("TypeName", BindingFlags.Instance | BindingFlags.Public);
			if (property != null && property.PropertyType == typeof(string) && property.CanWrite)
			{
				setTypeName = (Action<SqlParameter, string>)Delegate.CreateDelegate(typeof(Action<SqlParameter, string>), property.GetSetMethod());
			}
		}

		void SqlMapper.ICustomQueryParameter.AddParameter(IDbCommand command, string name)
		{
			IDbDataParameter dbDataParameter = command.CreateParameter();
			dbDataParameter.ParameterName = name;
			Set(dbDataParameter, table, typeName);
			command.Parameters.Add(dbDataParameter);
		}

		internal static void Set(IDbDataParameter parameter, DataTable table, string typeName)
		{
			parameter.Value = SqlMapper.SanitizeParameterValue(table);
			if (string.IsNullOrEmpty(typeName) && table != null)
			{
				typeName = table.GetTypeName();
			}
			SqlParameter sqlParameter;
			if (!string.IsNullOrEmpty(typeName) && (sqlParameter = (parameter as SqlParameter)) != null)
			{
				setTypeName?.Invoke(sqlParameter, typeName);
				sqlParameter.SqlDbType = SqlDbType.Structured;
			}
		}
	}
}
