using HM.Framework.Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public class MySqlAdapter : ISqlAdapter
{
	public async Task<int> InsertAsync(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert)
	{
		string sql = $"INSERT INTO {tableName} ({columnList}) VALUES ({parameterList})";
		await connection.ExecuteAsync(sql, entityToInsert, transaction, commandTimeout).ConfigureAwait(continueOnCapturedContext: false);
		dynamic val = ((dynamic)(await connection.QueryAsync<object>("SELECT LAST_INSERT_ID() id", null, transaction, commandTimeout).ConfigureAwait(continueOnCapturedContext: false)).First()).id;
		if (!((val == null) ? true : false))
		{
			PropertyInfo[] array = (keyProperties as PropertyInfo[]) ?? keyProperties.ToArray();
			if (array.Length != 0)
			{
				PropertyInfo propertyInfo = array[0];
				propertyInfo.SetValue(entityToInsert, Convert.ChangeType(val, propertyInfo.PropertyType), null);
				return (int)Convert.ToInt32(val);
			}
			return (int)Convert.ToInt32(val);
		}
		return 0;
	}

	public int Insert(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert)
	{
		string sql = $"insert into {tableName} ({columnList}) values ({parameterList})";
		connection.Execute(sql, entityToInsert, transaction, commandTimeout);
		IEnumerable<object> source = connection.Query("Select LAST_INSERT_ID() id", null, transaction, buffered: true, commandTimeout);
		dynamic val = ((dynamic)source.First()).id;
		if (!((val == null) ? true : false))
		{
			PropertyInfo[] array = (keyProperties as PropertyInfo[]) ?? keyProperties.ToArray();
			if (array.Length != 0)
			{
				PropertyInfo propertyInfo = array[0];
				propertyInfo.SetValue(entityToInsert, Convert.ChangeType(val, propertyInfo.PropertyType), null);
				return (int)Convert.ToInt32(val);
			}
			return (int)Convert.ToInt32(val);
		}
		return 0;
	}

	public void AppendColumnName(StringBuilder sb, string columnName)
	{
		sb.AppendFormat("`{0}`", columnName);
	}

	public void AppendColumnNameEqualsValue(StringBuilder sb, string columnName)
	{
		sb.AppendFormat("`{0}` = @{1}", columnName, columnName);
	}
}
