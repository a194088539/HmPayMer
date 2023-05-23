using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HM.Framework.Dapper
{
	public sealed class DefaultTypeMap : SqlMapper.ITypeMap
	{
		private readonly List<FieldInfo> _fields;

		private readonly Type _type;

		public static bool MatchNamesWithUnderscores
		{
			get;
			set;
		}

		public List<PropertyInfo> Properties
		{
			get;
		}

		public DefaultTypeMap(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			_fields = GetSettableFields(type);
			Properties = GetSettableProps(type);
			_type = type;
		}

		internal static MethodInfo GetPropertySetter(PropertyInfo propertyInfo, Type type)
		{
			if (propertyInfo.DeclaringType == type)
			{
				return propertyInfo.GetSetMethod(nonPublic: true);
			}
			return propertyInfo.DeclaringType.GetProperty(propertyInfo.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.DefaultBinder, propertyInfo.PropertyType, (from p in propertyInfo.GetIndexParameters()
			select p.ParameterType).ToArray(), null).GetSetMethod(nonPublic: true);
		}

		internal static List<PropertyInfo> GetSettableProps(Type t)
		{
			return (from p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			where GetPropertySetter(p, t) != null
			select p).ToList();
		}

		internal static List<FieldInfo> GetSettableFields(Type t)
		{
			return t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();
		}

		public ConstructorInfo FindConstructor(string[] names, Type[] types)
		{
			foreach (ConstructorInfo item in _type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).OrderBy(delegate(ConstructorInfo c)
			{
				if (!c.IsPublic)
				{
					if (!c.IsPrivate)
					{
						return 1;
					}
					return 2;
				}
				return 0;
			}).ThenBy((ConstructorInfo c) => c.GetParameters().Length))
			{
				ParameterInfo[] parameters = item.GetParameters();
				if (parameters.Length == 0)
				{
					return item;
				}
				if (parameters.Length == types.Length)
				{
					int i;
					for (i = 0; i < parameters.Length && string.Equals(parameters[i].Name, names[i], StringComparison.OrdinalIgnoreCase); i++)
					{
						if (!(types[i] == typeof(byte[])) || !(parameters[i].ParameterType.FullName == "System.Data.Linq.Binary"))
						{
							Type type = Nullable.GetUnderlyingType(parameters[i].ParameterType) ?? parameters[i].ParameterType;
							if (type != types[i] && !SqlMapper.HasTypeHandler(type) && (!type.IsEnum() || !(Enum.GetUnderlyingType(type) == types[i])) && (!(type == typeof(char)) || !(types[i] == typeof(string))) && (!type.IsEnum() || !(types[i] == typeof(string))))
							{
								break;
							}
						}
					}
					if (i == parameters.Length)
					{
						return item;
					}
				}
			}
			return null;
		}

		public ConstructorInfo FindExplicitConstructor()
		{
			List<ConstructorInfo> list = (from c in _type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			where c.GetCustomAttributes(typeof(ExplicitConstructorAttribute), inherit: true).Length != 0
			select c).ToList();
			if (list.Count == 1)
			{
				return list[0];
			}
			return null;
		}

		public SqlMapper.IMemberMap GetConstructorParameter(ConstructorInfo constructor, string columnName)
		{
			ParameterInfo[] parameters = constructor.GetParameters();
			return new SimpleMemberMap(columnName, parameters.FirstOrDefault((ParameterInfo p) => string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase)));
		}

		public SqlMapper.IMemberMap GetMember(string columnName)
		{
			PropertyInfo propertyInfo = Properties.Find((PropertyInfo p) => string.Equals(p.Name, columnName, StringComparison.Ordinal)) ?? Properties.Find((PropertyInfo p) => string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase));
			if (propertyInfo == null && MatchNamesWithUnderscores)
			{
				propertyInfo = (Properties.Find((PropertyInfo p) => string.Equals(p.Name, columnName.Replace("_", ""), StringComparison.Ordinal)) ?? Properties.Find((PropertyInfo p) => string.Equals(p.Name, columnName.Replace("_", ""), StringComparison.OrdinalIgnoreCase)));
			}
			if (propertyInfo != null)
			{
				return new SimpleMemberMap(columnName, propertyInfo);
			}
			string backingFieldName = "<" + columnName + ">k__BackingField";
			FieldInfo fieldInfo = _fields.Find((FieldInfo p) => string.Equals(p.Name, columnName, StringComparison.Ordinal)) ?? _fields.Find((FieldInfo p) => string.Equals(p.Name, backingFieldName, StringComparison.Ordinal)) ?? _fields.Find((FieldInfo p) => string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase)) ?? _fields.Find((FieldInfo p) => string.Equals(p.Name, backingFieldName, StringComparison.OrdinalIgnoreCase));
			if (fieldInfo == null && MatchNamesWithUnderscores)
			{
				string effectiveColumnName = columnName.Replace("_", "");
				backingFieldName = "<" + effectiveColumnName + ">k__BackingField";
				fieldInfo = (_fields.Find((FieldInfo p) => string.Equals(p.Name, effectiveColumnName, StringComparison.Ordinal)) ?? _fields.Find((FieldInfo p) => string.Equals(p.Name, backingFieldName, StringComparison.Ordinal)) ?? _fields.Find((FieldInfo p) => string.Equals(p.Name, effectiveColumnName, StringComparison.OrdinalIgnoreCase)) ?? _fields.Find((FieldInfo p) => string.Equals(p.Name, backingFieldName, StringComparison.OrdinalIgnoreCase)));
			}
			if (fieldInfo != null)
			{
				return new SimpleMemberMap(columnName, fieldInfo);
			}
			return null;
		}
	}
}
