using System;

namespace HM.Framework.Entity
{
	[AttributeUsage(AttributeTargets.Class)]
	public class TableAttribute : Attribute
	{
		public string Name
		{
			get;
			set;
		}

		public TableAttribute(string tableName)
		{
			Name = tableName;
		}
	}
}
