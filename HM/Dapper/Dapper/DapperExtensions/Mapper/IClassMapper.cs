using System;
using System.Collections.Generic;

namespace HM.Framework.DapperExtensions.Mapper
{
	public interface IClassMapper
	{
		string SchemaName
		{
			get;
		}

		string TableName
		{
			get;
		}

		IList<IPropertyMap> Properties
		{
			get;
		}

		Type EntityType
		{
			get;
		}
	}
	public interface IClassMapper<T> : IClassMapper where T : class
	{
	}
}
