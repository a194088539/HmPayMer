using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;

namespace HM.Framework.DapperExtensions.Mapper
{
	public class ClassMapper<T> : IClassMapper<T>, IClassMapper where T : class
	{
		public string SchemaName
		{
			get;
			protected set;
		}

		public string TableName
		{
			get;
			protected set;
		}

		public IList<IPropertyMap> Properties
		{
			get;
			private set;
		}

		public Type EntityType => typeof(T);

		protected Dictionary<Type, KeyType> PropertyTypeKeyTypeMapping
		{
			get;
			private set;
		}

		public ClassMapper()
		{
			PropertyTypeKeyTypeMapping = new Dictionary<Type, KeyType>
			{
				{
					typeof(byte),
					KeyType.Identity
				},
				{
					typeof(byte?),
					KeyType.Identity
				},
				{
					typeof(sbyte),
					KeyType.Identity
				},
				{
					typeof(sbyte?),
					KeyType.Identity
				},
				{
					typeof(short),
					KeyType.Identity
				},
				{
					typeof(short?),
					KeyType.Identity
				},
				{
					typeof(ushort),
					KeyType.Identity
				},
				{
					typeof(ushort?),
					KeyType.Identity
				},
				{
					typeof(int),
					KeyType.Identity
				},
				{
					typeof(int?),
					KeyType.Identity
				},
				{
					typeof(uint),
					KeyType.Identity
				},
				{
					typeof(uint?),
					KeyType.Identity
				},
				{
					typeof(long),
					KeyType.Identity
				},
				{
					typeof(long?),
					KeyType.Identity
				},
				{
					typeof(ulong),
					KeyType.Identity
				},
				{
					typeof(ulong?),
					KeyType.Identity
				},
				{
					typeof(BigInteger),
					KeyType.Identity
				},
				{
					typeof(BigInteger?),
					KeyType.Identity
				},
				{
					typeof(Guid),
					KeyType.Guid
				},
				{
					typeof(Guid?),
					KeyType.Guid
				}
			};
			Properties = new List<IPropertyMap>();
			Table(typeof(T).Name);
		}

		public virtual void Schema(string schemaName)
		{
			SchemaName = schemaName;
		}

		public virtual void Table(string tableName)
		{
			TableName = tableName;
		}

		protected virtual void AutoMap()
		{
			AutoMap(null);
		}

		protected virtual void AutoMap(Func<Type, PropertyInfo, bool> canMap)
		{
			Type typeFromHandle = typeof(T);
			bool flag = Properties.Any((IPropertyMap p) => p.KeyType != KeyType.NotAKey);
			PropertyMap propertyMap = null;
			PropertyInfo[] properties = typeFromHandle.GetProperties();
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (!Properties.Any((IPropertyMap p) => p.Name.Equals(propertyInfo.Name, StringComparison.InvariantCultureIgnoreCase)) && (canMap == null || canMap(typeFromHandle, propertyInfo)))
				{
					PropertyMap propertyMap2 = Map(propertyInfo);
					if (!flag)
					{
						if (string.Equals(propertyMap2.PropertyInfo.Name, "id", StringComparison.InvariantCultureIgnoreCase))
						{
							propertyMap = propertyMap2;
						}
						if (propertyMap == null && propertyMap2.PropertyInfo.Name.EndsWith("id", ignoreCase: true, CultureInfo.InvariantCulture))
						{
							propertyMap = propertyMap2;
						}
					}
				}
			}
			propertyMap?.Key(PropertyTypeKeyTypeMapping.ContainsKey(propertyMap.PropertyInfo.PropertyType) ? PropertyTypeKeyTypeMapping[propertyMap.PropertyInfo.PropertyType] : KeyType.Assigned);
		}

		protected PropertyMap Map(Expression<Func<T, object>> expression)
		{
			PropertyInfo propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
			return Map(propertyInfo);
		}

		protected PropertyMap Map(PropertyInfo propertyInfo)
		{
			PropertyMap propertyMap = new PropertyMap(propertyInfo);
			GuardForDuplicatePropertyMap(propertyMap);
			Properties.Add(propertyMap);
			return propertyMap;
		}

		private void GuardForDuplicatePropertyMap(PropertyMap result)
		{
			if (Properties.Any((IPropertyMap p) => p.Name.Equals(result.Name)))
			{
				throw new ArgumentException($"Duplicate mapping for property {result.Name} detected.");
			}
		}
	}
}
