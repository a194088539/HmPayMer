using System;
using System.Collections.Generic;
using System.Text;

namespace HM.Framework.DapperExtensions.Sql
{
	public class SqliteDialect : SqlDialectBase
	{
		public override string GetIdentitySql(string tableName)
		{
			return "SELECT LAST_INSERT_ROWID() AS [Id]";
		}

		public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters)
		{
			int firstResult = page * resultsPerPage;
			return GetSetSql(sql, firstResult, resultsPerPage, parameters);
		}

		public override string GetSetSql(string sql, int firstResult, int maxResults, IDictionary<string, object> parameters)
		{
			if (string.IsNullOrEmpty(sql))
			{
				throw new ArgumentNullException("SQL");
			}
			if (parameters == null)
			{
				throw new ArgumentNullException("Parameters");
			}
			string result = $"{sql} LIMIT @Offset, @Count";
			parameters.Add("@Offset", firstResult);
			parameters.Add("@Count", maxResults);
			return result;
		}

		public override string GetColumnName(string prefix, string columnName, string alias)
		{
			if (string.IsNullOrWhiteSpace(columnName))
			{
				throw new ArgumentNullException(columnName, "columnName cannot be null or empty.");
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat(columnName);
			if (!string.IsNullOrWhiteSpace(alias))
			{
				stringBuilder.AppendFormat(" AS {0}", QuoteString(alias));
			}
			return stringBuilder.ToString();
		}
	}
}
