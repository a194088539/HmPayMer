using HM.Framework.Dapper;
using HM.Framework.DapperExtensions.Mapper;
using HM.Framework.DapperExtensions.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HM.Framework.DapperExtensions
{
	public class DapperImplementor : IDapperImplementor
	{
		public ISqlGenerator SqlGenerator
		{
			get;
			private set;
		}

		public DapperImplementor(ISqlGenerator sqlGenerator)
		{
			SqlGenerator = sqlGenerator;
		}

		public T Get<T>(IDbConnection connection, dynamic id, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			IClassMapper map = SqlGenerator.Configuration.GetMap<T>();
			IPredicate predicate = (IPredicate)this.GetIdPredicate(map, id);
			return GetList<T>(connection, map, predicate, null, transaction, commandTimeout, buffered: true).SingleOrDefault();
		}

		public void Insert<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			IEnumerable<PropertyInfo> enumerable = null;
			IClassMapper map = SqlGenerator.Configuration.GetMap<T>();
			IEnumerable<IPropertyMap> enumerable2 = from p in map.Properties
			where p.KeyType != KeyType.NotAKey
			select p;
			IPropertyMap triggerIdentityColumn = map.Properties.SingleOrDefault((IPropertyMap p) => p.KeyType == KeyType.TriggerIdentity);
			List<DynamicParameters> list = new List<DynamicParameters>();
			if (triggerIdentityColumn != null)
			{
				enumerable = from p in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
				where p.Name != triggerIdentityColumn.PropertyInfo.Name
				select p;
			}
			foreach (T entity in entities)
			{
				foreach (IPropertyMap item in enumerable2)
				{
					if (item.KeyType == KeyType.Guid && (Guid)item.PropertyInfo.GetValue(entity, null) == Guid.Empty)
					{
						Guid nextGuid = SqlGenerator.Configuration.GetNextGuid();
						item.PropertyInfo.SetValue(entity, nextGuid, null);
					}
				}
				if (triggerIdentityColumn != null)
				{
					DynamicParameters dynamicParameters = new DynamicParameters();
					foreach (PropertyInfo item2 in enumerable)
					{
						dynamicParameters.Add(item2.Name, item2.GetValue(entity, null));
					}
					object value = typeof(T).GetProperty(triggerIdentityColumn.PropertyInfo.Name).GetValue(entity, null);
					DynamicParameters dynamicParameters2 = dynamicParameters;
					ParameterDirection? direction = ParameterDirection.Output;
					dynamicParameters2.Add("IdOutParam", value, null, direction);
					list.Add(dynamicParameters);
				}
			}
			string sql = SqlGenerator.Insert(map);
			if (triggerIdentityColumn == null)
			{
				connection.Execute(sql, entities, transaction, commandTimeout, CommandType.Text);
			}
			else
			{
				connection.Execute(sql, list, transaction, commandTimeout, CommandType.Text);
			}
		}

		public dynamic Insert<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			IClassMapper map = SqlGenerator.Configuration.GetMap<T>();
			List<IPropertyMap> list = map.Properties.Where(delegate(IPropertyMap p)
			{
				if (p.KeyType != KeyType.Guid)
				{
					return p.KeyType == KeyType.Assigned;
				}
				return true;
			}).ToList();
			IPropertyMap propertyMap = map.Properties.SingleOrDefault((IPropertyMap p) => p.KeyType == KeyType.Identity);
			IPropertyMap triggerIdentityColumn = map.Properties.SingleOrDefault((IPropertyMap p) => p.KeyType == KeyType.TriggerIdentity);
			foreach (IPropertyMap item in list)
			{
				if (item.KeyType == KeyType.Guid && (Guid)item.PropertyInfo.GetValue(entity, null) == Guid.Empty)
				{
					Guid nextGuid = SqlGenerator.Configuration.GetNextGuid();
					item.PropertyInfo.SetValue(entity, nextGuid, null);
				}
			}
			IDictionary<string, object> dictionary = new ExpandoObject();
			string text = SqlGenerator.Insert(map);
			if (propertyMap != null)
			{
				IEnumerable<long> source;
				if (SqlGenerator.SupportsMultipleStatements())
				{
					text = text + SqlGenerator.Configuration.Dialect.BatchSeperator + SqlGenerator.IdentitySql(map);
					source = connection.Query<long>(text, entity, transaction, buffered: false, commandTimeout, CommandType.Text);
				}
				else
				{
					connection.Execute(text, entity, transaction, commandTimeout, CommandType.Text);
					text = SqlGenerator.IdentitySql(map);
					source = connection.Query<long>(text, entity, transaction, buffered: false, commandTimeout, CommandType.Text);
				}
				int num = Convert.ToInt32(source.First());
				dictionary.Add(propertyMap.Name, num);
				propertyMap.PropertyInfo.SetValue(entity, num, null);
			}
			else if (triggerIdentityColumn != null)
			{
				DynamicParameters dynamicParameters = new DynamicParameters();
				foreach (PropertyInfo item2 in from p in entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
				where p.Name != triggerIdentityColumn.PropertyInfo.Name
				select p)
				{
					dynamicParameters.Add(item2.Name, item2.GetValue(entity, null));
				}
				object value = entity.GetType().GetProperty(triggerIdentityColumn.PropertyInfo.Name).GetValue(entity, null);
				DynamicParameters dynamicParameters2 = dynamicParameters;
				ParameterDirection? direction = ParameterDirection.Output;
				dynamicParameters2.Add("IdOutParam", value, null, direction);
				connection.Execute(text, dynamicParameters, transaction, commandTimeout, CommandType.Text);
				object value2 = dynamicParameters.Get<object>(SqlGenerator.Configuration.Dialect.ParameterPrefix.ToString() + "IdOutParam");
				dictionary.Add(triggerIdentityColumn.Name, value2);
				triggerIdentityColumn.PropertyInfo.SetValue(entity, value2, null);
			}
			else
			{
				connection.Execute(text, entity, transaction, commandTimeout, CommandType.Text);
			}
			foreach (IPropertyMap item3 in list)
			{
				dictionary.Add(item3.Name, item3.PropertyInfo.GetValue(entity, null));
			}
			if (dictionary.Count == 1)
			{
				return dictionary.First().Value;
			}
			return dictionary;
		}

		public bool Update<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			IClassMapper map = SqlGenerator.Configuration.GetMap<T>();
			IPredicate keyPredicate = GetKeyPredicate(map, entity);
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			string sql = SqlGenerator.Update(map, keyPredicate, dictionary);
			DynamicParameters dynamicParameters = new DynamicParameters();
			IEnumerable<IPropertyMap> columns = map.Properties.Where(delegate(IPropertyMap p)
			{
				if (!p.Ignored && !p.IsReadOnly && p.KeyType != KeyType.Identity)
				{
					return p.KeyType != KeyType.Assigned;
				}
				return false;
			});
			foreach (KeyValuePair<string, object> item in from property in ReflectionHelper.GetObjectValues(entity)
			where columns.Any((IPropertyMap c) => c.Name == property.Key)
			select property)
			{
				dynamicParameters.Add(item.Key, item.Value);
			}
			foreach (KeyValuePair<string, object> item2 in dictionary)
			{
				dynamicParameters.Add(item2.Key, item2.Value);
			}
			return connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
		}

		public bool Delete<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			IClassMapper map = SqlGenerator.Configuration.GetMap<T>();
			IPredicate keyPredicate = GetKeyPredicate(map, entity);
			return Delete<T>(connection, map, keyPredicate, transaction, commandTimeout);
		}

		public bool Delete<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			IClassMapper map = SqlGenerator.Configuration.GetMap<T>();
			IPredicate predicate2 = GetPredicate(map, predicate);
			return Delete<T>(connection, map, predicate2, transaction, commandTimeout);
		}

		public IEnumerable<T> GetList<T>(IDbConnection connection, object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class
		{
			IClassMapper map = SqlGenerator.Configuration.GetMap<T>();
			IPredicate predicate2 = GetPredicate(map, predicate);
			return GetList<T>(connection, map, predicate2, sort, transaction, commandTimeout, buffered: true);
		}

		public IEnumerable<T> GetPage<T>(IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class
		{
			IClassMapper map = SqlGenerator.Configuration.GetMap<T>();
			IPredicate predicate2 = GetPredicate(map, predicate);
			return GetPage<T>(connection, map, predicate2, sort, page, resultsPerPage, transaction, commandTimeout, buffered);
		}

		public IEnumerable<T> GetSet<T>(IDbConnection connection, object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class
		{
			IClassMapper map = SqlGenerator.Configuration.GetMap<T>();
			IPredicate predicate2 = GetPredicate(map, predicate);
			return GetSet<T>(connection, map, predicate2, sort, firstResult, maxResults, transaction, commandTimeout, buffered);
		}

		public int Count<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			IClassMapper map = SqlGenerator.Configuration.GetMap<T>();
			IPredicate predicate2 = GetPredicate(map, predicate);
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			string sql = SqlGenerator.Count(map, predicate2, dictionary);
			DynamicParameters dynamicParameters = new DynamicParameters();
			foreach (KeyValuePair<string, object> item in dictionary)
			{
				dynamicParameters.Add(item.Key, item.Value);
			}
			return (int)connection.Query(sql, dynamicParameters, transaction, buffered: false, commandTimeout, CommandType.Text).Single().Total;
		}

		public IMultipleResultReader GetMultiple(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout)
		{
			if (SqlGenerator.SupportsMultipleStatements())
			{
				return GetMultipleByBatch(connection, predicate, transaction, commandTimeout);
			}
			return GetMultipleBySequence(connection, predicate, transaction, commandTimeout);
		}

		protected IEnumerable<T> GetList<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			string sql = SqlGenerator.Select(classMap, predicate, sort, dictionary);
			DynamicParameters dynamicParameters = new DynamicParameters();
			foreach (KeyValuePair<string, object> item in dictionary)
			{
				dynamicParameters.Add(item.Key, item.Value);
			}
			return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
		}

		protected IEnumerable<T> GetPage<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			string sql = SqlGenerator.SelectPaged(classMap, predicate, sort, page, resultsPerPage, dictionary);
			DynamicParameters dynamicParameters = new DynamicParameters();
			foreach (KeyValuePair<string, object> item in dictionary)
			{
				dynamicParameters.Add(item.Key, item.Value);
			}
			return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
		}

		protected IEnumerable<T> GetSet<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			string sql = SqlGenerator.SelectSet(classMap, predicate, sort, firstResult, maxResults, dictionary);
			DynamicParameters dynamicParameters = new DynamicParameters();
			foreach (KeyValuePair<string, object> item in dictionary)
			{
				dynamicParameters.Add(item.Key, item.Value);
			}
			return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
		}

		protected bool Delete<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			string sql = SqlGenerator.Delete(classMap, predicate, dictionary);
			DynamicParameters dynamicParameters = new DynamicParameters();
			foreach (KeyValuePair<string, object> item in dictionary)
			{
				dynamicParameters.Add(item.Key, item.Value);
			}
			return connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
		}

		protected IPredicate GetPredicate(IClassMapper classMap, object predicate)
		{
			IPredicate predicate2 = predicate as IPredicate;
			if (predicate2 == null && predicate != null)
			{
				predicate2 = GetEntityPredicate(classMap, predicate);
			}
			return predicate2;
		}

		protected IPredicate GetIdPredicate(IClassMapper classMap, object id)
		{
			bool flag = ReflectionHelper.IsSimpleType(id.GetType());
			IEnumerable<IPropertyMap> enumerable = from p in classMap.Properties
			where p.KeyType != KeyType.NotAKey
			select p;
			IDictionary<string, object> dictionary = null;
			IList<IPredicate> list = new List<IPredicate>();
			if (!flag)
			{
				dictionary = ReflectionHelper.GetObjectValues(id);
			}
			foreach (IPropertyMap item in enumerable)
			{
				object value = id;
				if (!flag)
				{
					value = dictionary[item.Name];
				}
				IFieldPredicate fieldPredicate = Activator.CreateInstance(typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType)) as IFieldPredicate;
				fieldPredicate.Not = false;
				fieldPredicate.Operator = Operator.Eq;
				fieldPredicate.PropertyName = item.Name;
				fieldPredicate.Value = value;
				list.Add(fieldPredicate);
			}
			if (list.Count != 1)
			{
				return new PredicateGroup
				{
					Operator = GroupOperator.And,
					Predicates = list
				};
			}
			return list[0];
		}

		protected IPredicate GetKeyPredicate<T>(IClassMapper classMap, T entity) where T : class
		{
			IEnumerable<IPropertyMap> source = from p in classMap.Properties
			where p.KeyType != KeyType.NotAKey
			select p;
			if (!source.Any())
			{
				throw new ArgumentException("At least one Key column must be defined.");
			}
			IList<IPredicate> list = (from field in source
			select new FieldPredicate<T>
			{
				Not = false,
				Operator = Operator.Eq,
				PropertyName = field.Name,
				Value = field.PropertyInfo.GetValue(entity, null)
			}).Cast<IPredicate>().ToList();
			if (list.Count != 1)
			{
				return new PredicateGroup
				{
					Operator = GroupOperator.And,
					Predicates = list
				};
			}
			return list[0];
		}

		protected IPredicate GetEntityPredicate(IClassMapper classMap, object entity)
		{
			Type type = typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType);
			IList<IPredicate> list = new List<IPredicate>();
			foreach (KeyValuePair<string, object> objectValue in ReflectionHelper.GetObjectValues(entity))
			{
				IFieldPredicate fieldPredicate = Activator.CreateInstance(type) as IFieldPredicate;
				fieldPredicate.Not = false;
				fieldPredicate.Operator = Operator.Eq;
				fieldPredicate.PropertyName = objectValue.Key;
				fieldPredicate.Value = objectValue.Value;
				list.Add(fieldPredicate);
			}
			if (list.Count != 1)
			{
				return new PredicateGroup
				{
					Operator = GroupOperator.And,
					Predicates = list
				};
			}
			return list[0];
		}

		protected GridReaderResultReader GetMultipleByBatch(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			StringBuilder stringBuilder = new StringBuilder();
			foreach (GetMultiplePredicate.GetMultiplePredicateItem item in predicate.Items)
			{
				IClassMapper map = SqlGenerator.Configuration.GetMap(item.Type);
				IPredicate predicate2 = item.Value as IPredicate;
				if (predicate2 == null && item.Value != null)
				{
					predicate2 = GetPredicate(map, item.Value);
				}
				stringBuilder.AppendLine(SqlGenerator.Select(map, predicate2, item.Sort, dictionary) + SqlGenerator.Configuration.Dialect.BatchSeperator);
			}
			DynamicParameters dynamicParameters = new DynamicParameters();
			foreach (KeyValuePair<string, object> item2 in dictionary)
			{
				dynamicParameters.Add(item2.Key, item2.Value);
			}
			return new GridReaderResultReader(connection.QueryMultiple(stringBuilder.ToString(), dynamicParameters, transaction, commandTimeout, CommandType.Text));
		}

		protected SequenceReaderResultReader GetMultipleBySequence(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout)
		{
			IList<SqlMapper.GridReader> list = new List<SqlMapper.GridReader>();
			foreach (GetMultiplePredicate.GetMultiplePredicateItem item2 in predicate.Items)
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				IClassMapper map = SqlGenerator.Configuration.GetMap(item2.Type);
				IPredicate predicate2 = item2.Value as IPredicate;
				if (predicate2 == null && item2.Value != null)
				{
					predicate2 = GetPredicate(map, item2.Value);
				}
				string sql = SqlGenerator.Select(map, predicate2, item2.Sort, dictionary);
				DynamicParameters dynamicParameters = new DynamicParameters();
				foreach (KeyValuePair<string, object> item3 in dictionary)
				{
					dynamicParameters.Add(item3.Key, item3.Value);
				}
				SqlMapper.GridReader item = connection.QueryMultiple(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
				list.Add(item);
			}
			return new SequenceReaderResultReader(list);
		}
	}
}
