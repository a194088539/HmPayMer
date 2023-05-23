using HM.Framework.DapperExtensions.Mapper;
using HM.Framework.DapperExtensions.Sql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HM.Framework.DapperExtensions
{
	public class DapperExtensionsConfiguration : IDapperExtensionsConfiguration
	{
		private readonly ConcurrentDictionary<Type, IClassMapper> _classMaps = new ConcurrentDictionary<Type, IClassMapper>();

		public Type DefaultMapper
		{
			get;
			private set;
		}

		public IList<Assembly> MappingAssemblies
		{
			get;
			private set;
		}

		public ISqlDialect Dialect
		{
			get;
			private set;
		}

		public DapperExtensionsConfiguration()
			: this(typeof(AutoClassMapper<>), new List<Assembly>(), new SqlServerDialect())
		{
		}

		public DapperExtensionsConfiguration(Type defaultMapper, IList<Assembly> mappingAssemblies, ISqlDialect sqlDialect)
		{
			DefaultMapper = defaultMapper;
			MappingAssemblies = (mappingAssemblies ?? new List<Assembly>());
			Dialect = sqlDialect;
		}

		public IClassMapper GetMap(Type entityType)
		{
			if (!_classMaps.TryGetValue(entityType, out IClassMapper value))
			{
				Type type = GetMapType(entityType);
				if (type == null)
				{
					type = DefaultMapper.MakeGenericType(entityType);
				}
				value = (Activator.CreateInstance(type) as IClassMapper);
				_classMaps[entityType] = value;
			}
			return value;
		}

		public IClassMapper GetMap<T>() where T : class
		{
			return GetMap(typeof(T));
		}

		public void ClearCache()
		{
			_classMaps.Clear();
		}

		public Guid GetNextGuid()
		{
			byte[] array = Guid.NewGuid().ToByteArray();
			DateTime dateTime = new DateTime(1900, 1, 1);
			DateTime now = DateTime.Now;
			TimeSpan timeSpan = new TimeSpan(now.Ticks - dateTime.Ticks);
			TimeSpan timeOfDay = now.TimeOfDay;
			byte[] bytes = BitConverter.GetBytes(timeSpan.Days);
			byte[] bytes2 = BitConverter.GetBytes((long)(timeOfDay.TotalMilliseconds / 3.333333));
			Array.Reverse(bytes);
			Array.Reverse(bytes2);
			Array.Copy(bytes, bytes.Length - 2, array, array.Length - 6, 2);
			Array.Copy(bytes2, bytes2.Length - 4, array, array.Length - 4, 4);
			return new Guid(array);
		}

		protected virtual Type GetMapType(Type entityType)
		{
			Func<Assembly, Type> func = (Assembly a) => (from _003C_003Eh__TransparentIdentifier0 in (from type in a.GetTypes()
			select new
			{
				type = type,
				interfaceType = type.GetInterface(typeof(IClassMapper<>).FullName)
			}).Where(_003C_003Eh__TransparentIdentifier0 =>
			{
				if (_003C_003Eh__TransparentIdentifier0.interfaceType != null)
				{
					return _003C_003Eh__TransparentIdentifier0.interfaceType.GetGenericArguments()[0] == entityType;
				}
				return false;
			})
			select _003C_003Eh__TransparentIdentifier0.type).SingleOrDefault();
			Type type2 = func(entityType.Assembly);
			if (type2 != null)
			{
				return type2;
			}
			foreach (Assembly mappingAssembly in MappingAssemblies)
			{
				type2 = func(mappingAssembly);
				if (type2 != null)
				{
					return type2;
				}
			}
			return func(entityType.Assembly);
		}
	}
}
