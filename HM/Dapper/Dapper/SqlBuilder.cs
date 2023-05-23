using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HM.Framework.Dapper
{
	public class SqlBuilder
	{
		private class Clause
		{
			public string Sql
			{
				get;
				set;
			}

			public object Parameters
			{
				get;
				set;
			}

			public bool IsInclusive
			{
				get;
				set;
			}
		}

		private class Clauses : List<Clause>
		{
			private readonly string _joiner;

			private readonly string _prefix;

			private readonly string _postfix;

			public Clauses(string joiner, string prefix = "", string postfix = "")
			{
				_joiner = joiner;
				_prefix = prefix;
				_postfix = postfix;
			}

			public string ResolveClauses(DynamicParameters p)
			{
				using (Enumerator enumerator = GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Clause current = enumerator.Current;
						p.AddDynamicParams(current.Parameters);
					}
				}
				if (!this.Any((Clause a) => a.IsInclusive))
				{
					return _prefix + string.Join(_joiner, (from c in this
					select c.Sql).ToArray()) + _postfix;
				}
				return _prefix + string.Join(_joiner, (from a in this
				where !a.IsInclusive
				select a into c
				select c.Sql).Union(new string[1]
				{
					" ( " + string.Join(" OR ", (from a in this
					where a.IsInclusive
					select a into c
					select c.Sql).ToArray()) + " ) "
				}).ToArray()) + _postfix;
			}
		}

		public class Template
		{
			private readonly string _sql;

			private readonly SqlBuilder _builder;

			private readonly object _initParams;

			private int _dataSeq = -1;

			private static readonly Regex _regex = new Regex("\\/\\*\\*.+?\\*\\*\\/", RegexOptions.Multiline | RegexOptions.Compiled);

			private string rawSql;

			private object parameters;

			public string RawSql
			{
				get
				{
					ResolveSql();
					return rawSql;
				}
			}

			public object Parameters
			{
				get
				{
					ResolveSql();
					return parameters;
				}
			}

			public Template(SqlBuilder builder, string sql, dynamic parameters)
			{
				_initParams = parameters;
				_sql = sql;
				_builder = builder;
			}

			private void ResolveSql()
			{
				if (_dataSeq != _builder._seq)
				{
					DynamicParameters p = new DynamicParameters(_initParams);
					rawSql = _sql;
					foreach (KeyValuePair<string, Clauses> datum in _builder._data)
					{
						rawSql = rawSql.Replace("/**" + datum.Key + "**/", datum.Value.ResolveClauses(p));
					}
					parameters = p;
					rawSql = _regex.Replace(rawSql, "");
					_dataSeq = _builder._seq;
				}
			}
		}

		private readonly Dictionary<string, Clauses> _data = new Dictionary<string, Clauses>();

		private int _seq;

		public Template AddTemplate(string sql, dynamic parameters = null)
		{
			return new Template(this, sql, parameters);
		}

		protected SqlBuilder AddClause(string name, string sql, object parameters, string joiner, string prefix = "", string postfix = "", bool isInclusive = false)
		{
			if (!_data.TryGetValue(name, out Clauses value))
			{
				value = new Clauses(joiner, prefix, postfix);
				_data[name] = value;
			}
			value.Add(new Clause
			{
				Sql = sql,
				Parameters = parameters,
				IsInclusive = isInclusive
			});
			_seq++;
			return this;
		}

		public SqlBuilder Intersect(string sql, dynamic parameters = null)
		{
			return (SqlBuilder)this.AddClause("intersect", sql, parameters, "\nINTERSECT\n ", "\n ", "\n", false);
		}

		public SqlBuilder InnerJoin(string sql, dynamic parameters = null)
		{
			return (SqlBuilder)this.AddClause("innerjoin", sql, parameters, "\nINNER JOIN ", "\nINNER JOIN ", "\n", false);
		}

		public SqlBuilder LeftJoin(string sql, dynamic parameters = null)
		{
			return (SqlBuilder)this.AddClause("leftjoin", sql, parameters, "\nLEFT JOIN ", "\nLEFT JOIN ", "\n", false);
		}

		public SqlBuilder RightJoin(string sql, dynamic parameters = null)
		{
			return (SqlBuilder)this.AddClause("rightjoin", sql, parameters, "\nRIGHT JOIN ", "\nRIGHT JOIN ", "\n", false);
		}

		public SqlBuilder Where(string sql, dynamic parameters = null)
		{
			return (SqlBuilder)this.AddClause("where", sql, parameters, " AND ", "WHERE ", "\n", false);
		}

		public SqlBuilder OrWhere(string sql, dynamic parameters = null)
		{
			return (SqlBuilder)this.AddClause("where", sql, parameters, " OR ", "WHERE ", "\n", true);
		}

		public SqlBuilder OrderBy(string sql, dynamic parameters = null)
		{
			return (SqlBuilder)this.AddClause("orderby", sql, parameters, " , ", "ORDER BY ", "\n", false);
		}

		public SqlBuilder Select(string sql, dynamic parameters = null)
		{
			return (SqlBuilder)this.AddClause("select", sql, parameters, " , ", "", "\n", false);
		}

		public SqlBuilder AddParameters(dynamic parameters)
		{
			return (SqlBuilder)this.AddClause("--parameters", "", parameters, "", "", "", false);
		}

		public SqlBuilder Join(string sql, dynamic parameters = null)
		{
			return (SqlBuilder)this.AddClause("join", sql, parameters, "\nJOIN ", "\nJOIN ", "\n", false);
		}

		public SqlBuilder GroupBy(string sql, dynamic parameters = null)
		{
			return (SqlBuilder)this.AddClause("groupby", sql, parameters, " , ", "\nGROUP BY ", "\n", false);
		}

		public SqlBuilder Having(string sql, dynamic parameters = null)
		{
			return (SqlBuilder)this.AddClause("having", sql, parameters, "\nAND ", "HAVING ", "\n", false);
		}
	}
}
