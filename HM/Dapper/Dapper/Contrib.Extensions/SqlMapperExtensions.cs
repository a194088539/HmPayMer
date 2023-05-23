using HM.Framework.Entity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HM.Framework.Dapper.Contrib.Extensions
{
	public static class SqlMapperExtensions
	{
		public interface IProxy
		{
			bool IsDirty
			{
				get;
				set;
			}
		}

		public interface ITableNameMapper
		{
			string GetTableName(Type type);
		}

		public delegate string GetDatabaseTypeDelegate(IDbConnection connection);

		public delegate string TableNameMapperDelegate(Type type);

		private static class ProxyGenerator
		{
			private static readonly Dictionary<Type, Type> TypeCache = new Dictionary<Type, Type>();

			private static AssemblyBuilder GetAsmBuilder(string name)
			{
				return Thread.GetDomain().DefineDynamicAssembly(new AssemblyName
				{
					Name = name
				}, AssemblyBuilderAccess.Run);
			}

			public static T GetInterfaceProxy<T>()
			{
				Type typeFromHandle = typeof(T);
				if (TypeCache.TryGetValue(typeFromHandle, out Type value))
				{
					return (T)Activator.CreateInstance(value);
				}
				ModuleBuilder moduleBuilder = GetAsmBuilder(typeFromHandle.Name).DefineDynamicModule("SqlMapperExtensions." + typeFromHandle.Name);
				Type typeFromHandle2 = typeof(IProxy);
				TypeBuilder typeBuilder = moduleBuilder.DefineType(typeFromHandle.Name + "_" + Guid.NewGuid(), TypeAttributes.Public);
				typeBuilder.AddInterfaceImplementation(typeFromHandle);
				typeBuilder.AddInterfaceImplementation(typeFromHandle2);
				MethodInfo setIsDirtyMethod = CreateIsDirtyProperty(typeBuilder);
				PropertyInfo[] properties = typeof(T).GetProperties();
				foreach (PropertyInfo propertyInfo in properties)
				{
					bool isIdentity = propertyInfo.GetCustomAttributes(inherit: true).Any((object a) => a is KeyAttribute);
					CreateProperty<T>(typeBuilder, propertyInfo.Name, propertyInfo.PropertyType, setIsDirtyMethod, isIdentity);
				}
				Type type = typeBuilder.CreateType();
				TypeCache.Add(typeFromHandle, type);
				return (T)Activator.CreateInstance(type);
			}

			private static MethodInfo CreateIsDirtyProperty(TypeBuilder typeBuilder)
			{
				Type typeFromHandle = typeof(bool);
				FieldBuilder field = typeBuilder.DefineField("_IsDirty", typeFromHandle, FieldAttributes.Private);
				PropertyBuilder propertyBuilder = typeBuilder.DefineProperty("IsDirty", System.Reflection.PropertyAttributes.None, typeFromHandle, new Type[1]
				{
					typeFromHandle
				});
				MethodBuilder methodBuilder = typeBuilder.DefineMethod("get_IsDirty", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.VtableLayoutMask | MethodAttributes.SpecialName, typeFromHandle, Type.EmptyTypes);
				ILGenerator iLGenerator = methodBuilder.GetILGenerator();
				iLGenerator.Emit(OpCodes.Ldarg_0);
				iLGenerator.Emit(OpCodes.Ldfld, field);
				iLGenerator.Emit(OpCodes.Ret);
				MethodBuilder methodBuilder2 = typeBuilder.DefineMethod("set_IsDirty", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.VtableLayoutMask | MethodAttributes.SpecialName, null, new Type[1]
				{
					typeFromHandle
				});
				ILGenerator iLGenerator2 = methodBuilder2.GetILGenerator();
				iLGenerator2.Emit(OpCodes.Ldarg_0);
				iLGenerator2.Emit(OpCodes.Ldarg_1);
				iLGenerator2.Emit(OpCodes.Stfld, field);
				iLGenerator2.Emit(OpCodes.Ret);
				propertyBuilder.SetGetMethod(methodBuilder);
				propertyBuilder.SetSetMethod(methodBuilder2);
				MethodInfo method = typeof(IProxy).GetMethod("get_IsDirty");
				MethodInfo method2 = typeof(IProxy).GetMethod("set_IsDirty");
				typeBuilder.DefineMethodOverride(methodBuilder, method);
				typeBuilder.DefineMethodOverride(methodBuilder2, method2);
				return methodBuilder2;
			}

			private static void CreateProperty<T>(TypeBuilder typeBuilder, string propertyName, Type propType, MethodInfo setIsDirtyMethod, bool isIdentity)
			{
				FieldBuilder field = typeBuilder.DefineField("_" + propertyName, propType, FieldAttributes.Private);
				PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, System.Reflection.PropertyAttributes.None, propType, new Type[1]
				{
					propType
				});
				MethodBuilder methodBuilder = typeBuilder.DefineMethod("get_" + propertyName, MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig, propType, Type.EmptyTypes);
				ILGenerator iLGenerator = methodBuilder.GetILGenerator();
				iLGenerator.Emit(OpCodes.Ldarg_0);
				iLGenerator.Emit(OpCodes.Ldfld, field);
				iLGenerator.Emit(OpCodes.Ret);
				MethodBuilder methodBuilder2 = typeBuilder.DefineMethod("set_" + propertyName, MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig, null, new Type[1]
				{
					propType
				});
				ILGenerator iLGenerator2 = methodBuilder2.GetILGenerator();
				iLGenerator2.Emit(OpCodes.Ldarg_0);
				iLGenerator2.Emit(OpCodes.Ldarg_1);
				iLGenerator2.Emit(OpCodes.Stfld, field);
				iLGenerator2.Emit(OpCodes.Ldarg_0);
				iLGenerator2.Emit(OpCodes.Ldc_I4_1);
				iLGenerator2.Emit(OpCodes.Call, setIsDirtyMethod);
				iLGenerator2.Emit(OpCodes.Ret);
				if (isIdentity)
				{
					CustomAttributeBuilder customAttribute = new CustomAttributeBuilder(typeof(KeyAttribute).GetConstructor(new Type[0]), new object[0]);
					propertyBuilder.SetCustomAttribute(customAttribute);
				}
				propertyBuilder.SetGetMethod(methodBuilder);
				propertyBuilder.SetSetMethod(methodBuilder2);
				MethodInfo method = typeof(T).GetMethod("get_" + propertyName);
				MethodInfo method2 = typeof(T).GetMethod("set_" + propertyName);
				typeBuilder.DefineMethodOverride(methodBuilder, method);
				typeBuilder.DefineMethodOverride(methodBuilder2, method2);
			}
		}

		private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> KeyProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();

		private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> InsertKeyProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();

		private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> ExplicitKeyProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();

		private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> TypeProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();

		private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> ComputedProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();

		private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> IgnoreProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();

		private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> GetQueries = new ConcurrentDictionary<RuntimeTypeHandle, string>();

		private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> TypeTableName = new ConcurrentDictionary<RuntimeTypeHandle, string>();

		private static readonly ConcurrentDictionary<RuntimeTypeHandle, Dictionary<string, string>> TypeColumnNameDictionary = new ConcurrentDictionary<RuntimeTypeHandle, Dictionary<string, string>>();

		private static readonly ISqlAdapter DefaultAdapter = new SqlServerAdapter();

		private static readonly Dictionary<string, ISqlAdapter> AdapterDictionary = new Dictionary<string, ISqlAdapter>
		{
			["sqlconnection"] = new SqlServerAdapter(),
			["sqlceconnection"] = new SqlCeServerAdapter(),
			["npgsqlconnection"] = new PostgresAdapter(),
			["sqliteconnection"] = new SQLiteAdapter(),
			["mysqlconnection"] = new MySqlAdapter(),
			["fbconnection"] = new FbAdapter()
		};

		public static TableNameMapperDelegate TableNameMapper;

		public static GetDatabaseTypeDelegate GetDatabaseType;

		public static async Task<T> GetAsync<T>(this IDbConnection connection, dynamic id, IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : class
		{
			Type type = typeof(T);
			if (!GetQueries.TryGetValue(type.TypeHandle, out string value))
			{
				PropertyInfo singleKey = GetSingleKey<T>("GetAsync");
				string tableName = GetTableName(type);
				value = $"SELECT * FROM {tableName} WHERE {singleKey.Name} = @id";
				GetQueries[type.TypeHandle] = value;
			}
			DynamicParameters dynamicParameters = new DynamicParameters();
			dynamicParameters.Add("@id", id);
			if (!type.IsInterface())
			{
				return (await connection.QueryAsync<T>(value, dynamicParameters, transaction, commandTimeout).ConfigureAwait(continueOnCapturedContext: false)).FirstOrDefault();
			}
			IDictionary<string, object> dictionary = (await connection.QueryAsync<object>(value, dynamicParameters).ConfigureAwait(continueOnCapturedContext: false)).FirstOrDefault() as IDictionary<string, object>;
			if (dictionary == null)
			{
				return null;
			}
			T interfaceProxy = ProxyGenerator.GetInterfaceProxy<T>();
			foreach (PropertyInfo item in TypePropertiesCache(type))
			{
				object obj = dictionary[item.Name];
				if (obj != null)
				{
					if (item.PropertyType.IsGenericType() && item.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
					{
						Type underlyingType = Nullable.GetUnderlyingType(item.PropertyType);
						if (underlyingType != null)
						{
							item.SetValue(interfaceProxy, Convert.ChangeType(obj, underlyingType), null);
						}
					}
					else
					{
						item.SetValue(interfaceProxy, Convert.ChangeType(obj, item.PropertyType), null);
					}
				}
			}
			((IProxy)interfaceProxy).IsDirty = false;
			return interfaceProxy;
		}

		public static Task<IEnumerable<T>> GetAllAsync<T>(this IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : class
		{
			Type typeFromHandle = typeof(T);
			Type typeFromHandle2 = typeof(List<T>);
			if (!GetQueries.TryGetValue(typeFromHandle2.TypeHandle, out string value))
			{
				GetSingleKey<T>("GetAll");
				string tableName = GetTableName(typeFromHandle);
				value = "SELECT * FROM " + tableName;
				GetQueries[typeFromHandle2.TypeHandle] = value;
			}
			if (!typeFromHandle.IsInterface())
			{
				return connection.QueryAsync<T>(value, null, transaction, commandTimeout);
			}
			return GetAllAsyncImpl<T>(connection, transaction, commandTimeout, value, typeFromHandle);
		}

		private static async Task<IEnumerable<T>> GetAllAsyncImpl<T>(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string sql, Type type) where T : class
		{
			IEnumerable<object> obj = await connection.QueryAsync(sql).ConfigureAwait(continueOnCapturedContext: false);
			List<T> list = new List<T>();
			foreach (object item in obj)
			{
				IDictionary<string, object> dictionary = (IDictionary<string, object>)item;
				T interfaceProxy = ProxyGenerator.GetInterfaceProxy<T>();
				foreach (PropertyInfo item2 in TypePropertiesCache(type))
				{
					object obj2 = dictionary[item2.Name];
					if (obj2 != null)
					{
						if (item2.PropertyType.IsGenericType() && item2.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
						{
							Type underlyingType = Nullable.GetUnderlyingType(item2.PropertyType);
							if (underlyingType != null)
							{
								item2.SetValue(interfaceProxy, Convert.ChangeType(obj2, underlyingType), null);
							}
						}
						else
						{
							item2.SetValue(interfaceProxy, Convert.ChangeType(obj2, item2.PropertyType), null);
						}
					}
				}
				((IProxy)interfaceProxy).IsDirty = false;
				list.Add(interfaceProxy);
			}
			return list;
		}

		public static Task<int> InsertAsync<T>(this IDbConnection connection, T entityToInsert, IDbTransaction transaction = null, int? commandTimeout = default(int?), ISqlAdapter sqlAdapter = null) where T : class
		{
			Type type = typeof(T);
			sqlAdapter = (sqlAdapter ?? GetFormatter(connection));
			bool flag = false;
			if (type.IsArray)
			{
				flag = true;
				type = type.GetElementType();
			}
			else if (type.IsGenericType())
			{
				TypeInfo typeInfo = type.GetTypeInfo();
				if (typeInfo.ImplementedInterfaces.Any(delegate(Type ti)
				{
					if (ti.IsGenericType())
					{
						return ti.GetGenericTypeDefinition() == typeof(IEnumerable<>);
					}
					return false;
				}) || typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				{
					flag = true;
					type = type.GetGenericArguments()[0];
				}
			}
			string tableName = GetTableName(type);
			StringBuilder stringBuilder = new StringBuilder(null);
			List<PropertyInfo> first = TypePropertiesCache(type);
			List<PropertyInfo> list = KeyPropertiesCache(type);
			List<PropertyInfo> second = ComputedPropertiesCache(type);
			List<PropertyInfo> list2 = first.Except(list.Union(second)).ToList();
			for (int i = 0; i < list2.Count; i++)
			{
				PropertyInfo propertyInfo = list2[i];
				sqlAdapter.AppendColumnName(stringBuilder, propertyInfo.Name);
				if (i < list2.Count - 1)
				{
					stringBuilder.Append(", ");
				}
			}
			StringBuilder stringBuilder2 = new StringBuilder(null);
			for (int j = 0; j < list2.Count; j++)
			{
				PropertyInfo propertyInfo2 = list2[j];
				stringBuilder2.AppendFormat("@{0}", propertyInfo2.Name);
				if (j < list2.Count - 1)
				{
					stringBuilder2.Append(", ");
				}
			}
			if (!flag)
			{
				return sqlAdapter.InsertAsync(connection, transaction, commandTimeout, tableName, stringBuilder.ToString(), stringBuilder2.ToString(), list, entityToInsert);
			}
			string sql = $"INSERT INTO {tableName} ({stringBuilder}) values ({stringBuilder2})";
			return connection.ExecuteAsync(sql, entityToInsert, transaction, commandTimeout);
		}

		public static async Task<bool> UpdateAsync<T>(this IDbConnection connection, T entityToUpdate, IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : class
		{
			IProxy proxy;
			if ((proxy = (entityToUpdate as IProxy)) != null && !proxy.IsDirty)
			{
				return false;
			}
			Type type = typeof(T);
			if (type.IsArray)
			{
				type = type.GetElementType();
			}
			else if (type.IsGenericType())
			{
				TypeInfo typeInfo = type.GetTypeInfo();
				if (typeInfo.ImplementedInterfaces.Any(delegate(Type ti)
				{
					if (ti.IsGenericType())
					{
						return ti.GetGenericTypeDefinition() == typeof(IEnumerable<>);
					}
					return false;
				}) || typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				{
					type = type.GetGenericArguments()[0];
				}
			}
			List<PropertyInfo> list = KeyPropertiesCache(type);
			List<PropertyInfo> list2 = ExplicitKeyPropertiesCache(type);
			if (list.Count == 0 && list2.Count == 0)
			{
				throw new ArgumentException("Entity must have at least one [Key] or [ExplicitKey] property");
			}
			string tableName = GetTableName(type);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("update {0} set ", tableName);
			List<PropertyInfo> first = TypePropertiesCache(type);
			list.AddRange(list2);
			List<PropertyInfo> second = ComputedPropertiesCache(type);
			List<PropertyInfo> list3 = first.Except(list.Union(second)).ToList();
			ISqlAdapter formatter = GetFormatter(connection);
			for (int i = 0; i < list3.Count; i++)
			{
				PropertyInfo propertyInfo = list3[i];
				formatter.AppendColumnNameEqualsValue(stringBuilder, propertyInfo.Name);
				if (i < list3.Count - 1)
				{
					stringBuilder.Append(", ");
				}
			}
			stringBuilder.Append(" where ");
			for (int j = 0; j < list.Count; j++)
			{
				PropertyInfo propertyInfo2 = list[j];
				formatter.AppendColumnNameEqualsValue(stringBuilder, propertyInfo2.Name);
				if (j < list.Count - 1)
				{
					stringBuilder.Append(" and ");
				}
			}
			return await connection.ExecuteAsync(stringBuilder.ToString(), entityToUpdate, transaction, commandTimeout).ConfigureAwait(continueOnCapturedContext: false) > 0;
		}

		public static async Task<bool> DeleteAsync<T>(this IDbConnection connection, T entityToDelete, IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : class
		{
			if (entityToDelete == null)
			{
				throw new ArgumentException("Cannot Delete null Object", "entityToDelete");
			}
			Type type = typeof(T);
			if (type.IsArray)
			{
				type = type.GetElementType();
			}
			else if (type.IsGenericType())
			{
				TypeInfo typeInfo = type.GetTypeInfo();
				if (typeInfo.ImplementedInterfaces.Any(delegate(Type ti)
				{
					if (ti.IsGenericType())
					{
						return ti.GetGenericTypeDefinition() == typeof(IEnumerable<>);
					}
					return false;
				}) || typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				{
					type = type.GetGenericArguments()[0];
				}
			}
			List<PropertyInfo> list = KeyPropertiesCache(type);
			List<PropertyInfo> list2 = ExplicitKeyPropertiesCache(type);
			if (list.Count == 0 && list2.Count == 0)
			{
				throw new ArgumentException("Entity must have at least one [Key] or [ExplicitKey] property");
			}
			string tableName = GetTableName(type);
			list.AddRange(list2);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("DELETE FROM {0} WHERE ", tableName);
			for (int i = 0; i < list.Count; i++)
			{
				PropertyInfo propertyInfo = list[i];
				stringBuilder.AppendFormat("{0} = @{1}", propertyInfo.Name, propertyInfo.Name);
				if (i < list.Count - 1)
				{
					stringBuilder.Append(" AND ");
				}
			}
			return await connection.ExecuteAsync(stringBuilder.ToString(), entityToDelete, transaction, commandTimeout).ConfigureAwait(continueOnCapturedContext: false) > 0;
		}

		public static async Task<bool> DeleteAllAsync<T>(this IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : class
		{
			Type typeFromHandle = typeof(T);
			string sql = "DELETE FROM " + GetTableName(typeFromHandle);
			return await connection.ExecuteAsync(sql, null, transaction, commandTimeout).ConfigureAwait(continueOnCapturedContext: false) > 0;
		}

		private static Dictionary<string, string> ColumnNameDictionaryCache(Type type)
		{
			if (TypeColumnNameDictionary.TryGetValue(type.TypeHandle, out Dictionary<string, string> value))
			{
				return value;
			}
			List<PropertyInfo> list = (from p in TypePropertiesCache(type)
			where p.GetCustomAttributes(inherit: true).Any((object a) => a is ColumnAttrubute)
			select p).ToList();
			if (list != null && list.Count > 0)
			{
				value = new Dictionary<string, string>(list.Count);
				foreach (PropertyInfo item in list)
				{
					ColumnAttrubute customAttribute = item.GetCustomAttribute<ColumnAttrubute>();
					if (customAttribute != null && !value.ContainsKey(item.Name))
					{
						value.Add(item.Name, customAttribute.Name);
					}
				}
				TypeColumnNameDictionary[type.TypeHandle] = value;
			}
			return value;
		}

		private static List<PropertyInfo> IgnorePropertiesCache(Type type)
		{
			if (IgnoreProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> value))
			{
				return value.ToList();
			}
			List<PropertyInfo> list = (from p in TypePropertiesCache(type)
			where p.GetCustomAttributes(inherit: true).Any((object a) => a is IgnoreAttribute)
			select p).ToList();
			IgnoreProperties[type.TypeHandle] = list;
			return list;
		}

		private static List<PropertyInfo> ComputedPropertiesCache(Type type)
		{
			if (ComputedProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> value))
			{
				return value.ToList();
			}
			List<PropertyInfo> list = (from p in TypePropertiesCache(type)
			where p.GetCustomAttributes(inherit: true).Any((object a) => a is ComputedAttribute)
			select p).ToList();
			ComputedProperties[type.TypeHandle] = list;
			return list;
		}

		private static List<PropertyInfo> ExplicitKeyPropertiesCache(Type type)
		{
			if (ExplicitKeyProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> value))
			{
				return value.ToList();
			}
			List<PropertyInfo> list = (from p in TypePropertiesCache(type)
			where p.GetCustomAttributes(inherit: true).Any((object a) => a is ExplicitKeyAttribute)
			select p).ToList();
			ExplicitKeyProperties[type.TypeHandle] = list;
			return list;
		}

		private static List<PropertyInfo> KeyPropertiesCache(Type type)
		{
			if (KeyProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> value))
			{
				return value.ToList();
			}
			List<PropertyInfo> list = TypePropertiesCache(type);
			List<PropertyInfo> list2 = (from p in list
			where p.GetCustomAttributes(inherit: true).Any((object a) => a is KeyAttribute)
			select p).ToList();
			if (list2.Count == 0)
			{
				PropertyInfo propertyInfo = list.Find((PropertyInfo p) => string.Equals(p.Name, "id", StringComparison.CurrentCultureIgnoreCase));
				if (propertyInfo != null && !propertyInfo.GetCustomAttributes(inherit: true).Any((object a) => a is ExplicitKeyAttribute))
				{
					list2.Add(propertyInfo);
				}
			}
			KeyProperties[type.TypeHandle] = list2;
			return list2;
		}

		private static List<PropertyInfo> InsertKeyPropertiesCache(Type type)
		{
			if (ExplicitKeyProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> value))
			{
				return value.ToList();
			}
			List<PropertyInfo> list = (from p in TypePropertiesCache(type)
			where p.GetCustomAttributes(inherit: true).Any((object a) => a is InsertKeyAttribute)
			select p).ToList();
			InsertKeyProperties[type.TypeHandle] = list;
			return list;
		}

		private static List<PropertyInfo> TypePropertiesCache(Type type)
		{
			if (TypeProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> value))
			{
				return value.ToList();
			}
			PropertyInfo[] array = type.GetProperties().Where(IsWriteable).ToArray();
			TypeProperties[type.TypeHandle] = array;
			return array.ToList();
		}

		private static bool IsWriteable(PropertyInfo pi)
		{
			List<object> list = pi.GetCustomAttributes(typeof(WriteAttribute), inherit: false).AsList();
			if (list.Count != 1)
			{
				return true;
			}
			return ((WriteAttribute)list[0]).Write;
		}

		private static PropertyInfo GetSingleKey<T>(string method)
		{
			Type typeFromHandle = typeof(T);
			List<PropertyInfo> list = KeyPropertiesCache(typeFromHandle);
			List<PropertyInfo> list2 = ExplicitKeyPropertiesCache(typeFromHandle);
			int num = list.Count + list2.Count;
			if (num > 1)
			{
				throw new DataException($"{method}<T> only supports an entity with a single [Key] or [ExplicitKey] property");
			}
			if (num == 0)
			{
				throw new DataException($"{method}<T> only supports an entity with a [Key] or an [ExplicitKey] property");
			}
			if (list.Count <= 0)
			{
				return list2[0];
			}
			return list[0];
		}

		public static T Get<T>(this IDbConnection connection, dynamic id, IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : class
		{
			Type typeFromHandle = typeof(T);
			if (!GetQueries.TryGetValue(typeFromHandle.TypeHandle, out string value))
			{
				PropertyInfo singleKey = GetSingleKey<T>("Get");
				string tableName = GetTableName(typeFromHandle);
				value = $"select * from {tableName} where {singleKey.Name} = @id";
				GetQueries[typeFromHandle.TypeHandle] = value;
			}
			DynamicParameters dynamicParameters = new DynamicParameters();
			dynamicParameters.Add("@id", id);
			T val;
			if (typeFromHandle.IsInterface())
			{
				IDictionary<string, object> dictionary = connection.Query(value, dynamicParameters).FirstOrDefault() as IDictionary<string, object>;
				if (dictionary == null)
				{
					return null;
				}
				val = ProxyGenerator.GetInterfaceProxy<T>();
				foreach (PropertyInfo item in TypePropertiesCache(typeFromHandle))
				{
					object obj = dictionary[item.Name];
					if (obj != null)
					{
						if (item.PropertyType.IsGenericType() && item.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
						{
							Type underlyingType = Nullable.GetUnderlyingType(item.PropertyType);
							if (underlyingType != null)
							{
								item.SetValue(val, Convert.ChangeType(obj, underlyingType), null);
							}
						}
						else
						{
							item.SetValue(val, Convert.ChangeType(obj, item.PropertyType), null);
						}
					}
				}
				((IProxy)val).IsDirty = false;
			}
			else
			{
				val = connection.Query<T>(value, dynamicParameters, transaction, buffered: true, commandTimeout).FirstOrDefault();
			}
			return val;
		}

		public static IEnumerable<T> GetAll<T>(this IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : class
		{
			Type typeFromHandle = typeof(T);
			Type typeFromHandle2 = typeof(List<T>);
			if (!GetQueries.TryGetValue(typeFromHandle2.TypeHandle, out string value))
			{
				GetSingleKey<T>("GetAll");
				string tableName = GetTableName(typeFromHandle);
				value = "select * from " + tableName;
				GetQueries[typeFromHandle2.TypeHandle] = value;
			}
			if (!typeFromHandle.IsInterface())
			{
				return connection.Query<T>(value, null, transaction, buffered: true, commandTimeout);
			}
			IEnumerable<dynamic> enumerable = connection.Query(value);
			List<T> list = new List<T>();
			foreach (object item in enumerable)
			{
				IDictionary<string, object> dictionary = (IDictionary<string, object>)item;
				T interfaceProxy = ProxyGenerator.GetInterfaceProxy<T>();
				foreach (PropertyInfo item2 in TypePropertiesCache(typeFromHandle))
				{
					object obj = dictionary[item2.Name];
					if (obj != null)
					{
						if (item2.PropertyType.IsGenericType() && item2.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
						{
							Type underlyingType = Nullable.GetUnderlyingType(item2.PropertyType);
							if (underlyingType != null)
							{
								item2.SetValue(interfaceProxy, Convert.ChangeType(obj, underlyingType), null);
							}
						}
						else
						{
							item2.SetValue(interfaceProxy, Convert.ChangeType(obj, item2.PropertyType), null);
						}
					}
				}
				((IProxy)interfaceProxy).IsDirty = false;
				list.Add(interfaceProxy);
			}
			return list;
		}

		private static string GetTableName(Type type)
		{
			if (TypeTableName.TryGetValue(type.TypeHandle, out string value))
			{
				return value;
			}
			if (TableNameMapper != null)
			{
				value = TableNameMapper(type);
			}
			else
			{
				dynamic val = type.GetCustomAttributes(inherit: false).SingleOrDefault((object attr) => attr.GetType().Name == "TableAttribute");
				if (val != null)
				{
					value = (string)val.Name;
				}
				else
				{
					value = type.Name;
					if (type.IsInterface() && value.StartsWith("I"))
					{
						value = value.Substring(1);
					}
				}
			}
			TypeTableName[type.TypeHandle] = value;
			return value;
		}

		public static long Insert<T>(this IDbConnection connection, T entityToInsert, IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : class
		{
			bool flag = false;
			Type type = typeof(T);
			if (type.IsArray)
			{
				flag = true;
				type = type.GetElementType();
			}
			else if (type.IsGenericType())
			{
				TypeInfo typeInfo = type.GetTypeInfo();
				if (typeInfo.ImplementedInterfaces.Any(delegate(Type ti)
				{
					if (ti.IsGenericType())
					{
						return ti.GetGenericTypeDefinition() == typeof(IEnumerable<>);
					}
					return false;
				}) || typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				{
					flag = true;
					type = type.GetGenericArguments()[0];
				}
			}
			string tableName = GetTableName(type);
			StringBuilder stringBuilder = new StringBuilder(null);
			List<PropertyInfo> first = TypePropertiesCache(type);
			List<PropertyInfo> list = KeyPropertiesCache(type);
			List<PropertyInfo> list2 = InsertKeyPropertiesCache(type);
			List<PropertyInfo> second = ComputedPropertiesCache(type);
			List<PropertyInfo> second2 = IgnorePropertiesCache(type);
			List<PropertyInfo> list3 = first.Except(list.Union(second).Union(list2)).ToList();
			list3.AddRange(list2);
			list3 = list3.Except(second2).ToList();
			Dictionary<string, string> dictionary = ColumnNameDictionaryCache(type);
			ISqlAdapter formatter = GetFormatter(connection);
			for (int i = 0; i < list3.Count; i++)
			{
				string text = list3.ElementAt(i).Name;
				if (dictionary != null && dictionary.ContainsKey(text))
				{
					text = dictionary[text];
				}
				formatter.AppendColumnName(stringBuilder, text);
				if (i < list3.Count - 1)
				{
					stringBuilder.Append(", ");
				}
			}
			StringBuilder stringBuilder2 = new StringBuilder(null);
			for (int j = 0; j < list3.Count; j++)
			{
				string text2 = list3.ElementAt(j).Name;
				if (dictionary != null && dictionary.ContainsKey(text2))
				{
					text2 = dictionary[text2];
				}
				stringBuilder2.AppendFormat("@{0}", text2);
				if (j < list3.Count - 1)
				{
					stringBuilder2.Append(", ");
				}
			}
			bool num = connection.State == ConnectionState.Closed;
			if (num)
			{
				connection.Open();
			}
			int num2;
			if (list2.Any())
			{
				string sql = $"insert into {tableName} ({stringBuilder}) values ({stringBuilder2})";
				num2 = connection.Execute(sql, entityToInsert, transaction, commandTimeout);
			}
			else if (!flag)
			{
				num2 = formatter.Insert(connection, transaction, commandTimeout, tableName, stringBuilder.ToString(), stringBuilder2.ToString(), list, entityToInsert);
			}
			else
			{
				string sql2 = $"insert into {tableName} ({stringBuilder}) values ({stringBuilder2})";
				num2 = connection.Execute(sql2, entityToInsert, transaction, commandTimeout);
			}
			if (num)
			{
				connection.Close();
			}
			return num2;
		}

		public static long InsertBatch<T>(this IDbConnection conn, IEnumerable<T> entityList, IDbTransaction transaction = null) where T : class
		{
			long result = entityList.LongCount();
			Type typeFromHandle = typeof(T);
			string tableName = GetTableName(typeFromHandle);
			new StringBuilder(null);
			List<PropertyInfo> first = TypePropertiesCache(typeFromHandle);
			List<PropertyInfo> first2 = KeyPropertiesCache(typeFromHandle);
			List<PropertyInfo> list = InsertKeyPropertiesCache(typeFromHandle);
			List<PropertyInfo> second = ComputedPropertiesCache(typeFromHandle);
			List<PropertyInfo> second2 = IgnorePropertiesCache(typeFromHandle);
			List<PropertyInfo> list2 = first.Except(first2.Union(second).Union(list)).ToList();
			list2.AddRange(list);
			list2 = list2.Except(second2).ToList();
			Dictionary<string, string> dictionary = ColumnNameDictionaryCache(typeFromHandle);
			try
			{
				SqlTransaction externalTransaction = (SqlTransaction)transaction;
				using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(conn as SqlConnection, SqlBulkCopyOptions.TableLock, externalTransaction))
				{
					sqlBulkCopy.BatchSize = entityList.Count();
					sqlBulkCopy.DestinationTableName = tableName;
					DataTable dataTable = new DataTable(tableName);
					for (int i = 0; i < list2.Count; i++)
					{
						PropertyInfo propertyInfo = list2.ElementAt(i);
						string text = propertyInfo.Name;
						if (dictionary != null && dictionary.ContainsKey(text))
						{
							text = dictionary[text];
						}
						sqlBulkCopy.ColumnMappings.Add(text, text);
						dataTable.Columns.Add(propertyInfo.Name, Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType);
					}
					foreach (T entity in entityList)
					{
						object[] array = new object[list2.Count()];
						for (int j = 0; j < array.Length; j++)
						{
							array[j] = list2.ElementAt(j).GetValue(entity, null);
						}
						dataTable.Rows.Add(array);
					}
					conn.Open();
					sqlBulkCopy.WriteToServer(dataTable);
					return result;
				}
			}
			catch (Exception)
			{
				return 0L;
			}
		}

		public static bool Update<T>(this IDbConnection connection, T entityToUpdate, IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : class
		{
			IProxy proxy = entityToUpdate as IProxy;
			if (proxy != null && !proxy.IsDirty)
			{
				return false;
			}
			Type type = typeof(T);
			if (type.IsArray)
			{
				type = type.GetElementType();
			}
			else if (type.IsGenericType())
			{
				TypeInfo typeInfo = type.GetTypeInfo();
				if (typeInfo.ImplementedInterfaces.Any(delegate(Type ti)
				{
					if (ti.IsGenericType())
					{
						return ti.GetGenericTypeDefinition() == typeof(IEnumerable<>);
					}
					return false;
				}) || typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				{
					type = type.GetGenericArguments()[0];
				}
			}
			List<PropertyInfo> list = KeyPropertiesCache(type).ToList();
			List<PropertyInfo> list2 = ExplicitKeyPropertiesCache(type);
			if (!list.Any() && !list2.Any())
			{
				throw new ArgumentException("Entity must have at least one [Key] or [ExplicitKey] property");
			}
			string tableName = GetTableName(type);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("update {0} set ", tableName);
			List<PropertyInfo> first = TypePropertiesCache(type);
			list.AddRange(list2);
			List<PropertyInfo> second = ComputedPropertiesCache(type);
			List<PropertyInfo> list3 = first.Except(Enumerable.Union(second: IgnorePropertiesCache(type), first: list.Union(second))).ToList();
			Dictionary<string, string> dictionary = ColumnNameDictionaryCache(type);
			ISqlAdapter formatter = GetFormatter(connection);
			for (int i = 0; i < list3.Count; i++)
			{
				string text = list3.ElementAt(i).Name;
				if (dictionary != null && dictionary.ContainsKey(text))
				{
					text = dictionary[text];
				}
				formatter.AppendColumnNameEqualsValue(stringBuilder, text);
				if (i < list3.Count - 1)
				{
					stringBuilder.AppendFormat(", ");
				}
			}
			stringBuilder.Append(" where ");
			for (int j = 0; j < list.Count; j++)
			{
				string text2 = list.ElementAt(j).Name;
				if (dictionary != null && dictionary.ContainsKey(text2))
				{
					text2 = dictionary[text2];
				}
				formatter.AppendColumnNameEqualsValue(stringBuilder, text2);
				if (j < list.Count - 1)
				{
					stringBuilder.AppendFormat(" and ");
				}
			}
			return connection.Execute(stringBuilder.ToString(), entityToUpdate, transaction, commandTimeout) > 0;
		}

		public static bool Delete<T>(this IDbConnection connection, T entityToDelete, IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : class
		{
			if (entityToDelete == null)
			{
				throw new ArgumentException("Cannot Delete null Object", "entityToDelete");
			}
			Type type = typeof(T);
			if (type.IsArray)
			{
				type = type.GetElementType();
			}
			else if (type.IsGenericType())
			{
				type = type.GetGenericArguments()[0];
			}
			List<PropertyInfo> list = KeyPropertiesCache(type).ToList();
			List<PropertyInfo> list2 = ExplicitKeyPropertiesCache(type);
			if (!list.Any() && !list2.Any())
			{
				throw new ArgumentException("Entity must have at least one [Key] or [ExplicitKey] property");
			}
			string tableName = GetTableName(type);
			list.AddRange(list2);
			Dictionary<string, string> dictionary = ColumnNameDictionaryCache(type);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("delete from {0} where ", tableName);
			ISqlAdapter formatter = GetFormatter(connection);
			for (int i = 0; i < list.Count; i++)
			{
				string text = list.ElementAt(i).Name;
				if (dictionary != null && dictionary.ContainsKey(text))
				{
					text = dictionary[text];
				}
				formatter.AppendColumnNameEqualsValue(stringBuilder, text);
				if (i < list.Count - 1)
				{
					stringBuilder.AppendFormat(" and ");
				}
			}
			return connection.Execute(stringBuilder.ToString(), entityToDelete, transaction, commandTimeout) > 0;
		}

		public static bool DeleteAll<T>(this IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : class
		{
			string tableName = GetTableName(typeof(T));
			string sql = $"delete from {tableName}";
			return connection.Execute(sql, null, transaction, commandTimeout) > 0;
		}

		private static ISqlAdapter GetFormatter(IDbConnection connection)
		{
			string key = GetDatabaseType?.Invoke(connection).ToLower() ?? connection.GetType().Name.ToLower();
			if (AdapterDictionary.ContainsKey(key))
			{
				return AdapterDictionary[key];
			}
			return DefaultAdapter;
		}
	}
}
