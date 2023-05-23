using System;
using System.Reflection;

namespace HM.Framework.Dapper
{
	public sealed class CustomPropertyTypeMap : SqlMapper.ITypeMap
	{
		private readonly Type _type;

		private readonly Func<Type, string, PropertyInfo> _propertySelector;

		public CustomPropertyTypeMap(Type type, Func<Type, string, PropertyInfo> propertySelector)
		{
			if ((object)type == null)
			{
				throw new ArgumentNullException("type");
			}
			_type = type;
			if (propertySelector == null)
			{
				throw new ArgumentNullException("propertySelector");
			}
			_propertySelector = propertySelector;
		}

		public ConstructorInfo FindConstructor(string[] names, Type[] types)
		{
			return _type.GetConstructor(new Type[0]);
		}

		public ConstructorInfo FindExplicitConstructor()
		{
			return null;
		}

		public SqlMapper.IMemberMap GetConstructorParameter(ConstructorInfo constructor, string columnName)
		{
			throw new NotSupportedException();
		}

		public SqlMapper.IMemberMap GetMember(string columnName)
		{
			PropertyInfo propertyInfo = _propertySelector(_type, columnName);
			if (!(propertyInfo != null))
			{
				return null;
			}
			return new SimpleMemberMap(columnName, propertyInfo);
		}
	}
}
