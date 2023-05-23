using HM.Framework.DapperExtensions.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HM.Framework.DapperExtensions.Sql
{
	public class SqlGeneratorImpl : ISqlGenerator
	{
		public IDapperExtensionsConfiguration Configuration
		{
			get;
			private set;
		}

		public SqlGeneratorImpl(IDapperExtensionsConfiguration configuration)
		{
			Configuration = configuration;
		}

		public virtual string Select(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, IDictionary<string, object> parameters)
		{
			if (parameters == null)
			{
				throw new ArgumentNullException("Parameters");
			}
			StringBuilder stringBuilder = new StringBuilder($"SELECT {BuildSelectColumns(classMap)} FROM {GetTableName(classMap)}");
			if (predicate != null)
			{
				stringBuilder.Append(" WHERE ").Append(predicate.GetSql(this, parameters));
			}
			if (sort != null && sort.Any())
			{
				stringBuilder.Append(" ORDER BY ").Append((from s in sort
				select GetColumnName(classMap, s.PropertyName, includeAlias: false) + (s.Ascending ? " ASC" : " DESC")).AppendStrings());
			}
			return stringBuilder.ToString();
		}

		public virtual string SelectPaged(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDictionary<string, object> parameters)
		{
			if (sort == null || !sort.Any())
			{
				throw new ArgumentNullException("Sort", "Sort cannot be null or empty.");
			}
			if (parameters == null)
			{
				throw new ArgumentNullException("Parameters");
			}
			StringBuilder stringBuilder = new StringBuilder($"SELECT {BuildSelectColumns(classMap)} FROM {GetTableName(classMap)}");
			if (predicate != null)
			{
				stringBuilder.Append(" WHERE ").Append(predicate.GetSql(this, parameters));
			}
			string str = (from s in sort
			select GetColumnName(classMap, s.PropertyName, includeAlias: false) + (s.Ascending ? " ASC" : " DESC")).AppendStrings();
			stringBuilder.Append(" ORDER BY " + str);
			return Configuration.Dialect.GetPagingSql(stringBuilder.ToString(), page, resultsPerPage, parameters);
		}

		public virtual string SelectSet(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int firstResult, int maxResults, IDictionary<string, object> parameters)
		{
			if (sort == null || !sort.Any())
			{
				throw new ArgumentNullException("Sort", "Sort cannot be null or empty.");
			}
			if (parameters == null)
			{
				throw new ArgumentNullException("Parameters");
			}
			StringBuilder stringBuilder = new StringBuilder($"SELECT {BuildSelectColumns(classMap)} FROM {GetTableName(classMap)}");
			if (predicate != null)
			{
				stringBuilder.Append(" WHERE ").Append(predicate.GetSql(this, parameters));
			}
			string str = (from s in sort
			select GetColumnName(classMap, s.PropertyName, includeAlias: false) + (s.Ascending ? " ASC" : " DESC")).AppendStrings();
			stringBuilder.Append(" ORDER BY " + str);
			return Configuration.Dialect.GetSetSql(stringBuilder.ToString(), firstResult, maxResults, parameters);
		}

		public virtual string Count(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters)
		{
			if (parameters == null)
			{
				throw new ArgumentNullException("Parameters");
			}
			StringBuilder stringBuilder = new StringBuilder($"SELECT COUNT(*) AS {Configuration.Dialect.OpenQuote}Total{Configuration.Dialect.CloseQuote} FROM {GetTableName(classMap)}");
			if (predicate != null)
			{
				stringBuilder.Append(" WHERE ").Append(predicate.GetSql(this, parameters));
			}
			return stringBuilder.ToString();
		}

		public virtual string Insert(IClassMapper classMap)
		{
			IEnumerable<IPropertyMap> source = classMap.Properties.Where(delegate(IPropertyMap p)
			{
				if (!p.Ignored && !p.IsReadOnly && p.KeyType != KeyType.Identity)
				{
					return p.KeyType != KeyType.TriggerIdentity;
				}
				return false;
			});
			if (!source.Any())
			{
				throw new ArgumentException("No columns were mapped.");
			}
			IEnumerable<string> list = from p in source
			select GetColumnName(classMap, p, includeAlias: false);
			IEnumerable<string> list2 = from p in source
			select Configuration.Dialect.ParameterPrefix.ToString() + p.Name;
			string text = $"INSERT INTO {GetTableName(classMap)} ({list.AppendStrings()}) VALUES ({list2.AppendStrings()})";
			List<IPropertyMap> list3 = (from p in classMap.Properties
			where p.KeyType == KeyType.TriggerIdentity
			select p).ToList();
			if (list3.Count > 0)
			{
				if (list3.Count > 1)
				{
					throw new ArgumentException("TriggerIdentity generator cannot be used with multi-column keys");
				}
				text += $" RETURNING {(from p in list3 select GetColumnName(classMap, p, includeAlias: false)).First()} INTO {Configuration.Dialect.ParameterPrefix}IdOutParam";
			}
			return text;
		}

		public virtual string Update(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters)
		{
			if (predicate == null)
			{
				throw new ArgumentNullException("Predicate");
			}
			if (parameters == null)
			{
				throw new ArgumentNullException("Parameters");
			}
			IEnumerable<IPropertyMap> source = classMap.Properties.Where(delegate(IPropertyMap p)
			{
				if (!p.Ignored && !p.IsReadOnly && p.KeyType != KeyType.Identity)
				{
					return p.KeyType != KeyType.Assigned;
				}
				return false;
			});
			if (!source.Any())
			{
				throw new ArgumentException("No columns were mapped.");
			}
			IEnumerable<string> list = from p in source
			select $"{GetColumnName(classMap, p, includeAlias: false)} = {Configuration.Dialect.ParameterPrefix}{p.Name}";
			return $"UPDATE {GetTableName(classMap)} SET {list.AppendStrings()} WHERE {predicate.GetSql(this, parameters)}";
		}

		public virtual string Delete(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters)
		{
			if (predicate == null)
			{
				throw new ArgumentNullException("Predicate");
			}
			if (parameters == null)
			{
				throw new ArgumentNullException("Parameters");
			}
			StringBuilder stringBuilder = new StringBuilder($"DELETE FROM {GetTableName(classMap)}");
			stringBuilder.Append(" WHERE ").Append(predicate.GetSql(this, parameters));
			return stringBuilder.ToString();
		}

		public virtual string IdentitySql(IClassMapper classMap)
		{
			return Configuration.Dialect.GetIdentitySql(GetTableName(classMap));
		}

		public virtual string GetTableName(IClassMapper map)
		{
			return Configuration.Dialect.GetTableName(map.SchemaName, map.TableName, null);
		}

		public virtual string GetColumnName(IClassMapper map, IPropertyMap property, bool includeAlias)
		{
			string alias = null;
			if (property.ColumnName != property.Name && includeAlias)
			{
				alias = property.Name;
			}
			return Configuration.Dialect.GetColumnName(GetTableName(map), property.ColumnName, alias);
		}

		public virtual string GetColumnName(IClassMapper map, string propertyName, bool includeAlias)
		{
			IPropertyMap propertyMap = map.Properties.SingleOrDefault((IPropertyMap p) => p.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
			if (propertyMap == null)
			{
				throw new ArgumentException($"Could not find '{propertyName}' in Mapping.");
			}
			return GetColumnName(map, propertyMap, includeAlias);
		}

		public virtual bool SupportsMultipleStatements()
		{
			return Configuration.Dialect.SupportsMultipleStatements;
		}

		public virtual string BuildSelectColumns(IClassMapper classMap)
		{
			return (from p in classMap.Properties
			where !p.Ignored
			select GetColumnName(classMap, p, includeAlias: true)).AppendStrings();
		}
	}
}
