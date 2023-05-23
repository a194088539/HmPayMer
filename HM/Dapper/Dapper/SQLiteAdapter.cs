using HM.Framework.Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public class SQLiteAdapter : ISqlAdapter
{
	public async Task<int> InsertAsync(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert)
	{
		string sql = $"INSERT INTO {tableName} ({columnList}) VALUES ({parameterList}); SELECT last_insert_rowid() id";
		int num = (int)(await connection.QueryMultipleAsync(sql, entityToInsert, transaction, commandTimeout).ConfigureAwait(continueOnCapturedContext: false)).Read().First().id;
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
		string sql = $"INSERT INTO {tableName} ({columnList}) VALUES ({parameterList}); SELECT last_insert_rowid() id";
		SqlMapper.GridReader gridReader = connection.QueryMultiple(sql, entityToInsert, transaction, commandTimeout);
		int num = (int)gridReader.Read().First().id;
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
		sb.AppendFormat("\"{0}\"", columnName);
	}

	public void AppendColumnNameEqualsValue(StringBuilder sb, string columnName)
	{
		sb.AppendFormat("\"{0}\" = @{1}", columnName, columnName);
	}
}
