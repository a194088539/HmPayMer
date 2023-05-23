using System;

namespace HM.Framework.Entity
{
	[AttributeUsage(AttributeTargets.Property)]
	public class ColumnAttrubute : Attribute
	{
		public string Name
		{
			get;
			set;
		}

		public ColumnAttrubute(string columnName)
		{
			Name = columnName;
		}
	}
}
