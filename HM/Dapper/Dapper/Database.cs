using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace HM.Framework.Dapper
{
	public abstract class Database<TDatabase> : IDisposable where TDatabase : Database<TDatabase>, new()
	{
		public class Table<T, TId>
		{
			internal Database<TDatabase> database;

			internal string tableName;

			internal string likelyTableName;

			private static readonly ConcurrentDictionary<Type, List<string>> paramNameCache = new ConcurrentDictionary<Type, List<string>>();

			public string TableName
			{
				get
				{
					tableName = (tableName ?? database.DetermineTableName<T>(likelyTableName));
					return tableName;
				}
			}

			public virtual async Task<int?> InsertAsync(dynamic data)
			{
				List<string> paramNames = GetParamNames((object)data);
				paramNames.Remove("Id");
				string text = string.Join(",", paramNames);
				string text2 = string.Join(",", from p in paramNames
				select "@" + p);
				string sql = "set nocount on insert " + TableName + " (" + text + ") values (" + text2 + ") select cast(scope_identity() as int)";
				return (await database.QueryAsync<int?>(sql, (object)data).ConfigureAwait(continueOnCapturedContext: false)).Single();
			}

			public Task<int> UpdateAsync(TId id, dynamic data)
			{
				List<string> paramNames = GetParamNames((object)data);
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("update ").Append(TableName).Append(" set ");
				stringBuilder.AppendLine(string.Join(",", from n in paramNames
				where n != "Id"
				select n into p
				select p + "= @" + p));
				stringBuilder.Append("where Id = @Id");
				DynamicParameters dynamicParameters = new DynamicParameters(data);
				dynamicParameters.Add("Id", id);
				return database.ExecuteAsync(stringBuilder.ToString(), dynamicParameters);
			}

			public async Task<bool> DeleteAsync(TId id)
			{
				return await database.ExecuteAsync("delete from " + TableName + " where Id = @id", new
				{
					id
				}).ConfigureAwait(continueOnCapturedContext: false) > 0;
			}

			public Task<T> GetAsync(TId id)
			{
				return database.QueryFirstOrDefaultAsync<T>("select * from " + TableName + " where Id = @id", new
				{
					id
				});
			}

			public virtual Task<T> FirstAsync()
			{
				return database.QueryFirstOrDefaultAsync<T>("select top 1 * from " + TableName);
			}

			public Task<IEnumerable<T>> AllAsync()
			{
				return database.QueryAsync<T>("select * from " + TableName);
			}

			public Table(Database<TDatabase> database, string likelyTableName)
			{
				this.database = database;
				this.likelyTableName = likelyTableName;
			}

			public virtual int? Insert(dynamic data)
			{
				List<string> paramNames = GetParamNames((object)data);
				paramNames.Remove("Id");
				string text = string.Join(",", paramNames);
				string text2 = string.Join(",", from p in paramNames
				select "@" + p);
				string sql = "set nocount on insert " + TableName + " (" + text + ") values (" + text2 + ") select cast(scope_identity() as int)";
				return database.Query<int?>(sql, (object)data).Single();
			}

			public int Update(TId id, dynamic data)
			{
				List<string> paramNames = GetParamNames((object)data);
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("update ").Append(TableName).Append(" set ");
				stringBuilder.AppendLine(string.Join(",", from n in paramNames
				where n != "Id"
				select n into p
				select p + "= @" + p));
				stringBuilder.Append("where Id = @Id");
				DynamicParameters dynamicParameters = new DynamicParameters(data);
				dynamicParameters.Add("Id", id);
				return database.Execute(stringBuilder.ToString(), dynamicParameters);
			}

			public bool Delete(TId id)
			{
				return database.Execute("delete from " + TableName + " where Id = @id", new
				{
					id
				}) > 0;
			}

			public T Get(TId id)
			{
				return database.QueryFirstOrDefault<T>("select * from " + TableName + " where Id = @id", new
				{
					id
				});
			}

			public virtual T First()
			{
				return database.QueryFirstOrDefault<T>("select top 1 * from " + TableName);
			}

			public IEnumerable<T> All()
			{
				return database.Query<T>("select * from " + TableName);
			}

			internal static List<string> GetParamNames(object o)
			{
				DynamicParameters dynamicParameters;
				if ((dynamicParameters = (o as DynamicParameters)) != null)
				{
					return dynamicParameters.ParameterNames.ToList();
				}
				if (!paramNameCache.TryGetValue(o.GetType(), out List<string> value))
				{
					value = new List<string>();
					foreach (PropertyInfo item in from p in o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
					where p.GetGetMethod(nonPublic: false) != null
					select p)
					{
						IgnorePropertyAttribute ignorePropertyAttribute = item.GetCustomAttributes(typeof(IgnorePropertyAttribute), inherit: true).FirstOrDefault() as IgnorePropertyAttribute;
						if (ignorePropertyAttribute == null || !ignorePropertyAttribute.Value)
						{
							value.Add(item.Name);
						}
					}
					paramNameCache[o.GetType()] = value;
				}
				return value;
			}
		}

		public class Table<T> : Table<T, int>
		{
			public Table(Database<TDatabase> database, string likelyTableName)
				: base(database, likelyTableName)
			{
			}
		}

		private DbConnection _connection;

		private int _commandTimeout;

		private DbTransaction _transaction;

		internal static Action<TDatabase> tableConstructor;

		private static readonly ConcurrentDictionary<Type, string> tableNameMap = new ConcurrentDictionary<Type, string>();

		public Task<int> ExecuteAsync(string sql, dynamic param = null)
		{
			return _connection.ExecuteAsync(sql, (object)param, _transaction, _commandTimeout);
		}

		public Task<IEnumerable<T>> QueryAsync<T>(string sql, dynamic param = null)
		{
			return _connection.QueryAsync<T>(sql, (object)param, _transaction, _commandTimeout);
		}

		public Task<T> QueryFirstOrDefaultAsync<T>(string sql, dynamic param = null)
		{
			return _connection.QueryFirstOrDefaultAsync<T>(sql, (object)param, _transaction, _commandTimeout);
		}

		public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = default(int?))
		{
			return _connection.QueryAsync(sql, map, (object)param, transaction, buffered, splitOn, commandTimeout);
		}

		public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = default(int?))
		{
			return _connection.QueryAsync(sql, map, (object)param, transaction, buffered, splitOn, commandTimeout);
		}

		public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = default(int?))
		{
			return _connection.QueryAsync(sql, map, (object)param, transaction, buffered, splitOn, commandTimeout);
		}

		public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = default(int?))
		{
			return _connection.QueryAsync(sql, map, (object)param, transaction, buffered, splitOn, commandTimeout);
		}

		public Task<IEnumerable<dynamic>> QueryAsync(string sql, dynamic param = null)
		{
			return _connection.QueryAsync(sql, (object)param, _transaction);
		}

		public Task<SqlMapper.GridReader> QueryMultipleAsync(string sql, dynamic param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return (Task<SqlMapper.GridReader>)SqlMapper.QueryMultipleAsync(_connection, sql, param, transaction, commandTimeout, commandType);
		}

		public static TDatabase Init(DbConnection connection, int commandTimeout)
		{
			TDatabase val = new TDatabase();
			val.InitDatabase(connection, commandTimeout);
			return val;
		}

		internal void InitDatabase(DbConnection connection, int commandTimeout)
		{
			_connection = connection;
			_commandTimeout = commandTimeout;
			tableConstructor = (tableConstructor ?? CreateTableConstructorForTable());
			tableConstructor(this as TDatabase);
		}

		internal virtual Action<TDatabase> CreateTableConstructorForTable()
		{
			return CreateTableConstructor(typeof(Table<>), typeof(Table<, >));
		}

		public void BeginTransaction(IsolationLevel isolation = IsolationLevel.ReadCommitted)
		{
			_transaction = _connection.BeginTransaction(isolation);
		}

		public void CommitTransaction()
		{
			_transaction.Commit();
			_transaction = null;
		}

		public void RollbackTransaction()
		{
			_transaction.Rollback();
			_transaction = null;
		}

		protected Action<TDatabase> CreateTableConstructor(Type tableType)
		{
			return CreateTableConstructor(new Type[1]
			{
				tableType
			});
		}

		protected Action<TDatabase> CreateTableConstructor(params Type[] tableTypes)
		{
			DynamicMethod dynamicMethod = new DynamicMethod("ConstructInstances", null, new Type[1]
			{
				typeof(TDatabase)
			}, restrictedSkipVisibility: true);
			ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
			foreach (Tuple<MethodInfo, ConstructorInfo, string, Type> item in from p in GetType().GetProperties().Where(delegate(PropertyInfo p)
			{
				if (p.PropertyType.IsGenericType())
				{
					return tableTypes.Contains(p.PropertyType.GetGenericTypeDefinition());
				}
				return false;
			})
			select Tuple.Create(p.GetSetMethod(nonPublic: true), p.PropertyType.GetConstructor(new Type[2]
			{
				typeof(TDatabase),
				typeof(string)
			}), p.Name, p.DeclaringType))
			{
				iLGenerator.Emit(OpCodes.Ldarg_0);
				iLGenerator.Emit(OpCodes.Ldstr, item.Item3);
				iLGenerator.Emit(OpCodes.Newobj, item.Item2);
				LocalBuilder local = iLGenerator.DeclareLocal(item.Item2.DeclaringType);
				iLGenerator.Emit(OpCodes.Stloc, local);
				iLGenerator.Emit(OpCodes.Ldarg_0);
				iLGenerator.Emit(OpCodes.Castclass, item.Item4);
				iLGenerator.Emit(OpCodes.Ldloc, local);
				iLGenerator.Emit(OpCodes.Callvirt, item.Item1);
			}
			iLGenerator.Emit(OpCodes.Ret);
			return (Action<TDatabase>)dynamicMethod.CreateDelegate(typeof(Action<TDatabase>));
		}

		private string DetermineTableName<T>(string likelyTableName)
		{
			if (!tableNameMap.TryGetValue(typeof(T), out string value))
			{
				value = likelyTableName;
				if (!TableExists(value))
				{
					value = "[" + typeof(T).Name + "]";
				}
				tableNameMap[typeof(T)] = value;
			}
			return value;
		}

		private bool TableExists(string name)
		{
			string text = null;
			name = name.Replace("[", "");
			name = name.Replace("]", "");
			if (name.Contains("."))
			{
				string[] array = name.Split('.');
				if (array.Length == 2)
				{
					text = array[0];
					name = array[1];
				}
			}
			StringBuilder stringBuilder = new StringBuilder("select 1 from INFORMATION_SCHEMA.TABLES where ");
			if (!string.IsNullOrEmpty(text))
			{
				stringBuilder.Append("TABLE_SCHEMA = @schemaName AND ");
			}
			stringBuilder.Append("TABLE_NAME = @name");
			return _connection.Query(stringBuilder.ToString(), new
			{
				schemaName = text,
				name = name
			}, _transaction).Count() == 1;
		}

		public int Execute(string sql, dynamic param = null)
		{
			return _connection.Execute(sql, (object)param, _transaction, _commandTimeout);
		}

		public IEnumerable<T> Query<T>(string sql, dynamic param = null, bool buffered = true)
		{
			return _connection.Query<T>(sql, (object)param, _transaction, buffered, _commandTimeout);
		}

		public T QueryFirstOrDefault<T>(string sql, dynamic param = null)
		{
			return _connection.QueryFirstOrDefault<T>(sql, (object)param, _transaction, _commandTimeout);
		}

		public IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = default(int?))
		{
			return _connection.Query(sql, map, (object)param, transaction, buffered, splitOn, commandTimeout);
		}

		public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = default(int?))
		{
			return _connection.Query(sql, map, (object)param, transaction, buffered, splitOn, commandTimeout);
		}

		public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = default(int?))
		{
			return _connection.Query(sql, map, (object)param, transaction, buffered, splitOn, commandTimeout);
		}

		public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = default(int?))
		{
			return _connection.Query(sql, map, (object)param, transaction, buffered, splitOn, commandTimeout);
		}

		public IEnumerable<dynamic> Query(string sql, dynamic param = null, bool buffered = true)
		{
			return _connection.Query(sql, (object)param, _transaction, buffered);
		}

		public SqlMapper.GridReader QueryMultiple(string sql, dynamic param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return (SqlMapper.GridReader)SqlMapper.QueryMultiple(_connection, sql, param, transaction, commandTimeout, commandType);
		}

		public void Dispose()
		{
			if (_connection.State != 0)
			{
				_transaction?.Rollback();
				_connection.Close();
				_connection = null;
			}
		}
	}
}
