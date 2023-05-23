using System;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace HM.Framework.Dapper
{
	public struct CommandDefinition
	{
		private static SqlMapper.Link<Type, Action<IDbCommand>> commandInitCache;

		public string CommandText
		{
			get;
		}

		public object Parameters
		{
			get;
		}

		public IDbTransaction Transaction
		{
			get;
		}

		public int? CommandTimeout
		{
			get;
		}

		public CommandType? CommandType
		{
			get;
		}

		public bool Buffered => (Flags & CommandFlags.Buffered) != CommandFlags.None;

		internal bool AddToCache => (Flags & CommandFlags.NoCache) == CommandFlags.None;

		public CommandFlags Flags
		{
			get;
		}

		public bool Pipelined => (Flags & CommandFlags.Pipelined) != CommandFlags.None;

		public CancellationToken CancellationToken
		{
			get;
		}

		internal static CommandDefinition ForCallback(object parameters)
		{
			if (parameters is DynamicParameters)
			{
				return new CommandDefinition(parameters);
			}
			return default(CommandDefinition);
		}

		internal void OnCompleted()
		{
			(Parameters as SqlMapper.IParameterCallbacks)?.OnCompleted();
		}

		public CommandDefinition(string commandText, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?), CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = default(CancellationToken))
		{
			CommandText = commandText;
			Parameters = parameters;
			Transaction = transaction;
			CommandTimeout = commandTimeout;
			CommandType = commandType;
			Flags = flags;
			CancellationToken = cancellationToken;
		}

		private CommandDefinition(object parameters)
		{
			this = default(CommandDefinition);
			Parameters = parameters;
		}

		internal IDbCommand SetupCommand(IDbConnection cnn, Action<IDbCommand, object> paramReader)
		{
			IDbCommand dbCommand = cnn.CreateCommand();
			GetInit(dbCommand.GetType())?.Invoke(dbCommand);
			if (Transaction != null)
			{
				dbCommand.Transaction = Transaction;
			}
			dbCommand.CommandText = CommandText;
			if (CommandTimeout.HasValue)
			{
				dbCommand.CommandTimeout = CommandTimeout.Value;
			}
			else if (SqlMapper.Settings.CommandTimeout.HasValue)
			{
				dbCommand.CommandTimeout = SqlMapper.Settings.CommandTimeout.Value;
			}
			if (CommandType.HasValue)
			{
				dbCommand.CommandType = CommandType.Value;
			}
			paramReader?.Invoke(dbCommand, Parameters);
			return dbCommand;
		}

		private static Action<IDbCommand> GetInit(Type commandType)
		{
			if (commandType == null)
			{
				return null;
			}
			if (SqlMapper.Link<Type, Action<IDbCommand>>.TryGet(commandInitCache, commandType, out Action<IDbCommand> value))
			{
				return value;
			}
			MethodInfo basicPropertySetter = GetBasicPropertySetter(commandType, "BindByName", typeof(bool));
			MethodInfo basicPropertySetter2 = GetBasicPropertySetter(commandType, "InitialLONGFetchSize", typeof(int));
			value = null;
			if (basicPropertySetter != null || basicPropertySetter2 != null)
			{
				DynamicMethod dynamicMethod = new DynamicMethod(commandType.Name + "_init", null, new Type[1]
				{
					typeof(IDbCommand)
				});
				ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
				if (basicPropertySetter != null)
				{
					iLGenerator.Emit(OpCodes.Ldarg_0);
					iLGenerator.Emit(OpCodes.Castclass, commandType);
					iLGenerator.Emit(OpCodes.Ldc_I4_1);
					iLGenerator.EmitCall(OpCodes.Callvirt, basicPropertySetter, null);
				}
				if (basicPropertySetter2 != null)
				{
					iLGenerator.Emit(OpCodes.Ldarg_0);
					iLGenerator.Emit(OpCodes.Castclass, commandType);
					iLGenerator.Emit(OpCodes.Ldc_I4_M1);
					iLGenerator.EmitCall(OpCodes.Callvirt, basicPropertySetter2, null);
				}
				iLGenerator.Emit(OpCodes.Ret);
				value = (Action<IDbCommand>)dynamicMethod.CreateDelegate(typeof(Action<IDbCommand>));
			}
			SqlMapper.Link<Type, Action<IDbCommand>>.TryAdd(ref commandInitCache, commandType, ref value);
			return value;
		}

		private static MethodInfo GetBasicPropertySetter(Type declaringType, string name, Type expectedType)
		{
			PropertyInfo property = declaringType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
			if ((object)property != null && property.CanWrite && property.PropertyType == expectedType && property.GetIndexParameters().Length == 0)
			{
				return property.GetSetMethod();
			}
			return null;
		}
	}
}
