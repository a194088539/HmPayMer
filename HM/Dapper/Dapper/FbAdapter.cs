using HM.Framework.Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public class FbAdapter : ISqlAdapter
{
	public async Task<int> InsertAsync(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert)
	{
		string sql = $"insert into {tableName} ({columnList}) values ({parameterList})";
		await connection.ExecuteAsync(sql, entityToInsert, transaction, commandTimeout).ConfigureAwait(continueOnCapturedContext: false);
		PropertyInfo[] propertyInfos = (keyProperties as PropertyInfo[]) ?? keyProperties.ToArray();
		string name = propertyInfos[0].Name;
		dynamic val = ((dynamic)(await connection.QueryAsync($"SELECT FIRST 1 {name} ID FROM {tableName} ORDER BY {name} DESC", null, transaction, commandTimeout).ConfigureAwait(continueOnCapturedContext: false)).First()).ID;
		if (!((val == null) ? true : false))
		{
			if (propertyInfos.Length != 0)
			{
				PropertyInfo propertyInfo = propertyInfos[0];
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
		PropertyInfo[] array = (keyProperties as PropertyInfo[]) ?? keyProperties.ToArray();
		string name = array[0].Name;
		IEnumerable<object> source = connection.Query($"SELECT FIRST 1 {name} ID FROM {tableName} ORDER BY {name} DESC", null, transaction, buffered: true, commandTimeout);
		dynamic val = ((dynamic)source.First()).ID;
		if (!((val == null) ? true : false))
		{
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
		sb.AppendFormat("{0}", columnName);
	}

	public void AppendColumnNameEqualsValue(StringBuilder sb, string columnName)
	{
		sb.AppendFormat("{0} = @{1}", columnName, columnName);
	}
}
