using Microsoft.SqlServer.Server;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace HM.Framework.Dapper
{
	public static class SqlMapper
	{
		private struct AsyncExecState
		{
			public readonly DbCommand Command;

			public readonly Task<int> Task;

			public AsyncExecState(DbCommand command, Task<int> task)
			{
				Command = command;
				Task = task;
			}
		}

		private class CacheInfo
		{
			private int hitCount;

			public DeserializerState Deserializer
			{
				get;
				set;
			}

			public Func<IDataReader, object>[] OtherDeserializers
			{
				get;
				set;
			}

			public Action<IDbCommand, object> ParamReader
			{
				get;
				set;
			}

			public int GetHitCount()
			{
				return Interlocked.CompareExchange(ref hitCount, 0, 0);
			}

			public void RecordHit()
			{
				Interlocked.Increment(ref hitCount);
			}
		}

		private class PropertyInfoByNameComparer : IComparer<PropertyInfo>
		{
			public int Compare(PropertyInfo x, PropertyInfo y)
			{
				return string.CompareOrdinal(x.Name, y.Name);
			}
		}

		[Flags]
		internal enum Row
		{
			First = 0x0,
			FirstOrDefault = 0x1,
			Single = 0x2,
			SingleOrDefault = 0x3
		}

		private sealed class DapperRow : IDynamicMetaObjectProvider, IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable, IReadOnlyDictionary<string, object>, IReadOnlyCollection<KeyValuePair<string, object>>
		{
			private sealed class DeadValue
			{
				public static readonly DeadValue Default = new DeadValue();

				private DeadValue()
				{
				}
			}

			private readonly DapperTable table;

			private object[] values;

			int ICollection<KeyValuePair<string, object>>.Count
			{
				get
				{
					int num = 0;
					for (int i = 0; i < values.Length; i++)
					{
						if (!(values[i] is DeadValue))
						{
							num++;
						}
					}
					return num;
				}
			}

			bool ICollection<KeyValuePair<string, object>>.IsReadOnly
			{
				get
				{
					return false;
				}
			}

			object IDictionary<string, object>.this[string key]
			{
				get
				{
					TryGetValue(key, out object value);
					return value;
				}
				set
				{
					SetValue(key, value, isAdd: false);
				}
			}

			ICollection<string> IDictionary<string, object>.Keys
			{
				get
				{
					return (from kv in this
					select kv.Key).ToArray();
				}
			}

			ICollection<object> IDictionary<string, object>.Values
			{
				get
				{
					return (from kv in this
					select kv.Value).ToArray();
				}
			}

			int IReadOnlyCollection<KeyValuePair<string, object>>.Count
			{
				get
				{
					return values.Count((object t) => !(t is DeadValue));
				}
			}

			object IReadOnlyDictionary<string, object>.this[string key]
			{
				get
				{
					TryGetValue(key, out object value);
					return value;
				}
			}

			IEnumerable<string> IReadOnlyDictionary<string, object>.Keys
			{
				get
				{
					return from kv in this
					select kv.Key;
				}
			}

			IEnumerable<object> IReadOnlyDictionary<string, object>.Values
			{
				get
				{
					return from kv in this
					select kv.Value;
				}
			}

			public DapperRow(DapperTable table, object[] values)
			{
				if (table == null)
				{
					throw new ArgumentNullException("table");
				}
				this.table = table;
				if (values == null)
				{
					throw new ArgumentNullException("values");
				}
				this.values = values;
			}

			public bool TryGetValue(string key, out object value)
			{
				int num = table.IndexOfName(key);
				if (num < 0)
				{
					value = null;
					return false;
				}
				value = ((num < values.Length) ? values[num] : null);
				if (value is DeadValue)
				{
					value = null;
					return false;
				}
				return true;
			}

			public override string ToString()
			{
				StringBuilder stringBuilder = GetStringBuilder().Append("{DapperRow");
				using (IEnumerator<KeyValuePair<string, object>> enumerator = GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<string, object> current = enumerator.Current;
						object value = current.Value;
						stringBuilder.Append(", ").Append(current.Key);
						if (value != null)
						{
							stringBuilder.Append(" = '").Append(current.Value).Append('\'');
						}
						else
						{
							stringBuilder.Append(" = NULL");
						}
					}
				}
				return stringBuilder.Append('}').__ToStringRecycle();
			}

			DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
			{
				return new DapperRowMetaObject(parameter, BindingRestrictions.Empty, this);
			}

			public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
			{
				string[] names = table.FieldNames;
				for (int i = 0; i < names.Length; i++)
				{
					object obj = (i < values.Length) ? values[i] : null;
					if (!(obj is DeadValue))
					{
						yield return new KeyValuePair<string, object>(names[i], obj);
					}
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
			{
				((IDictionary<string, object>)this).Add(item.Key, item.Value);
			}

			void ICollection<KeyValuePair<string, object>>.Clear()
			{
				for (int i = 0; i < values.Length; i++)
				{
					values[i] = DeadValue.Default;
				}
			}

			bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
			{
				if (TryGetValue(item.Key, out object value))
				{
					return object.Equals(value, item.Value);
				}
				return false;
			}

			void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
			{
				using (IEnumerator<KeyValuePair<string, object>> enumerator = GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<string, object> current = enumerator.Current;
						array[arrayIndex++] = current;
					}
				}
			}

			bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
			{
				return ((IDictionary<string, object>)this).Remove(item.Key);
			}

			bool IDictionary<string, object>.ContainsKey(string key)
			{
				int num = table.IndexOfName(key);
				if (num < 0 || num >= values.Length || values[num] is DeadValue)
				{
					return false;
				}
				return true;
			}

			void IDictionary<string, object>.Add(string key, object value)
			{
				SetValue(key, value, isAdd: true);
			}

			bool IDictionary<string, object>.Remove(string key)
			{
				int num = table.IndexOfName(key);
				if (num < 0 || num >= values.Length || values[num] is DeadValue)
				{
					return false;
				}
				values[num] = DeadValue.Default;
				return true;
			}

			public object SetValue(string key, object value)
			{
				return SetValue(key, value, isAdd: false);
			}

			private object SetValue(string key, object value, bool isAdd)
			{
				if (key == null)
				{
					throw new ArgumentNullException("key");
				}
				int num = table.IndexOfName(key);
				if (num < 0)
				{
					num = table.AddField(key);
				}
				else if (isAdd && num < values.Length && !(values[num] is DeadValue))
				{
					throw new ArgumentException("An item with the same key has already been added", "key");
				}
				int num2 = values.Length;
				if (num2 <= num)
				{
					Array.Resize(ref values, table.FieldCount);
					for (int i = num2; i < values.Length; i++)
					{
						values[i] = DeadValue.Default;
					}
				}
				return values[num] = value;
			}

			bool IReadOnlyDictionary<string, object>.ContainsKey(string key)
			{
				int num = table.IndexOfName(key);
				if (num >= 0 && num < values.Length)
				{
					return !(values[num] is DeadValue);
				}
				return false;
			}
		}

		private sealed class DapperRowMetaObject : DynamicMetaObject
		{
			private static readonly MethodInfo getValueMethod = typeof(IDictionary<string, object>).GetProperty("Item").GetGetMethod();

			private static readonly MethodInfo setValueMethod = typeof(DapperRow).GetMethod("SetValue", new Type[2]
			{
				typeof(string),
				typeof(object)
			});

			public DapperRowMetaObject(Expression expression, BindingRestrictions restrictions)
				: base(expression, restrictions)
			{
			}

			public DapperRowMetaObject(Expression expression, BindingRestrictions restrictions, object value)
				: base(expression, restrictions, value)
			{
			}

			private DynamicMetaObject CallMethod(MethodInfo method, Expression[] parameters)
			{
				return new DynamicMetaObject(Expression.Call(Expression.Convert(base.Expression, base.LimitType), method, parameters), BindingRestrictions.GetTypeRestriction(base.Expression, base.LimitType));
			}

			public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
			{
				Expression[] parameters = new Expression[1]
				{
					Expression.Constant(binder.Name)
				};
				return CallMethod(getValueMethod, parameters);
			}

			public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
			{
				Expression[] parameters = new Expression[1]
				{
					Expression.Constant(binder.Name)
				};
				return CallMethod(getValueMethod, parameters);
			}

			public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
			{
				Expression[] parameters = new Expression[2]
				{
					Expression.Constant(binder.Name),
					value.Expression
				};
				return CallMethod(setValueMethod, parameters);
			}
		}

		private sealed class DapperTable
		{
			private string[] fieldNames;

			private readonly Dictionary<string, int> fieldNameLookup;

			internal string[] FieldNames => fieldNames;

			public int FieldCount => fieldNames.Length;

			public DapperTable(string[] fieldNames)
			{
				if (fieldNames == null)
				{
					throw new ArgumentNullException("fieldNames");
				}
				this.fieldNames = fieldNames;
				fieldNameLookup = new Dictionary<string, int>(fieldNames.Length, StringComparer.Ordinal);
				for (int num = fieldNames.Length - 1; num >= 0; num--)
				{
					string text = fieldNames[num];
					if (text != null)
					{
						fieldNameLookup[text] = num;
					}
				}
			}

			internal int IndexOfName(string name)
			{
				if (name == null || !fieldNameLookup.TryGetValue(name, out int value))
				{
					return -1;
				}
				return value;
			}

			internal int AddField(string name)
			{
				if (name == null)
				{
					throw new ArgumentNullException("name");
				}
				if (fieldNameLookup.ContainsKey(name))
				{
					throw new InvalidOperationException("Field already exists: " + name);
				}
				int num = fieldNames.Length;
				Array.Resize(ref fieldNames, num + 1);
				fieldNames[num] = name;
				fieldNameLookup[name] = num;
				return num;
			}

			internal bool FieldExists(string key)
			{
				if (key != null)
				{
					return fieldNameLookup.ContainsKey(key);
				}
				return false;
			}
		}

		private struct DeserializerState
		{
			public readonly int Hash;

			public readonly Func<IDataReader, object> Func;

			public DeserializerState(int hash, Func<IDataReader, object> func)
			{
				Hash = hash;
				Func = func;
			}
		}

		private class DontMap
		{
		}

		public class GridReader : IDisposable
		{
			private readonly CancellationToken cancel;

			private IDataReader reader;

			private readonly Identity identity;

			private readonly bool addToCache;

			private int gridIndex;

			private int readCount;

			private readonly IParameterCallbacks callbacks;

			public bool IsConsumed
			{
				get;
				private set;
			}

			public IDbCommand Command
			{
				get;
				set;
			}

			internal GridReader(IDbCommand command, IDataReader reader, Identity identity, DynamicParameters dynamicParams, bool addToCache, CancellationToken cancel)
				: this(command, reader, identity, dynamicParams, addToCache)
			{
				this.cancel = cancel;
			}

			public Task<IEnumerable<dynamic>> ReadAsync(bool buffered = true)
			{
				return ReadAsyncImpl<object>(typeof(DapperRow), buffered);
			}

			public Task<dynamic> ReadFirstAsync()
			{
				return ReadRowAsyncImpl<object>(typeof(DapperRow), Row.First);
			}

			public Task<dynamic> ReadFirstOrDefaultAsync()
			{
				return ReadRowAsyncImpl<object>(typeof(DapperRow), Row.FirstOrDefault);
			}

			public Task<dynamic> ReadSingleAsync()
			{
				return ReadRowAsyncImpl<object>(typeof(DapperRow), Row.Single);
			}

			public Task<dynamic> ReadSingleOrDefaultAsync()
			{
				return ReadRowAsyncImpl<object>(typeof(DapperRow), Row.SingleOrDefault);
			}

			public Task<IEnumerable<object>> ReadAsync(Type type, bool buffered = true)
			{
				if (type == null)
				{
					throw new ArgumentNullException("type");
				}
				return ReadAsyncImpl<object>(type, buffered);
			}

			public Task<object> ReadFirstAsync(Type type)
			{
				if (type == null)
				{
					throw new ArgumentNullException("type");
				}
				return ReadRowAsyncImpl<object>(type, Row.First);
			}

			public Task<object> ReadFirstOrDefaultAsync(Type type)
			{
				if (type == null)
				{
					throw new ArgumentNullException("type");
				}
				return ReadRowAsyncImpl<object>(type, Row.FirstOrDefault);
			}

			public Task<object> ReadSingleAsync(Type type)
			{
				if (type == null)
				{
					throw new ArgumentNullException("type");
				}
				return ReadRowAsyncImpl<object>(type, Row.Single);
			}

			public Task<object> ReadSingleOrDefaultAsync(Type type)
			{
				if (type == null)
				{
					throw new ArgumentNullException("type");
				}
				return ReadRowAsyncImpl<object>(type, Row.SingleOrDefault);
			}

			public Task<IEnumerable<T>> ReadAsync<T>(bool buffered = true)
			{
				return ReadAsyncImpl<T>(typeof(T), buffered);
			}

			public Task<T> ReadFirstAsync<T>()
			{
				return ReadRowAsyncImpl<T>(typeof(T), Row.First);
			}

			public Task<T> ReadFirstOrDefaultAsync<T>()
			{
				return ReadRowAsyncImpl<T>(typeof(T), Row.FirstOrDefault);
			}

			public Task<T> ReadSingleAsync<T>()
			{
				return ReadRowAsyncImpl<T>(typeof(T), Row.Single);
			}

			public Task<T> ReadSingleOrDefaultAsync<T>()
			{
				return ReadRowAsyncImpl<T>(typeof(T), Row.SingleOrDefault);
			}

			private async Task NextResultAsync()
			{
				if (await((DbDataReader)reader).NextResultAsync(cancel).ConfigureAwait(continueOnCapturedContext: false))
				{
					readCount++;
					gridIndex++;
					IsConsumed = false;
				}
				else
				{
					reader.Dispose();
					reader = null;
					callbacks?.OnCompleted();
					Dispose();
				}
			}

			private Task<IEnumerable<T>> ReadAsyncImpl<T>(Type type, bool buffered)
			{
				if (reader == null)
				{
					throw new ObjectDisposedException(GetType().FullName, "The reader has been disposed; this can happen after all data has been consumed");
				}
				if (IsConsumed)
				{
					throw new InvalidOperationException("Query results must be consumed in the correct order, and each result can only be consumed once");
				}
				CacheInfo cacheInfo = GetCacheInfo(identity.ForGrid(type, gridIndex), null, addToCache);
				DeserializerState deserializerState = cacheInfo.Deserializer;
				int columnHash = GetColumnHash(reader);
				if (deserializerState.Func == null || deserializerState.Hash != columnHash)
				{
					deserializerState = (cacheInfo.Deserializer = new DeserializerState(columnHash, GetDeserializer(type, reader, 0, -1, returnNullIfFirstMissing: false)));
				}
				IsConsumed = true;
				if (buffered && reader is DbDataReader)
				{
					return ReadBufferedAsync<T>(gridIndex, deserializerState.Func);
				}
				IEnumerable<T> enumerable = ReadDeferred<T>(gridIndex, deserializerState.Func, type);
				if (buffered)
				{
					enumerable = enumerable.ToList();
				}
				return Task.FromResult(enumerable);
			}

			private Task<T> ReadRowAsyncImpl<T>(Type type, Row row)
			{
				DbDataReader dbDataReader;
				if ((dbDataReader = (reader as DbDataReader)) != null)
				{
					return ReadRowAsyncImplViaDbReader<T>(dbDataReader, type, row);
				}
				return Task.FromResult(ReadRow<T>(type, row));
			}

			private async Task<T> ReadRowAsyncImplViaDbReader<T>(DbDataReader reader, Type type, Row row)
			{
				if (reader == null)
				{
					throw new ObjectDisposedException(GetType().FullName, "The reader has been disposed; this can happen after all data has been consumed");
				}
				if (IsConsumed)
				{
					throw new InvalidOperationException("Query results must be consumed in the correct order, and each result can only be consumed once");
				}
				IsConsumed = true;
				T result = default(T);
				if (await reader.ReadAsync(cancel).ConfigureAwait(continueOnCapturedContext: false) && reader.FieldCount != 0)
				{
					CacheInfo cacheInfo = GetCacheInfo(identity.ForGrid(type, gridIndex), null, addToCache);
					DeserializerState deserializerState = cacheInfo.Deserializer;
					int columnHash = GetColumnHash(reader);
					if (deserializerState.Func == null || deserializerState.Hash != columnHash)
					{
						deserializerState = (cacheInfo.Deserializer = new DeserializerState(columnHash, GetDeserializer(type, reader, 0, -1, returnNullIfFirstMissing: false)));
					}
					result = (T)deserializerState.Func(reader);
					bool flag = (row & Row.Single) != Row.First;
					if (flag)
					{
						flag = await reader.ReadAsync(cancel).ConfigureAwait(continueOnCapturedContext: false);
					}
					if (flag)
					{
						ThrowMultipleRows(row);
					}
					while (await reader.ReadAsync(cancel).ConfigureAwait(continueOnCapturedContext: false))
					{
					}
				}
				else if ((row & Row.FirstOrDefault) == Row.First)
				{
					ThrowZeroRows(row);
				}
				await NextResultAsync().ConfigureAwait(continueOnCapturedContext: false);
				return result;
			}

			private async Task<IEnumerable<T>> ReadBufferedAsync<T>(int index, Func<IDataReader, object> deserializer)
			{
				IEnumerable<T> result;
				try
				{
					DbDataReader reader = (DbDataReader)this.reader;
					List<T> buffer = new List<T>();
					while (true)
					{
						bool flag = index == gridIndex;
						if (flag)
						{
							flag = await reader.ReadAsync(cancel).ConfigureAwait(continueOnCapturedContext: false);
						}
						if (!flag)
						{
							break;
						}
						buffer.Add((T)deserializer(reader));
					}
					result = buffer;
				}
				finally
				{
					if (index == gridIndex)
					{
						await NextResultAsync().ConfigureAwait(continueOnCapturedContext: false);
					}
				}
				return result;
			}

			internal GridReader(IDbCommand command, IDataReader reader, Identity identity, IParameterCallbacks callbacks, bool addToCache)
			{
				Command = command;
				this.reader = reader;
				this.identity = identity;
				this.callbacks = callbacks;
				this.addToCache = addToCache;
			}

			public IEnumerable<dynamic> Read(bool buffered = true)
			{
				return ReadImpl<object>(typeof(DapperRow), buffered);
			}

			public dynamic ReadFirst()
			{
				return ReadRow<object>(typeof(DapperRow), Row.First);
			}

			public dynamic ReadFirstOrDefault()
			{
				return ReadRow<object>(typeof(DapperRow), Row.FirstOrDefault);
			}

			public dynamic ReadSingle()
			{
				return ReadRow<object>(typeof(DapperRow), Row.Single);
			}

			public dynamic ReadSingleOrDefault()
			{
				return ReadRow<object>(typeof(DapperRow), Row.SingleOrDefault);
			}

			public IEnumerable<T> Read<T>(bool buffered = true)
			{
				return ReadImpl<T>(typeof(T), buffered);
			}

			public T ReadFirst<T>()
			{
				return ReadRow<T>(typeof(T), Row.First);
			}

			public T ReadFirstOrDefault<T>()
			{
				return ReadRow<T>(typeof(T), Row.FirstOrDefault);
			}

			public T ReadSingle<T>()
			{
				return ReadRow<T>(typeof(T), Row.Single);
			}

			public T ReadSingleOrDefault<T>()
			{
				return ReadRow<T>(typeof(T), Row.SingleOrDefault);
			}

			public IEnumerable<object> Read(Type type, bool buffered = true)
			{
				if (type == null)
				{
					throw new ArgumentNullException("type");
				}
				return ReadImpl<object>(type, buffered);
			}

			public object ReadFirst(Type type)
			{
				if (type == null)
				{
					throw new ArgumentNullException("type");
				}
				return ReadRow<object>(type, Row.First);
			}

			public object ReadFirstOrDefault(Type type)
			{
				if (type == null)
				{
					throw new ArgumentNullException("type");
				}
				return ReadRow<object>(type, Row.FirstOrDefault);
			}

			public object ReadSingle(Type type)
			{
				if (type == null)
				{
					throw new ArgumentNullException("type");
				}
				return ReadRow<object>(type, Row.Single);
			}

			public object ReadSingleOrDefault(Type type)
			{
				if (type == null)
				{
					throw new ArgumentNullException("type");
				}
				return ReadRow<object>(type, Row.SingleOrDefault);
			}

			private IEnumerable<T> ReadImpl<T>(Type type, bool buffered)
			{
				if (reader == null)
				{
					throw new ObjectDisposedException(GetType().FullName, "The reader has been disposed; this can happen after all data has been consumed");
				}
				if (IsConsumed)
				{
					throw new InvalidOperationException("Query results must be consumed in the correct order, and each result can only be consumed once");
				}
				CacheInfo cacheInfo = GetCacheInfo(identity.ForGrid(type, gridIndex), null, addToCache);
				DeserializerState deserializerState = cacheInfo.Deserializer;
				int columnHash = GetColumnHash(reader);
				if (deserializerState.Func == null || deserializerState.Hash != columnHash)
				{
					deserializerState = (cacheInfo.Deserializer = new DeserializerState(columnHash, GetDeserializer(type, reader, 0, -1, returnNullIfFirstMissing: false)));
				}
				IsConsumed = true;
				IEnumerable<T> enumerable = ReadDeferred<T>(gridIndex, deserializerState.Func, type);
				if (!buffered)
				{
					return enumerable;
				}
				return enumerable.ToList();
			}

			private T ReadRow<T>(Type type, Row row)
			{
				if (reader == null)
				{
					throw new ObjectDisposedException(GetType().FullName, "The reader has been disposed; this can happen after all data has been consumed");
				}
				if (IsConsumed)
				{
					throw new InvalidOperationException("Query results must be consumed in the correct order, and each result can only be consumed once");
				}
				IsConsumed = true;
				T result = default(T);
				if (reader.Read() && reader.FieldCount != 0)
				{
					CacheInfo cacheInfo = GetCacheInfo(identity.ForGrid(type, gridIndex), null, addToCache);
					DeserializerState deserializerState = cacheInfo.Deserializer;
					int columnHash = GetColumnHash(reader);
					if (deserializerState.Func == null || deserializerState.Hash != columnHash)
					{
						deserializerState = (cacheInfo.Deserializer = new DeserializerState(columnHash, GetDeserializer(type, reader, 0, -1, returnNullIfFirstMissing: false)));
					}
					object obj = deserializerState.Func(reader);
					if (obj == null || obj is T)
					{
						result = (T)obj;
					}
					else
					{
						Type conversionType = Nullable.GetUnderlyingType(type) ?? type;
						result = (T)Convert.ChangeType(obj, conversionType, CultureInfo.InvariantCulture);
					}
					if ((row & Row.Single) != 0 && reader.Read())
					{
						ThrowMultipleRows(row);
					}
					while (reader.Read())
					{
					}
				}
				else if ((row & Row.FirstOrDefault) == Row.First)
				{
					ThrowZeroRows(row);
				}
				NextResult();
				return result;
			}

			private IEnumerable<TReturn> MultiReadInternal<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(Delegate func, string splitOn)
			{
				Identity identity = this.identity.ForGrid(typeof(TReturn), new Type[7]
				{
					typeof(TFirst),
					typeof(TSecond),
					typeof(TThird),
					typeof(TFourth),
					typeof(TFifth),
					typeof(TSixth),
					typeof(TSeventh)
				}, gridIndex);
				IsConsumed = true;
				try
				{
					foreach (TReturn item in ((IDbConnection)null).MultiMapImpl<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(default(CommandDefinition), func, splitOn, reader, identity, finalize: false))
					{
						yield return item;
					}
				}
				finally
				{
					NextResult();
				}
			}

			private IEnumerable<TReturn> MultiReadInternal<TReturn>(Type[] types, Func<object[], TReturn> map, string splitOn)
			{
				Identity identity = this.identity.ForGrid(typeof(TReturn), types, gridIndex);
				try
				{
					foreach (TReturn item in ((IDbConnection)null).MultiMapImpl(default(CommandDefinition), types, map, splitOn, reader, identity, finalize: false))
					{
						yield return item;
					}
				}
				finally
				{
					NextResult();
				}
			}

			public IEnumerable<TReturn> Read<TFirst, TSecond, TReturn>(Func<TFirst, TSecond, TReturn> func, string splitOn = "id", bool buffered = true)
			{
				IEnumerable<TReturn> enumerable = MultiReadInternal<TFirst, TSecond, DontMap, DontMap, DontMap, DontMap, DontMap, TReturn>(func, splitOn);
				if (!buffered)
				{
					return enumerable;
				}
				return enumerable.ToList();
			}

			public IEnumerable<TReturn> Read<TFirst, TSecond, TThird, TReturn>(Func<TFirst, TSecond, TThird, TReturn> func, string splitOn = "id", bool buffered = true)
			{
				IEnumerable<TReturn> enumerable = MultiReadInternal<TFirst, TSecond, TThird, DontMap, DontMap, DontMap, DontMap, TReturn>(func, splitOn);
				if (!buffered)
				{
					return enumerable;
				}
				return enumerable.ToList();
			}

			public IEnumerable<TReturn> Read<TFirst, TSecond, TThird, TFourth, TReturn>(Func<TFirst, TSecond, TThird, TFourth, TReturn> func, string splitOn = "id", bool buffered = true)
			{
				IEnumerable<TReturn> enumerable = MultiReadInternal<TFirst, TSecond, TThird, TFourth, DontMap, DontMap, DontMap, TReturn>(func, splitOn);
				if (!buffered)
				{
					return enumerable;
				}
				return enumerable.ToList();
			}

			public IEnumerable<TReturn> Read<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> func, string splitOn = "id", bool buffered = true)
			{
				IEnumerable<TReturn> enumerable = MultiReadInternal<TFirst, TSecond, TThird, TFourth, TFifth, DontMap, DontMap, TReturn>(func, splitOn);
				if (!buffered)
				{
					return enumerable;
				}
				return enumerable.ToList();
			}

			public IEnumerable<TReturn> Read<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> func, string splitOn = "id", bool buffered = true)
			{
				IEnumerable<TReturn> enumerable = MultiReadInternal<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, DontMap, TReturn>(func, splitOn);
				if (!buffered)
				{
					return enumerable;
				}
				return enumerable.ToList();
			}

			public IEnumerable<TReturn> Read<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> func, string splitOn = "id", bool buffered = true)
			{
				IEnumerable<TReturn> enumerable = MultiReadInternal<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(func, splitOn);
				if (!buffered)
				{
					return enumerable;
				}
				return enumerable.ToList();
			}

			public IEnumerable<TReturn> Read<TReturn>(Type[] types, Func<object[], TReturn> map, string splitOn = "id", bool buffered = true)
			{
				IEnumerable<TReturn> enumerable = MultiReadInternal(types, map, splitOn);
				if (!buffered)
				{
					return enumerable;
				}
				return enumerable.ToList();
			}

			private IEnumerable<T> ReadDeferred<T>(int index, Func<IDataReader, object> deserializer, Type effectiveType)
			{
				try
				{
					Type convertToType = Nullable.GetUnderlyingType(effectiveType) ?? effectiveType;
					while (index == gridIndex && reader.Read())
					{
						object obj = deserializer(reader);
						if (obj == null || obj is T)
						{
							yield return (T)obj;
						}
						else
						{
							yield return (T)Convert.ChangeType(obj, convertToType, CultureInfo.InvariantCulture);
						}
					}
				}
				finally
				{
					if (index == gridIndex)
					{
						NextResult();
					}
				}
			}

			private void NextResult()
			{
				if (reader.NextResult())
				{
					readCount++;
					gridIndex++;
					IsConsumed = false;
				}
				else
				{
					reader.Dispose();
					reader = null;
					callbacks?.OnCompleted();
					Dispose();
				}
			}

			public void Dispose()
			{
				if (reader != null)
				{
					if (!reader.IsClosed)
					{
						Command?.Cancel();
					}
					reader.Dispose();
					reader = null;
				}
				if (Command != null)
				{
					Command.Dispose();
					Command = null;
				}
			}
		}

		public interface ICustomQueryParameter
		{
			void AddParameter(IDbCommand command, string name);
		}

		public class Identity : IEquatable<Identity>
		{
			public readonly string sql;

			public readonly CommandType? commandType;

			public readonly int hashCode;

			public readonly int gridIndex;

			public readonly Type type;

			public readonly string connectionString;

			public readonly Type parametersType;

			internal Identity ForGrid(Type primaryType, int gridIndex)
			{
				return new Identity(sql, commandType, connectionString, primaryType, parametersType, null, gridIndex);
			}

			internal Identity ForGrid(Type primaryType, Type[] otherTypes, int gridIndex)
			{
				return new Identity(sql, commandType, connectionString, primaryType, parametersType, otherTypes, gridIndex);
			}

			public Identity ForDynamicParameters(Type type)
			{
				return new Identity(sql, commandType, connectionString, this.type, type, null, -1);
			}

			internal Identity(string sql, CommandType? commandType, IDbConnection connection, Type type, Type parametersType, Type[] otherTypes)
				: this(sql, commandType, connection.ConnectionString, type, parametersType, otherTypes, 0)
			{
			}

			private Identity(string sql, CommandType? commandType, string connectionString, Type type, Type parametersType, Type[] otherTypes, int gridIndex)
			{
				this.sql = sql;
				this.commandType = commandType;
				this.connectionString = connectionString;
				this.type = type;
				this.parametersType = parametersType;
				this.gridIndex = gridIndex;
				hashCode = 17;
				hashCode = hashCode * 23 + commandType.GetHashCode();
				hashCode = hashCode * 23 + gridIndex.GetHashCode();
				hashCode = hashCode * 23 + (sql?.GetHashCode() ?? 0);
				hashCode = hashCode * 23 + (type?.GetHashCode() ?? 0);
				if (otherTypes != null)
				{
					foreach (Type type2 in otherTypes)
					{
						hashCode = hashCode * 23 + (type2?.GetHashCode() ?? 0);
					}
				}
				hashCode = hashCode * 23 + ((connectionString != null) ? connectionStringComparer.GetHashCode(connectionString) : 0);
				hashCode = hashCode * 23 + (parametersType?.GetHashCode() ?? 0);
			}

			public override bool Equals(object obj)
			{
				return Equals(obj as Identity);
			}

			public override int GetHashCode()
			{
				return hashCode;
			}

			public bool Equals(Identity other)
			{
				if (other != null && gridIndex == other.gridIndex && type == other.type && sql == other.sql && commandType == other.commandType && connectionStringComparer.Equals(connectionString, other.connectionString))
				{
					return parametersType == other.parametersType;
				}
				return false;
			}
		}

		public interface IDynamicParameters
		{
			void AddParameters(IDbCommand command, Identity identity);
		}

		public interface IMemberMap
		{
			string ColumnName
			{
				get;
			}

			Type MemberType
			{
				get;
			}

			PropertyInfo Property
			{
				get;
			}

			FieldInfo Field
			{
				get;
			}

			ParameterInfo Parameter
			{
				get;
			}
		}

		public interface IParameterCallbacks : IDynamicParameters
		{
			void OnCompleted();
		}

		public interface IParameterLookup : IDynamicParameters
		{
			object this[string name]
			{
				get;
			}
		}

		public interface ITypeHandler
		{
			void SetValue(IDbDataParameter parameter, object value);

			object Parse(Type destinationType, object value);
		}

		public interface ITypeMap
		{
			ConstructorInfo FindConstructor(string[] names, Type[] types);

			ConstructorInfo FindExplicitConstructor();

			IMemberMap GetConstructorParameter(ConstructorInfo constructor, string columnName);

			IMemberMap GetMember(string columnName);
		}

		internal class Link<TKey, TValue> where TKey : class
		{
			public TKey Key
			{
				get;
			}

			public TValue Value
			{
				get;
			}

			public Link<TKey, TValue> Tail
			{
				get;
			}

			public static bool TryGet(Link<TKey, TValue> link, TKey key, out TValue value)
			{
				while (link != null)
				{
					if (key == link.Key)
					{
						value = link.Value;
						return true;
					}
					link = link.Tail;
				}
				value = default(TValue);
				return false;
			}

			public static bool TryAdd(ref Link<TKey, TValue> head, TKey key, ref TValue value)
			{
				Link<TKey, TValue> link;
				Link<TKey, TValue> value3;
				do
				{
					link = Interlocked.CompareExchange(ref head, null, null);
					if (TryGet(link, key, out TValue value2))
					{
						value = value2;
						return false;
					}
					value3 = new Link<TKey, TValue>(key, value, link);
				}
				while (Interlocked.CompareExchange(ref head, value3, link) != link);
				return true;
			}

			private Link(TKey key, TValue value, Link<TKey, TValue> tail)
			{
				Key = key;
				Value = value;
				Tail = tail;
			}
		}

		internal struct LiteralToken
		{
			internal static readonly IList<LiteralToken> None = new LiteralToken[0];

			public string Token
			{
				get;
			}

			public string Member
			{
				get;
			}

			internal LiteralToken(string token, string member)
			{
				Token = token;
				Member = member;
			}
		}

		public static class Settings
		{
			private const CommandBehavior DefaultAllowedCommandBehaviors = ~CommandBehavior.SingleResult;

			internal static CommandBehavior AllowedCommandBehaviors
			{
				get;
				private set;
			}

			public static bool UseSingleResultOptimization
			{
				get
				{
					return (AllowedCommandBehaviors & CommandBehavior.SingleResult) != CommandBehavior.Default;
				}
				set
				{
					SetAllowedCommandBehaviors(CommandBehavior.SingleResult, value);
				}
			}

			public static bool UseSingleRowOptimization
			{
				get
				{
					return (AllowedCommandBehaviors & CommandBehavior.SingleRow) != CommandBehavior.Default;
				}
				set
				{
					SetAllowedCommandBehaviors(CommandBehavior.SingleRow, value);
				}
			}

			public static int? CommandTimeout
			{
				get;
				set;
			}

			public static bool ApplyNullValues
			{
				get;
				set;
			}

			public static bool PadListExpansions
			{
				get;
				set;
			}

			public static int InListStringSplitCount
			{
				get;
				set;
			}

			private static void SetAllowedCommandBehaviors(CommandBehavior behavior, bool enabled)
			{
				if (enabled)
				{
					AllowedCommandBehaviors |= behavior;
				}
				else
				{
					AllowedCommandBehaviors &= ~behavior;
				}
			}

			internal static bool DisableCommandBehaviorOptimizations(CommandBehavior behavior, Exception ex)
			{
				if (AllowedCommandBehaviors == ~CommandBehavior.SingleResult && (behavior & (CommandBehavior.SingleResult | CommandBehavior.SingleRow)) != 0 && (ex.Message.Contains("SingleResult") || ex.Message.Contains("SingleRow")))
				{
					SetAllowedCommandBehaviors(CommandBehavior.SingleResult | CommandBehavior.SingleRow, enabled: false);
					return true;
				}
				return false;
			}

			static Settings()
			{
				AllowedCommandBehaviors = ~CommandBehavior.SingleResult;
				InListStringSplitCount = -1;
				SetDefaults();
			}

			public static void SetDefaults()
			{
				CommandTimeout = null;
				ApplyNullValues = false;
			}
		}

		private class TypeDeserializerCache
		{
			private struct DeserializerKey : IEquatable<DeserializerKey>
			{
				private readonly int startBound;

				private readonly int length;

				private readonly bool returnNullIfFirstMissing;

				private readonly IDataReader reader;

				private readonly string[] names;

				private readonly Type[] types;

				private readonly int hashCode;

				public DeserializerKey(int hashCode, int startBound, int length, bool returnNullIfFirstMissing, IDataReader reader, bool copyDown)
				{
					this.hashCode = hashCode;
					this.startBound = startBound;
					this.length = length;
					this.returnNullIfFirstMissing = returnNullIfFirstMissing;
					if (copyDown)
					{
						this.reader = null;
						names = new string[length];
						types = new Type[length];
						int num = startBound;
						for (int i = 0; i < length; i++)
						{
							names[i] = reader.GetName(num);
							types[i] = reader.GetFieldType(num++);
						}
					}
					else
					{
						this.reader = reader;
						names = null;
						types = null;
					}
				}

				public override int GetHashCode()
				{
					return hashCode;
				}

				public override string ToString()
				{
					if (names != null)
					{
						return string.Join(", ", names);
					}
					if (reader != null)
					{
						StringBuilder stringBuilder = new StringBuilder();
						int num = startBound;
						for (int i = 0; i < length; i++)
						{
							if (i != 0)
							{
								stringBuilder.Append(", ");
							}
							stringBuilder.Append(reader.GetName(num++));
						}
						return stringBuilder.ToString();
					}
					return ((ValueType)this).ToString();
				}

				public override bool Equals(object obj)
				{
					if (obj is DeserializerKey)
					{
						return Equals((DeserializerKey)obj);
					}
					return false;
				}

				public bool Equals(DeserializerKey other)
				{
					if (hashCode != other.hashCode || startBound != other.startBound || length != other.length || returnNullIfFirstMissing != other.returnNullIfFirstMissing)
					{
						return false;
					}
					int num = 0;
					while (num < length)
					{
						string[] array = names;
						string a = ((array != null) ? array[num] : null) ?? reader?.GetName(startBound + num);
						string[] array2 = other.names;
						if (!(a != (((array2 != null) ? array2[num] : null) ?? other.reader?.GetName(startBound + num))))
						{
							Type[] array3 = types;
							Type left = ((array3 != null) ? array3[num] : null) ?? reader?.GetFieldType(startBound + num);
							Type[] array4 = other.types;
							if (!(left != (((array4 != null) ? array4[num] : null) ?? other.reader?.GetFieldType(startBound + num))))
							{
								num++;
								continue;
							}
						}
						return false;
					}
					return true;
				}
			}

			private static readonly Hashtable byType = new Hashtable();

			private readonly Type type;

			private readonly Dictionary<DeserializerKey, Func<IDataReader, object>> readers = new Dictionary<DeserializerKey, Func<IDataReader, object>>();

			private TypeDeserializerCache(Type type)
			{
				this.type = type;
			}

			internal static void Purge(Type type)
			{
				lock (byType)
				{
					byType.Remove(type);
				}
			}

			internal static void Purge()
			{
				lock (byType)
				{
					byType.Clear();
				}
			}

			internal static Func<IDataReader, object> GetReader(Type type, IDataReader reader, int startBound, int length, bool returnNullIfFirstMissing)
			{
				TypeDeserializerCache typeDeserializerCache = (TypeDeserializerCache)byType[type];
				if (typeDeserializerCache == null)
				{
					lock (byType)
					{
						typeDeserializerCache = (TypeDeserializerCache)byType[type];
						if (typeDeserializerCache == null)
						{
							typeDeserializerCache = (TypeDeserializerCache)(byType[type] = new TypeDeserializerCache(type));
						}
					}
				}
				return typeDeserializerCache.GetReader(reader, startBound, length, returnNullIfFirstMissing);
			}

			private Func<IDataReader, object> GetReader(IDataReader reader, int startBound, int length, bool returnNullIfFirstMissing)
			{
				if (length < 0)
				{
					length = reader.FieldCount - startBound;
				}
				int num = GetColumnHash(reader, startBound, length);
				if (returnNullIfFirstMissing)
				{
					num *= -27;
				}
				DeserializerKey key = new DeserializerKey(num, startBound, length, returnNullIfFirstMissing, reader, copyDown: false);
				Func<IDataReader, object> value;
				lock (readers)
				{
					if (readers.TryGetValue(key, out value))
					{
						return value;
					}
				}
				value = GetTypeDeserializerImpl(type, reader, startBound, length, returnNullIfFirstMissing);
				key = new DeserializerKey(num, startBound, length, returnNullIfFirstMissing, reader, copyDown: true);
				lock (readers)
				{
					return readers[key] = value;
				}
			}
		}

		public abstract class TypeHandler<T> : ITypeHandler
		{
			public abstract void SetValue(IDbDataParameter parameter, T value);

			public abstract T Parse(object value);

			void ITypeHandler.SetValue(IDbDataParameter parameter, object value)
			{
				if (value is DBNull)
				{
					parameter.Value = value;
				}
				else
				{
					SetValue(parameter, (T)value);
				}
			}

			object ITypeHandler.Parse(Type destinationType, object value)
			{
				return Parse(value);
			}
		}

		public abstract class StringTypeHandler<T> : TypeHandler<T>
		{
			protected abstract T Parse(string xml);

			protected abstract string Format(T xml);

			public override void SetValue(IDbDataParameter parameter, T value)
			{
				parameter.Value = ((value == null) ? ((IConvertible)DBNull.Value) : ((IConvertible)Format(value)));
			}

			public override T Parse(object value)
			{
				if (value == null || value is DBNull)
				{
					return default(T);
				}
				return Parse((string)value);
			}
		}

		[Obsolete("This method is for internal use only", false)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static class TypeHandlerCache<T>
		{
			private static ITypeHandler handler;

			[Obsolete("This method is for internal use only", true)]
			public static T Parse(object value)
			{
				return (T)handler.Parse(typeof(T), value);
			}

			[Obsolete("This method is for internal use only", true)]
			public static void SetValue(IDbDataParameter parameter, object value)
			{
				handler.SetValue(parameter, value);
			}

			internal static void SetHandler(ITypeHandler handler)
			{
				TypeHandlerCache<T>.handler = handler;
			}
		}

		public class UdtTypeHandler : ITypeHandler
		{
			private readonly string udtTypeName;

			public UdtTypeHandler(string udtTypeName)
			{
				if (string.IsNullOrEmpty(udtTypeName))
				{
					throw new ArgumentException("Cannot be null or empty", udtTypeName);
				}
				this.udtTypeName = udtTypeName;
			}

			object ITypeHandler.Parse(Type destinationType, object value)
			{
				if (!(value is DBNull))
				{
					return value;
				}
				return null;
			}

			void ITypeHandler.SetValue(IDbDataParameter parameter, object value)
			{
				parameter.Value = SanitizeParameterValue(value);
				if (parameter is SqlParameter && !(value is DBNull))
				{
					((SqlParameter)parameter).SqlDbType = SqlDbType.Udt;
					((SqlParameter)parameter).UdtTypeName = udtTypeName;
				}
			}
		}

		private static readonly ConcurrentDictionary<Identity, CacheInfo> _queryCache;

		private const int COLLECT_PER_ITEMS = 1000;

		private const int COLLECT_HIT_COUNT_MIN = 0;

		private static int collect;

		private static Dictionary<Type, DbType> typeMap;

		private static Dictionary<Type, ITypeHandler> typeHandlers;

		internal const string LinqBinary = "System.Data.Linq.Binary";

		private const string ObsoleteInternalUsageOnly = "This method is for internal use only";

		private static readonly int[] ErrTwoRows;

		private static readonly int[] ErrZeroRows;

		private static readonly Regex smellsLikeOleDb;

		private static readonly Regex literalTokens;

		private static readonly Regex pseudoPositional;

		internal static readonly MethodInfo format;

		private static readonly Dictionary<TypeCode, MethodInfo> toStrings;

		private static readonly MethodInfo StringReplace;

		private static readonly MethodInfo InvariantCulture;

		private static readonly MethodInfo enumParse;

		private static readonly MethodInfo getItem;

		public static Func<Type, ITypeMap> TypeMapProvider;

		private static readonly Hashtable _typeMaps;

		private static IEqualityComparer<string> connectionStringComparer;

		private const string DataTableTypeNameKey = "dapper:TypeName";

		[ThreadStatic]
		private static StringBuilder perThreadStringBuilderCache;

		public static IEqualityComparer<string> ConnectionStringComparer
		{
			get
			{
				return connectionStringComparer;
			}
			set
			{
				connectionStringComparer = (value ?? StringComparer.Ordinal);
			}
		}

		public static event EventHandler QueryCachePurged;

		public static Task<IEnumerable<dynamic>> QueryAsync(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.QueryAsync<object>(typeof(DapperRow), new CommandDefinition(sql, param, transaction, commandTimeout, commandType));
		}

		public static Task<IEnumerable<dynamic>> QueryAsync(this IDbConnection cnn, CommandDefinition command)
		{
			return cnn.QueryAsync<object>(typeof(DapperRow), command);
		}

		public static Task<dynamic> QueryFirstAsync(this IDbConnection cnn, CommandDefinition command)
		{
			return cnn.QueryRowAsync<object>(Row.First, typeof(DapperRow), command);
		}

		public static Task<dynamic> QueryFirstOrDefaultAsync(this IDbConnection cnn, CommandDefinition command)
		{
			return cnn.QueryRowAsync<object>(Row.FirstOrDefault, typeof(DapperRow), command);
		}

		public static Task<dynamic> QuerySingleAsync(this IDbConnection cnn, CommandDefinition command)
		{
			return cnn.QueryRowAsync<object>(Row.Single, typeof(DapperRow), command);
		}

		public static Task<dynamic> QuerySingleOrDefaultAsync(this IDbConnection cnn, CommandDefinition command)
		{
			return cnn.QueryRowAsync<object>(Row.SingleOrDefault, typeof(DapperRow), command);
		}

		public static Task<IEnumerable<T>> QueryAsync<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.QueryAsync<T>(typeof(T), new CommandDefinition(sql, param, transaction, commandTimeout, commandType));
		}

		public static Task<T> QueryFirstAsync<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.QueryRowAsync<T>(Row.First, typeof(T), new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None));
		}

		public static Task<T> QueryFirstOrDefaultAsync<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.QueryRowAsync<T>(Row.FirstOrDefault, typeof(T), new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None));
		}

		public static Task<T> QuerySingleAsync<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.QueryRowAsync<T>(Row.Single, typeof(T), new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None));
		}

		public static Task<T> QuerySingleOrDefaultAsync<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.QueryRowAsync<T>(Row.SingleOrDefault, typeof(T), new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None));
		}

		public static Task<dynamic> QueryFirstAsync(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.QueryRowAsync<object>(Row.First, typeof(DapperRow), new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None));
		}

		public static Task<dynamic> QueryFirstOrDefaultAsync(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.QueryRowAsync<object>(Row.FirstOrDefault, typeof(DapperRow), new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None));
		}

		public static Task<dynamic> QuerySingleAsync(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.QueryRowAsync<object>(Row.Single, typeof(DapperRow), new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None));
		}

		public static Task<dynamic> QuerySingleOrDefaultAsync(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.QueryRowAsync<object>(Row.SingleOrDefault, typeof(DapperRow), new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None));
		}

		public static Task<IEnumerable<object>> QueryAsync(this IDbConnection cnn, Type type, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			return cnn.QueryAsync<object>(type, new CommandDefinition(sql, param, transaction, commandTimeout, commandType));
		}

		public static Task<object> QueryFirstAsync(this IDbConnection cnn, Type type, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			return cnn.QueryRowAsync<object>(Row.First, type, new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None));
		}

		public static Task<object> QueryFirstOrDefaultAsync(this IDbConnection cnn, Type type, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			return cnn.QueryRowAsync<object>(Row.FirstOrDefault, type, new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None));
		}

		public static Task<object> QuerySingleAsync(this IDbConnection cnn, Type type, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			return cnn.QueryRowAsync<object>(Row.Single, type, new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None));
		}

		public static Task<object> QuerySingleOrDefaultAsync(this IDbConnection cnn, Type type, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			return cnn.QueryRowAsync<object>(Row.SingleOrDefault, type, new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None));
		}

		public static Task<IEnumerable<T>> QueryAsync<T>(this IDbConnection cnn, CommandDefinition command)
		{
			return cnn.QueryAsync<T>(typeof(T), command);
		}

		public static Task<IEnumerable<object>> QueryAsync(this IDbConnection cnn, Type type, CommandDefinition command)
		{
			return cnn.QueryAsync<object>(type, command);
		}

		public static Task<object> QueryFirstAsync(this IDbConnection cnn, Type type, CommandDefinition command)
		{
			return cnn.QueryRowAsync<object>(Row.First, type, command);
		}

		public static Task<T> QueryFirstAsync<T>(this IDbConnection cnn, CommandDefinition command)
		{
			return cnn.QueryRowAsync<T>(Row.First, typeof(T), command);
		}

		public static Task<object> QueryFirstOrDefaultAsync(this IDbConnection cnn, Type type, CommandDefinition command)
		{
			return cnn.QueryRowAsync<object>(Row.FirstOrDefault, type, command);
		}

		public static Task<T> QueryFirstOrDefaultAsync<T>(this IDbConnection cnn, CommandDefinition command)
		{
			return cnn.QueryRowAsync<T>(Row.FirstOrDefault, typeof(T), command);
		}

		public static Task<object> QuerySingleAsync(this IDbConnection cnn, Type type, CommandDefinition command)
		{
			return cnn.QueryRowAsync<object>(Row.Single, type, command);
		}

		public static Task<T> QuerySingleAsync<T>(this IDbConnection cnn, CommandDefinition command)
		{
			return cnn.QueryRowAsync<T>(Row.Single, typeof(T), command);
		}

		public static Task<object> QuerySingleOrDefaultAsync(this IDbConnection cnn, Type type, CommandDefinition command)
		{
			return cnn.QueryRowAsync<object>(Row.SingleOrDefault, type, command);
		}

		public static Task<T> QuerySingleOrDefaultAsync<T>(this IDbConnection cnn, CommandDefinition command)
		{
			return cnn.QueryRowAsync<T>(Row.SingleOrDefault, typeof(T), command);
		}

		private static Task<DbDataReader> ExecuteReaderWithFlagsFallbackAsync(DbCommand cmd, bool wasClosed, CommandBehavior behavior, CancellationToken cancellationToken)
		{
			Task<DbDataReader> task = cmd.ExecuteReaderAsync(GetBehavior(wasClosed, behavior), cancellationToken);
			if (task.Status == TaskStatus.Faulted && Settings.DisableCommandBehaviorOptimizations(behavior, task.Exception.InnerException))
			{
				return cmd.ExecuteReaderAsync(GetBehavior(wasClosed, behavior), cancellationToken);
			}
			return task;
		}

		private static Task TryOpenAsync(this IDbConnection cnn, CancellationToken cancel)
		{
			DbConnection dbConnection;
			if ((dbConnection = (cnn as DbConnection)) != null)
			{
				return dbConnection.OpenAsync(cancel);
			}
			throw new InvalidOperationException("Async operations require use of a DbConnection or an already-open IDbConnection");
		}

		private static DbCommand TrySetupAsyncCommand(this CommandDefinition command, IDbConnection cnn, Action<IDbCommand, object> paramReader)
		{
			DbCommand result;
			if ((result = (command.SetupCommand(cnn, paramReader) as DbCommand)) != null)
			{
				return result;
			}
			throw new InvalidOperationException("Async operations require use of a DbConnection or an IDbConnection where .CreateCommand() returns a DbCommand");
		}

		private static async Task<IEnumerable<T>> QueryAsync<T>(this IDbConnection cnn, Type effectiveType, CommandDefinition command)
		{
			object parameters = command.Parameters;
			Identity identity = new Identity(command.CommandText, command.CommandType, cnn, effectiveType, parameters?.GetType(), null);
			CacheInfo info = GetCacheInfo(identity, parameters, command.AddToCache);
			bool wasClosed = cnn.State == ConnectionState.Closed;
			CancellationToken cancel = command.CancellationToken;
			using (DbCommand cmd = command.TrySetupAsyncCommand(cnn, info.ParamReader))
			{
				DbDataReader reader = null;
				try
				{
					if (wasClosed)
					{
						await cnn.TryOpenAsync(cancel).ConfigureAwait(continueOnCapturedContext: false);
					}
					reader = await ExecuteReaderWithFlagsFallbackAsync(cmd, wasClosed, CommandBehavior.SingleResult | CommandBehavior.SequentialAccess, cancel).ConfigureAwait(continueOnCapturedContext: false);
					DeserializerState deserializerState = info.Deserializer;
					int columnHash = GetColumnHash(reader);
					if (deserializerState.Func == null || deserializerState.Hash != columnHash)
					{
						if (reader.FieldCount == 0)
						{
							return Enumerable.Empty<T>();
						}
						deserializerState = (info.Deserializer = new DeserializerState(columnHash, GetDeserializer(effectiveType, reader, 0, -1, returnNullIfFirstMissing: false)));
						if (command.AddToCache)
						{
							SetQueryCache(identity, info);
						}
					}
					Func<IDataReader, object> func = deserializerState.Func;
					if (command.Buffered)
					{
						List<T> buffer = new List<T>();
						Type convertToType = Nullable.GetUnderlyingType(effectiveType) ?? effectiveType;
						while (await reader.ReadAsync(cancel).ConfigureAwait(continueOnCapturedContext: false))
						{
							object obj = func(reader);
							if (obj == null || obj is T)
							{
								buffer.Add((T)obj);
							}
							else
							{
								buffer.Add((T)Convert.ChangeType(obj, convertToType, CultureInfo.InvariantCulture));
							}
						}
						while (await reader.NextResultAsync(cancel).ConfigureAwait(continueOnCapturedContext: false))
						{
						}
						command.OnCompleted();
						return buffer;
					}
					wasClosed = false;
					IEnumerable<T> result = ExecuteReaderSync<T>(reader, func, command.Parameters);
					reader = null;
					return result;
				}
				finally
				{
					using (reader)
					{
					}
					if (wasClosed)
					{
						cnn.Close();
					}
				}
			}
		}

		private static async Task<T> QueryRowAsync<T>(this IDbConnection cnn, Row row, Type effectiveType, CommandDefinition command)
		{
			object parameters = command.Parameters;
			Identity identity = new Identity(command.CommandText, command.CommandType, cnn, effectiveType, parameters?.GetType(), null);
			CacheInfo info = GetCacheInfo(identity, parameters, command.AddToCache);
			bool wasClosed = cnn.State == ConnectionState.Closed;
			CancellationToken cancel = command.CancellationToken;
			using (DbCommand cmd = command.TrySetupAsyncCommand(cnn, info.ParamReader))
			{
				DbDataReader reader = null;
				try
				{
					if (wasClosed)
					{
						await cnn.TryOpenAsync(cancel).ConfigureAwait(continueOnCapturedContext: false);
					}
					reader = await ExecuteReaderWithFlagsFallbackAsync(cmd, wasClosed, ((row & Row.Single) != 0) ? (CommandBehavior.SingleResult | CommandBehavior.SequentialAccess) : (CommandBehavior.SingleResult | CommandBehavior.SingleRow | CommandBehavior.SequentialAccess), cancel).ConfigureAwait(continueOnCapturedContext: false);
					T result = default(T);
					if (await reader.ReadAsync(cancel).ConfigureAwait(continueOnCapturedContext: false) && reader.FieldCount != 0)
					{
						DeserializerState deserializerState = info.Deserializer;
						int columnHash = GetColumnHash(reader);
						if (deserializerState.Func == null || deserializerState.Hash != columnHash)
						{
							deserializerState = (info.Deserializer = new DeserializerState(columnHash, GetDeserializer(effectiveType, reader, 0, -1, returnNullIfFirstMissing: false)));
							if (command.AddToCache)
							{
								SetQueryCache(identity, info);
							}
						}
						object obj = deserializerState.Func(reader);
						result = ((obj != null && !(obj is T)) ? ((T)Convert.ChangeType(obj, Nullable.GetUnderlyingType(effectiveType) ?? effectiveType, CultureInfo.InvariantCulture)) : ((T)obj));
						bool flag = (row & Row.Single) != Row.First;
						if (flag)
						{
							flag = await reader.ReadAsync(cancel).ConfigureAwait(continueOnCapturedContext: false);
						}
						if (flag)
						{
							ThrowMultipleRows(row);
						}
						while (await reader.ReadAsync(cancel).ConfigureAwait(continueOnCapturedContext: false))
						{
						}
					}
					else if ((row & Row.FirstOrDefault) == Row.First)
					{
						ThrowZeroRows(row);
					}
					while (await reader.NextResultAsync(cancel).ConfigureAwait(continueOnCapturedContext: false))
					{
					}
					return result;
				}
				finally
				{
					using (reader)
					{
					}
					if (wasClosed)
					{
						cnn.Close();
					}
				}
			}
		}

		public static Task<int> ExecuteAsync(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.ExecuteAsync(new CommandDefinition(sql, param, transaction, commandTimeout, commandType));
		}

		public static Task<int> ExecuteAsync(this IDbConnection cnn, CommandDefinition command)
		{
			object parameters = command.Parameters;
			IEnumerable multiExec = GetMultiExec(parameters);
			if (multiExec != null)
			{
				return ExecuteMultiImplAsync(cnn, command, multiExec);
			}
			return ExecuteImplAsync(cnn, command, parameters);
		}

		private static async Task<int> ExecuteMultiImplAsync(IDbConnection cnn, CommandDefinition command, IEnumerable multiExec)
		{
			bool isFirst = true;
			int total = 0;
			bool wasClosed = cnn.State == ConnectionState.Closed;
			try
			{
				if (wasClosed)
				{
					await cnn.TryOpenAsync(command.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				CacheInfo info = null;
				string masterSql = null;
				if ((command.Flags & CommandFlags.Pipelined) != 0)
				{
					Queue<AsyncExecState> pending = new Queue<AsyncExecState>(100);
					DbCommand cmd2 = null;
					try
					{
						foreach (object item in multiExec)
						{
							if (isFirst)
							{
								isFirst = false;
								cmd2 = command.TrySetupAsyncCommand(cnn, null);
								masterSql = cmd2.CommandText;
								info = GetCacheInfo(new Identity(command.CommandText, cmd2.CommandType, cnn, null, item.GetType(), null), item, command.AddToCache);
							}
							else if (pending.Count >= 100)
							{
								AsyncExecState recycled = pending.Dequeue();
								int num = total;
								total = num + await recycled.Task.ConfigureAwait(continueOnCapturedContext: false);
								cmd2 = recycled.Command;
								cmd2.CommandText = masterSql;
								cmd2.Parameters.Clear();
							}
							else
							{
								cmd2 = command.TrySetupAsyncCommand(cnn, null);
							}
							info.ParamReader(cmd2, item);
							pending.Enqueue(new AsyncExecState(cmd2, cmd2.ExecuteNonQueryAsync(command.CancellationToken)));
							cmd2 = null;
						}
						while (pending.Count != 0)
						{
							AsyncExecState asyncExecState = pending.Dequeue();
							using (asyncExecState.Command)
							{
							}
							int num = total;
							total = num + await asyncExecState.Task.ConfigureAwait(continueOnCapturedContext: false);
						}
					}
					finally
					{
						using (cmd2)
						{
						}
						while (pending.Count != 0)
						{
							using (pending.Dequeue().Command)
							{
							}
						}
					}
				}
				else
				{
					using (DbCommand cmd = command.TrySetupAsyncCommand(cnn, null))
					{
						foreach (object item2 in multiExec)
						{
							if (isFirst)
							{
								masterSql = cmd.CommandText;
								isFirst = false;
								info = GetCacheInfo(new Identity(command.CommandText, cmd.CommandType, cnn, null, item2.GetType(), null), item2, command.AddToCache);
							}
							else
							{
								cmd.CommandText = masterSql;
								cmd.Parameters.Clear();
							}
							info.ParamReader(cmd, item2);
							int num = total;
							total = num + await cmd.ExecuteNonQueryAsync(command.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						}
					}
				}
				command.OnCompleted();
				return total;
			}
			finally
			{
				if (wasClosed)
				{
					cnn.Close();
				}
			}
		}

		private static async Task<int> ExecuteImplAsync(IDbConnection cnn, CommandDefinition command, object param)
		{
			CacheInfo cacheInfo = GetCacheInfo(new Identity(command.CommandText, command.CommandType, cnn, null, param?.GetType(), null), param, command.AddToCache);
			bool wasClosed = cnn.State == ConnectionState.Closed;
			using (DbCommand cmd = command.TrySetupAsyncCommand(cnn, cacheInfo.ParamReader))
			{
				try
				{
					if (wasClosed)
					{
						await cnn.TryOpenAsync(command.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					}
					int result = await cmd.ExecuteNonQueryAsync(command.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					command.OnCompleted();
					return result;
				}
				finally
				{
					if (wasClosed)
					{
						cnn.Close();
					}
				}
			}
		}

		public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(this IDbConnection cnn, string sql, Func<TFirst, TSecond, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.MultiMapAsync<TFirst, TSecond, DontMap, DontMap, DontMap, DontMap, DontMap, TReturn>(new CommandDefinition(sql, param, transaction, commandTimeout, commandType, buffered ? CommandFlags.Buffered : CommandFlags.None), map, splitOn);
		}

		public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(this IDbConnection cnn, CommandDefinition command, Func<TFirst, TSecond, TReturn> map, string splitOn = "Id")
		{
			return cnn.MultiMapAsync<TFirst, TSecond, DontMap, DontMap, DontMap, DontMap, DontMap, TReturn>(command, map, splitOn);
		}

		public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.MultiMapAsync<TFirst, TSecond, TThird, DontMap, DontMap, DontMap, DontMap, TReturn>(new CommandDefinition(sql, param, transaction, commandTimeout, commandType, buffered ? CommandFlags.Buffered : CommandFlags.None), map, splitOn);
		}

		public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(this IDbConnection cnn, CommandDefinition command, Func<TFirst, TSecond, TThird, TReturn> map, string splitOn = "Id")
		{
			return cnn.MultiMapAsync<TFirst, TSecond, TThird, DontMap, DontMap, DontMap, DontMap, TReturn>(command, map, splitOn);
		}

		public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.MultiMapAsync<TFirst, TSecond, TThird, TFourth, DontMap, DontMap, DontMap, TReturn>(new CommandDefinition(sql, param, transaction, commandTimeout, commandType, buffered ? CommandFlags.Buffered : CommandFlags.None), map, splitOn);
		}

		public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(this IDbConnection cnn, CommandDefinition command, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, string splitOn = "Id")
		{
			return cnn.MultiMapAsync<TFirst, TSecond, TThird, TFourth, DontMap, DontMap, DontMap, TReturn>(command, map, splitOn);
		}

		public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.MultiMapAsync<TFirst, TSecond, TThird, TFourth, TFifth, DontMap, DontMap, TReturn>(new CommandDefinition(sql, param, transaction, commandTimeout, commandType, buffered ? CommandFlags.Buffered : CommandFlags.None), map, splitOn);
		}

		public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(this IDbConnection cnn, CommandDefinition command, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, string splitOn = "Id")
		{
			return cnn.MultiMapAsync<TFirst, TSecond, TThird, TFourth, TFifth, DontMap, DontMap, TReturn>(command, map, splitOn);
		}

		public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.MultiMapAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, DontMap, TReturn>(new CommandDefinition(sql, param, transaction, commandTimeout, commandType, buffered ? CommandFlags.Buffered : CommandFlags.None), map, splitOn);
		}

		public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(this IDbConnection cnn, CommandDefinition command, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, string splitOn = "Id")
		{
			return cnn.MultiMapAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, DontMap, TReturn>(command, map, splitOn);
		}

		public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.MultiMapAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(new CommandDefinition(sql, param, transaction, commandTimeout, commandType, buffered ? CommandFlags.Buffered : CommandFlags.None), map, splitOn);
		}

		public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(this IDbConnection cnn, CommandDefinition command, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, string splitOn = "Id")
		{
			return cnn.MultiMapAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(command, map, splitOn);
		}

		private static async Task<IEnumerable<TReturn>> MultiMapAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(this IDbConnection cnn, CommandDefinition command, Delegate map, string splitOn)
		{
			object parameters = command.Parameters;
			Identity identity = new Identity(command.CommandText, command.CommandType, cnn, typeof(TFirst), parameters?.GetType(), new Type[7]
			{
				typeof(TFirst),
				typeof(TSecond),
				typeof(TThird),
				typeof(TFourth),
				typeof(TFifth),
				typeof(TSixth),
				typeof(TSeventh)
			});
			CacheInfo info = GetCacheInfo(identity, parameters, command.AddToCache);
			bool wasClosed = cnn.State == ConnectionState.Closed;
			try
			{
				if (wasClosed)
				{
					await cnn.TryOpenAsync(command.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				using (DbCommand cmd = command.TrySetupAsyncCommand(cnn, info.ParamReader))
				{
					using (DbDataReader reader = await ExecuteReaderWithFlagsFallbackAsync(cmd, wasClosed, CommandBehavior.SingleResult | CommandBehavior.SequentialAccess, command.CancellationToken).ConfigureAwait(continueOnCapturedContext: false))
					{
						if (!command.Buffered)
						{
							wasClosed = false;
						}
						IEnumerable<TReturn> enumerable = ((IDbConnection)null).MultiMapImpl<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(CommandDefinition.ForCallback(command.Parameters), map, splitOn, reader, identity, finalize: true);
						return command.Buffered ? enumerable.ToList() : enumerable;
					}
				}
			}
			finally
			{
				if (wasClosed)
				{
					cnn.Close();
				}
			}
		}

		public static Task<IEnumerable<TReturn>> QueryAsync<TReturn>(this IDbConnection cnn, string sql, Type[] types, Func<object[], TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			CommandDefinition command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, buffered ? CommandFlags.Buffered : CommandFlags.None);
			return cnn.MultiMapAsync(command, types, map, splitOn);
		}

		private static async Task<IEnumerable<TReturn>> MultiMapAsync<TReturn>(this IDbConnection cnn, CommandDefinition command, Type[] types, Func<object[], TReturn> map, string splitOn)
		{
			if (types.Length >= 1)
			{
				object parameters = command.Parameters;
				Identity identity = new Identity(command.CommandText, command.CommandType, cnn, types[0], parameters?.GetType(), types);
				CacheInfo info = GetCacheInfo(identity, parameters, command.AddToCache);
				bool wasClosed = cnn.State == ConnectionState.Closed;
				try
				{
					if (wasClosed)
					{
						await cnn.TryOpenAsync(command.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					}
					using (DbCommand cmd = command.TrySetupAsyncCommand(cnn, info.ParamReader))
					{
						using (DbDataReader reader = await ExecuteReaderWithFlagsFallbackAsync(cmd, wasClosed, CommandBehavior.SingleResult | CommandBehavior.SequentialAccess, command.CancellationToken).ConfigureAwait(continueOnCapturedContext: false))
						{
							IEnumerable<TReturn> enumerable = ((IDbConnection)null).MultiMapImpl(default(CommandDefinition), types, map, splitOn, reader, identity, finalize: true);
							return command.Buffered ? enumerable.ToList() : enumerable;
						}
					}
				}
				finally
				{
					if (wasClosed)
					{
						cnn.Close();
					}
				}
			}
			throw new ArgumentException("you must provide at least one type to deserialize");
		}

		private static IEnumerable<T> ExecuteReaderSync<T>(IDataReader reader, Func<IDataReader, object> func, object parameters)
		{
			using (reader)
			{
				while (reader.Read())
				{
					yield return (T)func(reader);
				}
				while (reader.NextResult())
				{
				}
				(parameters as IParameterCallbacks)?.OnCompleted();
			}
		}

		public static Task<GridReader> QueryMultipleAsync(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.QueryMultipleAsync(new CommandDefinition(sql, param, transaction, commandTimeout, commandType));
		}

		public static async Task<GridReader> QueryMultipleAsync(this IDbConnection cnn, CommandDefinition command)
		{
			object parameters = command.Parameters;
			Identity identity = new Identity(command.CommandText, command.CommandType, cnn, typeof(GridReader), parameters?.GetType(), null);
			CacheInfo info = GetCacheInfo(identity, parameters, command.AddToCache);
			DbCommand cmd = null;
			IDataReader reader = null;
			bool wasClosed = cnn.State == ConnectionState.Closed;
			try
			{
				if (wasClosed)
				{
					await cnn.TryOpenAsync(command.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				cmd = command.TrySetupAsyncCommand(cnn, info.ParamReader);
				reader = await ExecuteReaderWithFlagsFallbackAsync(cmd, wasClosed, CommandBehavior.SequentialAccess, command.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				GridReader result = new GridReader(cmd, reader, identity, command.Parameters as DynamicParameters, command.AddToCache, command.CancellationToken);
				wasClosed = false;
				return result;
			}
			catch
			{
				if (reader != null)
				{
					if (!reader.IsClosed)
					{
						try
						{
							cmd.Cancel();
						}
						catch
						{
						}
					}
					reader.Dispose();
				}
				cmd?.Dispose();
				if (wasClosed)
				{
					cnn.Close();
				}
				throw;
			}
		}

		public static Task<IDataReader> ExecuteReaderAsync(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return ExecuteReaderImplAsync(cnn, new CommandDefinition(sql, param, transaction, commandTimeout, commandType), CommandBehavior.Default);
		}

		public static Task<IDataReader> ExecuteReaderAsync(this IDbConnection cnn, CommandDefinition command)
		{
			return ExecuteReaderImplAsync(cnn, command, CommandBehavior.Default);
		}

		public static Task<IDataReader> ExecuteReaderAsync(this IDbConnection cnn, CommandDefinition command, CommandBehavior commandBehavior)
		{
			return ExecuteReaderImplAsync(cnn, command, commandBehavior);
		}

		private static async Task<IDataReader> ExecuteReaderImplAsync(IDbConnection cnn, CommandDefinition command, CommandBehavior commandBehavior)
		{
			Action<IDbCommand, object> parameterReader = GetParameterReader(cnn, ref command);
			DbCommand cmd = null;
			bool wasClosed = cnn.State == ConnectionState.Closed;
			try
			{
				cmd = command.TrySetupAsyncCommand(cnn, parameterReader);
				if (wasClosed)
				{
					await cnn.TryOpenAsync(command.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				DbDataReader result = await ExecuteReaderWithFlagsFallbackAsync(cmd, wasClosed, commandBehavior, command.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				wasClosed = false;
				return result;
			}
			finally
			{
				if (wasClosed)
				{
					cnn.Close();
				}
				cmd?.Dispose();
			}
		}

		public static Task<object> ExecuteScalarAsync(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return ExecuteScalarImplAsync<object>(cnn, new CommandDefinition(sql, param, transaction, commandTimeout, commandType));
		}

		public static Task<T> ExecuteScalarAsync<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return ExecuteScalarImplAsync<T>(cnn, new CommandDefinition(sql, param, transaction, commandTimeout, commandType));
		}

		public static Task<object> ExecuteScalarAsync(this IDbConnection cnn, CommandDefinition command)
		{
			return ExecuteScalarImplAsync<object>(cnn, command);
		}

		public static Task<T> ExecuteScalarAsync<T>(this IDbConnection cnn, CommandDefinition command)
		{
			return ExecuteScalarImplAsync<T>(cnn, command);
		}

		private static async Task<T> ExecuteScalarImplAsync<T>(IDbConnection cnn, CommandDefinition command)
		{
			Action<IDbCommand, object> paramReader = null;
			object parameters = command.Parameters;
			if (parameters != null)
			{
				paramReader = GetCacheInfo(new Identity(command.CommandText, command.CommandType, cnn, null, parameters.GetType(), null), command.Parameters, command.AddToCache).ParamReader;
			}
			DbCommand cmd = null;
			bool wasClosed = cnn.State == ConnectionState.Closed;
			object value;
			try
			{
				cmd = command.TrySetupAsyncCommand(cnn, paramReader);
				if (wasClosed)
				{
					await cnn.TryOpenAsync(command.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				value = await cmd.ExecuteScalarAsync(command.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				command.OnCompleted();
			}
			finally
			{
				if (wasClosed)
				{
					cnn.Close();
				}
				cmd?.Dispose();
			}
			return Parse<T>(value);
		}

		private static int GetColumnHash(IDataReader reader, int startBound = 0, int length = -1)
		{
			int num = (length < 0) ? reader.FieldCount : (startBound + length);
			int num2 = -37 * startBound + num;
			for (int i = startBound; i < num; i++)
			{
				num2 = -79 * (num2 * 31 + (reader.GetName(i)?.GetHashCode() ?? 0)) + (reader.GetFieldType(i)?.GetHashCode() ?? 0);
			}
			return num2;
		}

		private static void OnQueryCachePurged()
		{
			SqlMapper.QueryCachePurged?.Invoke(null, EventArgs.Empty);
		}

		private static void SetQueryCache(Identity key, CacheInfo value)
		{
			if (Interlocked.Increment(ref collect) == 1000)
			{
				CollectCacheGarbage();
			}
			_queryCache[key] = value;
		}

		private static void CollectCacheGarbage()
		{
			try
			{
				foreach (KeyValuePair<Identity, CacheInfo> item in _queryCache)
				{
					if (item.Value.GetHitCount() <= 0)
					{
						_queryCache.TryRemove(item.Key, out CacheInfo _);
					}
				}
			}
			finally
			{
				Interlocked.Exchange(ref collect, 0);
			}
		}

		private static bool TryGetQueryCache(Identity key, out CacheInfo value)
		{
			if (_queryCache.TryGetValue(key, out value))
			{
				value.RecordHit();
				return true;
			}
			value = null;
			return false;
		}

		public static void PurgeQueryCache()
		{
			_queryCache.Clear();
			TypeDeserializerCache.Purge();
			OnQueryCachePurged();
		}

		private static void PurgeQueryCacheByType(Type type)
		{
			foreach (KeyValuePair<Identity, CacheInfo> item in _queryCache)
			{
				if (item.Key.type == type)
				{
					_queryCache.TryRemove(item.Key, out CacheInfo _);
				}
			}
			TypeDeserializerCache.Purge(type);
		}

		public static int GetCachedSQLCount()
		{
			return _queryCache.Count;
		}

		public static IEnumerable<Tuple<string, string, int>> GetCachedSQL(int ignoreHitCountAbove = int.MaxValue)
		{
			IEnumerable<Tuple<string, string, int>> enumerable = from pair in _queryCache
			select Tuple.Create(pair.Key.connectionString, pair.Key.sql, pair.Value.GetHitCount());
			if (ignoreHitCountAbove >= 2147483647)
			{
				return enumerable;
			}
			return from tuple in enumerable
			where tuple.Item3 <= ignoreHitCountAbove
			select tuple;
		}

		public static IEnumerable<Tuple<int, int>> GetHashCollissions()
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			foreach (Identity key2 in _queryCache.Keys)
			{
				if (!dictionary.TryGetValue(key2.hashCode, out int value))
				{
					dictionary.Add(key2.hashCode, 1);
				}
				else
				{
					dictionary[key2.hashCode] = value + 1;
				}
			}
			return dictionary.Where(delegate(KeyValuePair<int, int> pair)
			{
				KeyValuePair<int, int> keyValuePair2 = pair;
				return keyValuePair2.Value > 1;
			}).Select(delegate(KeyValuePair<int, int> pair)
			{
				KeyValuePair<int, int> keyValuePair = pair;
				int key = keyValuePair.Key;
				keyValuePair = pair;
				return Tuple.Create(key, keyValuePair.Value);
			});
		}

		static SqlMapper()
		{
			_queryCache = new ConcurrentDictionary<Identity, CacheInfo>();
			ErrTwoRows = new int[2];
			ErrZeroRows = new int[0];
			smellsLikeOleDb = new Regex("(?<![\\p{L}\\p{N}@_])[?@:](?![\\p{L}\\p{N}@_])", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant);
			literalTokens = new Regex("(?<![\\p{L}\\p{N}_])\\{=([\\p{L}\\p{N}_]+)\\}", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant);
			pseudoPositional = new Regex("\\?([\\p{L}_][\\p{L}\\p{N}_]*)\\?", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
			format = typeof(SqlMapper).GetMethod("Format", BindingFlags.Static | BindingFlags.Public);
			toStrings = new Type[12]
			{
				typeof(bool),
				typeof(sbyte),
				typeof(byte),
				typeof(ushort),
				typeof(short),
				typeof(uint),
				typeof(int),
				typeof(ulong),
				typeof(long),
				typeof(float),
				typeof(double),
				typeof(decimal)
			}.ToDictionary((Type x) => TypeExtensions.GetTypeCode(x), (Type x) => x.GetPublicInstanceMethod("ToString", new Type[1]
			{
				typeof(IFormatProvider)
			}));
			StringReplace = typeof(string).GetPublicInstanceMethod("Replace", new Type[2]
			{
				typeof(string),
				typeof(string)
			});
			InvariantCulture = typeof(CultureInfo).GetProperty("InvariantCulture", BindingFlags.Static | BindingFlags.Public).GetGetMethod();
			enumParse = typeof(Enum).GetMethod("Parse", new Type[3]
			{
				typeof(Type),
				typeof(string),
				typeof(bool)
			});
			getItem = (from p in typeof(IDataRecord).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(delegate(PropertyInfo p)
			{
				if (p.GetIndexParameters().Length != 0)
				{
					return p.GetIndexParameters()[0].ParameterType == typeof(int);
				}
				return false;
			})
			select p.GetGetMethod()).First();
			TypeMapProvider = ((Type type) => new DefaultTypeMap(type));
			_typeMaps = new Hashtable();
			connectionStringComparer = StringComparer.Ordinal;
			typeMap = new Dictionary<Type, DbType>
			{
				[typeof(byte)] = DbType.Byte,
				[typeof(sbyte)] = DbType.SByte,
				[typeof(short)] = DbType.Int16,
				[typeof(ushort)] = DbType.UInt16,
				[typeof(int)] = DbType.Int32,
				[typeof(uint)] = DbType.UInt32,
				[typeof(long)] = DbType.Int64,
				[typeof(ulong)] = DbType.UInt64,
				[typeof(float)] = DbType.Single,
				[typeof(double)] = DbType.Double,
				[typeof(decimal)] = DbType.Decimal,
				[typeof(bool)] = DbType.Boolean,
				[typeof(string)] = DbType.String,
				[typeof(char)] = DbType.StringFixedLength,
				[typeof(Guid)] = DbType.Guid,
				[typeof(DateTime)] = DbType.DateTime,
				[typeof(DateTimeOffset)] = DbType.DateTimeOffset,
				[typeof(TimeSpan)] = DbType.Time,
				[typeof(byte[])] = DbType.Binary,
				[typeof(byte?)] = DbType.Byte,
				[typeof(sbyte?)] = DbType.SByte,
				[typeof(short?)] = DbType.Int16,
				[typeof(ushort?)] = DbType.UInt16,
				[typeof(int?)] = DbType.Int32,
				[typeof(uint?)] = DbType.UInt32,
				[typeof(long?)] = DbType.Int64,
				[typeof(ulong?)] = DbType.UInt64,
				[typeof(float?)] = DbType.Single,
				[typeof(double?)] = DbType.Double,
				[typeof(decimal?)] = DbType.Decimal,
				[typeof(bool?)] = DbType.Boolean,
				[typeof(char?)] = DbType.StringFixedLength,
				[typeof(Guid?)] = DbType.Guid,
				[typeof(DateTime?)] = DbType.DateTime,
				[typeof(DateTimeOffset?)] = DbType.DateTimeOffset,
				[typeof(TimeSpan?)] = DbType.Time,
				[typeof(object)] = DbType.Object
			};
			ResetTypeHandlers(clone: false);
		}

		public static void ResetTypeHandlers()
		{
			ResetTypeHandlers(clone: true);
		}

		private static void ResetTypeHandlers(bool clone)
		{
			typeHandlers = new Dictionary<Type, ITypeHandler>();
			AddTypeHandlerImpl(typeof(DataTable), new DataTableHandler(), clone);
			try
			{
				AddSqlDataRecordsTypeHandler(clone);
			}
			catch
			{
			}
			AddTypeHandlerImpl(typeof(XmlDocument), new XmlDocumentHandler(), clone);
			AddTypeHandlerImpl(typeof(XDocument), new XDocumentHandler(), clone);
			AddTypeHandlerImpl(typeof(XElement), new XElementHandler(), clone);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void AddSqlDataRecordsTypeHandler(bool clone)
		{
			AddTypeHandlerImpl(typeof(IEnumerable<SqlDataRecord>), new SqlDataRecordHandler(), clone);
		}

		public static void AddTypeMap(Type type, DbType dbType)
		{
			Dictionary<Type, DbType> dictionary = typeMap;
			if (!dictionary.TryGetValue(type, out DbType value) || value != dbType)
			{
				typeMap = new Dictionary<Type, DbType>(dictionary)
				{
					[type] = dbType
				};
			}
		}

		public static void RemoveTypeMap(Type type)
		{
			Dictionary<Type, DbType> dictionary = typeMap;
			if (dictionary.ContainsKey(type))
			{
				Dictionary<Type, DbType> dictionary2 = new Dictionary<Type, DbType>(dictionary);
				dictionary2.Remove(type);
				typeMap = dictionary2;
			}
		}

		public static void AddTypeHandler(Type type, ITypeHandler handler)
		{
			AddTypeHandlerImpl(type, handler, clone: true);
		}

		internal static bool HasTypeHandler(Type type)
		{
			return typeHandlers.ContainsKey(type);
		}

		public static void AddTypeHandlerImpl(Type type, ITypeHandler handler, bool clone)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			Type type2 = null;
			if (type.IsValueType())
			{
				Type underlyingType = Nullable.GetUnderlyingType(type);
				if (underlyingType == null)
				{
					type2 = typeof(Nullable<>).MakeGenericType(type);
				}
				else
				{
					type2 = type;
					type = underlyingType;
				}
			}
			Dictionary<Type, ITypeHandler> dictionary = typeHandlers;
			if (!dictionary.TryGetValue(type, out ITypeHandler value) || handler != value)
			{
				Dictionary<Type, ITypeHandler> dictionary2 = clone ? new Dictionary<Type, ITypeHandler>(dictionary) : dictionary;
				typeof(TypeHandlerCache<>).MakeGenericType(type).GetMethod("SetHandler", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[1]
				{
					handler
				});
				if (type2 != null)
				{
					typeof(TypeHandlerCache<>).MakeGenericType(type2).GetMethod("SetHandler", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[1]
					{
						handler
					});
				}
				if (handler == null)
				{
					dictionary2.Remove(type);
					if (type2 != null)
					{
						dictionary2.Remove(type2);
					}
				}
				else
				{
					dictionary2[type] = handler;
					if (type2 != null)
					{
						dictionary2[type2] = handler;
					}
				}
				typeHandlers = dictionary2;
			}
		}

		public static void AddTypeHandler<T>(TypeHandler<T> handler)
		{
			AddTypeHandlerImpl(typeof(T), handler, clone: true);
		}

		[Obsolete("This method is for internal use only", false)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static DbType GetDbType(object value)
		{
			if (value == null || value is DBNull)
			{
				return DbType.Object;
			}
			ITypeHandler handler;
			return LookupDbType(value.GetType(), "n/a", demand: false, out handler);
		}

		[Obsolete("This method is for internal use only", false)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static DbType LookupDbType(Type type, string name, bool demand, out ITypeHandler handler)
		{
			handler = null;
			Type underlyingType = Nullable.GetUnderlyingType(type);
			if (underlyingType != null)
			{
				type = underlyingType;
			}
			if (type.IsEnum() && !typeMap.ContainsKey(type))
			{
				type = Enum.GetUnderlyingType(type);
			}
			if (typeMap.TryGetValue(type, out DbType value))
			{
				return value;
			}
			if (type.FullName == "System.Data.Linq.Binary")
			{
				return DbType.Binary;
			}
			if (typeHandlers.TryGetValue(type, out handler))
			{
				return DbType.Object;
			}
			if (!typeof(IEnumerable).IsAssignableFrom(type))
			{
				switch (type.FullName)
				{
				case "Microsoft.SqlServer.Types.SqlGeography":
					AddTypeHandler(type, handler = new UdtTypeHandler("geography"));
					return DbType.Object;
				case "Microsoft.SqlServer.Types.SqlGeometry":
					AddTypeHandler(type, handler = new UdtTypeHandler("geometry"));
					return DbType.Object;
				case "Microsoft.SqlServer.Types.SqlHierarchyId":
					AddTypeHandler(type, handler = new UdtTypeHandler("hierarchyid"));
					return DbType.Object;
				default:
					if (demand)
					{
						throw new NotSupportedException($"The member {name} of type {type.FullName} cannot be used as a parameter value");
					}
					return DbType.Object;
				}
			}
			return (DbType)(-1);
		}

		public static List<T> AsList<T>(this IEnumerable<T> source)
		{
			if (source != null && !(source is List<T>))
			{
				return source.ToList();
			}
			return (List<T>)source;
		}

		public static int Execute(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			CommandDefinition command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType);
			return cnn.ExecuteImpl(ref command);
		}

		public static int Execute(this IDbConnection cnn, CommandDefinition command)
		{
			return cnn.ExecuteImpl(ref command);
		}

		public static object ExecuteScalar(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			CommandDefinition command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType);
			return ExecuteScalarImpl<object>(cnn, ref command);
		}

		public static T ExecuteScalar<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			CommandDefinition command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType);
			return ExecuteScalarImpl<T>(cnn, ref command);
		}

		public static object ExecuteScalar(this IDbConnection cnn, CommandDefinition command)
		{
			return ExecuteScalarImpl<object>(cnn, ref command);
		}

		public static T ExecuteScalar<T>(this IDbConnection cnn, CommandDefinition command)
		{
			return ExecuteScalarImpl<T>(cnn, ref command);
		}

		private static IEnumerable GetMultiExec(object param)
		{
			if (!(param is IEnumerable) || param is string || param is IEnumerable<KeyValuePair<string, object>> || param is IDynamicParameters)
			{
				return null;
			}
			return (IEnumerable)param;
		}

		private static int ExecuteImpl(this IDbConnection cnn, ref CommandDefinition command)
		{
			object parameters = command.Parameters;
			IEnumerable multiExec = GetMultiExec(parameters);
			CacheInfo cacheInfo = null;
			if (multiExec != null)
			{
				if ((command.Flags & CommandFlags.Pipelined) == CommandFlags.None)
				{
					bool flag = true;
					int num = 0;
					bool flag2 = cnn.State == ConnectionState.Closed;
					try
					{
						if (flag2)
						{
							cnn.Open();
						}
						using (IDbCommand dbCommand = command.SetupCommand(cnn, null))
						{
							string commandText = null;
							foreach (object item in multiExec)
							{
								if (flag)
								{
									commandText = dbCommand.CommandText;
									flag = false;
									cacheInfo = GetCacheInfo(new Identity(command.CommandText, dbCommand.CommandType, cnn, null, item.GetType(), null), item, command.AddToCache);
								}
								else
								{
									dbCommand.CommandText = commandText;
									dbCommand.Parameters.Clear();
								}
								cacheInfo.ParamReader(dbCommand, item);
								num += dbCommand.ExecuteNonQuery();
							}
						}
						command.OnCompleted();
						return num;
					}
					finally
					{
						if (flag2)
						{
							cnn.Close();
						}
					}
				}
				return ExecuteMultiImplAsync(cnn, command, multiExec).Result;
			}
			if (parameters != null)
			{
				cacheInfo = GetCacheInfo(new Identity(command.CommandText, command.CommandType, cnn, null, parameters.GetType(), null), parameters, command.AddToCache);
			}
			return ExecuteCommand(cnn, ref command, (parameters == null) ? null : cacheInfo.ParamReader);
		}

		public static IDataReader ExecuteReader(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			CommandDefinition command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType);
			IDbCommand cmd;
			IDataReader reader = ExecuteReaderImpl(cnn, ref command, CommandBehavior.Default, out cmd);
			return new WrappedReader(cmd, reader);
		}

		public static IDataReader ExecuteReader(this IDbConnection cnn, CommandDefinition command)
		{
			IDbCommand cmd;
			IDataReader reader = ExecuteReaderImpl(cnn, ref command, CommandBehavior.Default, out cmd);
			return new WrappedReader(cmd, reader);
		}

		public static IDataReader ExecuteReader(this IDbConnection cnn, CommandDefinition command, CommandBehavior commandBehavior)
		{
			IDbCommand cmd;
			IDataReader reader = ExecuteReaderImpl(cnn, ref command, commandBehavior, out cmd);
			return new WrappedReader(cmd, reader);
		}

		public static IEnumerable<dynamic> Query(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.Query<DapperRow>(sql, param, transaction, buffered, commandTimeout, commandType);
		}

		public static dynamic QueryFirst(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.QueryFirst<DapperRow>(sql, param, transaction, commandTimeout, commandType);
		}

		public static dynamic QueryFirstOrDefault(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.QueryFirstOrDefault<DapperRow>(sql, param, transaction, commandTimeout, commandType);
		}

		public static dynamic QuerySingle(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.QuerySingle<DapperRow>(sql, param, transaction, commandTimeout, commandType);
		}

		public static dynamic QuerySingleOrDefault(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.QuerySingleOrDefault<DapperRow>(sql, param, transaction, commandTimeout, commandType);
		}

		public static IEnumerable<T> Query<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			CommandDefinition command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, buffered ? CommandFlags.Buffered : CommandFlags.None);
			IEnumerable<T> enumerable = cnn.QueryImpl<T>(command, typeof(T));
			if (!command.Buffered)
			{
				return enumerable;
			}
			return enumerable.ToList();
		}

		public static T QueryFirst<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			CommandDefinition command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None);
			return QueryRowImpl<T>(cnn, Row.First, ref command, typeof(T));
		}

		public static T QueryFirstOrDefault<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			CommandDefinition command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None);
			return QueryRowImpl<T>(cnn, Row.FirstOrDefault, ref command, typeof(T));
		}

		public static T QuerySingle<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			CommandDefinition command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None);
			return QueryRowImpl<T>(cnn, Row.Single, ref command, typeof(T));
		}

		public static T QuerySingleOrDefault<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			CommandDefinition command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None);
			return QueryRowImpl<T>(cnn, Row.SingleOrDefault, ref command, typeof(T));
		}

		public static IEnumerable<object> Query(this IDbConnection cnn, Type type, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			CommandDefinition command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, buffered ? CommandFlags.Buffered : CommandFlags.None);
			IEnumerable<object> enumerable = cnn.QueryImpl<object>(command, type);
			if (!command.Buffered)
			{
				return enumerable;
			}
			return enumerable.ToList();
		}

		public static object QueryFirst(this IDbConnection cnn, Type type, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			CommandDefinition command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None);
			return QueryRowImpl<object>(cnn, Row.First, ref command, type);
		}

		public static object QueryFirstOrDefault(this IDbConnection cnn, Type type, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			CommandDefinition command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None);
			return QueryRowImpl<object>(cnn, Row.FirstOrDefault, ref command, type);
		}

		public static object QuerySingle(this IDbConnection cnn, Type type, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			CommandDefinition command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None);
			return QueryRowImpl<object>(cnn, Row.Single, ref command, type);
		}

		public static object QuerySingleOrDefault(this IDbConnection cnn, Type type, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			CommandDefinition command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None);
			return QueryRowImpl<object>(cnn, Row.SingleOrDefault, ref command, type);
		}

		public static IEnumerable<T> Query<T>(this IDbConnection cnn, CommandDefinition command)
		{
			IEnumerable<T> enumerable = cnn.QueryImpl<T>(command, typeof(T));
			if (!command.Buffered)
			{
				return enumerable;
			}
			return enumerable.ToList();
		}

		public static T QueryFirst<T>(this IDbConnection cnn, CommandDefinition command)
		{
			return QueryRowImpl<T>(cnn, Row.First, ref command, typeof(T));
		}

		public static T QueryFirstOrDefault<T>(this IDbConnection cnn, CommandDefinition command)
		{
			return QueryRowImpl<T>(cnn, Row.FirstOrDefault, ref command, typeof(T));
		}

		public static T QuerySingle<T>(this IDbConnection cnn, CommandDefinition command)
		{
			return QueryRowImpl<T>(cnn, Row.Single, ref command, typeof(T));
		}

		public static T QuerySingleOrDefault<T>(this IDbConnection cnn, CommandDefinition command)
		{
			return QueryRowImpl<T>(cnn, Row.SingleOrDefault, ref command, typeof(T));
		}

		public static GridReader QueryMultiple(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			CommandDefinition command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType);
			return cnn.QueryMultipleImpl(ref command);
		}

		public static GridReader QueryMultiple(this IDbConnection cnn, CommandDefinition command)
		{
			return cnn.QueryMultipleImpl(ref command);
		}

		private static GridReader QueryMultipleImpl(this IDbConnection cnn, ref CommandDefinition command)
		{
			object parameters = command.Parameters;
			Identity identity = new Identity(command.CommandText, command.CommandType, cnn, typeof(GridReader), parameters?.GetType(), null);
			CacheInfo cacheInfo = GetCacheInfo(identity, parameters, command.AddToCache);
			IDbCommand dbCommand = null;
			IDataReader dataReader = null;
			bool flag = cnn.State == ConnectionState.Closed;
			try
			{
				if (flag)
				{
					cnn.Open();
				}
				dbCommand = command.SetupCommand(cnn, cacheInfo.ParamReader);
				dataReader = ExecuteReaderWithFlagsFallback(dbCommand, flag, CommandBehavior.SequentialAccess);
				GridReader result = new GridReader(dbCommand, dataReader, identity, command.Parameters as DynamicParameters, command.AddToCache);
				dbCommand = null;
				flag = false;
				return result;
			}
			catch
			{
				if (dataReader != null)
				{
					if (!dataReader.IsClosed)
					{
						try
						{
							dbCommand?.Cancel();
						}
						catch
						{
						}
					}
					dataReader.Dispose();
				}
				dbCommand?.Dispose();
				if (flag)
				{
					cnn.Close();
				}
				throw;
			}
		}

		private static IDataReader ExecuteReaderWithFlagsFallback(IDbCommand cmd, bool wasClosed, CommandBehavior behavior)
		{
			try
			{
				return cmd.ExecuteReader(GetBehavior(wasClosed, behavior));
			}
			catch (ArgumentException ex)
			{
				if (!Settings.DisableCommandBehaviorOptimizations(behavior, ex))
				{
					throw;
				}
				return cmd.ExecuteReader(GetBehavior(wasClosed, behavior));
			}
		}

		private static IEnumerable<T> QueryImpl<T>(this IDbConnection cnn, CommandDefinition command, Type effectiveType)
		{
			object parameters = command.Parameters;
			Identity identity = new Identity(command.CommandText, command.CommandType, cnn, effectiveType, parameters?.GetType(), null);
			CacheInfo cacheInfo = GetCacheInfo(identity, parameters, command.AddToCache);
			IDbCommand cmd = null;
			IDataReader reader = null;
			bool wasClosed = cnn.State == ConnectionState.Closed;
			try
			{
				cmd = command.SetupCommand(cnn, cacheInfo.ParamReader);
				if (wasClosed)
				{
					cnn.Open();
				}
				reader = ExecuteReaderWithFlagsFallback(cmd, wasClosed, CommandBehavior.SingleResult | CommandBehavior.SequentialAccess);
				wasClosed = false;
				DeserializerState deserializerState = cacheInfo.Deserializer;
				int columnHash = GetColumnHash(reader);
				if (deserializerState.Func != null && deserializerState.Hash == columnHash)
				{
					goto IL_0175;
				}
				if (reader.FieldCount != 0)
				{
					DeserializerState deserializerState3 = cacheInfo.Deserializer = new DeserializerState(columnHash, GetDeserializer(effectiveType, reader, 0, -1, returnNullIfFirstMissing: false));
					deserializerState = deserializerState3;
					if (command.AddToCache)
					{
						SetQueryCache(identity, cacheInfo);
					}
					goto IL_0175;
				}
				goto end_IL_00a1;
				IL_0175:
				Func<IDataReader, object> func = deserializerState.Func;
				Type convertToType = Nullable.GetUnderlyingType(effectiveType) ?? effectiveType;
				while (reader.Read())
				{
					object obj = func(reader);
					if (obj == null || obj is T)
					{
						yield return (T)obj;
					}
					else
					{
						yield return (T)Convert.ChangeType(obj, convertToType, CultureInfo.InvariantCulture);
					}
				}
				while (reader.NextResult())
				{
				}
				reader.Dispose();
				reader = null;
				command.OnCompleted();
				end_IL_00a1:;
			}
			finally
			{
				if (reader != null)
				{
					if (!reader.IsClosed)
					{
						try
						{
							cmd.Cancel();
						}
						catch
						{
						}
					}
					reader.Dispose();
				}
				if (wasClosed)
				{
					cnn.Close();
				}
				cmd?.Dispose();
			}
		}

		private static void ThrowMultipleRows(Row row)
		{
			switch (row)
			{
			case Row.Single:
				ErrTwoRows.Single();
				break;
			case Row.SingleOrDefault:
				ErrTwoRows.SingleOrDefault();
				break;
			default:
				throw new InvalidOperationException();
			}
		}

		private static void ThrowZeroRows(Row row)
		{
			switch (row)
			{
			case Row.First:
				ErrZeroRows.First();
				break;
			case Row.Single:
				ErrZeroRows.Single();
				break;
			default:
				throw new InvalidOperationException();
			}
		}

		private static T QueryRowImpl<T>(IDbConnection cnn, Row row, ref CommandDefinition command, Type effectiveType)
		{
			object parameters = command.Parameters;
			Identity identity = new Identity(command.CommandText, command.CommandType, cnn, effectiveType, parameters?.GetType(), null);
			CacheInfo cacheInfo = GetCacheInfo(identity, parameters, command.AddToCache);
			IDbCommand dbCommand = null;
			IDataReader dataReader = null;
			bool flag = cnn.State == ConnectionState.Closed;
			try
			{
				dbCommand = command.SetupCommand(cnn, cacheInfo.ParamReader);
				if (flag)
				{
					cnn.Open();
				}
				dataReader = ExecuteReaderWithFlagsFallback(dbCommand, flag, ((row & Row.Single) != 0) ? (CommandBehavior.SingleResult | CommandBehavior.SequentialAccess) : (CommandBehavior.SingleResult | CommandBehavior.SingleRow | CommandBehavior.SequentialAccess));
				flag = false;
				T result = default(T);
				if (dataReader.Read() && dataReader.FieldCount != 0)
				{
					DeserializerState deserializerState = cacheInfo.Deserializer;
					int columnHash = GetColumnHash(dataReader);
					if (deserializerState.Func == null || deserializerState.Hash != columnHash)
					{
						DeserializerState deserializerState3 = cacheInfo.Deserializer = new DeserializerState(columnHash, GetDeserializer(effectiveType, dataReader, 0, -1, returnNullIfFirstMissing: false));
						deserializerState = deserializerState3;
						if (command.AddToCache)
						{
							SetQueryCache(identity, cacheInfo);
						}
					}
					object obj = deserializerState.Func(dataReader);
					if (obj == null || obj is T)
					{
						result = (T)obj;
					}
					else
					{
						Type conversionType = Nullable.GetUnderlyingType(effectiveType) ?? effectiveType;
						result = (T)Convert.ChangeType(obj, conversionType, CultureInfo.InvariantCulture);
					}
					if ((row & Row.Single) != 0 && dataReader.Read())
					{
						ThrowMultipleRows(row);
					}
					while (dataReader.Read())
					{
					}
				}
				else if ((row & Row.FirstOrDefault) == Row.First)
				{
					ThrowZeroRows(row);
				}
				while (dataReader.NextResult())
				{
				}
				dataReader.Dispose();
				dataReader = null;
				command.OnCompleted();
				return result;
			}
			finally
			{
				if (dataReader != null)
				{
					if (!dataReader.IsClosed)
					{
						try
						{
							dbCommand.Cancel();
						}
						catch
						{
						}
					}
					dataReader.Dispose();
				}
				if (flag)
				{
					cnn.Close();
				}
				dbCommand?.Dispose();
			}
		}

		public static IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(this IDbConnection cnn, string sql, Func<TFirst, TSecond, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.MultiMap<TFirst, TSecond, DontMap, DontMap, DontMap, DontMap, DontMap, TReturn>(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
		}

		public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TReturn>(this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.MultiMap<TFirst, TSecond, TThird, DontMap, DontMap, DontMap, DontMap, TReturn>(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
		}

		public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TReturn>(this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.MultiMap<TFirst, TSecond, TThird, TFourth, DontMap, DontMap, DontMap, TReturn>(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
		}

		public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.MultiMap<TFirst, TSecond, TThird, TFourth, TFifth, DontMap, DontMap, TReturn>(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
		}

		public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.MultiMap<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, DontMap, TReturn>(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
		}

		public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			return cnn.MultiMap<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
		}

		public static IEnumerable<TReturn> Query<TReturn>(this IDbConnection cnn, string sql, Type[] types, Func<object[], TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
		{
			CommandDefinition command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, buffered ? CommandFlags.Buffered : CommandFlags.None);
			IEnumerable<TReturn> enumerable = cnn.MultiMapImpl(command, types, map, splitOn, null, null, finalize: true);
			if (!buffered)
			{
				return enumerable;
			}
			return enumerable.ToList();
		}

		private static IEnumerable<TReturn> MultiMap<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(this IDbConnection cnn, string sql, Delegate map, object param, IDbTransaction transaction, bool buffered, string splitOn, int? commandTimeout, CommandType? commandType)
		{
			CommandDefinition command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, buffered ? CommandFlags.Buffered : CommandFlags.None);
			IEnumerable<TReturn> enumerable = cnn.MultiMapImpl<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(command, map, splitOn, null, null, finalize: true);
			if (!buffered)
			{
				return enumerable;
			}
			return enumerable.ToList();
		}

		private static IEnumerable<TReturn> MultiMapImpl<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(this IDbConnection cnn, CommandDefinition command, Delegate map, string splitOn, IDataReader reader, Identity identity, bool finalize)
		{
			object parameters = command.Parameters;
			identity = (identity ?? new Identity(command.CommandText, command.CommandType, cnn, typeof(TFirst), parameters?.GetType(), new Type[7]
			{
				typeof(TFirst),
				typeof(TSecond),
				typeof(TThird),
				typeof(TFourth),
				typeof(TFifth),
				typeof(TSixth),
				typeof(TSeventh)
			}));
			CacheInfo cacheInfo = GetCacheInfo(identity, parameters, command.AddToCache);
			IDbCommand ownedCommand = null;
			IDataReader ownedReader = null;
			bool wasClosed = cnn != null && cnn.State == ConnectionState.Closed;
			try
			{
				if (reader == null)
				{
					ownedCommand = command.SetupCommand(cnn, cacheInfo.ParamReader);
					if (wasClosed)
					{
						cnn.Open();
					}
					ownedReader = ExecuteReaderWithFlagsFallback(ownedCommand, wasClosed, CommandBehavior.SingleResult | CommandBehavior.SequentialAccess);
					reader = ownedReader;
				}
				DeserializerState deserializerState = default(DeserializerState);
				int columnHash = GetColumnHash(reader);
				Func<IDataReader, object>[] otherDeserializers;
				if ((deserializerState = cacheInfo.Deserializer).Func == null || (otherDeserializers = cacheInfo.OtherDeserializers) == null || columnHash != deserializerState.Hash)
				{
					Func<IDataReader, object>[] array = GenerateDeserializers(new Type[7]
					{
						typeof(TFirst),
						typeof(TSecond),
						typeof(TThird),
						typeof(TFourth),
						typeof(TFifth),
						typeof(TSixth),
						typeof(TSeventh)
					}, splitOn, reader);
					DeserializerState deserializerState3 = cacheInfo.Deserializer = new DeserializerState(columnHash, array[0]);
					deserializerState = deserializerState3;
					Func<IDataReader, object>[] array3 = cacheInfo.OtherDeserializers = array.Skip(1).ToArray();
					otherDeserializers = array3;
					if (command.AddToCache)
					{
						SetQueryCache(identity, cacheInfo);
					}
				}
				Func<IDataReader, TReturn> mapIt = GenerateMapper<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(deserializerState.Func, otherDeserializers, map);
				if (mapIt != null)
				{
					while (reader.Read())
					{
						yield return mapIt(reader);
					}
					if (finalize)
					{
						while (reader.NextResult())
						{
						}
						command.OnCompleted();
					}
				}
			}
			finally
			{
				try
				{
					ownedReader?.Dispose();
				}
				finally
				{
					ownedCommand?.Dispose();
					if (wasClosed)
					{
						cnn.Close();
					}
				}
			}
		}

		private static CommandBehavior GetBehavior(bool close, CommandBehavior @default)
		{
			return (close ? (@default | CommandBehavior.CloseConnection) : @default) & Settings.AllowedCommandBehaviors;
		}

		private static IEnumerable<TReturn> MultiMapImpl<TReturn>(this IDbConnection cnn, CommandDefinition command, Type[] types, Func<object[], TReturn> map, string splitOn, IDataReader reader, Identity identity, bool finalize)
		{
			if (types.Length < 1)
			{
				throw new ArgumentException("you must provide at least one type to deserialize");
			}
			object parameters = command.Parameters;
			identity = (identity ?? new Identity(command.CommandText, command.CommandType, cnn, types[0], parameters?.GetType(), types));
			CacheInfo cacheInfo = GetCacheInfo(identity, parameters, command.AddToCache);
			IDbCommand ownedCommand = null;
			IDataReader ownedReader = null;
			bool wasClosed = cnn != null && cnn.State == ConnectionState.Closed;
			try
			{
				if (reader == null)
				{
					ownedCommand = command.SetupCommand(cnn, cacheInfo.ParamReader);
					if (wasClosed)
					{
						cnn.Open();
					}
					ownedReader = ExecuteReaderWithFlagsFallback(ownedCommand, wasClosed, CommandBehavior.SingleResult | CommandBehavior.SequentialAccess);
					reader = ownedReader;
				}
				int columnHash = GetColumnHash(reader);
				DeserializerState deserializerState;
				Func<IDataReader, object>[] otherDeserializers;
				if ((deserializerState = cacheInfo.Deserializer).Func == null || (otherDeserializers = cacheInfo.OtherDeserializers) == null || columnHash != deserializerState.Hash)
				{
					Func<IDataReader, object>[] array = GenerateDeserializers(types, splitOn, reader);
					DeserializerState deserializerState3 = cacheInfo.Deserializer = new DeserializerState(columnHash, array[0]);
					deserializerState = deserializerState3;
					Func<IDataReader, object>[] array3 = cacheInfo.OtherDeserializers = array.Skip(1).ToArray();
					otherDeserializers = array3;
					SetQueryCache(identity, cacheInfo);
				}
				Func<IDataReader, TReturn> mapIt = GenerateMapper(types.Length, deserializerState.Func, otherDeserializers, map);
				if (mapIt != null)
				{
					while (reader.Read())
					{
						yield return mapIt(reader);
					}
					if (finalize)
					{
						while (reader.NextResult())
						{
						}
						command.OnCompleted();
					}
				}
			}
			finally
			{
				try
				{
					ownedReader?.Dispose();
				}
				finally
				{
					ownedCommand?.Dispose();
					if (wasClosed)
					{
						cnn.Close();
					}
				}
			}
		}

		private static Func<IDataReader, TReturn> GenerateMapper<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(Func<IDataReader, object> deserializer, Func<IDataReader, object>[] otherDeserializers, object map)
		{
			switch (otherDeserializers.Length)
			{
			case 1:
				return (IDataReader r) => ((Func<TFirst, TSecond, TReturn>)map)((TFirst)deserializer(r), (TSecond)otherDeserializers[0](r));
			case 2:
				return (IDataReader r) => ((Func<TFirst, TSecond, TThird, TReturn>)map)((TFirst)deserializer(r), (TSecond)otherDeserializers[0](r), (TThird)otherDeserializers[1](r));
			case 3:
				return (IDataReader r) => ((Func<TFirst, TSecond, TThird, TFourth, TReturn>)map)((TFirst)deserializer(r), (TSecond)otherDeserializers[0](r), (TThird)otherDeserializers[1](r), (TFourth)otherDeserializers[2](r));
			case 4:
				return (IDataReader r) => ((Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>)map)((TFirst)deserializer(r), (TSecond)otherDeserializers[0](r), (TThird)otherDeserializers[1](r), (TFourth)otherDeserializers[2](r), (TFifth)otherDeserializers[3](r));
			case 5:
				return (IDataReader r) => ((Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>)map)((TFirst)deserializer(r), (TSecond)otherDeserializers[0](r), (TThird)otherDeserializers[1](r), (TFourth)otherDeserializers[2](r), (TFifth)otherDeserializers[3](r), (TSixth)otherDeserializers[4](r));
			case 6:
				return (IDataReader r) => ((Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>)map)((TFirst)deserializer(r), (TSecond)otherDeserializers[0](r), (TThird)otherDeserializers[1](r), (TFourth)otherDeserializers[2](r), (TFifth)otherDeserializers[3](r), (TSixth)otherDeserializers[4](r), (TSeventh)otherDeserializers[5](r));
			default:
				throw new NotSupportedException();
			}
		}

		private static Func<IDataReader, TReturn> GenerateMapper<TReturn>(int length, Func<IDataReader, object> deserializer, Func<IDataReader, object>[] otherDeserializers, Func<object[], TReturn> map)
		{
			return delegate(IDataReader r)
			{
				object[] array = new object[length];
				array[0] = deserializer(r);
				for (int i = 1; i < length; i++)
				{
					array[i] = otherDeserializers[i - 1](r);
				}
				return map(array);
			};
		}

		private static Func<IDataReader, object>[] GenerateDeserializers(Type[] types, string splitOn, IDataReader reader)
		{
			List<Func<IDataReader, object>> list = new List<Func<IDataReader, object>>();
			string[] array = (from s in splitOn.Split(',')
			select s.Trim()).ToArray();
			bool flag = array.Length > 1;
			if (types[0] == typeof(object))
			{
				bool flag2 = true;
				int num = 0;
				int num2 = 0;
				string splitOn2 = array[num2];
				foreach (Type type in types)
				{
					if (type == typeof(DontMap))
					{
						break;
					}
					int nextSplitDynamic = GetNextSplitDynamic(num, splitOn2, reader);
					if (flag && num2 < array.Length - 1)
					{
						splitOn2 = array[++num2];
					}
					list.Add(GetDeserializer(type, reader, num, nextSplitDynamic - num, !flag2));
					num = nextSplitDynamic;
					flag2 = false;
				}
			}
			else
			{
				int num3 = reader.FieldCount;
				int num4 = array.Length - 1;
				string splitOn3 = array[num4];
				for (int num5 = types.Length - 1; num5 >= 0; num5--)
				{
					Type type2 = types[num5];
					if (!(type2 == typeof(DontMap)))
					{
						int num6 = 0;
						if (num5 > 0)
						{
							num6 = GetNextSplit(num3, splitOn3, reader);
							if (flag && num4 > 0)
							{
								splitOn3 = array[--num4];
							}
						}
						list.Add(GetDeserializer(type2, reader, num6, num3 - num6, num5 > 0));
						num3 = num6;
					}
				}
				list.Reverse();
			}
			return list.ToArray();
		}

		private static int GetNextSplitDynamic(int startIdx, string splitOn, IDataReader reader)
		{
			if (startIdx == reader.FieldCount)
			{
				throw MultiMapException(reader);
			}
			if (splitOn == "*")
			{
				return ++startIdx;
			}
			for (int i = startIdx + 1; i < reader.FieldCount; i++)
			{
				if (string.Equals(splitOn, reader.GetName(i), StringComparison.OrdinalIgnoreCase))
				{
					return i;
				}
			}
			return reader.FieldCount;
		}

		private static int GetNextSplit(int startIdx, string splitOn, IDataReader reader)
		{
			if (splitOn == "*")
			{
				return --startIdx;
			}
			for (int num = startIdx - 1; num > 0; num--)
			{
				if (string.Equals(splitOn, reader.GetName(num), StringComparison.OrdinalIgnoreCase))
				{
					return num;
				}
			}
			throw MultiMapException(reader);
		}

		private static CacheInfo GetCacheInfo(Identity identity, object exampleParameters, bool addToCache)
		{
			if (!TryGetQueryCache(identity, out CacheInfo value))
			{
				if (GetMultiExec(exampleParameters) != null)
				{
					throw new InvalidOperationException("An enumerable sequence of parameters (arrays, lists, etc) is not allowed in this context");
				}
				value = new CacheInfo();
				if (identity.parametersType != null)
				{
					Action<IDbCommand, object> action;
					if (exampleParameters is IDynamicParameters)
					{
						action = delegate(IDbCommand cmd, object obj)
						{
							((IDynamicParameters)obj).AddParameters(cmd, identity);
						};
					}
					else if (exampleParameters is IEnumerable<KeyValuePair<string, object>>)
					{
						action = delegate(IDbCommand cmd, object obj)
						{
							((IDynamicParameters)new DynamicParameters(obj)).AddParameters(cmd, identity);
						};
					}
					else
					{
						IList<LiteralToken> literals = GetLiteralTokens(identity.sql);
						action = CreateParamInfoGenerator(identity, checkForDuplicates: false, removeUnused: true, literals);
					}
					if ((!identity.commandType.HasValue || identity.commandType == CommandType.Text) && ShouldPassByPosition(identity.sql))
					{
						Action<IDbCommand, object> tail = action;
						action = delegate(IDbCommand cmd, object obj)
						{
							tail(cmd, obj);
							PassByPosition(cmd);
						};
					}
					value.ParamReader = action;
				}
				if (addToCache)
				{
					SetQueryCache(identity, value);
				}
			}
			return value;
		}

		private static bool ShouldPassByPosition(string sql)
		{
			if (sql != null && sql.IndexOf('?') >= 0)
			{
				return pseudoPositional.IsMatch(sql);
			}
			return false;
		}

		private static void PassByPosition(IDbCommand cmd)
		{
			if (cmd.Parameters.Count != 0)
			{
				Dictionary<string, IDbDataParameter> parameters = new Dictionary<string, IDbDataParameter>(StringComparer.Ordinal);
				foreach (IDbDataParameter parameter in cmd.Parameters)
				{
					if (!string.IsNullOrEmpty(parameter.ParameterName))
					{
						parameters[parameter.ParameterName] = parameter;
					}
				}
				HashSet<string> consumed = new HashSet<string>(StringComparer.Ordinal);
				bool firstMatch = true;
				cmd.CommandText = pseudoPositional.Replace(cmd.CommandText, delegate(Match match)
				{
					string value = match.Groups[1].Value;
					if (!consumed.Add(value))
					{
						throw new InvalidOperationException("When passing parameters by position, each parameter can only be referenced once");
					}
					if (parameters.TryGetValue(value, out IDbDataParameter value2))
					{
						if (firstMatch)
						{
							firstMatch = false;
							cmd.Parameters.Clear();
						}
						cmd.Parameters.Add(value2);
						parameters.Remove(value);
						consumed.Add(value);
						return "?";
					}
					return match.Value;
				});
			}
		}

		private static Func<IDataReader, object> GetDeserializer(Type type, IDataReader reader, int startBound, int length, bool returnNullIfFirstMissing)
		{
			if (type == typeof(object) || type == typeof(DapperRow))
			{
				return GetDapperRowDeserializer(reader, startBound, length, returnNullIfFirstMissing);
			}
			Type type2 = null;
			if (!typeMap.ContainsKey(type) && !type.IsEnum() && !(type.FullName == "System.Data.Linq.Binary") && (!type.IsValueType() || !((type2 = Nullable.GetUnderlyingType(type)) != null) || !type2.IsEnum()))
			{
				if (typeHandlers.TryGetValue(type, out ITypeHandler value))
				{
					return GetHandlerDeserializer(value, type, startBound);
				}
				return GetTypeDeserializer(type, reader, startBound, length, returnNullIfFirstMissing);
			}
			return GetStructDeserializer(type, type2 ?? type, startBound);
		}

		private static Func<IDataReader, object> GetHandlerDeserializer(ITypeHandler handler, Type type, int startBound)
		{
			return (IDataReader reader) => handler.Parse(type, reader.GetValue(startBound));
		}

		private static Exception MultiMapException(IDataRecord reader)
		{
			bool flag = false;
			try
			{
				flag = (reader != null && reader.FieldCount != 0);
			}
			catch
			{
			}
			if (flag)
			{
				return new ArgumentException("When using the multi-mapping APIs ensure you set the splitOn param if you have keys other than Id", "splitOn");
			}
			return new InvalidOperationException("No columns were selected");
		}

		internal static Func<IDataReader, object> GetDapperRowDeserializer(IDataRecord reader, int startBound, int length, bool returnNullIfFirstMissing)
		{
			int fieldCount = reader.FieldCount;
			if (length == -1)
			{
				length = fieldCount - startBound;
			}
			if (fieldCount <= startBound)
			{
				throw MultiMapException(reader);
			}
			int effectiveFieldCount = Math.Min(fieldCount - startBound, length);
			DapperTable table = null;
			return delegate(IDataReader r)
			{
				if (table == null)
				{
					string[] array = new string[effectiveFieldCount];
					for (int i = 0; i < effectiveFieldCount; i++)
					{
						array[i] = r.GetName(i + startBound);
					}
					table = new DapperTable(array);
				}
				object[] array2 = new object[effectiveFieldCount];
				if (returnNullIfFirstMissing)
				{
					array2[0] = r.GetValue(startBound);
					if (array2[0] is DBNull)
					{
						return null;
					}
				}
				if (startBound == 0)
				{
					for (int j = 0; j < array2.Length; j++)
					{
						object value = r.GetValue(j);
						array2[j] = ((value is DBNull) ? null : value);
					}
				}
				else
				{
					for (int k = returnNullIfFirstMissing ? 1 : 0; k < effectiveFieldCount; k++)
					{
						object value2 = r.GetValue(k + startBound);
						array2[k] = ((value2 is DBNull) ? null : value2);
					}
				}
				return new DapperRow(table, array2);
			};
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This method is for internal use only", false)]
		public static char ReadChar(object value)
		{
			if (value == null || value is DBNull)
			{
				throw new ArgumentNullException("value");
			}
			string text = value as string;
			if (text == null || text.Length != 1)
			{
				throw new ArgumentException("A single-character was expected", "value");
			}
			return text[0];
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This method is for internal use only", false)]
		public static char? ReadNullableChar(object value)
		{
			if (value == null || value is DBNull)
			{
				return null;
			}
			string text = value as string;
			if (text == null || text.Length != 1)
			{
				throw new ArgumentException("A single-character was expected", "value");
			}
			return text[0];
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This method is for internal use only", true)]
		public static IDbDataParameter FindOrAddParameter(IDataParameterCollection parameters, IDbCommand command, string name)
		{
			IDbDataParameter dbDataParameter;
			if (parameters.Contains(name))
			{
				dbDataParameter = (IDbDataParameter)parameters[name];
			}
			else
			{
				dbDataParameter = command.CreateParameter();
				dbDataParameter.ParameterName = name;
				parameters.Add(dbDataParameter);
			}
			return dbDataParameter;
		}

		internal static int GetListPaddingExtraCount(int count)
		{
			if ((uint)count <= 5u)
			{
				return 0;
			}
			if (count < 0)
			{
				return 0;
			}
			int num;
			if (count <= 150)
			{
				num = 10;
			}
			else if (count <= 750)
			{
				num = 50;
			}
			else if (count <= 2000)
			{
				num = 100;
			}
			else if (count <= 2070)
			{
				num = 10;
			}
			else
			{
				if (count <= 2100)
				{
					return 0;
				}
				num = 200;
			}
			int num2 = count % num;
			if (num2 != 0)
			{
				return num - num2;
			}
			return 0;
		}

		private static string GetInListRegex(string name, bool byPosition)
		{
			if (!byPosition)
			{
				return "([?@:]" + Regex.Escape(name) + ")(?!\\w)(\\s+(?i)unknown(?-i))?";
			}
			return "(\\?)" + Regex.Escape(name) + "\\?(?!\\w)(\\s+(?i)unknown(?-i))?";
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This method is for internal use only", false)]
		public static void PackListParameters(IDbCommand command, string namePrefix, object value)
		{
			if (FeatureSupport.Get(command.Connection).Arrays)
			{
				IDbDataParameter dbDataParameter = command.CreateParameter();
				dbDataParameter.Value = SanitizeParameterValue(value);
				dbDataParameter.ParameterName = namePrefix;
				command.Parameters.Add(dbDataParameter);
			}
			else
			{
				bool byPosition = ShouldPassByPosition(command.CommandText);
				IEnumerable list = value as IEnumerable;
				int count = 0;
				bool flag = value is IEnumerable<string>;
				bool flag2 = value is IEnumerable<DbString>;
				DbType dbType = DbType.AnsiString;
				int inListStringSplitCount = Settings.InListStringSplitCount;
				bool flag3 = inListStringSplitCount >= 0 && TryStringSplit(ref list, inListStringSplitCount, namePrefix, command, byPosition);
				if (list != null && !flag3)
				{
					object obj = null;
					foreach (object item in list)
					{
						if (++count == 1)
						{
							if (item == null)
							{
								throw new NotSupportedException("The first item in a list-expansion cannot be null");
							}
							if (!flag2)
							{
								dbType = LookupDbType(item.GetType(), "", demand: true, out ITypeHandler _);
							}
						}
						string text = namePrefix + count.ToString();
						if (flag2 && item is DbString)
						{
							(item as DbString).AddParameter(command, text);
						}
						else
						{
							IDbDataParameter dbDataParameter2 = command.CreateParameter();
							dbDataParameter2.ParameterName = text;
							if (flag)
							{
								dbDataParameter2.Size = 4000;
								if (item != null && ((string)item).Length > 4000)
								{
									dbDataParameter2.Size = -1;
								}
							}
							object obj3 = dbDataParameter2.Value = SanitizeParameterValue(item);
							object obj4 = obj3;
							if (obj4 != null && !(obj4 is DBNull))
							{
								obj = obj4;
							}
							if (dbDataParameter2.DbType != dbType)
							{
								dbDataParameter2.DbType = dbType;
							}
							command.Parameters.Add(dbDataParameter2);
						}
					}
					if (Settings.PadListExpansions && !flag2 && obj != null)
					{
						int listPaddingExtraCount = GetListPaddingExtraCount(count);
						for (int i = 0; i < listPaddingExtraCount; i++)
						{
							count++;
							IDbDataParameter dbDataParameter3 = command.CreateParameter();
							dbDataParameter3.ParameterName = namePrefix + count.ToString();
							if (flag)
							{
								dbDataParameter3.Size = 4000;
							}
							dbDataParameter3.DbType = dbType;
							dbDataParameter3.Value = obj;
							command.Parameters.Add(dbDataParameter3);
						}
					}
				}
				if (!flag3)
				{
					string inListRegex = GetInListRegex(namePrefix, byPosition);
					if (count == 0)
					{
						command.CommandText = Regex.Replace(command.CommandText, inListRegex, delegate(Match match)
						{
							string value4 = match.Groups[1].Value;
							if (match.Groups[2].Success)
							{
								return match.Value;
							}
							return "(SELECT " + value4 + " WHERE 1 = 0)";
						}, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant);
						IDbDataParameter dbDataParameter4 = command.CreateParameter();
						dbDataParameter4.ParameterName = namePrefix;
						dbDataParameter4.Value = DBNull.Value;
						command.Parameters.Add(dbDataParameter4);
					}
					else
					{
						command.CommandText = Regex.Replace(command.CommandText, inListRegex, delegate(Match match)
						{
							string value2 = match.Groups[1].Value;
							if (match.Groups[2].Success)
							{
								string value3 = match.Groups[2].Value;
								StringBuilder stringBuilder = GetStringBuilder().Append(value2).Append(1).Append(value3);
								for (int j = 2; j <= count; j++)
								{
									stringBuilder.Append(',').Append(value2).Append(j)
										.Append(value3);
								}
								return stringBuilder.__ToStringRecycle();
							}
							StringBuilder stringBuilder2 = GetStringBuilder().Append('(').Append(value2);
							if (!byPosition)
							{
								stringBuilder2.Append(1);
							}
							for (int k = 2; k <= count; k++)
							{
								stringBuilder2.Append(',').Append(value2);
								if (!byPosition)
								{
									stringBuilder2.Append(k);
								}
							}
							return stringBuilder2.Append(')').__ToStringRecycle();
						}, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant);
					}
				}
			}
		}

		private static bool TryStringSplit(ref IEnumerable list, int splitAt, string namePrefix, IDbCommand command, bool byPosition)
		{
			if (list == null || splitAt < 0)
			{
				return false;
			}
			IEnumerable enumerable = list;
			if (enumerable != null)
			{
				IEnumerable<int> enumerable2;
				if ((enumerable2 = (enumerable as IEnumerable<int>)) != null)
				{
					IEnumerable<int> list2 = enumerable2;
					return TryStringSplit(ref list2, splitAt, namePrefix, command, "int", byPosition, delegate(StringBuilder sb, int i)
					{
						sb.Append(i.ToString(CultureInfo.InvariantCulture));
					});
				}
				IEnumerable<long> enumerable3;
				if ((enumerable3 = (enumerable as IEnumerable<long>)) != null)
				{
					IEnumerable<long> list3 = enumerable3;
					return TryStringSplit(ref list3, splitAt, namePrefix, command, "bigint", byPosition, delegate(StringBuilder sb, long i)
					{
						sb.Append(i.ToString(CultureInfo.InvariantCulture));
					});
				}
				IEnumerable<short> enumerable4;
				if ((enumerable4 = (enumerable as IEnumerable<short>)) != null)
				{
					IEnumerable<short> list4 = enumerable4;
					return TryStringSplit(ref list4, splitAt, namePrefix, command, "smallint", byPosition, delegate(StringBuilder sb, short i)
					{
						sb.Append(i.ToString(CultureInfo.InvariantCulture));
					});
				}
				IEnumerable<byte> enumerable5;
				if ((enumerable5 = (enumerable as IEnumerable<byte>)) != null)
				{
					IEnumerable<byte> list5 = enumerable5;
					return TryStringSplit(ref list5, splitAt, namePrefix, command, "tinyint", byPosition, delegate(StringBuilder sb, byte i)
					{
						sb.Append(i.ToString(CultureInfo.InvariantCulture));
					});
				}
			}
			return false;
		}

		private static bool TryStringSplit<T>(ref IEnumerable<T> list, int splitAt, string namePrefix, IDbCommand command, string colType, bool byPosition, Action<StringBuilder, T> append)
		{
			ICollection<T> collection = list as ICollection<T>;
			if (collection == null)
			{
				collection = (ICollection<T>)(list = list.ToList());
			}
			if (collection.Count < splitAt)
			{
				return false;
			}
			string varName = null;
			string inListRegex = GetInListRegex(namePrefix, byPosition);
			string commandText = Regex.Replace(command.CommandText, inListRegex, delegate(Match match)
			{
				string value2 = match.Groups[1].Value;
				if (match.Groups[2].Success)
				{
					return match.Value;
				}
				varName = value2;
				return "(select cast([value] as " + colType + ") from string_split(" + value2 + ",','))";
			}, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant);
			if (varName == null)
			{
				return false;
			}
			command.CommandText = commandText;
			IDbDataParameter dbDataParameter = command.CreateParameter();
			dbDataParameter.ParameterName = namePrefix;
			dbDataParameter.DbType = DbType.AnsiString;
			dbDataParameter.Size = -1;
			string value;
			using (IEnumerator<T> enumerator = collection.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					StringBuilder stringBuilder = GetStringBuilder();
					append(stringBuilder, enumerator.Current);
					while (enumerator.MoveNext())
					{
						append(stringBuilder.Append(','), enumerator.Current);
					}
					value = stringBuilder.ToString();
				}
				else
				{
					value = "";
				}
			}
			dbDataParameter.Value = value;
			command.Parameters.Add(dbDataParameter);
			return true;
		}

		[Obsolete("This method is for internal use only", false)]
		public static object SanitizeParameterValue(object value)
		{
			if (value == null)
			{
				return DBNull.Value;
			}
			if (value is Enum)
			{
				switch ((!(value is IConvertible)) ? TypeExtensions.GetTypeCode(Enum.GetUnderlyingType(value.GetType())) : ((IConvertible)value).GetTypeCode())
				{
				case TypeCode.Byte:
					return (byte)value;
				case TypeCode.SByte:
					return (sbyte)value;
				case TypeCode.Int16:
					return (short)value;
				case TypeCode.Int32:
					return (int)value;
				case TypeCode.Int64:
					return (long)value;
				case TypeCode.UInt16:
					return (ushort)value;
				case TypeCode.UInt32:
					return (uint)value;
				case TypeCode.UInt64:
					return (ulong)value;
				}
			}
			return value;
		}

		private static IEnumerable<PropertyInfo> FilterParameters(IEnumerable<PropertyInfo> parameters, string sql)
		{
			List<PropertyInfo> list = new List<PropertyInfo>(16);
			foreach (PropertyInfo parameter in parameters)
			{
				if (Regex.IsMatch(sql, "[?@:]" + parameter.Name + "([^\\p{L}\\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
				{
					list.Add(parameter);
				}
			}
			return list;
		}

		public static void ReplaceLiterals(this IParameterLookup parameters, IDbCommand command)
		{
			IList<LiteralToken> list = GetLiteralTokens(command.CommandText);
			if (list.Count != 0)
			{
				ReplaceLiterals(parameters, command, list);
			}
		}

		[Obsolete("This method is for internal use only")]
		public static string Format(object value)
		{
			if (value != null)
			{
				switch (TypeExtensions.GetTypeCode(value.GetType()))
				{
				case TypeCode.DBNull:
					return "null";
				case TypeCode.Boolean:
					if (!(bool)value)
					{
						return "0";
					}
					return "1";
				case TypeCode.Byte:
					return ((byte)value).ToString(CultureInfo.InvariantCulture);
				case TypeCode.SByte:
					return ((sbyte)value).ToString(CultureInfo.InvariantCulture);
				case TypeCode.UInt16:
					return ((ushort)value).ToString(CultureInfo.InvariantCulture);
				case TypeCode.Int16:
					return ((short)value).ToString(CultureInfo.InvariantCulture);
				case TypeCode.UInt32:
					return ((uint)value).ToString(CultureInfo.InvariantCulture);
				case TypeCode.Int32:
					return ((int)value).ToString(CultureInfo.InvariantCulture);
				case TypeCode.UInt64:
					return ((ulong)value).ToString(CultureInfo.InvariantCulture);
				case TypeCode.Int64:
					return ((long)value).ToString(CultureInfo.InvariantCulture);
				case TypeCode.Single:
					return ((float)value).ToString(CultureInfo.InvariantCulture);
				case TypeCode.Double:
					return ((double)value).ToString(CultureInfo.InvariantCulture);
				case TypeCode.Decimal:
					return ((decimal)value).ToString(CultureInfo.InvariantCulture);
				default:
				{
					IEnumerable multiExec = GetMultiExec(value);
					if (multiExec != null)
					{
						StringBuilder stringBuilder = null;
						bool flag = true;
						foreach (object item in multiExec)
						{
							if (flag)
							{
								stringBuilder = GetStringBuilder().Append('(');
								flag = false;
							}
							else
							{
								stringBuilder.Append(',');
							}
							stringBuilder.Append(Format(item));
						}
						if (flag)
						{
							return "(select null where 1=0)";
						}
						return stringBuilder.Append(')').__ToStringRecycle();
					}
					throw new NotSupportedException(value.GetType().Name);
				}
				}
			}
			return "null";
		}

		internal static void ReplaceLiterals(IParameterLookup parameters, IDbCommand command, IList<LiteralToken> tokens)
		{
			string text = command.CommandText;
			foreach (LiteralToken token in tokens)
			{
				string newValue = Format(parameters[token.Member]);
				text = text.Replace(token.Token, newValue);
			}
			command.CommandText = text;
		}

		internal static IList<LiteralToken> GetLiteralTokens(string sql)
		{
			if (string.IsNullOrEmpty(sql))
			{
				return LiteralToken.None;
			}
			if (!literalTokens.IsMatch(sql))
			{
				return LiteralToken.None;
			}
			MatchCollection matchCollection = literalTokens.Matches(sql);
			HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
			List<LiteralToken> list = new List<LiteralToken>(matchCollection.Count);
			foreach (Match item in matchCollection)
			{
				string value = item.Value;
				if (hashSet.Add(item.Value))
				{
					list.Add(new LiteralToken(value, item.Groups[1].Value));
				}
			}
			if (list.Count != 0)
			{
				return list;
			}
			return LiteralToken.None;
		}

		public static Action<IDbCommand, object> CreateParamInfoGenerator(Identity identity, bool checkForDuplicates, bool removeUnused)
		{
			return CreateParamInfoGenerator(identity, checkForDuplicates, removeUnused, GetLiteralTokens(identity.sql));
		}

		private static bool IsValueTuple(Type type)
		{
			if ((object)type != null && type.IsValueType())
			{
				return type.FullName.StartsWith("System.ValueTuple`", StringComparison.Ordinal);
			}
			return false;
		}

		private static List<IMemberMap> GetValueTupleMembers(Type type, string[] names)
		{
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
			List<IMemberMap> list = new List<IMemberMap>(names.Length);
			for (int i = 0; i < names.Length; i++)
			{
				FieldInfo fieldInfo = null;
				string text = "Item" + (i + 1).ToString(CultureInfo.InvariantCulture);
				FieldInfo[] array = fields;
				foreach (FieldInfo fieldInfo2 in array)
				{
					if (fieldInfo2.Name == text)
					{
						fieldInfo = fieldInfo2;
						break;
					}
				}
				list.Add((fieldInfo == null) ? null : new SimpleMemberMap(string.IsNullOrWhiteSpace(names[i]) ? text : names[i], fieldInfo));
			}
			return list;
		}

		internal static Action<IDbCommand, object> CreateParamInfoGenerator(Identity identity, bool checkForDuplicates, bool removeUnused, IList<LiteralToken> literals)
		{
			Type parametersType = identity.parametersType;
			if (IsValueTuple(parametersType))
			{
				throw new NotSupportedException("ValueTuple should not be used for parameters - the language-level names are not available to use as parameter names, and it adds unnecessary boxing");
			}
			bool flag = false;
			if (removeUnused && identity.commandType.GetValueOrDefault(CommandType.Text) == CommandType.Text)
			{
				flag = !smellsLikeOleDb.IsMatch(identity.sql);
			}
			DynamicMethod dynamicMethod = new DynamicMethod("ParamInfo" + Guid.NewGuid().ToString(), null, new Type[2]
			{
				typeof(IDbCommand),
				typeof(object)
			}, parametersType, skipVisibility: true);
			ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
			bool flag2 = parametersType.IsValueType();
			bool flag3 = false;
			iLGenerator.Emit(OpCodes.Ldarg_1);
			if (flag2)
			{
				iLGenerator.DeclareLocal(parametersType.MakePointerType());
				iLGenerator.Emit(OpCodes.Unbox, parametersType);
			}
			else
			{
				iLGenerator.DeclareLocal(parametersType);
				iLGenerator.Emit(OpCodes.Castclass, parametersType);
			}
			iLGenerator.Emit(OpCodes.Stloc_0);
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.EmitCall(OpCodes.Callvirt, typeof(IDbCommand).GetProperty("Parameters").GetGetMethod(), null);
			PropertyInfo[] properties = parametersType.GetProperties();
			List<PropertyInfo> list = new List<PropertyInfo>(properties.Length);
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (propertyInfo.GetIndexParameters().Length == 0)
				{
					list.Add(propertyInfo);
				}
			}
			ConstructorInfo[] constructors = parametersType.GetConstructors();
			IEnumerable<PropertyInfo> enumerable = null;
			ParameterInfo[] parameters;
			if (constructors.Length == 1 && list.Count == (parameters = constructors[0].GetParameters()).Length)
			{
				bool flag4 = true;
				for (int j = 0; j < list.Count; j++)
				{
					if (!string.Equals(list[j].Name, parameters[j].Name, StringComparison.OrdinalIgnoreCase))
					{
						flag4 = false;
						break;
					}
				}
				if (flag4)
				{
					enumerable = list;
				}
				else
				{
					Dictionary<string, int> dictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
					ParameterInfo[] array = parameters;
					foreach (ParameterInfo parameterInfo in array)
					{
						dictionary[parameterInfo.Name] = parameterInfo.Position;
					}
					if (dictionary.Count == list.Count)
					{
						int[] array2 = new int[list.Count];
						flag4 = true;
						for (int l = 0; l < list.Count; l++)
						{
							if (!dictionary.TryGetValue(list[l].Name, out int value))
							{
								flag4 = false;
								break;
							}
							array2[l] = value;
						}
						if (flag4)
						{
							enumerable = list.ToArray();
							Array.Sort(array2, (PropertyInfo[])enumerable);
						}
					}
				}
			}
			if (enumerable == null)
			{
				list.Sort(new PropertyInfoByNameComparer());
				enumerable = list;
			}
			if (flag)
			{
				enumerable = FilterParameters(enumerable, identity.sql);
			}
			OpCode opcode = flag2 ? OpCodes.Call : OpCodes.Callvirt;
			foreach (PropertyInfo item in enumerable)
			{
				if (typeof(ICustomQueryParameter).IsAssignableFrom(item.PropertyType))
				{
					iLGenerator.Emit(OpCodes.Ldloc_0);
					iLGenerator.Emit(opcode, item.GetGetMethod());
					iLGenerator.Emit(OpCodes.Ldarg_0);
					iLGenerator.Emit(OpCodes.Ldstr, item.Name);
					iLGenerator.EmitCall(OpCodes.Callvirt, item.PropertyType.GetMethod("AddParameter"), null);
				}
				else
				{
					ITypeHandler handler;
					DbType dbType = LookupDbType(item.PropertyType, item.Name, demand: true, out handler);
					if (dbType == (DbType)(-1))
					{
						iLGenerator.Emit(OpCodes.Ldarg_0);
						iLGenerator.Emit(OpCodes.Ldstr, item.Name);
						iLGenerator.Emit(OpCodes.Ldloc_0);
						iLGenerator.Emit(opcode, item.GetGetMethod());
						if (item.PropertyType.IsValueType())
						{
							iLGenerator.Emit(OpCodes.Box, item.PropertyType);
						}
						iLGenerator.EmitCall(OpCodes.Call, typeof(SqlMapper).GetMethod("PackListParameters"), null);
					}
					else
					{
						iLGenerator.Emit(OpCodes.Dup);
						iLGenerator.Emit(OpCodes.Ldarg_0);
						if (checkForDuplicates)
						{
							iLGenerator.Emit(OpCodes.Ldstr, item.Name);
							iLGenerator.EmitCall(OpCodes.Call, typeof(SqlMapper).GetMethod("FindOrAddParameter"), null);
						}
						else
						{
							iLGenerator.EmitCall(OpCodes.Callvirt, typeof(IDbCommand).GetMethod("CreateParameter"), null);
							iLGenerator.Emit(OpCodes.Dup);
							iLGenerator.Emit(OpCodes.Ldstr, item.Name);
							iLGenerator.EmitCall(OpCodes.Callvirt, typeof(IDataParameter).GetProperty("ParameterName").GetSetMethod(), null);
						}
						if (dbType != DbType.Time && handler == null)
						{
							iLGenerator.Emit(OpCodes.Dup);
							if (dbType == DbType.Object && item.PropertyType == typeof(object))
							{
								iLGenerator.Emit(OpCodes.Ldloc_0);
								iLGenerator.Emit(opcode, item.GetGetMethod());
								iLGenerator.Emit(OpCodes.Call, typeof(SqlMapper).GetMethod("GetDbType", BindingFlags.Static | BindingFlags.Public));
							}
							else
							{
								EmitInt32(iLGenerator, (int)dbType);
							}
							iLGenerator.EmitCall(OpCodes.Callvirt, typeof(IDataParameter).GetProperty("DbType").GetSetMethod(), null);
						}
						iLGenerator.Emit(OpCodes.Dup);
						EmitInt32(iLGenerator, 1);
						iLGenerator.EmitCall(OpCodes.Callvirt, typeof(IDataParameter).GetProperty("Direction").GetSetMethod(), null);
						iLGenerator.Emit(OpCodes.Dup);
						iLGenerator.Emit(OpCodes.Ldloc_0);
						iLGenerator.Emit(opcode, item.GetGetMethod());
						bool flag6;
						if (item.PropertyType.IsValueType())
						{
							Type type = item.PropertyType;
							Type underlyingType = Nullable.GetUnderlyingType(type);
							bool flag5 = false;
							if ((underlyingType ?? type).IsEnum())
							{
								if (underlyingType != null)
								{
									flag5 = (flag6 = true);
								}
								else
								{
									flag6 = false;
									switch (TypeExtensions.GetTypeCode(Enum.GetUnderlyingType(type)))
									{
									case TypeCode.Byte:
										type = typeof(byte);
										break;
									case TypeCode.SByte:
										type = typeof(sbyte);
										break;
									case TypeCode.Int16:
										type = typeof(short);
										break;
									case TypeCode.Int32:
										type = typeof(int);
										break;
									case TypeCode.Int64:
										type = typeof(long);
										break;
									case TypeCode.UInt16:
										type = typeof(ushort);
										break;
									case TypeCode.UInt32:
										type = typeof(uint);
										break;
									case TypeCode.UInt64:
										type = typeof(ulong);
										break;
									}
								}
							}
							else
							{
								flag6 = (underlyingType != null);
							}
							iLGenerator.Emit(OpCodes.Box, type);
							if (flag5)
							{
								flag6 = false;
								iLGenerator.EmitCall(OpCodes.Call, typeof(SqlMapper).GetMethod("SanitizeParameterValue"), null);
							}
						}
						else
						{
							flag6 = true;
						}
						if (flag6)
						{
							if ((dbType == DbType.String || dbType == DbType.AnsiString) && !flag3)
							{
								iLGenerator.DeclareLocal(typeof(int));
								flag3 = true;
							}
							iLGenerator.Emit(OpCodes.Dup);
							Label label = iLGenerator.DefineLabel();
							Label? label2 = (dbType == DbType.String || dbType == DbType.AnsiString) ? new Label?(iLGenerator.DefineLabel()) : null;
							iLGenerator.Emit(OpCodes.Brtrue_S, label);
							iLGenerator.Emit(OpCodes.Pop);
							iLGenerator.Emit(OpCodes.Ldsfld, typeof(DBNull).GetField("Value"));
							if (dbType == DbType.String || dbType == DbType.AnsiString)
							{
								EmitInt32(iLGenerator, 0);
								iLGenerator.Emit(OpCodes.Stloc_1);
							}
							if (label2.HasValue)
							{
								iLGenerator.Emit(OpCodes.Br_S, label2.Value);
							}
							iLGenerator.MarkLabel(label);
							if (item.PropertyType == typeof(string))
							{
								iLGenerator.Emit(OpCodes.Dup);
								iLGenerator.EmitCall(OpCodes.Callvirt, typeof(string).GetProperty("Length").GetGetMethod(), null);
								EmitInt32(iLGenerator, 4000);
								iLGenerator.Emit(OpCodes.Cgt);
								Label label3 = iLGenerator.DefineLabel();
								Label label4 = iLGenerator.DefineLabel();
								iLGenerator.Emit(OpCodes.Brtrue_S, label3);
								EmitInt32(iLGenerator, 4000);
								iLGenerator.Emit(OpCodes.Br_S, label4);
								iLGenerator.MarkLabel(label3);
								EmitInt32(iLGenerator, -1);
								iLGenerator.MarkLabel(label4);
								iLGenerator.Emit(OpCodes.Stloc_1);
							}
							if (item.PropertyType.FullName == "System.Data.Linq.Binary")
							{
								iLGenerator.EmitCall(OpCodes.Callvirt, item.PropertyType.GetMethod("ToArray", BindingFlags.Instance | BindingFlags.Public), null);
							}
							if (label2.HasValue)
							{
								iLGenerator.MarkLabel(label2.Value);
							}
						}
						if (handler != null)
						{
							iLGenerator.Emit(OpCodes.Call, typeof(TypeHandlerCache<>).MakeGenericType(item.PropertyType).GetMethod("SetValue"));
						}
						else
						{
							iLGenerator.EmitCall(OpCodes.Callvirt, typeof(IDataParameter).GetProperty("Value").GetSetMethod(), null);
						}
						if (item.PropertyType == typeof(string))
						{
							Label label5 = iLGenerator.DefineLabel();
							iLGenerator.Emit(OpCodes.Ldloc_1);
							iLGenerator.Emit(OpCodes.Brfalse_S, label5);
							iLGenerator.Emit(OpCodes.Dup);
							iLGenerator.Emit(OpCodes.Ldloc_1);
							iLGenerator.EmitCall(OpCodes.Callvirt, typeof(IDbDataParameter).GetProperty("Size").GetSetMethod(), null);
							iLGenerator.MarkLabel(label5);
						}
						if (checkForDuplicates)
						{
							iLGenerator.Emit(OpCodes.Pop);
						}
						else
						{
							iLGenerator.EmitCall(OpCodes.Callvirt, typeof(IList).GetMethod("Add"), null);
							iLGenerator.Emit(OpCodes.Pop);
						}
					}
				}
			}
			iLGenerator.Emit(OpCodes.Pop);
			if (literals.Count != 0 && list != null)
			{
				iLGenerator.Emit(OpCodes.Ldarg_0);
				iLGenerator.Emit(OpCodes.Ldarg_0);
				PropertyInfo property = typeof(IDbCommand).GetProperty("CommandText");
				iLGenerator.EmitCall(OpCodes.Callvirt, property.GetGetMethod(), null);
				Dictionary<Type, LocalBuilder> dictionary2 = null;
				LocalBuilder value2 = null;
				foreach (LiteralToken literal in literals)
				{
					PropertyInfo propertyInfo2 = null;
					PropertyInfo propertyInfo3 = null;
					string member = literal.Member;
					for (int m = 0; m < list.Count; m++)
					{
						string name = list[m].Name;
						if (string.Equals(name, member, StringComparison.OrdinalIgnoreCase))
						{
							propertyInfo3 = list[m];
							if (string.Equals(name, member, StringComparison.Ordinal))
							{
								propertyInfo2 = propertyInfo3;
								break;
							}
						}
					}
					PropertyInfo propertyInfo4 = propertyInfo2 ?? propertyInfo3;
					if (propertyInfo4 != null)
					{
						iLGenerator.Emit(OpCodes.Ldstr, literal.Token);
						iLGenerator.Emit(OpCodes.Ldloc_0);
						iLGenerator.EmitCall(opcode, propertyInfo4.GetGetMethod(), null);
						Type propertyType = propertyInfo4.PropertyType;
						TypeCode typeCode = TypeExtensions.GetTypeCode(propertyType);
						if (typeCode != TypeCode.Boolean)
						{
							if ((uint)(typeCode - 5) <= 10u)
							{
								MethodInfo toString = GetToString(typeCode);
								if (value2 == null || value2.LocalType != propertyType)
								{
									if (dictionary2 == null)
									{
										dictionary2 = new Dictionary<Type, LocalBuilder>();
										value2 = null;
									}
									else if (!dictionary2.TryGetValue(propertyType, out value2))
									{
										value2 = null;
									}
									if (value2 == null)
									{
										value2 = iLGenerator.DeclareLocal(propertyType);
										dictionary2.Add(propertyType, value2);
									}
								}
								iLGenerator.Emit(OpCodes.Stloc, value2);
								iLGenerator.Emit(OpCodes.Ldloca, value2);
								iLGenerator.EmitCall(OpCodes.Call, InvariantCulture, null);
								iLGenerator.EmitCall(OpCodes.Call, toString, null);
							}
							else
							{
								if (propertyType.IsValueType())
								{
									iLGenerator.Emit(OpCodes.Box, propertyType);
								}
								iLGenerator.EmitCall(OpCodes.Call, format, null);
							}
						}
						else
						{
							Label label6 = iLGenerator.DefineLabel();
							Label label7 = iLGenerator.DefineLabel();
							iLGenerator.Emit(OpCodes.Brtrue_S, label6);
							iLGenerator.Emit(OpCodes.Ldstr, "0");
							iLGenerator.Emit(OpCodes.Br_S, label7);
							iLGenerator.MarkLabel(label6);
							iLGenerator.Emit(OpCodes.Ldstr, "1");
							iLGenerator.MarkLabel(label7);
						}
						iLGenerator.EmitCall(OpCodes.Callvirt, StringReplace, null);
					}
				}
				iLGenerator.EmitCall(OpCodes.Callvirt, property.GetSetMethod(), null);
			}
			iLGenerator.Emit(OpCodes.Ret);
			return (Action<IDbCommand, object>)dynamicMethod.CreateDelegate(typeof(Action<IDbCommand, object>));
		}

		private static MethodInfo GetToString(TypeCode typeCode)
		{
			if (!toStrings.TryGetValue(typeCode, out MethodInfo value))
			{
				return null;
			}
			return value;
		}

		private static int ExecuteCommand(IDbConnection cnn, ref CommandDefinition command, Action<IDbCommand, object> paramReader)
		{
			IDbCommand dbCommand = null;
			bool flag = cnn.State == ConnectionState.Closed;
			try
			{
				dbCommand = command.SetupCommand(cnn, paramReader);
				if (flag)
				{
					cnn.Open();
				}
				int result = dbCommand.ExecuteNonQuery();
				command.OnCompleted();
				return result;
			}
			finally
			{
				if (flag)
				{
					cnn.Close();
				}
				dbCommand?.Dispose();
			}
		}

		private static T ExecuteScalarImpl<T>(IDbConnection cnn, ref CommandDefinition command)
		{
			Action<IDbCommand, object> paramReader = null;
			object parameters = command.Parameters;
			if (parameters != null)
			{
				paramReader = GetCacheInfo(new Identity(command.CommandText, command.CommandType, cnn, null, parameters.GetType(), null), command.Parameters, command.AddToCache).ParamReader;
			}
			IDbCommand dbCommand = null;
			bool flag = cnn.State == ConnectionState.Closed;
			object value;
			try
			{
				dbCommand = command.SetupCommand(cnn, paramReader);
				if (flag)
				{
					cnn.Open();
				}
				value = dbCommand.ExecuteScalar();
				command.OnCompleted();
			}
			finally
			{
				if (flag)
				{
					cnn.Close();
				}
				dbCommand?.Dispose();
			}
			return Parse<T>(value);
		}

		private static IDataReader ExecuteReaderImpl(IDbConnection cnn, ref CommandDefinition command, CommandBehavior commandBehavior, out IDbCommand cmd)
		{
			Action<IDbCommand, object> parameterReader = GetParameterReader(cnn, ref command);
			cmd = null;
			bool flag = cnn.State == ConnectionState.Closed;
			bool flag2 = true;
			try
			{
				cmd = command.SetupCommand(cnn, parameterReader);
				if (flag)
				{
					cnn.Open();
				}
				IDataReader result = ExecuteReaderWithFlagsFallback(cmd, flag, commandBehavior);
				flag = false;
				flag2 = false;
				return result;
			}
			finally
			{
				if (flag)
				{
					cnn.Close();
				}
				if (cmd != null && flag2)
				{
					cmd.Dispose();
				}
			}
		}

		private static Action<IDbCommand, object> GetParameterReader(IDbConnection cnn, ref CommandDefinition command)
		{
			object parameters = command.Parameters;
			IEnumerable multiExec = GetMultiExec(parameters);
			CacheInfo cacheInfo = null;
			if (multiExec != null)
			{
				throw new NotSupportedException("MultiExec is not supported by ExecuteReader");
			}
			if (parameters != null)
			{
				cacheInfo = GetCacheInfo(new Identity(command.CommandText, command.CommandType, cnn, null, parameters.GetType(), null), parameters, command.AddToCache);
			}
			return cacheInfo?.ParamReader;
		}

		private static Func<IDataReader, object> GetStructDeserializer(Type type, Type effectiveType, int index)
		{
			if (type == typeof(char))
			{
				return (IDataReader r) => ReadChar(r.GetValue(index));
			}
			if (type == typeof(char?))
			{
				return (IDataReader r) => ReadNullableChar(r.GetValue(index));
			}
			if (type.FullName == "System.Data.Linq.Binary")
			{
				return (IDataReader r) => Activator.CreateInstance(type, r.GetValue(index));
			}
			if (effectiveType.IsEnum())
			{
				return delegate(IDataReader r)
				{
					object obj = r.GetValue(index);
					if (obj is float || obj is double || obj is decimal)
					{
						obj = Convert.ChangeType(obj, Enum.GetUnderlyingType(effectiveType), CultureInfo.InvariantCulture);
					}
					if (!(obj is DBNull))
					{
						return Enum.ToObject(effectiveType, obj);
					}
					return null;
				};
			}
			if (typeHandlers.TryGetValue(type, out ITypeHandler handler))
			{
				return delegate(IDataReader r)
				{
					object value2 = r.GetValue(index);
					if (!(value2 is DBNull))
					{
						return handler.Parse(type, value2);
					}
					return null;
				};
			}
			return delegate(IDataReader r)
			{
				object value = r.GetValue(index);
				if (!(value is DBNull))
				{
					return value;
				}
				return null;
			};
		}

		private static T Parse<T>(object value)
		{
			if (value == null || value is DBNull)
			{
				return default(T);
			}
			if (value is T)
			{
				return (T)value;
			}
			Type typeFromHandle = typeof(T);
			typeFromHandle = (Nullable.GetUnderlyingType(typeFromHandle) ?? typeFromHandle);
			if (typeFromHandle.IsEnum())
			{
				if (value is float || value is double || value is decimal)
				{
					value = Convert.ChangeType(value, Enum.GetUnderlyingType(typeFromHandle), CultureInfo.InvariantCulture);
				}
				return (T)Enum.ToObject(typeFromHandle, value);
			}
			if (typeHandlers.TryGetValue(typeFromHandle, out ITypeHandler value2))
			{
				return (T)value2.Parse(typeFromHandle, value);
			}
			return (T)Convert.ChangeType(value, typeFromHandle, CultureInfo.InvariantCulture);
		}

		public static ITypeMap GetTypeMap(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			ITypeMap typeMap = (ITypeMap)_typeMaps[type];
			if (typeMap == null)
			{
				lock (_typeMaps)
				{
					typeMap = (ITypeMap)_typeMaps[type];
					if (typeMap != null)
					{
						return typeMap;
					}
					typeMap = TypeMapProvider(type);
					_typeMaps[type] = typeMap;
					return typeMap;
				}
			}
			return typeMap;
		}

		public static void SetTypeMap(Type type, ITypeMap map)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (map == null || map is DefaultTypeMap)
			{
				lock (_typeMaps)
				{
					_typeMaps.Remove(type);
				}
			}
			else
			{
				lock (_typeMaps)
				{
					_typeMaps[type] = map;
				}
			}
			PurgeQueryCacheByType(type);
		}

		public static Func<IDataReader, object> GetTypeDeserializer(Type type, IDataReader reader, int startBound = 0, int length = -1, bool returnNullIfFirstMissing = false)
		{
			return TypeDeserializerCache.GetReader(type, reader, startBound, length, returnNullIfFirstMissing);
		}

		private static LocalBuilder GetTempLocal(ILGenerator il, ref Dictionary<Type, LocalBuilder> locals, Type type, bool initAndLoad)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			locals = (locals ?? new Dictionary<Type, LocalBuilder>());
			if (!locals.TryGetValue(type, out LocalBuilder value))
			{
				value = il.DeclareLocal(type);
				locals.Add(type, value);
			}
			if (initAndLoad)
			{
				il.Emit(OpCodes.Ldloca, (short)value.LocalIndex);
				il.Emit(OpCodes.Initobj, type);
				il.Emit(OpCodes.Ldloca, (short)value.LocalIndex);
				il.Emit(OpCodes.Ldobj, type);
			}
			return value;
		}

		private static Func<IDataReader, object> GetTypeDeserializerImpl(Type type, IDataReader reader, int startBound = 0, int length = -1, bool returnNullIfFirstMissing = false)
		{
			Type type2 = type.IsValueType() ? typeof(object) : type;
			DynamicMethod dynamicMethod = new DynamicMethod("Deserialize" + Guid.NewGuid().ToString(), type2, new Type[1]
			{
				typeof(IDataReader)
			}, type, skipVisibility: true);
			ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
			iLGenerator.DeclareLocal(typeof(int));
			iLGenerator.DeclareLocal(type);
			iLGenerator.Emit(OpCodes.Ldc_I4_0);
			iLGenerator.Emit(OpCodes.Stloc_0);
			if (length == -1)
			{
				length = reader.FieldCount - startBound;
			}
			if (reader.FieldCount <= startBound)
			{
				throw MultiMapException(reader);
			}
			string[] names = (from i in Enumerable.Range(startBound, length)
			select reader.GetName(i)).ToArray();
			ITypeMap typeMap = GetTypeMap(type);
			int num = startBound;
			ConstructorInfo specializedConstructor = null;
			bool flag = false;
			Dictionary<Type, LocalBuilder> locals = null;
			if (type.IsValueType())
			{
				iLGenerator.Emit(OpCodes.Ldloca_S, (byte)1);
				iLGenerator.Emit(OpCodes.Initobj, type);
			}
			else
			{
				Type[] array = new Type[length];
				for (int j = startBound; j < startBound + length; j++)
				{
					array[j - startBound] = reader.GetFieldType(j);
				}
				ConstructorInfo constructorInfo = typeMap.FindExplicitConstructor();
				if (constructorInfo != null)
				{
					ParameterInfo[] parameters = constructorInfo.GetParameters();
					foreach (ParameterInfo parameterInfo in parameters)
					{
						if (!parameterInfo.ParameterType.IsValueType())
						{
							iLGenerator.Emit(OpCodes.Ldnull);
						}
						else
						{
							GetTempLocal(iLGenerator, ref locals, parameterInfo.ParameterType, initAndLoad: true);
						}
					}
					iLGenerator.Emit(OpCodes.Newobj, constructorInfo);
					iLGenerator.Emit(OpCodes.Stloc_1);
					flag = typeof(ISupportInitialize).IsAssignableFrom(type);
					if (flag)
					{
						iLGenerator.Emit(OpCodes.Ldloc_1);
						iLGenerator.EmitCall(OpCodes.Callvirt, typeof(ISupportInitialize).GetMethod("BeginInit"), null);
					}
				}
				else
				{
					ConstructorInfo constructorInfo2 = typeMap.FindConstructor(names, array);
					if (constructorInfo2 == null)
					{
						string arg = "(" + string.Join(", ", array.Select((Type t, int i) => t.FullName + " " + names[i]).ToArray()) + ")";
						throw new InvalidOperationException($"A parameterless default constructor or one matching signature {arg} is required for {type.FullName} materialization");
					}
					if (constructorInfo2.GetParameters().Length == 0)
					{
						iLGenerator.Emit(OpCodes.Newobj, constructorInfo2);
						iLGenerator.Emit(OpCodes.Stloc_1);
						flag = typeof(ISupportInitialize).IsAssignableFrom(type);
						if (flag)
						{
							iLGenerator.Emit(OpCodes.Ldloc_1);
							iLGenerator.EmitCall(OpCodes.Callvirt, typeof(ISupportInitialize).GetMethod("BeginInit"), null);
						}
					}
					else
					{
						specializedConstructor = constructorInfo2;
					}
				}
			}
			iLGenerator.BeginExceptionBlock();
			if (type.IsValueType())
			{
				iLGenerator.Emit(OpCodes.Ldloca_S, (byte)1);
			}
			else if (specializedConstructor == null)
			{
				iLGenerator.Emit(OpCodes.Ldloc_1);
			}
			List<IMemberMap> obj = IsValueTuple(type) ? GetValueTupleMembers(type, names) : ((specializedConstructor != null) ? (from n in names
			select typeMap.GetConstructorParameter(specializedConstructor, n)) : names.Select((string n) => typeMap.GetMember(n))).ToList();
			bool flag2 = true;
			Label label = iLGenerator.DefineLabel();
			int num2 = -1;
			int localIndex = iLGenerator.DeclareLocal(typeof(object)).LocalIndex;
			bool applyNullValues = Settings.ApplyNullValues;
			foreach (IMemberMap item in obj)
			{
				if (item != null)
				{
					if (specializedConstructor == null)
					{
						iLGenerator.Emit(OpCodes.Dup);
					}
					Label label2 = iLGenerator.DefineLabel();
					Label label3 = iLGenerator.DefineLabel();
					iLGenerator.Emit(OpCodes.Ldarg_0);
					EmitInt32(iLGenerator, num);
					iLGenerator.Emit(OpCodes.Dup);
					iLGenerator.Emit(OpCodes.Stloc_0);
					iLGenerator.Emit(OpCodes.Callvirt, getItem);
					iLGenerator.Emit(OpCodes.Dup);
					StoreLocal(iLGenerator, localIndex);
					Type fieldType = reader.GetFieldType(num);
					Type memberType = item.MemberType;
					if (memberType == typeof(char) || memberType == typeof(char?))
					{
						iLGenerator.EmitCall(OpCodes.Call, typeof(SqlMapper).GetMethod((memberType == typeof(char)) ? "ReadChar" : "ReadNullableChar", BindingFlags.Static | BindingFlags.Public), null);
					}
					else
					{
						iLGenerator.Emit(OpCodes.Dup);
						iLGenerator.Emit(OpCodes.Isinst, typeof(DBNull));
						iLGenerator.Emit(OpCodes.Brtrue_S, label2);
						Type underlyingType = Nullable.GetUnderlyingType(memberType);
						Type type3 = ((object)underlyingType != null && underlyingType.IsEnum()) ? underlyingType : memberType;
						if (type3.IsEnum())
						{
							Type underlyingType2 = Enum.GetUnderlyingType(type3);
							if (fieldType == typeof(string))
							{
								if (num2 == -1)
								{
									num2 = iLGenerator.DeclareLocal(typeof(string)).LocalIndex;
								}
								iLGenerator.Emit(OpCodes.Castclass, typeof(string));
								StoreLocal(iLGenerator, num2);
								iLGenerator.Emit(OpCodes.Ldtoken, type3);
								iLGenerator.EmitCall(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"), null);
								LoadLocal(iLGenerator, num2);
								iLGenerator.Emit(OpCodes.Ldc_I4_1);
								iLGenerator.EmitCall(OpCodes.Call, enumParse, null);
								iLGenerator.Emit(OpCodes.Unbox_Any, type3);
							}
							else
							{
								FlexibleConvertBoxedFromHeadOfStack(iLGenerator, fieldType, type3, underlyingType2);
							}
							if (underlyingType != null)
							{
								iLGenerator.Emit(OpCodes.Newobj, memberType.GetConstructor(new Type[1]
								{
									underlyingType
								}));
							}
						}
						else if (memberType.FullName == "System.Data.Linq.Binary")
						{
							iLGenerator.Emit(OpCodes.Unbox_Any, typeof(byte[]));
							iLGenerator.Emit(OpCodes.Newobj, memberType.GetConstructor(new Type[1]
							{
								typeof(byte[])
							}));
						}
						else
						{
							TypeCode typeCode = TypeExtensions.GetTypeCode(fieldType);
							TypeCode typeCode2 = TypeExtensions.GetTypeCode(type3);
							bool flag3;
							if ((flag3 = typeHandlers.ContainsKey(type3)) || fieldType == type3 || typeCode == typeCode2 || typeCode == TypeExtensions.GetTypeCode(underlyingType))
							{
								if (flag3)
								{
									iLGenerator.EmitCall(OpCodes.Call, typeof(TypeHandlerCache<>).MakeGenericType(type3).GetMethod("Parse"), null);
								}
								else
								{
									iLGenerator.Emit(OpCodes.Unbox_Any, type3);
								}
							}
							else
							{
								FlexibleConvertBoxedFromHeadOfStack(iLGenerator, fieldType, underlyingType ?? type3, null);
								if (underlyingType != null)
								{
									iLGenerator.Emit(OpCodes.Newobj, type3.GetConstructor(new Type[1]
									{
										underlyingType
									}));
								}
							}
						}
					}
					if (specializedConstructor == null)
					{
						if (item.Property != null)
						{
							iLGenerator.Emit(type.IsValueType() ? OpCodes.Call : OpCodes.Callvirt, DefaultTypeMap.GetPropertySetter(item.Property, type));
						}
						else
						{
							iLGenerator.Emit(OpCodes.Stfld, item.Field);
						}
					}
					iLGenerator.Emit(OpCodes.Br_S, label3);
					iLGenerator.MarkLabel(label2);
					if (specializedConstructor != null)
					{
						iLGenerator.Emit(OpCodes.Pop);
						if (item.MemberType.IsValueType())
						{
							int localIndex2 = iLGenerator.DeclareLocal(item.MemberType).LocalIndex;
							LoadLocalAddress(iLGenerator, localIndex2);
							iLGenerator.Emit(OpCodes.Initobj, item.MemberType);
							LoadLocal(iLGenerator, localIndex2);
						}
						else
						{
							iLGenerator.Emit(OpCodes.Ldnull);
						}
					}
					else if (applyNullValues && (!memberType.IsValueType() || Nullable.GetUnderlyingType(memberType) != null))
					{
						iLGenerator.Emit(OpCodes.Pop);
						if (memberType.IsValueType())
						{
							GetTempLocal(iLGenerator, ref locals, memberType, initAndLoad: true);
						}
						else
						{
							iLGenerator.Emit(OpCodes.Ldnull);
						}
						if (item.Property != null)
						{
							iLGenerator.Emit(type.IsValueType() ? OpCodes.Call : OpCodes.Callvirt, DefaultTypeMap.GetPropertySetter(item.Property, type));
						}
						else
						{
							iLGenerator.Emit(OpCodes.Stfld, item.Field);
						}
					}
					else
					{
						iLGenerator.Emit(OpCodes.Pop);
						iLGenerator.Emit(OpCodes.Pop);
					}
					if (flag2 && returnNullIfFirstMissing)
					{
						iLGenerator.Emit(OpCodes.Pop);
						iLGenerator.Emit(OpCodes.Ldnull);
						iLGenerator.Emit(OpCodes.Stloc_1);
						iLGenerator.Emit(OpCodes.Br, label);
					}
					iLGenerator.MarkLabel(label3);
				}
				flag2 = false;
				num++;
			}
			if (type.IsValueType())
			{
				iLGenerator.Emit(OpCodes.Pop);
			}
			else
			{
				if (specializedConstructor != null)
				{
					iLGenerator.Emit(OpCodes.Newobj, specializedConstructor);
				}
				iLGenerator.Emit(OpCodes.Stloc_1);
				if (flag)
				{
					iLGenerator.Emit(OpCodes.Ldloc_1);
					iLGenerator.EmitCall(OpCodes.Callvirt, typeof(ISupportInitialize).GetMethod("EndInit"), null);
				}
			}
			iLGenerator.MarkLabel(label);
			iLGenerator.BeginCatchBlock(typeof(Exception));
			iLGenerator.Emit(OpCodes.Ldloc_0);
			iLGenerator.Emit(OpCodes.Ldarg_0);
			LoadLocal(iLGenerator, localIndex);
			iLGenerator.EmitCall(OpCodes.Call, typeof(SqlMapper).GetMethod("ThrowDataException"), null);
			iLGenerator.EndExceptionBlock();
			iLGenerator.Emit(OpCodes.Ldloc_1);
			if (type.IsValueType())
			{
				iLGenerator.Emit(OpCodes.Box, type);
			}
			iLGenerator.Emit(OpCodes.Ret);
			Type funcType = Expression.GetFuncType(typeof(IDataReader), type2);
			return (Func<IDataReader, object>)dynamicMethod.CreateDelegate(funcType);
		}

		private static void FlexibleConvertBoxedFromHeadOfStack(ILGenerator il, Type from, Type to, Type via)
		{
			MethodInfo @operator;
			if (from == (via ?? to))
			{
				il.Emit(OpCodes.Unbox_Any, to);
			}
			else if ((@operator = GetOperator(from, to)) != null)
			{
				il.Emit(OpCodes.Unbox_Any, from);
				il.Emit(OpCodes.Call, @operator);
			}
			else
			{
				bool flag = false;
				OpCode opcode = default(OpCode);
				TypeCode typeCode = TypeExtensions.GetTypeCode(from);
				if (typeCode == TypeCode.Boolean || (uint)(typeCode - 5) <= 9u)
				{
					flag = true;
					switch (TypeExtensions.GetTypeCode(via ?? to))
					{
					case TypeCode.Byte:
						opcode = OpCodes.Conv_Ovf_I1_Un;
						break;
					case TypeCode.SByte:
						opcode = OpCodes.Conv_Ovf_I1;
						break;
					case TypeCode.UInt16:
						opcode = OpCodes.Conv_Ovf_I2_Un;
						break;
					case TypeCode.Int16:
						opcode = OpCodes.Conv_Ovf_I2;
						break;
					case TypeCode.UInt32:
						opcode = OpCodes.Conv_Ovf_I4_Un;
						break;
					case TypeCode.Boolean:
					case TypeCode.Int32:
						opcode = OpCodes.Conv_Ovf_I4;
						break;
					case TypeCode.UInt64:
						opcode = OpCodes.Conv_Ovf_I8_Un;
						break;
					case TypeCode.Int64:
						opcode = OpCodes.Conv_Ovf_I8;
						break;
					case TypeCode.Single:
						opcode = OpCodes.Conv_R4;
						break;
					case TypeCode.Double:
						opcode = OpCodes.Conv_R8;
						break;
					default:
						flag = false;
						break;
					}
				}
				if (flag)
				{
					il.Emit(OpCodes.Unbox_Any, from);
					il.Emit(opcode);
					if (to == typeof(bool))
					{
						il.Emit(OpCodes.Ldc_I4_0);
						il.Emit(OpCodes.Ceq);
						il.Emit(OpCodes.Ldc_I4_0);
						il.Emit(OpCodes.Ceq);
					}
				}
				else
				{
					il.Emit(OpCodes.Ldtoken, via ?? to);
					il.EmitCall(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"), null);
					il.EmitCall(OpCodes.Call, typeof(Convert).GetMethod("ChangeType", new Type[2]
					{
						typeof(object),
						typeof(Type)
					}), null);
					il.Emit(OpCodes.Unbox_Any, to);
				}
			}
		}

		private static MethodInfo GetOperator(Type from, Type to)
		{
			if (to == null)
			{
				return null;
			}
			MethodInfo[] methods;
			MethodInfo[] methods2;
			return ResolveOperator(methods = from.GetMethods(BindingFlags.Static | BindingFlags.Public), from, to, "op_Implicit") ?? ResolveOperator(methods2 = to.GetMethods(BindingFlags.Static | BindingFlags.Public), from, to, "op_Implicit") ?? ResolveOperator(methods, from, to, "op_Explicit") ?? ResolveOperator(methods2, from, to, "op_Explicit");
		}

		private static MethodInfo ResolveOperator(MethodInfo[] methods, Type from, Type to, string name)
		{
			for (int i = 0; i < methods.Length; i++)
			{
				if (!(methods[i].Name != name) && !(methods[i].ReturnType != to))
				{
					ParameterInfo[] parameters = methods[i].GetParameters();
					if (parameters.Length == 1 && !(parameters[0].ParameterType != from))
					{
						return methods[i];
					}
				}
			}
			return null;
		}

		private static void LoadLocal(ILGenerator il, int index)
		{
			if (index < 0 || index >= 32767)
			{
				throw new ArgumentNullException("index");
			}
			switch (index)
			{
			case 0:
				il.Emit(OpCodes.Ldloc_0);
				break;
			case 1:
				il.Emit(OpCodes.Ldloc_1);
				break;
			case 2:
				il.Emit(OpCodes.Ldloc_2);
				break;
			case 3:
				il.Emit(OpCodes.Ldloc_3);
				break;
			default:
				if (index <= 255)
				{
					il.Emit(OpCodes.Ldloc_S, (byte)index);
				}
				else
				{
					il.Emit(OpCodes.Ldloc, (short)index);
				}
				break;
			}
		}

		private static void StoreLocal(ILGenerator il, int index)
		{
			if (index < 0 || index >= 32767)
			{
				throw new ArgumentNullException("index");
			}
			switch (index)
			{
			case 0:
				il.Emit(OpCodes.Stloc_0);
				break;
			case 1:
				il.Emit(OpCodes.Stloc_1);
				break;
			case 2:
				il.Emit(OpCodes.Stloc_2);
				break;
			case 3:
				il.Emit(OpCodes.Stloc_3);
				break;
			default:
				if (index <= 255)
				{
					il.Emit(OpCodes.Stloc_S, (byte)index);
				}
				else
				{
					il.Emit(OpCodes.Stloc, (short)index);
				}
				break;
			}
		}

		private static void LoadLocalAddress(ILGenerator il, int index)
		{
			if (index < 0 || index >= 32767)
			{
				throw new ArgumentNullException("index");
			}
			if (index <= 255)
			{
				il.Emit(OpCodes.Ldloca_S, (byte)index);
			}
			else
			{
				il.Emit(OpCodes.Ldloca, (short)index);
			}
		}

		[Obsolete("This method is for internal use only", false)]
		public static void ThrowDataException(Exception ex, int index, IDataReader reader, object value)
		{
			Exception ex3;
			try
			{
				string arg = "(n/a)";
				string arg2 = "(n/a)";
				if (reader != null && index >= 0 && index < reader.FieldCount)
				{
					arg = reader.GetName(index);
					try
					{
						arg2 = ((value != null && !(value is DBNull)) ? (Convert.ToString(value) + " - " + TypeExtensions.GetTypeCode(value.GetType())) : "<null>");
					}
					catch (Exception ex2)
					{
						arg2 = ex2.Message;
					}
				}
				ex3 = new DataException($"Error parsing column {index} ({arg}={arg2})", ex);
			}
			catch
			{
				ex3 = new DataException(ex.Message, ex);
			}
			throw ex3;
		}

		private static void EmitInt32(ILGenerator il, int value)
		{
			switch (value)
			{
			case -1:
				il.Emit(OpCodes.Ldc_I4_M1);
				break;
			case 0:
				il.Emit(OpCodes.Ldc_I4_0);
				break;
			case 1:
				il.Emit(OpCodes.Ldc_I4_1);
				break;
			case 2:
				il.Emit(OpCodes.Ldc_I4_2);
				break;
			case 3:
				il.Emit(OpCodes.Ldc_I4_3);
				break;
			case 4:
				il.Emit(OpCodes.Ldc_I4_4);
				break;
			case 5:
				il.Emit(OpCodes.Ldc_I4_5);
				break;
			case 6:
				il.Emit(OpCodes.Ldc_I4_6);
				break;
			case 7:
				il.Emit(OpCodes.Ldc_I4_7);
				break;
			case 8:
				il.Emit(OpCodes.Ldc_I4_8);
				break;
			default:
				if (value >= -128 && value <= 127)
				{
					il.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
				}
				else
				{
					il.Emit(OpCodes.Ldc_I4, value);
				}
				break;
			}
		}

		public static ICustomQueryParameter AsTableValuedParameter(this DataTable table, string typeName = null)
		{
			return new TableValuedParameter(table, typeName);
		}

		public static void SetTypeName(this DataTable table, string typeName)
		{
			if (table != null)
			{
				if (string.IsNullOrEmpty(typeName))
				{
					table.ExtendedProperties.Remove("dapper:TypeName");
				}
				else
				{
					table.ExtendedProperties["dapper:TypeName"] = typeName;
				}
			}
		}

		public static string GetTypeName(this DataTable table)
		{
			return table?.ExtendedProperties["dapper:TypeName"] as string;
		}

		public static ICustomQueryParameter AsTableValuedParameter(this IEnumerable<SqlDataRecord> list, string typeName = null)
		{
			return new SqlDataRecordListTVPParameter(list, typeName);
		}

		private static StringBuilder GetStringBuilder()
		{
			StringBuilder stringBuilder = perThreadStringBuilderCache;
			if (stringBuilder != null)
			{
				perThreadStringBuilderCache = null;
				stringBuilder.Length = 0;
				return stringBuilder;
			}
			return new StringBuilder();
		}

		private static string __ToStringRecycle(this StringBuilder obj)
		{
			if (obj != null)
			{
				string result = obj.ToString();
				perThreadStringBuilderCache = (perThreadStringBuilderCache ?? obj);
				return result;
			}
			return "";
		}

		public static IEnumerable<T> Parse<T>(this IDataReader reader)
		{
			if (reader.Read())
			{
				Func<IDataReader, object> deser = GetDeserializer(typeof(T), reader, 0, -1, returnNullIfFirstMissing: false);
				do
				{
					yield return (T)deser(reader);
				}
				while (reader.Read());
			}
		}

		public static IEnumerable<object> Parse(this IDataReader reader, Type type)
		{
			if (reader.Read())
			{
				Func<IDataReader, object> deser = GetDeserializer(type, reader, 0, -1, returnNullIfFirstMissing: false);
				do
				{
					yield return deser(reader);
				}
				while (reader.Read());
			}
		}

		public static IEnumerable<dynamic> Parse(this IDataReader reader)
		{
			if (reader.Read())
			{
				Func<IDataReader, object> deser = GetDapperRowDeserializer(reader, 0, -1, returnNullIfFirstMissing: false);
				do
				{
					yield return (dynamic)deser(reader);
				}
				while (reader.Read());
			}
		}

		public static Func<IDataReader, object> GetRowParser(this IDataReader reader, Type type, int startIndex = 0, int length = -1, bool returnNullIfFirstMissing = false)
		{
			return GetDeserializer(type, reader, startIndex, length, returnNullIfFirstMissing);
		}

		//public static Func<IDataReader, T> GetRowParser<T>(this IDataReader reader, Type concreteType = null, int startIndex = 0, int length = -1, bool returnNullIfFirstMissing = false)
		//{
		//	concreteType = (concreteType ?? typeof(T));
		//	Func<IDataReader, object> func = GetDeserializer(concreteType, reader, startIndex, length, returnNullIfFirstMissing);
		//	if (concreteType.IsValueType())
		//	{
		//		return (IDataReader _) => (T)func(_);
		//	}
		//	return (Func<IDataReader, T>)func;
		//}
	}
}
