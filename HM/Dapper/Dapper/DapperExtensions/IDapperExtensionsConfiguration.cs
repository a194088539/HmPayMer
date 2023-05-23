using HM.Framework.DapperExtensions.Mapper;
using HM.Framework.DapperExtensions.Sql;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace HM.Framework.DapperExtensions
{
	public interface IDapperExtensionsConfiguration
	{
		Type DefaultMapper
		{
			get;
		}

		IList<Assembly> MappingAssemblies
		{
			get;
		}

		ISqlDialect Dialect
		{
			get;
		}

		IClassMapper GetMap(Type entityType);

		IClassMapper GetMap<T>() where T : class;

		void ClearCache();

		Guid GetNextGuid();
	}
}
