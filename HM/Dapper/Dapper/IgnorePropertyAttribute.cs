using System;

namespace HM.Framework.Dapper
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class IgnorePropertyAttribute : Attribute
	{
		public bool Value
		{
			get;
			set;
		}

		public IgnorePropertyAttribute(bool ignore)
		{
			Value = ignore;
		}
	}
}
