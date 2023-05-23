using System.Configuration;

namespace HM.DAL
{
	public class DbConnectionString : ConfigurationElement
	{
		[ConfigurationProperty("DbName", IsRequired = true)]
		public string Name
		{
			get
			{
				return base["DbName"].ToString();
			}
		}

		[ConfigurationProperty("ConnectionString", IsRequired = true)]
		public string ConnectionString
		{
			get
			{
				return base["ConnectionString"].ToString();
			}
		}
	}
}
