using HM.Framework.DapperExtensions.Mapper;
using System;
using System.Collections.Generic;
using System.Data;

namespace HM.Framework.DapperExtensions
{
	public interface IDatabase : IDisposable
	{
		bool HasActiveTransaction
		{
			get;
		}

		IDbConnection Connection
		{
			get;
		}

		void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

		void Commit();

		void Rollback();

		void RunInTransaction(Action action);

		T RunInTransaction<T>(Func<T> func);

		T Get<T>(dynamic id, IDbTransaction transaction, int? commandTimeout = default(int?)) where T : class;

		T Get<T>(dynamic id, int? commandTimeout = default(int?)) where T : class;

		void Insert<T>(IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout = default(int?)) where T : class;

		void Insert<T>(IEnumerable<T> entities, int? commandTimeout = default(int?)) where T : class;

		dynamic Insert<T>(T entity, IDbTransaction transaction, int? commandTimeout = default(int?)) where T : class;

		dynamic Insert<T>(T entity, int? commandTimeout = default(int?)) where T : class;

		bool Update<T>(T entity, IDbTransaction transaction, int? commandTimeout = default(int?)) where T : class;

		bool Update<T>(T entity, int? commandTimeout = default(int?)) where T : class;

		bool Delete<T>(T entity, IDbTransaction transaction, int? commandTimeout = default(int?)) where T : class;

		bool Delete<T>(T entity, int? commandTimeout = default(int?)) where T : class;

		bool Delete<T>(object predicate, IDbTransaction transaction, int? commandTimeout = default(int?)) where T : class;

		bool Delete<T>(object predicate, int? commandTimeout = default(int?)) where T : class;

		IEnumerable<T> GetList<T>(object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout = default(int?), bool buffered = true) where T : class;

		IEnumerable<T> GetList<T>(object predicate = null, IList<ISort> sort = null, int? commandTimeout = default(int?), bool buffered = true) where T : class;

		IEnumerable<T> GetPage<T>(object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout = default(int?), bool buffered = true) where T : class;

		IEnumerable<T> GetPage<T>(object predicate, IList<ISort> sort, int page, int resultsPerPage, int? commandTimeout = default(int?), bool buffered = true) where T : class;

		IEnumerable<T> GetSet<T>(object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class;

		IEnumerable<T> GetSet<T>(object predicate, IList<ISort> sort, int firstResult, int maxResults, int? commandTimeout, bool buffered) where T : class;

		int Count<T>(object predicate, IDbTransaction transaction, int? commandTimeout = default(int?)) where T : class;

		int Count<T>(object predicate, int? commandTimeout = default(int?)) where T : class;

		IMultipleResultReader GetMultiple(GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout = default(int?));

		IMultipleResultReader GetMultiple(GetMultiplePredicate predicate, int? commandTimeout = default(int?));

		void ClearCache();

		Guid GetNextGuid();

		IClassMapper GetMap<T>() where T : class;
	}
}
