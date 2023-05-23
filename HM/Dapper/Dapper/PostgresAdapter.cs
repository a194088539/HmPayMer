using HM.Framework.Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public class PostgresAdapter : ISqlAdapter
{
	public async Task<int> InsertAsync(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("INSERT INTO {0} ({1}) VALUES ({2})", tableName, columnList, parameterList);
		PropertyInfo[] propertyInfos = (keyProperties as PropertyInfo[]) ?? keyProperties.ToArray();
		PropertyInfo[] array;
		if (propertyInfos.Length == 0)
		{
			stringBuilder.Append(" RETURNING *");
		}
		else
		{
			stringBuilder.Append(" RETURNING ");
			bool flag = true;
			array = propertyInfos;
			foreach (PropertyInfo propertyInfo in array)
			{
				if (!flag)
				{
					stringBuilder.Append(", ");
				}
				flag = false;
				stringBuilder.Append(propertyInfo.Name);
			}
		}
		IEnumerable<object> source = await connection.QueryAsync(stringBuilder.ToString(), entityToInsert, transaction, commandTimeout).ConfigureAwait(continueOnCapturedContext: false);
		int num = 0;
		array = propertyInfos;
		foreach (PropertyInfo propertyInfo2 in array)
		{
			object value = ((IDictionary<string, object>)source.First())[propertyInfo2.Name.ToLower()];
			propertyInfo2.SetValue(entityToInsert, value, null);
			if (num == 0)
			{
				num = Convert.ToInt32(value);
			}
		}
		return num;
	}

	public int Insert(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("insert into {0} ({1}) values ({2})", tableName, columnList, parameterList);
		PropertyInfo[] array = (keyProperties as PropertyInfo[]) ?? keyProperties.ToArray();
		PropertyInfo[] array2;
		if (array.Length == 0)
		{
			stringBuilder.Append(" RETURNING *");
		}
		else
		{
			stringBuilder.Append(" RETURNING ");
			bool flag = true;
			array2 = array;
			foreach (PropertyInfo propertyInfo in array2)
			{
				if (!flag)
				{
					stringBuilder.Append(", ");
				}
				flag = false;
				stringBuilder.Append(propertyInfo.Name);
			}
		}
		List<object> list = connection.Query(stringBuilder.ToString(), entityToInsert, transaction, buffered: true, commandTimeout).ToList();
		int num = 0;
		array2 = array;
		foreach (PropertyInfo propertyInfo2 in array2)
		{
			object value = ((IDictionary<string, object>)list[0])[propertyInfo2.Name.ToLower()];
			propertyInfo2.SetValue(entityToInsert, value, null);
			if (num == 0)
			{
				num = Convert.ToInt32(value);
			}
		}
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
