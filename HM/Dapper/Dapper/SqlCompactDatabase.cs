using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace HM.Framework.Dapper
{
	public abstract class SqlCompactDatabase<TDatabase> : Database<TDatabase> where TDatabase : Database<TDatabase>, new()
	{
		public class SqlCompactTable<T> : Table<T>
		{
			public SqlCompactTable(Database<TDatabase> database, string likelyTableName)
				: base(database, likelyTableName)
			{
			}

			public override int? Insert(dynamic data)
			{
				List<string> paramNames = Table<T, int>.GetParamNames((object)data);
				paramNames.Remove("Id");
				string text = string.Join(",", paramNames);
				string text2 = string.Join(",", from p in paramNames
				select "@" + p);
				string sql = "insert " + base.TableName + " (" + text + ") values (" + text2 + ")";
				if (database.Execute(sql, (object)data) != 1)
				{
					return null;
				}
				return (int)database.Query<decimal>("SELECT @@IDENTITY AS LastInsertedId").Single();
			}
		}

		public static TDatabase Init(DbConnection connection)
		{
			TDatabase val = new TDatabase();
			val.InitDatabase(connection, 0);
			return val;
		}

		internal override Action<TDatabase> CreateTableConstructorForTable()
		{
			return CreateTableConstructor(typeof(SqlCompactTable<>));
		}
	}
}
