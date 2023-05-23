using System;
using System.Reflection;

namespace HM.Framework.Dapper
{
	internal sealed class SimpleMemberMap : SqlMapper.IMemberMap
	{
		public string ColumnName
		{
			get;
		}

		public Type MemberType
		{
			get
			{
				object obj = Field?.FieldType;
				if (obj == null)
				{
					obj = Property?.PropertyType;
					if (obj == null)
					{
						ParameterInfo parameter = Parameter;
						if (parameter == null)
						{
							return null;
						}
						obj = parameter.ParameterType;
					}
				}
				return (Type)obj;
			}
		}

		public PropertyInfo Property
		{
			get;
		}

		public FieldInfo Field
		{
			get;
		}

		public ParameterInfo Parameter
		{
			get;
		}

		public SimpleMemberMap(string columnName, PropertyInfo property)
		{
			if (columnName == null)
			{
				throw new ArgumentNullException("columnName");
			}
			ColumnName = columnName;
			if ((object)property == null)
			{
				throw new ArgumentNullException("property");
			}
			Property = property;
		}

		public SimpleMemberMap(string columnName, FieldInfo field)
		{
			if (columnName == null)
			{
				throw new ArgumentNullException("columnName");
			}
			ColumnName = columnName;
			if ((object)field == null)
			{
				throw new ArgumentNullException("field");
			}
			Field = field;
		}

		public SimpleMemberMap(string columnName, ParameterInfo parameter)
		{
			if (columnName == null)
			{
				throw new ArgumentNullException("columnName");
			}
			ColumnName = columnName;
			if (parameter == null)
			{
				throw new ArgumentNullException("parameter");
			}
			Parameter = parameter;
		}
	}
}
