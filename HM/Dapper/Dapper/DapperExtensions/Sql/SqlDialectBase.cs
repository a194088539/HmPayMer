using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HM.Framework.DapperExtensions.Sql
{
	public abstract class SqlDialectBase : ISqlDialect
	{
		public virtual char OpenQuote => '"';

		public virtual char CloseQuote => '"';

		public virtual string BatchSeperator => ";" + Environment.NewLine;

		public virtual bool SupportsMultipleStatements => true;

		public virtual char ParameterPrefix => '@';

		public string EmptyExpression => "1=1";

		public virtual string GetTableName(string schemaName, string tableName, string alias)
		{
			if (string.IsNullOrWhiteSpace(tableName))
			{
				throw new ArgumentNullException("TableName", "tableName cannot be null or empty.");
			}
			StringBuilder stringBuilder = new StringBuilder();
			if (!string.IsNullOrWhiteSpace(schemaName))
			{
				stringBuilder.AppendFormat(QuoteString(schemaName) + ".");
			}
			stringBuilder.AppendFormat(QuoteString(tableName));
			if (!string.IsNullOrWhiteSpace(alias))
			{
				stringBuilder.AppendFormat(" AS {0}", QuoteString(alias));
			}
			return stringBuilder.ToString();
		}

		public virtual string GetColumnName(string prefix, string columnName, string alias)
		{
			if (string.IsNullOrWhiteSpace(columnName))
			{
				throw new ArgumentNullException("ColumnName", "columnName cannot be null or empty.");
			}
			StringBuilder stringBuilder = new StringBuilder();
			if (!string.IsNullOrWhiteSpace(prefix))
			{
				stringBuilder.AppendFormat(QuoteString(prefix) + ".");
			}
			stringBuilder.AppendFormat(QuoteString(columnName));
			if (!string.IsNullOrWhiteSpace(alias))
			{
				stringBuilder.AppendFormat(" AS {0}", QuoteString(alias));
			}
			return stringBuilder.ToString();
		}

		public abstract string GetIdentitySql(string tableName);

		public abstract string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters);

		public abstract string GetSetSql(string sql, int firstResult, int maxResults, IDictionary<string, object> parameters);

		public virtual bool IsQuoted(string value)
		{
			if (value.Trim()[0] == OpenQuote)
			{
				return value.Trim().Last() == CloseQuote;
			}
			return false;
		}

		public virtual string QuoteString(string value)
		{
			if (IsQuoted(value) || value == "*")
			{
				return value;
			}
			return $"{OpenQuote}{value.Trim()}{CloseQuote}";
		}

		public virtual string UnQuoteString(string value)
		{
			if (!IsQuoted(value))
			{
				return value;
			}
			return value.Substring(1, value.Length - 2);
		}
	}
}
