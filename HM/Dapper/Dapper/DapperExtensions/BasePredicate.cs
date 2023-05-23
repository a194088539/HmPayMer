using HM.Framework.DapperExtensions.Mapper;
using HM.Framework.DapperExtensions.Sql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HM.Framework.DapperExtensions
{
	public abstract class BasePredicate : IBasePredicate, IPredicate
	{
		public string PropertyName
		{
			get;
			set;
		}

		public abstract string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters);

		protected virtual string GetColumnName(Type entityType, ISqlGenerator sqlGenerator, string propertyName)
		{
			IClassMapper map = sqlGenerator.Configuration.GetMap(entityType);
			if (map == null)
			{
				throw new NullReferenceException($"Map was not found for {entityType}");
			}
			IPropertyMap propertyMap = map.Properties.SingleOrDefault((IPropertyMap p) => p.Name == propertyName);
			if (propertyMap == null)
			{
				throw new NullReferenceException($"{propertyName} was not found for {entityType}");
			}
			return sqlGenerator.GetColumnName(map, propertyMap, includeAlias: false);
		}
	}
}
