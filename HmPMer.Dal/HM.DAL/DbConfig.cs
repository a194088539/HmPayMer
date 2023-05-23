using HM.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace HM.DAL
{
	public class DbConfig : ConfigurationSection
	{
		public List<string> ConnectionStrings
		{
			get
			{
                string connectionStrings = "Data Source=127.0.0.1;Initial Catalog=HmPayMerdb;Persist Security Info=True;User ID=sa;Password=Fr20729327;max pool size=1000";

                List<string> list = new List<string>();
				string[] array = connectionStrings.Split(new string[1]
				{
					"|||"
				}, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < array.Length; i++)
				{
					list.Add(array[i]);
				}
				return list;
			}
		}

		public List<string> ConnectionWriteString
		{
			get
			{
                string connectionStrings = "Data Source=127.0.0.1;Initial Catalog=HmPayMerdb;Persist Security Info=True;User ID=sa;Password=Fr20729327;max pool size=1000";
                List<string> list = new List<string>();
				string[] array = connectionStrings.Split(new string[1]
				{
					"|||"
				}, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < array.Length; i++)
				{
					list.Add(array[i]);
				}
				return list;
			}
		}
	}
}
