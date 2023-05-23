using System;
using System.Data;

namespace HM.Framework.Dapper
{
	internal class FeatureSupport
	{
		private static readonly FeatureSupport Default = new FeatureSupport(arrays: false);

		private static readonly FeatureSupport Postgres = new FeatureSupport(arrays: true);

		public bool Arrays
		{
			get;
		}

		public static FeatureSupport Get(IDbConnection connection)
		{
			if (string.Equals(connection?.GetType().Name, "npgsqlconnection", StringComparison.OrdinalIgnoreCase))
			{
				return Postgres;
			}
			return Default;
		}

		private FeatureSupport(bool arrays)
		{
			Arrays = arrays;
		}
	}
}
