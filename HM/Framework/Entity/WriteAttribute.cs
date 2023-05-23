using System;

namespace HM.Framework.Entity
{
	[AttributeUsage(AttributeTargets.Property)]
	public class WriteAttribute : Attribute
	{
		public bool Write
		{
			get;
		}

		public WriteAttribute(bool write)
		{
			Write = write;
		}
	}
}
