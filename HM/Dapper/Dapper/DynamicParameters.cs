using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace HM.Framework.Dapper
{
	public class DynamicParameters : SqlMapper.IDynamicParameters, SqlMapper.IParameterLookup, SqlMapper.IParameterCallbacks
	{
		internal static class CachedOutputSetters<T>
		{
			public static readonly Hashtable Cache = new Hashtable();
		}

		private sealed class ParamInfo
		{
			public string Name
			{
				get;
				set;
			}

			public object Value
			{
				get;
				set;
			}

			public ParameterDirection ParameterDirection
			{
				get;
				set;
			}

			public DbType? DbType
			{
				get;
				set;
			}

			public int? Size
			{
				get;
				set;
			}

			public IDbDataParameter AttachedParam
			{
				get;
				set;
			}

			internal Action<object, DynamicParameters> OutputCallback
			{
				get;
				set;
			}

			internal object OutputTarget
			{
				get;
				set;
			}

			internal bool CameFromTemplate
			{
				get;
				set;
			}

			public byte? Precision
			{
				get;
				set;
			}

			public byte? Scale
			{
				get;
				set;
			}
		}

		internal const DbType EnumerableMultiParameter = (DbType)(-1);

		private static readonly Dictionary<SqlMapper.Identity, Action<IDbCommand, object>> paramReaderCache = new Dictionary<SqlMapper.Identity, Action<IDbCommand, object>>();

		private readonly Dictionary<string, ParamInfo> parameters = new Dictionary<string, ParamInfo>();

		private List<object> templates;

		private List<Action> outputCallbacks;

		object SqlMapper.IParameterLookup.this[string name]
		{
			get
			{
				if (!parameters.TryGetValue(name, out ParamInfo value))
				{
					return null;
				}
				return value.Value;
			}
		}

		public bool RemoveUnused
		{
			get;
			set;
		}

		public IEnumerable<string> ParameterNames => from p in parameters
		select p.Key;

		public DynamicParameters()
		{
			RemoveUnused = true;
		}

		public DynamicParameters(object template)
		{
			RemoveUnused = true;
			AddDynamicParams(template);
		}

		public void AddDynamicParams(object param)
		{
			if (param != null)
			{
				DynamicParameters dynamicParameters = param as DynamicParameters;
				if (dynamicParameters == null)
				{
					IEnumerable<KeyValuePair<string, object>> enumerable = param as IEnumerable<KeyValuePair<string, object>>;
					if (enumerable == null)
					{
						templates = (templates ?? new List<object>());
						templates.Add(param);
					}
					else
					{
						foreach (KeyValuePair<string, object> item in enumerable)
						{
							Add(item.Key, item.Value, null, null, null);
						}
					}
				}
				else
				{
					if (dynamicParameters.parameters != null)
					{
						foreach (KeyValuePair<string, ParamInfo> parameter in dynamicParameters.parameters)
						{
							parameters.Add(parameter.Key, parameter.Value);
						}
					}
					if (dynamicParameters.templates != null)
					{
						templates = (templates ?? new List<object>());
						foreach (object template in dynamicParameters.templates)
						{
							templates.Add(template);
						}
					}
				}
			}
		}

		public void Add(string name, object value, DbType? dbType, ParameterDirection? direction, int? size)
		{
			parameters[Clean(name)] = new ParamInfo
			{
				Name = name,
				Value = value,
				ParameterDirection = (direction ?? ParameterDirection.Input),
				DbType = dbType,
				Size = size
			};
		}

		public void Add(string name, object value = null, DbType? dbType = default(DbType?), ParameterDirection? direction = default(ParameterDirection?), int? size = default(int?), byte? precision = default(byte?), byte? scale = default(byte?))
		{
			parameters[Clean(name)] = new ParamInfo
			{
				Name = name,
				Value = value,
				ParameterDirection = (direction ?? ParameterDirection.Input),
				DbType = dbType,
				Size = size,
				Precision = precision,
				Scale = scale
			};
		}

		private static string Clean(string name)
		{
			if (!string.IsNullOrEmpty(name))
			{
				char c = name[0];
				if (c == ':' || c == '?' || c == '@')
				{
					return name.Substring(1);
				}
			}
			return name;
		}

		void SqlMapper.IDynamicParameters.AddParameters(IDbCommand command, SqlMapper.Identity identity)
		{
			AddParameters(command, identity);
		}

		protected void AddParameters(IDbCommand command, SqlMapper.Identity identity)
		{
			IList<SqlMapper.LiteralToken> literalTokens = SqlMapper.GetLiteralTokens(identity.sql);
			if (templates != null)
			{
				foreach (object template in templates)
				{
					SqlMapper.Identity identity2 = identity.ForDynamicParameters(template.GetType());
					Action<IDbCommand, object> value;
					lock (paramReaderCache)
					{
						if (!paramReaderCache.TryGetValue(identity2, out value))
						{
							value = SqlMapper.CreateParamInfoGenerator(identity2, checkForDuplicates: true, RemoveUnused, literalTokens);
							paramReaderCache[identity2] = value;
						}
					}
					value(command, template);
				}
				foreach (IDbDataParameter parameter in command.Parameters)
				{
					if (!parameters.ContainsKey(parameter.ParameterName))
					{
						parameters.Add(parameter.ParameterName, new ParamInfo
						{
							AttachedParam = parameter,
							CameFromTemplate = true,
							DbType = parameter.DbType,
							Name = parameter.ParameterName,
							ParameterDirection = parameter.Direction,
							Size = parameter.Size,
							Value = parameter.Value
						});
					}
				}
				List<Action> list = outputCallbacks;
				if (list != null)
				{
					foreach (Action item in list)
					{
						item();
					}
				}
			}
			foreach (ParamInfo value3 in parameters.Values)
			{
				if (!value3.CameFromTemplate)
				{
					DbType? dbType = value3.DbType;
					object value2 = value3.Value;
					string text = Clean(value3.Name);
					bool flag = value2 is SqlMapper.ICustomQueryParameter;
					SqlMapper.ITypeHandler handler = null;
					if (!dbType.HasValue && value2 != null && !flag)
					{
						dbType = SqlMapper.LookupDbType(value2.GetType(), text, demand: true, out handler);
					}
					if (flag)
					{
						((SqlMapper.ICustomQueryParameter)value2).AddParameter(command, text);
					}
					else if (dbType == (DbType)(-1))
					{
						SqlMapper.PackListParameters(command, text, value2);
					}
					else
					{
						bool num = !command.Parameters.Contains(text);
						IDbDataParameter dbDataParameter2;
						if (num)
						{
							dbDataParameter2 = command.CreateParameter();
							dbDataParameter2.ParameterName = text;
						}
						else
						{
							dbDataParameter2 = (IDbDataParameter)command.Parameters[text];
						}
						dbDataParameter2.Direction = value3.ParameterDirection;
						if (handler == null)
						{
							dbDataParameter2.Value = SqlMapper.SanitizeParameterValue(value2);
							if (dbType.HasValue && dbDataParameter2.DbType != dbType)
							{
								dbDataParameter2.DbType = dbType.Value;
							}
							string obj = value2 as string;
							if (obj != null && obj.Length <= 4000)
							{
								dbDataParameter2.Size = 4000;
							}
							if (value3.Size.HasValue)
							{
								dbDataParameter2.Size = value3.Size.Value;
							}
							if (value3.Precision.HasValue)
							{
								dbDataParameter2.Precision = value3.Precision.Value;
							}
							if (value3.Scale.HasValue)
							{
								dbDataParameter2.Scale = value3.Scale.Value;
							}
						}
						else
						{
							if (dbType.HasValue)
							{
								dbDataParameter2.DbType = dbType.Value;
							}
							if (value3.Size.HasValue)
							{
								dbDataParameter2.Size = value3.Size.Value;
							}
							if (value3.Precision.HasValue)
							{
								dbDataParameter2.Precision = value3.Precision.Value;
							}
							if (value3.Scale.HasValue)
							{
								dbDataParameter2.Scale = value3.Scale.Value;
							}
							handler.SetValue(dbDataParameter2, value2 ?? DBNull.Value);
						}
						if (num)
						{
							command.Parameters.Add(dbDataParameter2);
						}
						value3.AttachedParam = dbDataParameter2;
					}
				}
			}
			if (literalTokens.Count != 0)
			{
				SqlMapper.ReplaceLiterals(this, command, literalTokens);
			}
		}

		public T Get<T>(string name)
		{
			ParamInfo paramInfo = parameters[Clean(name)];
			IDbDataParameter attachedParam = paramInfo.AttachedParam;
			object obj = (attachedParam == null) ? paramInfo.Value : attachedParam.Value;
			if (obj == DBNull.Value)
			{
				if (default(T) != null)
				{
					throw new ApplicationException("Attempting to cast a DBNull to a non nullable type! Note that out/return parameters will not have updated values until the data stream completes (after the 'foreach' for Query(..., buffered: false), or after the GridReader has been disposed for QueryMultiple)");
				}
				return default(T);
			}
			return (T)obj;
		}

		public DynamicParameters Output<T>(T target, Expression<Func<T, object>> expression, DbType? dbType = default(DbType?), int? size = default(int?))
		{
			string failMessage = "Expression must be a property/field chain off of a(n) {0} instance";
			failMessage = string.Format(failMessage, typeof(T).Name);
			Action action = delegate
			{
				throw new InvalidOperationException(failMessage);
			};
			MemberExpression lastMemberAccess = expression.Body as MemberExpression;
			if (lastMemberAccess == null || (!(lastMemberAccess.Member is PropertyInfo) && !(lastMemberAccess.Member is FieldInfo)))
			{
				if (expression.Body.NodeType == ExpressionType.Convert && expression.Body.Type == typeof(object) && ((UnaryExpression)expression.Body).Operand is MemberExpression)
				{
					lastMemberAccess = (MemberExpression)((UnaryExpression)expression.Body).Operand;
				}
				else
				{
					action();
				}
			}
			MemberExpression memberExpression = lastMemberAccess;
			List<string> list = new List<string>();
			List<MemberExpression> list2 = new List<MemberExpression>();
			do
			{
				list.Insert(0, memberExpression?.Member.Name);
				list2.Insert(0, memberExpression);
				ParameterExpression parameterExpression = memberExpression?.Expression as ParameterExpression;
				memberExpression = (memberExpression?.Expression as MemberExpression);
				if (parameterExpression != null && parameterExpression.Type == typeof(T))
				{
					break;
				}
				if (memberExpression == null || (!(memberExpression.Member is PropertyInfo) && !(memberExpression.Member is FieldInfo)))
				{
					action();
				}
			}
			while (memberExpression != null);
			string dynamicParamName = string.Concat(list.ToArray());
			string key = string.Join("|", list.ToArray());
			Hashtable cache = CachedOutputSetters<T>.Cache;
			Action<object, DynamicParameters> setter = (Action<object, DynamicParameters>)cache[key];
			if (setter == null)
			{
				DynamicMethod dynamicMethod = new DynamicMethod("ExpressionParam" + Guid.NewGuid().ToString(), null, new Type[2]
				{
					typeof(object),
					GetType()
				}, restrictedSkipVisibility: true);
				ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
				iLGenerator.Emit(OpCodes.Ldarg_0);
				iLGenerator.Emit(OpCodes.Castclass, typeof(T));
				for (int i = 0; i < list2.Count - 1; i++)
				{
					MemberInfo member = list2[0].Member;
					if (member is PropertyInfo)
					{
						MethodInfo getMethod = ((PropertyInfo)member).GetGetMethod(nonPublic: true);
						iLGenerator.Emit(OpCodes.Callvirt, getMethod);
					}
					else
					{
						iLGenerator.Emit(OpCodes.Ldfld, (FieldInfo)member);
					}
				}
				MethodInfo meth = GetType().GetMethod("Get", new Type[1]
				{
					typeof(string)
				}).MakeGenericMethod(lastMemberAccess.Type);
				iLGenerator.Emit(OpCodes.Ldarg_1);
				iLGenerator.Emit(OpCodes.Ldstr, dynamicParamName);
				iLGenerator.Emit(OpCodes.Callvirt, meth);
				MemberInfo member2 = lastMemberAccess.Member;
				if (member2 is PropertyInfo)
				{
					MethodInfo setMethod = ((PropertyInfo)member2).GetSetMethod(nonPublic: true);
					iLGenerator.Emit(OpCodes.Callvirt, setMethod);
				}
				else
				{
					iLGenerator.Emit(OpCodes.Stfld, (FieldInfo)member2);
				}
				iLGenerator.Emit(OpCodes.Ret);
				setter = (Action<object, DynamicParameters>)dynamicMethod.CreateDelegate(typeof(Action<object, DynamicParameters>));
				lock (cache)
				{
					cache[key] = setter;
				}
			}
			(outputCallbacks ?? (outputCallbacks = new List<Action>())).Add(delegate
			{
				Type type = lastMemberAccess?.Type;
				int num = (!size.HasValue && type == typeof(string)) ? 4000 : (size ?? 0);
				if (parameters.TryGetValue(dynamicParamName, out ParamInfo value))
				{
					ParameterDirection parameterDirection3 = value.ParameterDirection = (value.AttachedParam.Direction = ParameterDirection.InputOutput);
					if (value.AttachedParam.Size == 0)
					{
						ParamInfo paramInfo = value;
						int value2 = value.AttachedParam.Size = num;
						paramInfo.Size = value2;
					}
				}
				else
				{
					dbType = ((!dbType.HasValue) ? new DbType?(SqlMapper.LookupDbType(type, type?.Name, demand: true, out SqlMapper.ITypeHandler _)) : dbType);
					Add(dynamicParamName, expression.Compile()(target), null, ParameterDirection.InputOutput, num);
				}
				value = parameters[dynamicParamName];
				value.OutputCallback = setter;
				value.OutputTarget = target;
			});
			return this;
		}

		void SqlMapper.IParameterCallbacks.OnCompleted()
		{
			foreach (ParamInfo item in parameters.Select(delegate(KeyValuePair<string, ParamInfo> p)
			{
				KeyValuePair<string, ParamInfo> keyValuePair = p;
				return keyValuePair.Value;
			}))
			{
				item.OutputCallback?.Invoke(item.OutputTarget, this);
			}
		}
	}
}
