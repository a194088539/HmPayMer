using HM.Framework.Dapper;
using HM.Framework.Dapper.Contrib.Extensions;
using HM.Framework.Logging;
using HmPMer.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace HM.DAL
{
	public class DalContext
	{
		private static string connectionString;

		private static string connectionWriteString;

		static DalContext()
		{
			connectionString = "";
			connectionWriteString = "";
			try
			{
				connectionString = GetConnectionString();
				connectionWriteString = GetConnectionWriteString();
			}
			catch (Exception exception)
			{
				LogUtil.Error("初始化数据库异常", exception);
			}
		}

		private static string GetConnectionString()
		{
			return new DbConfig().ConnectionStrings[0];
		}

		private static string GetConnectionWriteString()
		{
			return new DbConfig().ConnectionWriteString[0];
		}

		private static IDbConnection CreateConnection()
		{
			return new SqlConnection(connectionString);
		}

		private static IDbConnection CreateConnectionWrite()
		{
			return new SqlConnection(connectionWriteString);
		}

		public static T GetModel<T>(string sql, object param = null)
		{
			try
			{
				using (IDbConnection cnn = CreateConnection())
				{
					return cnn.QueryFirstOrDefault<T>(sql, param);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public static T GetModelForProce<T>(string proName, object param = null)
		{
			try
			{
				using (IDbConnection cnn = CreateConnection())
				{
					return cnn.QueryFirstOrDefault<T>(proName, param, null, null, CommandType.StoredProcedure);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public static List<T> GetList<T>(string sql, object param = null)
		{
			try
			{
				using (IDbConnection cnn = CreateConnection())
				{
					return cnn.Query<T>(sql, param).ToList();
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public static List<T> GetListForProce<T>(string proName, object param = null)
		{
			try
			{
				using (IDbConnection cnn = CreateConnection())
				{
					return cnn.Query<T>(proName, param, null, buffered: true, null, CommandType.StoredProcedure).ToList();
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public static List<T> GetPage<T>(string sql, string sqlCount, string fields, string sort, ref Paging paging, object param = null)
		{
			paging.TotalCount = GetSingVal<int>(sqlCount, param);
			string empty = string.Empty;
			empty = ((paging.PageIndex != 1) ? string.Format("\r\n                WITH PAGETABLE AS (\r\n                    SELECT row_number() over (order by {4}) as TB_ROW, {0} \r\n                    FROM  (\r\n\t                    {1}\r\n                    ) as _TABLE)\r\n                    SELECT TOP {2} * \t  \r\n                    FROM PAGETABLE WHERE TB_ROW>{3} ", fields, sql, paging.PageSize, (paging.PageIndex - 1) * paging.PageSize, sort) : $"\r\n               SELECT TOP {paging.PageSize} {fields}\r\n                FROM  (\r\n\t               {sql}\r\n                ) as _TABLE ORDER BY {sort}");
			return GetList<T>(empty, param);
		}

		public static T GetSingVal<T>(string sql, object param = null)
		{
			try
			{
				using (IDbConnection cnn = CreateConnection())
				{
					return cnn.QuerySingleOrDefault<T>(sql, param);
				}
			}
			catch (Exception)
			{
				return default(T);
			}
		}

		public static T GetSingValForProce<T>(string proName, object param = null)
		{
			try
			{
				using (IDbConnection cnn = CreateConnection())
				{
					return cnn.QuerySingleOrDefault<T>(proName, param, null, null, CommandType.StoredProcedure);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public static DataSet GetDataSet(string sql)
		{
			try
			{
				using (IDbConnection dbConnection = CreateConnection())
				{
					dbConnection.Open();
					SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sql, connectionString);
					DataSet dataSet = new DataSet();
					sqlDataAdapter.Fill(dataSet);
					return dataSet;
				}
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static DataTable GetDataTable(string sql)
		{
			DataSet dataSet = GetDataSet(sql);
			if (dataSet != null && dataSet.Tables.Count > 0)
			{
				return dataSet.Tables[0];
			}
			return null;
		}

		public static long Insert<T>(T model) where T : class
		{
			try
			{
				using (IDbConnection connection = CreateConnectionWrite())
				{
					return connection.Insert(model);
				}
			}
			catch (Exception ex)
			{
				LogUtil.Error("Insert.Error", ex);
				throw ex;
			}
		}

		public static long InsertBat<T>(IEnumerable<T> list) where T : class
		{
			try
			{
				using (IDbConnection conn = CreateConnectionWrite())
				{
					return conn.InsertBatch(list);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public static bool Update<T>(T model) where T : class
		{
			try
			{
				using (IDbConnection connection = CreateConnectionWrite())
				{
					return connection.Update(model);
				}
			}
			catch (Exception ex)
			{
				LogUtil.Error("Update.Error", ex);
				throw ex;
			}
		}

		public static bool Delete<T>(T model) where T : class
		{
			try
			{
				using (IDbConnection connection = CreateConnectionWrite())
				{
					return connection.Delete(model);
				}
			}
			catch (Exception ex)
			{
				LogUtil.Error("Delete.Error", ex);
				throw ex;
			}
		}

		public static int ExecuteSql(string sql, object param = null)
		{
			try
			{
				using (IDbConnection cnn = CreateConnectionWrite())
				{
					return cnn.Execute(sql, param);
				}
			}
			catch (Exception exception)
			{
				LogUtil.Error("ExecuteSql.Error,sql=" + sql, exception);
				return -1;
			}
		}

		public static int ExecuteSql(List<string> sqlList, object param = null)
		{
			int num = 0;
			try
			{
				using (IDbConnection dbConnection = CreateConnectionWrite())
				{
					dbConnection.Open();
					foreach (string sql in sqlList)
					{
						num += dbConnection.Execute(sql, param);
					}
					return num;
				}
			}
			catch (Exception exception)
			{
				LogUtil.Error("ExecuteSql.Error", exception);
				return -1;
			}
		}

		public static int ExecuteSqlTransaction(List<Tuple<string, object>> list)
		{
			int num = 0;
			IDbTransaction dbTransaction = null;
			try
			{
				using (IDbConnection dbConnection = CreateConnectionWrite())
				{
					dbConnection.Open();
					dbTransaction = dbConnection.BeginTransaction();
					foreach (Tuple<string, object> item3 in list)
					{
						string item = item3.Item1;
						object item2 = item3.Item2;
						num += dbConnection.Execute(item, item2, dbTransaction);
					}
					dbTransaction.Commit();
					return num;
				}
			}
			catch (Exception exception)
			{
				dbTransaction?.Rollback();
				LogUtil.Error("ExecuteSqlTransaction", exception);
				return 0;
			}
		}

		public static int ExecuteProce(string proName, DynamicParameters param)
		{
			try
			{
				using (IDbConnection cnn = CreateConnection())
				{
					return cnn.Execute(proName, param, null, null, CommandType.StoredProcedure);
				}
			}
			catch (Exception ex)
			{
				LogUtil.Error("ExecuteProce.proName=" + proName, ex);
				throw ex;
			}
		}

        public static string EscapeString(string str)
        {
            if(str == null)
            {
                return null;
            }
            return str.Replace("'", "").Replace(";","");
        }

    }
}
