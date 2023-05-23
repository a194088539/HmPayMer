using HM.DAL;
using HmPMer.Entity;
using System.Collections.Generic;
using System.Text;

namespace HmPMer.Dal
{
	public class InterfaceDal
	{
		public List<InterfaceBusiness> LoadInterfaceBusiness(InterfaceBusiness param, ref Paging paging)
		{
			string str = $"\r\n            SELECT\tA.*\r\n                   ,C.RiskSchemeId\r\n\t               ,C.SchemeName AS [RiskSchemeName] \r\n\t            FROM InterfaceBusiness    AS A\r\n\t            LEFT JOIN dbo.RiskSetting AS B ON B.RiskSettingType={1} AND A.Code=B.TargetId\r\n\t            LEFT JOIN dbo.RiskScheme  AS C ON B.RiskSchemeId=C.RiskSchemeId\r\n\t            WHERE 1=1 ";
			string str2 = " select count(*) from InterfaceBusiness A WHERE 1=1  ";
			string text = "";
            param.Name = DalContext.EscapeString(param.Name);
            if (!string.IsNullOrEmpty(param.Name))
			{
				text += $" AND A.Name like '%{param.Name}%' ";
			}
			string sql = str + text;
			str2 += text;
			return DalContext.GetPage<InterfaceBusiness>(sql, str2, " * ", " OrderNo DESC ", ref paging);
		}

		public List<InterfaceBusiness> GetInterfaceBusinessList(int isEnabled)
		{
			string text = " select * from InterfaceBusiness  ";
			if (isEnabled != -1)
			{
				text = text + " where isEnabled= " + isEnabled;
			}
			return DalContext.GetList<InterfaceBusiness>(text);
		}

		public List<InterfaceBusiness> GetIBForAgentPay(int AgentPay)
		{
			return DalContext.GetList<InterfaceBusiness>(" select * from InterfaceBusiness where AgentPay=@AgentPay ", new
			{
				AgentPay
			});
		}

		public InterfaceBusiness GetInterfaceBusinessModel(string Code)
		{
			return DalContext.GetModel<InterfaceBusiness>(" select * from InterfaceBusiness Where Code=@Code ", new
			{
				Code
			});
		}

		public int UpInterfaceBusinessEnabled(string Code, int IsEnabled)
		{
			return DalContext.ExecuteSql(" update InterfaceBusiness set IsEnabled=@IsEnabled where Code=@Code ", new
			{
				Code,
				IsEnabled
			});
		}

		public int UpInterfaceBusinessAgentPay(string Code, int AgentPay)
		{
			return DalContext.ExecuteSql(" update InterfaceBusiness set AgentPay=@AgentPay where Code=@Code ", new
			{
				Code,
				AgentPay
			});
		}

		public long AddInterfaceBusiness(InterfaceBusiness Model)
		{
			return DalContext.Insert(Model);
		}

		public bool UpdateInterfaceBusiness(InterfaceBusiness Model)
		{
			return DalContext.Update(Model);
		}

		public int DelInterface(string Code)
		{
			return DalContext.ExecuteSql("delete InterfaceBusiness where Code=@Code;\r\n                        delete InterfaceAccount where Code=@Code;\r\n                        delete InterfaceType where [type]=1 And InterfaceCode=@Code;\r\n                        delete PayRate where RateType=1 And UserId=@Code;\r\n                        delete RiskSetting where RiskSettingType=1 and targetid=@Code", new
			{
				Code
			});
		}

		public List<PayType> GetInterfaceType(string Code, int Type)
		{
			return DalContext.GetList<PayType>(" select B.*,A.DefaulInfaceCode from InterfaceType A inner join PayType B On A.PayCode=B.PayCode \r\n                            where A.InterfaceCode=@Code And A.Type=@Type order by B.PaySort desc ", new
			{
				Code,
				Type
			});
		}

