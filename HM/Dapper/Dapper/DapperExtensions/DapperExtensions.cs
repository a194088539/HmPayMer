using HM.Framework.DapperExtensions.Mapper;
using HM.Framework.DapperExtensions.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace HM.Framework.DapperExtensions
{
	public static class DapperExtensions
	{
		private static readonly object _lock;

		private static Func<IDapperExtensionsConfiguration, IDapperImplementor> _instanceFactory;

		private static IDapperImplementor _instance;

		private static IDapperExtensionsConfiguration _configuration;

		public static Type DefaultMapper
		{
			get
			{
				return _configuration.DefaultMapper;
			}
			set
			{
				Configure(value, _configuration.MappingAssemblies, _configuration.Dialect);
			}
		}

		public static ISqlDialect SqlDialect
		{
			get
			{
				return _configuration.Dialect;
			}
			set
			{
				Configure(_configuration.DefaultMapper, _configuration.MappingAssemblies, value);
			}
		}

		public static Func<IDapperExtensionsConfiguration, IDapperImplementor> InstanceFactory
		{
			get
			{
				if (_instanceFactory == null)
				{
					_instanceFactory = ((IDapperExtensionsConfiguration config) => new DapperImplementor(new SqlGeneratorImpl(config)));
				}
				return _instanceFactory;
			}
			set
			{
				_instanceFactory = value;
				Configure(_configuration.DefaultMapper, _configuration.MappingAssemblies, _configuration.Dialect);
			}
		}

		private static IDapperImplementor Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (_lock)
					{
						if (_instance == null)
						{
							_instance = InstanceFactory(_configuration);
						}
					}
				}
				return _instance;
			}
		}

		static DapperExtensions()
		{
			_lock = new object();
			Configure(typeof(AutoClassMapper<>), new List<Assembly>(), new SqlServerDialect());
		}

		public static void SetMappingAssemblies(IList<Assembly> assemblies)
		{
			Configure(_configuration.DefaultMapper, assemblies, _configuration.Dialect);
		}

		public static void Configure(IDapperExtensionsConfiguration configuration)
		{
			_instance = null;
			_configuration = configuration;
		}

		public static void Configure(Type defaultMapper, IList<Assembly> mappingAssemblies, ISqlDialect sqlDialect)
		{
			Configure(new DapperExtensionsConfiguration(defaultMapper, mappingAssemblies, sqlDialect));
		}

		public static T Get<T>(this IDbConnection connection, dynamic id, IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : class
		{
			dynamic val = Instance.Get<T>(connection, id, transaction, commandTimeout);
			return (T)val;
		}

		public static void Insert<T>(this IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : class
		{
			Instance.Insert(connection, entities, transaction, commandTimeout);
		}

		public static dynamic Insert<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : class
		{
			return Instance.Insert(connection, entity, transaction, commandTimeout);
		}

		public static bool Update<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : class
		{
			return Instance.Update(connection, entity, transaction, commandTimeout);
		}

		public static bool Delete<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : class
		{
			return Instance.Delete(connection, entity, transaction, commandTimeout);
		}

		public static bool Delete<T>(this IDbConnection connection, object predicate, IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : class
		{
			return Instance.Delete<T>(connection, predicate, transaction, commandTimeout);
		}

		public static IEnumerable<T> GetList<T>(this IDbConnection connection, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), bool buffered = false) where T : class
		{
			return Instance.GetList<T>(connection, predicate, sort, transaction, commandTimeout, buffered);
		}

		public static IEnumerable<T> GetPage<T>(this IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction = null, int? commandTimeout = default(int?), bool buffered = false) where T : class
		{
			return Instance.GetPage<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered);
		}

		public static IEnumerable<T> GetSet<T>(this IDbConnection connection, object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction = null, int? commandTimeout = default(int?), bool buffered = false) where T : class
		{
			return Instance.GetSet<T>(connection, predicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered);
		}

		public static int Count<T>(this IDbConnection connection, object predicate, IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : class
		{
			return Instance.Count<T>(connection, predicate, transaction, commandTimeout);
		}

		public static IMultipleResultReader GetMultiple(this IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction = null, int? commandTimeout = default(int?))
		{
			return Instance.GetMultiple(connection, predicate, transaction, commandTimeout);
		}

		public static IClassMapper GetMap<T>() where T : class
		{
			return Instance.SqlGenerator.Configuration.GetMap<T>();
		}

		public static void ClearCache()
		{
			Instance.SqlGenerator.Configuration.ClearCache();
		}

		public static Guid GetNextGuid()
		{
			return Instance.SqlGenerator.Configuration.GetNextGuid();
		}
	}
}
