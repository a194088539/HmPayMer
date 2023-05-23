using System;
using System.Reflection;

namespace HM.Framework.DapperExtensions.Mapper
{
	public class PropertyMap : IPropertyMap
	{
		public string Name => PropertyInfo.Name;

		public string ColumnName
		{
			get;
			private set;
		}

		public KeyType KeyType
		{
			get;
			private set;
		}

		public bool Ignored
		{
			get;
			private set;
		}

		public bool IsReadOnly
		{
			get;
			private set;
		}

		public PropertyInfo PropertyInfo
		{
			get;
			private set;
		}

		public PropertyMap(PropertyInfo propertyInfo)
		{
			PropertyInfo = propertyInfo;
			ColumnName = PropertyInfo.Name;
		}

		public PropertyMap Column(string columnName)
		{
			ColumnName = columnName;
			return this;
		}

		public PropertyMap Key(KeyType keyType)
		{
			if (Ignored)
			{
				throw new ArgumentException($"'{Name}' is ignored and cannot be made a key field. ");
			}
			if (IsReadOnly)
			{
				throw new ArgumentException($"'{Name}' is readonly and cannot be made a key field. ");
			}
			KeyType = keyType;
			return this;
		}

		public PropertyMap Ignore()
		{
			if (KeyType != 0)
			{
				throw new ArgumentException($"'{Name}' is a key field and cannot be ignored.");
			}
			Ignored = true;
			return this;
		}

		public PropertyMap ReadOnly()
		{
			if (KeyType != 0)
			{
				throw new ArgumentException($"'{Name}' is a key field and cannot be marked readonly.");
			}
			IsReadOnly = true;
			return this;
		}
	}
}
