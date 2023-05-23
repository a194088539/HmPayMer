using System.Reflection;

namespace HM.Framework.DapperExtensions.Mapper
{
	public interface IPropertyMap
	{
		string Name
		{
			get;
		}

		string ColumnName
		{
			get;
		}

		bool Ignored
		{
			get;
		}

		bool IsReadOnly
		{
			get;
		}

		KeyType KeyType
		{
			get;
		}

		PropertyInfo PropertyInfo
		{
			get;
		}
	}
}
