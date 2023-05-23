using System;
using System.Collections.Generic;
using System.Text;

namespace HM.Framework.DapperExtensions.Sql
{
	public class OracleDialect : SqlDialectBase
	{
		public override bool SupportsMultipleStatements => false;

		public override char ParameterPrefix => ':';

		public override char OpenQuote => '"';

		public override char CloseQuote => '"';

		public override string GetIdentitySql(string tableName)
		{
			throw new NotImplementedException("Oracle does not support get last inserted identity.");
		}

		public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters)
		{
			int num = page * resultsPerPage;
			int num2 = (page + 1) * resultsPerPage;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("SELECT * FROM (");
			stringBuilder.AppendLine("SELECT \"_ss_dapper_1_\".*, ROWNUM RNUM FROM (");
			stringBuilder.Append(sql);
			stringBuilder.AppendLine(") \"_ss_dapper_1_\"");
			stringBuilder.AppendLine("WHERE ROWNUM <= :topLimit) \"_ss_dapper_2_\" ");
			stringBuilder.AppendLine("WHERE \"_ss_dapper_2_\".RNUM > :toSkip");
			parameters.Add(":topLimit", num2);
			parameters.Add(":toSkip", num);
			return stringBuilder.ToString();
		}

		public override string GetSetSql(string sql, int firstResult, int maxResults, IDictionary<string, object> parameters)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("SELECT * FROM (");
			stringBuilder.AppendLine("SELECT \"_ss_dapper_1_\".*, ROWNUM RNUM FROM (");
			stringBuilder.Append(sql);
			stringBuilder.AppendLine(") \"_ss_dapper_1_\"");
			stringBuilder.AppendLine("WHERE ROWNUM <= :topLimit) \"_ss_dapper_2_\" ");
			stringBuilder.AppendLine("WHERE \"_ss_dapper_2_\".RNUM > :toSkip");
			parameters.Add(":topLimit", maxResults + firstResult);
			parameters.Add(":toSkip", firstResult);
			return stringBuilder.ToString();
		}

		public override string QuoteString(string value)
		{
			if (value != null && value[0] == '`')
			{
				return $"{OpenQuote}{value.Substring(1, value.Length - 2)}{CloseQuote}";
			}
			return value.ToUpper();
		}
	}
}
