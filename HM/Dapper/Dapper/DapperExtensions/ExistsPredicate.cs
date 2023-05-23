using HM.Framework.DapperExtensions.Mapper;
using HM.Framework.DapperExtensions.Sql;
using System;
using System.Collections.Generic;

namespace HM.Framework.DapperExtensions
{
	public class ExistsPredicate<TSub> : IExistsPredicate, IPredicate where TSub : class
	{
		public IPredicate Predicate
		{
			get;
			set;
		}

		public bool Not
		{
			get;
			set;
		}

		public string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters)
		{
			IClassMapper classMapper = GetClassMapper(typeof(TSub), sqlGenerator.Configuration);
			return string.Format("({0}EXISTS (SELECT 1 FROM {1} WHERE {2}))", Not ? "NOT " : string.Empty, sqlGenerator.GetTableName(classMapper), Predicate.GetSql(sqlGenerator, parameters));
		}

		protected virtual IClassMapper GetClassMapper(Type type, IDapperExtensionsConfiguration configuration)
		{
			IClassMapper map = configuration.GetMap(type);
			if (map == null)
			{
				throw new NullReferenceException($"Map was not found for {type}");
			}
			return map;
		}
	}
}
