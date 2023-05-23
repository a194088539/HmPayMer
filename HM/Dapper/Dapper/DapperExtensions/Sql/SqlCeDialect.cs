using System;
using System.Collections.Generic;
using System.Text;

namespace HM.Framework.DapperExtensions.Sql
{
	public class SqlCeDialect : SqlDialectBase
	{
		public override char OpenQuote => '[';

		public override char CloseQuote => ']';

		public override bool SupportsMultipleStatements => false;

		public override string GetTableName(string schemaName, string tableName, string alias)
		{
			if (string.IsNullOrWhiteSpace(tableName))
			{
				throw new ArgumentNullException("TableName");
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(OpenQuote);
			if (!string.IsNullOrWhiteSpace(schemaName))
			{
				stringBuilder.AppendFormat("{0}_", schemaName);
			}
			stringBuilder.AppendFormat("{0}{1}", tableName, CloseQuote);
			if (!string.IsNullOrWhiteSpace(alias))
			{
				stringBuilder.AppendFormat(" AS {0}{1}{2}", OpenQuote, alias, CloseQuote);
			}
			return stringBuilder.ToString();
		}

		public override string GetIdentitySql(string tableName)
		{
			return "SELECT CAST(@@IDENTITY AS BIGINT) AS [Id]";
		}

		public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters)
		{
			int firstResult = page * resultsPerPage;
			return GetSetSql(sql, firstResult, resultsPerPage, parameters);
		}

		public override string GetSetSql(string sql, int firstResult, int maxResults, IDictionary<string, object> parameters)
		{
			string result = $"{sql} OFFSET @firstResult ROWS FETCH NEXT @maxResults ROWS ONLY";
			parameters.Add("@firstResult", firstResult);
			parameters.Add("@maxResults", maxResults);
			return result;
		}
	}
}
