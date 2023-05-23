using System;

namespace HM.Framework.Dapper
{
	[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
	public sealed class ExplicitConstructorAttribute : Attribute
	{
	}
}
