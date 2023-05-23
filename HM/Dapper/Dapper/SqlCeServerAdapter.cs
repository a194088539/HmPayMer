using HM.Framework.Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public class SqlCeServerAdapter : ISqlAdapter
{
	public async Task<int> InsertAsync(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert)
	{
		string sql = $"INSERT INTO {tableName} ({columnList}) VALUES ({parameterList})";
		await connection.ExecuteAsync(sql, entityToInsert, transaction, commandTimeout).ConfigureAwait(continueOnCapturedContext: false);
		List<object> list = (await connection.QueryAsync<object>("SELECT @@IDENTITY id", null, transaction, commandTimeout).ConfigureAwait(continueOnCapturedContext: false)).ToList();
		if ((dynamic)list[0] == null || ((dynamic)list[0]).id == null)
		{
			return 0;
		}
		int num = (int)((dynamic)list[0]).id;
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
		string sql = $"insert into {tableName} ({columnList}) values ({parameterList})";
		connection.Execute(sql, entityToInsert, transaction, commandTimeout);
		List<object> list = connection.Query("select @@IDENTITY id", null, transaction, buffered: true, commandTimeout).ToList();
		if (((dynamic)list[0]).id == null)
		{
			return 0;
		}
		int num = (int)((dynamic)list[0]).id;
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
