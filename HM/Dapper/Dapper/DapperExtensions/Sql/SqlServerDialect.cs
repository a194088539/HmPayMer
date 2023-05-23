using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HM.Framework.DapperExtensions.Sql
{
	public class SqlServerDialect : SqlDialectBase
	{
		public override char OpenQuote => '[';

		public override char CloseQuote => ']';

		public override string GetIdentitySql(string tableName)
		{
			return $"SELECT CAST(SCOPE_IDENTITY()  AS BIGINT) AS [Id]";
		}

		public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters)
		{
			int firstResult = page * resultsPerPage + 1;
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
			int startIndex = GetSelectEnd(sql) + 1;
			string text = GetOrderByClause(sql);
			if (text == null)
			{
				text = "ORDER BY CURRENT_TIMESTAMP";
			}
			string text2 = GetColumnNames(sql).Aggregate(new StringBuilder(), (StringBuilder sb, string s) => ((sb.Length == 0) ? sb : sb.Append(", ")).Append(GetColumnName("_proj", s, null)), (StringBuilder sb) => sb.ToString());
			string text3 = sql.Replace(" " + text, string.Empty).Insert(startIndex, string.Format("ROW_NUMBER() OVER(ORDER BY {0}) AS {1}, ", text.Substring(9), GetColumnName(null, "_row_number", null)));
			string result = string.Format("SELECT TOP({0}) {1} FROM ({2}) [_proj] WHERE {3} >= @_pageStartRow ORDER BY {3}", maxResults, text2.Trim(), text3, GetColumnName("_proj", "_row_number", null));
			parameters.Add("@_pageStartRow", firstResult);
			return result;
		}

		protected string GetOrderByClause(string sql)
		{
			int num = sql.LastIndexOf(" ORDER BY ", StringComparison.InvariantCultureIgnoreCase);
			if (num == -1)
			{
				return null;
			}
			string text = sql.Substring(num).Trim();
			int num2 = text.IndexOf(" WHERE ", StringComparison.InvariantCultureIgnoreCase);
			if (num2 == -1)
			{
				return text;
			}
			return text.Substring(0, num2).Trim();
		}

		protected int GetFromStart(string sql)
		{
			int num = 0;
			string[] array = sql.Split(' ');
			int num2 = 0;
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (text.Equals("SELECT", StringComparison.InvariantCultureIgnoreCase))
				{
					num++;
				}
				if (text.Equals("FROM", StringComparison.InvariantCultureIgnoreCase))
				{
					num--;
					if (num == 0)
					{
						break;
					}
				}
				num2 += text.Length + 1;
			}
			return num2;
		}

		protected virtual int GetSelectEnd(string sql)
		{
			if (sql.StartsWith("SELECT DISTINCT", StringComparison.InvariantCultureIgnoreCase))
			{
				return 15;
			}
			if (sql.StartsWith("SELECT", StringComparison.InvariantCultureIgnoreCase))
			{
				return 6;
			}
			throw new ArgumentException("SQL must be a SELECT statement.", "sql");
		}

		protected virtual IList<string> GetColumnNames(string sql)
		{
			int selectEnd = GetSelectEnd(sql);
			int fromStart = GetFromStart(sql);
			string[] array = sql.Substring(selectEnd, fromStart - selectEnd).Split(',');
			List<string> list = new List<string>();
			string[] array2 = array;
			foreach (string text in array2)
			{
				int num = text.IndexOf(" AS ", StringComparison.InvariantCultureIgnoreCase);
				if (num > 0)
				{
					list.Add(text.Substring(num + 4).Trim());
				}
				else
				{
					string[] array3 = text.Split('.');
					list.Add(array3[array3.Length - 1].Trim());
				}
			}
			return list;
		}
	}
}
