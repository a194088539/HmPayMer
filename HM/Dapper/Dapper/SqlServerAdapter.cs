using HM.Framework.Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public class SqlServerAdapter : ISqlAdapter
{
	public async Task<int> InsertAsync(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert)
	{
		string sql = $"INSERT INTO {tableName} ({columnList}) values ({parameterList}); SELECT SCOPE_IDENTITY() id";
		dynamic val = (await connection.QueryMultipleAsync(sql, entityToInsert, transaction, commandTimeout).ConfigureAwait(continueOnCapturedContext: false)).Read().FirstOrDefault();
		if (val == null || val.id == null)
		{
			return 0;
		}
		int num = (int)val.id;
		PropertyInfo[] array = (keyProperties as PropertyInfo[]) ?? keyProperties.ToArray();
		if (array.Length == 0)
		{
			return num;
		}
		PropertyInfo propertyInfo = array[0];
		propertyInfo.SetValue(entityToInsert, Convert.ChangeType(num, propertyInfo.PropertyType), null);
		return num;
	}

	public int Insert(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert)
	{
		string sql = $"insert into {tableName} ({columnList}) values ({parameterList});select SCOPE_IDENTITY() id";
		dynamic val = connection.QueryMultiple(sql, entityToInsert, transaction, commandTimeout).Read().FirstOrDefault();
		if (val == null || val.id == null)
		{
			return 0;
		}
		int num = (int)val.id;
		PropertyInfo[] array = (keyProperties as PropertyInfo[]) ?? keyProperties.ToArray();
		if (array.Length == 0)
		{
			return num;
		}
		PropertyInfo propertyInfo = array[0];
		propertyInfo.SetValue(entityToInsert, Convert.ChangeType(num, propertyInfo.PropertyType), null);
		return num;
	}

	public void AppendColumnName(StringBuilder sb, string columnName)
	{
		sb.AppendFormat("[{0}]", columnName);
	}

	public void AppendColumnNameEqualsValue(StringBuilder sb, string columnName)
	{
		sb.AppendFormat("[{0}] = @{1}", columnName, columnName);
	}
}