        public List<InterfaceType> GetInterfaceTypeOnly(string Code, int Type)
        {
            return DalContext.GetList<InterfaceType>(" select A.* from InterfaceType A where A.InterfaceCode=@Code And A.Type=@Type ", new
            {
                Code,
                Type
            });
        }

        public List<PayType> GetInterfaceChannelType(string Code, int Type)
		{
			return DalContext.GetList<PayType>(" select c.ChannelName PayName, C.Code as DefaulInfaceCode from InterfaceType A \r\n            inner join PayType B On A.PayCode=B.PayCode \r\n            left join [PayChannel] C on C.paycode=B.paycode\r\n            where A.InterfaceCode=@Code And A.Type=@Type order by B.PaySort desc ", new
			{
				Code,
				Type
			});
		}

		public long SetInterfaceType(List<InterfaceType> listModel, string InterfaceCode)
		{
			int num = DalContext.ExecuteSql(" delete InterfaceType where InterfaceCode=@InterfaceCode ", new
			{
				InterfaceCode
			});
			if (listModel != null && listModel.Count > 0)
			{
				return DalContext.InsertBat(listModel);
			}
			return num;
		}

		public List<InterfaceAccount> LoadInterfaceAccount(InterfaceAccount param, ref Paging paging)
		{
			string str = " select count(*) from InterfaceAccount A where 1=1  ";
			string text = "";
            param.Code = DalContext.EscapeString(param.Code);
            param.Account = DalContext.EscapeString(param.Account);
            if (!string.IsNullOrEmpty(param.Code))
			{
				text += $" AND A.Code='{param.Code}' ";
			}
			if (!string.IsNullOrEmpty(param.Account))
			{
				text += $" AND A.Account like '%{param.Account}%' ";
			}
			if (param.IsEnabled > -1)
			{
				text += $" AND A.IsEnabled={param.IsEnabled} ";
			}
			string sql = " select  A.*  from interfaceaccount as A where 1=1 " + text;
			str += text;
			return DalContext.GetPage<InterfaceAccount>(sql, str, " * ", " OrderNo DESC ", ref paging);
		}

		public List<InterfaceAccount> LoadInterfaceAccount(string code, int isEnabled = -1)
		{
			string text = " select * from InterfaceAccount where Code=@Code ";
			if (isEnabled > -1)
			{
				text += $" and IsEnabled={isEnabled} ";
			}
			return DalContext.GetList<InterfaceAccount>(text, new
			{
				Code = code
			});
		}

		public InterfaceAccount GetInterfaceAccountModel(string Id)
		{
			return DalContext.GetModel<InterfaceAccount>(" select * from InterfaceAccount Where Id=@Id ", new
			{
				Id
			});
		}

		public InterfaceAccount GetInterfaceAccountFroAccount(string Account)
		{
			return DalContext.GetModel<InterfaceAccount>(" select * from InterfaceAccount Where Account=@Account ", new
			{
				Account
			});
		}

		public int UpInterfaceAccountEnabled(string Id, int IsEnabled)
		{
			return DalContext.ExecuteSql(" update InterfaceAccount set IsEnabled=@IsEnabled where Id=@Id ", new
			{
				Id,
				IsEnabled
			});
		}

		public long AddInterfaceAccount(InterfaceAccount Model)
		{
			return DalContext.Insert(Model);
		}

		public bool UpdateInterfaceAccount(InterfaceAccount Model)
		{
			return DalContext.Update(Model);
		}

		public bool DelInterfaceAccountBat(List<string> idlist)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < idlist.Count; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.AppendFormat("'{0}'", DalContext.EscapeString(idlist[i]));
			}
			return DalContext.ExecuteSql(" DELETE FROM InterfaceAccount WHERE ID IN (" + stringBuilder.ToString() + ") ") > 0;
		}

		public long AddInterfaceAccountBat(List<InterfaceAccount> list)
		{
			return DalContext.InsertBat(list);
		}
	}
}
