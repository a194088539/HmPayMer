using HM.DAL;
using HmPMer.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace HmPMer.Dal
{
	public class WithdrawDal
	{
		public List<WithdrawChannelInfo> GetWithdrawChannelPageList(WithdrawChannelInfo parm, ref Paging paging)
		{
			string str = " select count(*) from WithdrawChannel A WHERE 1=1  ";
			string text = "";
            parm.Name = DalContext.EscapeString(parm.Name);
            if (!string.IsNullOrEmpty(parm.Name))
			{
				text = text + " And ( A.Name like '%" + parm.Name + "%' )";
			}
			string sql = " select A.*,B.Name InterfaceName from WithdrawChannel A left join InterfaceBusiness B on A.InterfaceCode=B.Code Where 1=1  " + text;
			str += text;
			return DalContext.GetPage<WithdrawChannelInfo>(sql, str, " * ", " IsEnabled desc, Id asc ", ref paging);
		}

		public List<WithdrawChannel> GetWithdrawChannelList(int isEnabled = -1)
		{
			string text = " select * from WithdrawChannel(nolock) where 1=1 ";
			if (isEnabled > -1)
			{
				text += $" and IsEnabled={isEnabled} ";
			}
			return DalContext.GetList<WithdrawChannel>(text);
		}

		public WithdrawChannelInfo WithdrawChannelGetModel(string Id)
		{
			return DalContext.GetModel<WithdrawChannelInfo>(" select A.*,B.Name InterfaceName from WithdrawChannel A left join InterfaceBusiness B on A.InterfaceCode=B.Code Where A.Id=@Id  ", new
			{
				Id
			});
		}

		public WithdrawChannel WithdrawChannelGetModelByCode(string code)
		{
			return DalContext.GetModel<WithdrawChannelInfo>(" select * from WithdrawChannel where Code=@Code  ", new
			{
				Code = code
			});
		}

		public int UpWCIsEnabled(string Id, int IsEnabled)
		{
			return DalContext.ExecuteSql(" update WithdrawChannel set IsEnabled=@IsEnabled where Id=@Id ", new
			{
				Id,
				IsEnabled
			});
		}

		public long WithdrawChannelAdd(WithdrawChannel Model)
		{
			return DalContext.Insert(Model);
		}

		public bool WithdrawChannelUpdate(WithdrawChannel Model)
		{
			return DalContext.Update(Model);
		}

		public List<WithdrawSchemeinfo> GetWithdrawSchemePageList(WithdrawSchemeinfo parm, ref Paging paging)
		{
			string str = " select count(*) from WithdrawScheme A WHERE 1=1  ";
			string text = "";
			if (!string.IsNullOrEmpty(parm.SchemeName))
			{
				text = text + " And ( A.SchemeName like '%" + parm.SchemeName + "%' )";
			}
			string sql = " select A.*,B.Name InterfaceName from WithdrawScheme  A left join InterfaceBusiness B on A.DefaulInfaceCode=B.Code Where 1=1  " + text;
			str += text;
			return DalContext.GetPage<WithdrawSchemeinfo>(sql, str, " * ", " sort asc ", ref paging);
		}

		public List<WithdrawScheme> GetAllWithdrawSchemeList(int UserType)
		{
			return DalContext.GetList<WithdrawScheme>(" select * from WithdrawScheme where UserType=@UserType ", new
			{
				UserType
			});
		}

		public WithdrawScheme WithdrawSchemeGetModel(string Id)
		{
			return DalContext.GetModel<WithdrawScheme>(" select * from WithdrawScheme A Where A.Id=@Id  ", new
			{
				Id
			});
		}

		public long WithdrawSchemeAdd(WithdrawScheme Model)
		{
			return DalContext.Insert(Model);
		}

		public bool WithdrawSchemeUpdate(WithdrawScheme Model)
		{
			return DalContext.Update(Model);
		}

		public int DelWithdrawScheme(string Id)
		{
			return DalContext.ExecuteSql("delete WithdrawScheme where Id=@Id", new
			{
				Id
			});
		}

		public List<WithdrawOrderInfo> GetWithdrawOrderPageList(WithdrawOrderQueryParam parm, ref Paging paging)
		{
			string str = " select count(*) from WithdrawOrder A WHERE A.PayState!=3 And A.PayState!=1 And A.PayState!=5  ";
			string withdrawOrderWhere = GetWithdrawOrderWhere(parm);
			string sql = "select A.*,B.Name InterfaceName from WithdrawOrder A  Left Join InterfaceBusiness B On A.InterfaceCode=B.Code Where A.PayState!=3 And A.PayState!=1 And A.PayState!=5  " + withdrawOrderWhere;
			str += withdrawOrderWhere;
			return DalContext.GetPage<WithdrawOrderInfo>(sql, str, " * ", " AddTime asc ", ref paging, parm);
		}

		public DataTable GetWithdrawOrderTable(WithdrawOrderQueryParam parm)
		{
			return DalContext.GetDataTable(" \r\n                select A.UserId 商户Id,A.OrderId 订单ID, Convert(decimal(10,2), A.WithdrawAmt/100) 提现金额,\r\n                Convert(decimal(10,2), A.Handing/100) 手续费,Convert(decimal(10,2), A.Amt/100) 扣款总额,\r\n                case A.PayState \r\n                    when 0 then '待付款' when 1 then '成功' \r\n                    when 2 then '待确认' when 3 then '失败'\r\n                    when 4 then '处理中' else '' end 付款状态,\r\n\t                A.AuditDesc 说明,A.AddTime 申请时间,A.ChannelOrderNo 通道订单号,A.BankName 银行名称,\r\n\t                A.BankCode 银行卡号,A.FactName 真实姓名,A.ProvinceName + A.CityName + A.BankAddress 支行地址,\r\n\t                B.Name 接口商,A.[Attach] 备注\r\n                from WithdrawOrder A Left Join InterfaceBusiness B On A.InterfaceCode=B.Code \r\n                Where A.PayState!=3 And A.PayState!=1  And A.PayState!=5\r\n                 " + GetWithdrawOrderWhere(parm) + " order by A.AddTime desc ");
		}

		public string GetWithdrawOrderWhere(WithdrawOrderQueryParam parm)
		{
			string text = "";
            parm.UserId = DalContext.EscapeString(parm.UserId);
            parm.InterfaceCode = DalContext.EscapeString(parm.InterfaceCode);
            if (parm.OrderType > 0)
			{
				text += $" AND A.OrderType={parm.OrderType} ";
			}
            if (!string.IsNullOrEmpty(parm.UserId))
			{
				text += $" AND A.UserId='{parm.UserId}' ";
			}
            if (parm.InterfaceCode != "-1")
			{
				text = text + " And A.InterfaceCode='" + parm.InterfaceCode + "'";
			}
			if (parm.PayState != -1)
			{
				text = text + " And A.PayState=" + parm.PayState;
			}
			if (parm.AddTimeBgin.HasValue)
			{
				text += $" And A.AddTime>='{parm.AddTimeBgin.Value:yyyy-MM-dd HH:mm:ss}' ";
			}
			if (parm.AddTimeEnd.HasValue)
			{
				text += $" And A.AddTime<='{parm.AddTimeEnd.Value:yyyy-MM-dd HH:mm:ss}' ";
			}
			return text;
		}

		public List<WithdrawOrderInfo> GetWithdrawOrderPageListUi(WithdrawOrderQueryParam parm, ref Paging paging)
		{
			string str = " select count(*) from WithdrawOrder A WHERE 1=1  ";
			string str2 = WithdrawOrderUiWhere(parm);
			string sql = "select A.*,B.Name InterfaceName from WithdrawOrder A  Left Join InterfaceBusiness B On A.InterfaceCode=B.Code Where 1=1  " + str2;
			str += str2;
			return DalContext.GetPage<WithdrawOrderInfo>(sql, str, " * ", " AddTime desc ", ref paging, parm);
		}

		public DataTable GetWithdrawOrderUiTable(WithdrawOrderQueryParam parm)
		{
			return DalContext.GetDataTable(" \r\n                select A.OrderId  订单ID,Convert(decimal(10, 2), A.WithdrawAmt / 100) 提现金额,\r\n                Convert(decimal(10, 2), A.Handing / 100) 手续费,\r\n                Convert(decimal(10, 2), A.Amt / 100) 结算金额,\r\n                A.BankName 银行名称,A.BankCode 银行卡号,A.FactName 姓名,\r\n                case A.PayState\r\n                when 0 then '处理中' when 1 then '成功'\r\n                when 2 then '处理中' when 3 then '失败' when 4 then '处理中' else '' end 支付状态,\r\n                case A.OrderType\r\n                when 1 then '前台申请' when 2 then '前台申请'\r\n                when 3 then '系统清算' else '' end 申请类型,\r\n                A.AddTime 提交时间,A.[Attach] 备注\r\n                 from WithdrawOrder A \r\n                Left Join InterfaceBusiness B On A.InterfaceCode=B.Code Where 1=1 " + WithdrawOrderUiWhere(parm) + " order by A.AddTime desc ");
		}

		private string WithdrawOrderUiWhere(WithdrawOrderQueryParam parm)
		{
			string text = "";
            parm.UserId = DalContext.EscapeString(parm.UserId);
            if (parm.OrderType > 0)
			{
				text = ((parm.OrderType != 1) ? (text + $" AND A.OrderType={parm.OrderType} ") : (text + $" AND A.OrderType in (1,3) "));
			}
			if (!string.IsNullOrEmpty(parm.UserId))
			{
				text += $" AND A.UserId='{parm.UserId}' ";
			}
			if (parm.PayState != -1)
			{
				text = ((parm.PayState == 0) ? (text + " And A.PayState in (0,2,4)") : (text + " And A.PayState=" + parm.PayState));
			}
			if (parm.AddTimeBgin.HasValue)
			{
				text += $" And A.AddTime>='{parm.AddTimeBgin.Value:yyyy-MM-dd HH:mm:ss}' ";
			}
			if (parm.AddTimeEnd.HasValue)
			{
				text += $" And A.AddTime<='{parm.AddTimeEnd.Value:yyyy-MM-dd HH:mm:ss}' ";
			}
			return text;
		}

		public long WithdrawOrderAdd(WithdrawOrder model)
		{
			long num = DalContext.Insert(model);
			if (num > 0)
			{
				DalContext.ExecuteSql(" update userAmt set Balance=Balance-@Amt,Freeze=Freeze+@Amt where UserId=@UserId and Balance-@Amt>=0 ", model);
			}
			return num;
		}

		public WithdrawOrder GetWithdrawOrderModel(string OrderId)
		{
			return DalContext.GetModel<WithdrawOrder>(" select * from WithdrawOrder where OrderId=@OrderId ", new
			{
				OrderId
			});
		}

		public int GetWithdrawOrderTarnCount(string orderId, params int[] payState)
		{
			string text = " select count(*) as v from WithdrawOrderTarn where OrderId=@OrderId ";
			if (payState.Length != 0)
			{
				text += " and TarnState in ( ";
				for (int i = 0; i < payState.Length; i++)
				{
					if (i > 0)
					{
						text += ", ";
					}
					text += DalContext.EscapeString(payState[i].ToString());
				}
				text += " ) ";
			}
			return DalContext.GetSingVal<int>(text, new
			{
				OrderId = orderId
			});
		}

		public WithdrawOrderTarn GetWithdrawOrderTarn(string orderId)
		{
			return DalContext.GetModel<WithdrawOrderTarn>(" select top 1 * from WithdrawOrderTarn where OrderId=@OrderId and TarnState=@TarnState ", new
			{
				OrderId = orderId,
				TarnState = 3
			});
		}

		public bool WithdrawOrderTarnAdd(WithdrawOrderTarn model)
		{
			return DalContext.Insert(model) > 0;
		}

		public int UpdateInterfaceCode(string InterfaceCode, string OrderId)
		{
			return DalContext.ExecuteSql(" update WithdrawOrder set InterfaceCode=@InterfaceCode where  OrderId=@OrderId  ", new
			{
				InterfaceCode,
				OrderId
			});
		}

		public int UpdateOrderPayState(int PayState, string AuditDesc, string OrderId)
		{
			return DalContext.ExecuteSql(" Update WithdrawOrder set PayState=@PayState,AuditDesc=@AuditDesc where OrderId=@OrderId ", new
			{
				PayState,
				OrderId,
				AuditDesc
			});
		}

		public bool CompleteWithdrawTarn(WithdrawOrderTarn tarn, WithdrawOrder order)
		{
			List<Tuple<string, object>> list = new List<Tuple<string, object>>();
			list.Add(new Tuple<string, object>("  \r\n            update WithdrawOrderTarn \r\n               set  ChannelOrderNo=@ChannelOrderNo\r\n                   ,TarnState=@TarnState\r\n                   ,TarnRemark=@TarnRemark\r\n\t               ,ModifyTime=@ModifyTime\r\n\t               ,ModifyDesc=@ModifyDesc\r\n            where OrderId=@OrderId ", tarn));
			if (tarn.TarnState == 1)
			{
				list.Add(new Tuple<string, object>("  \r\n                update WithdrawOrder \r\n                   set  ChannelOrderNo=@ChannelOrderNo\r\n                       ,PayState=@PayState\r\n                       ,Attach=''\r\n\t                   ,UpdateTime=@UpdateTime\t                  \r\n                where OrderId=@OrderId and PayState in(0,2,4) ", new
				{
					OrderId = tarn.OrderId,
					ChannelOrderNo = tarn.ChannelOrderNo,
					PayState = 1,
					UpdateTime = DateTime.Now
				}));
				list.Add(new Tuple<string, object>(" update UserAmt set Freeze=Freeze-@Freeze where UserId=@UserId ", new
				{
					UserId = order.UserId,
					Freeze = order.Amt
				}));
			}
			else if (tarn.TarnState == 2)
			{
				list.Add(new Tuple<string, object>("  \r\n                update WithdrawOrder \r\n                   set  ChannelOrderNo=@ChannelOrderNo\r\n                       ,PayState=@PayState\r\n\t                   ,UpdateTime=@UpdateTime\t   \r\n                       ,Attach='付款通道繁忙，付款订单已撤销！'\r\n                where OrderId=@OrderId and PayState in(0,2) ", new
				{
					OrderId = tarn.OrderId,
					ChannelOrderNo = tarn.ChannelOrderNo,
					PayState = 3,
					UpdateTime = DateTime.Now
				}));
				list.Add(new Tuple<string, object>(" update UserAmt set Freeze=Freeze-@Freeze,Balance=Balance+@Balance where UserId=@UserId ", new
				{
					UserId = order.UserId,
					Freeze = order.Amt,
					Balance = order.Amt
				}));
			}
			else if (tarn.TarnState == 3)
			{
				list.Add(new Tuple<string, object>("  \r\n                update WithdrawOrder \r\n                   set  ChannelOrderNo=@ChannelOrderNo\r\n                       ,PayState=@PayState\r\n\t                   ,UpdateTime=@UpdateTime\t                  \r\n                       ,Attach='付款已经提交到银行，等待处理！'\r\n                where OrderId=@OrderId ", new
				{
					OrderId = tarn.OrderId,
					ChannelOrderNo = tarn.ChannelOrderNo,
					PayState = 4,
					UpdateTime = DateTime.Now
				}));
			}
			else if (tarn.TarnState == 4)
			{
				list.Add(new Tuple<string, object>("  \r\n                update WithdrawOrder \r\n                   set  PayState=@PayState\r\n\t                   ,UpdateTime=@UpdateTime\t   \r\n                       ,Attach='系统退回！'\r\n                where OrderId=@OrderId and PayState in(0,2) ", new
				{
					OrderId = tarn.OrderId,
					PayState = 5,
					UpdateTime = DateTime.Now
				}));
				list.Add(new Tuple<string, object>(" update UserAmt set Freeze=Freeze-@Freeze,Balance=Balance+@Balance where UserId=@UserId ", new
				{
					UserId = order.UserId,
					Freeze = order.Amt,
					Balance = order.Amt
				}));
			}
			return DalContext.ExecuteSqlTransaction(list) > 0;
		}

		public List<WithdrawOrder> GetWithdrawOrderListByDay(string userId, DateTime begin, DateTime end)
		{
			return DalContext.GetList<WithdrawOrder>(" select * from WithdrawOrder where UserId=@UserId and AddTime>=@Begin and AddTime<=@End " + " AND PayState IN (0,1,3)  ", new
			{
				UserId = userId,
				Begin = begin,
				End = end
			});
		}

		public List<WithdrawOrderTarn> GetWithdrawOrderTarnPageList(WithdrawOrderTarn parm, ref Paging paging)
		{
			string str = " select count(*) from WithdrawOrderTarn A Inner Join WithdrawOrder B On A.OrderId=B.OrderId \r\n                            Where 1 = 1  ";
			string withdrawOrderWhere = GetWithdrawOrderWhere(parm);
			string sql = " select A.*,B.UserId,B.Handing,C.Name InterfaceName from WithdrawOrderTarn A \r\n                    Inner Join WithdrawOrder B On A.OrderId=B.OrderId   \r\n                    Left Join InterfaceBusiness C On A.InterfaceCode=C.Code\r\n                    Where 1=1  " + withdrawOrderWhere;
			str += withdrawOrderWhere;
			return DalContext.GetPage<WithdrawOrderTarn>(sql, str, " * ", " AddTime desc ", ref paging);
		}

		public DataTable GetWithdrawOrderTarnTable(WithdrawOrderTarn parm)
		{
			return DalContext.GetDataTable(" select B.UserId 商户Id,A.OrderId 订单Id,A.ChannelOrderNo 通道订单Id,C.Name 支付接口,A.FactName 姓名,\r\n                A.BankCode 银行账号,A.BankAddress 支行地址,Convert(decimal(10,2), A.Amount/100) 交易金额,Convert(decimal(10,2), B.Handing/100) 手续费,\r\n                A.TarnState 交易状态,\r\n                case A.TarnState \r\n                when 0 then '待交易' when 1 then '成功' \r\n                when 2 then '失败' when 3 then '付款中'\r\n                else '' end 交易状态,\r\n                A.TarnRemark 备注,A.AddTime 提交时间\r\n                    from WithdrawOrderTarn A \r\n                Inner Join WithdrawOrder B On A.OrderId=B.OrderId   \r\n                Left Join InterfaceBusiness C On A.InterfaceCode=C.Code\r\n                Where 1=1  " + GetWithdrawOrderWhere(parm) + " order by A.AddTime desc ");
		}

		private string GetWithdrawOrderWhere(WithdrawOrderTarn parm)
		{
			string text = "";

            parm.OrderId = DalContext.EscapeString(parm.OrderId);
            parm.ChannelOrderNo = DalContext.EscapeString(parm.ChannelOrderNo);
            parm.UserId = DalContext.EscapeString(parm.UserId);
            parm.FactName = DalContext.EscapeString(parm.FactName);
            parm.BankCode = DalContext.EscapeString(parm.BankCode);
            parm.InterfaceCode = DalContext.EscapeString(parm.InterfaceCode);
            if (!string.IsNullOrEmpty(parm.OrderId))
			{
				text = text + " And A.OrderId='" + parm.OrderId + "'";
			}
			if (!string.IsNullOrEmpty(parm.ChannelOrderNo))
			{
				text = text + " And A.ChannelOrderNo='" + parm.ChannelOrderNo + "'";
			}
			if (!string.IsNullOrEmpty(parm.UserId))
			{
				text = text + " And B.UserId='" + parm.UserId + "'";
			}
			if (!string.IsNullOrEmpty(parm.FactName))
			{
				text = text + " And A.FactName='" + parm.FactName + "'";
			}
			if (!string.IsNullOrEmpty(parm.BankCode))
			{
				text = text + " And A.BankCode='" + parm.BankCode + "'";
			}
			if (parm.InterfaceCode != "-1")
			{
				text = text + " And A.InterfaceCode='" + parm.InterfaceCode + "'";
			}
			if (parm.TarnState > -1)
			{
				text = text + " And A.TarnState=" + parm.TarnState;
			}
			if (parm.BeginTime.HasValue)
			{
				text += $" AND A.[AddTime]>='{parm.BeginTime.Value:yyyy-MM-dd HH:mm:ss}' ";
			}
			if (parm.EndTime.HasValue)
			{
				text += $" AND A.[AddTime]<='{parm.EndTime.Value:yyyy-MM-dd HH:mm:ss}' ";
			}
			return text;
		}

		public long AddAccountScheme(AccountScheme Model)
		{
			return DalContext.Insert(Model);
		}

		public AccountScheme GetAccountScheme(string Id)
		{
			return DalContext.GetModel<AccountScheme>(" select * from AccountScheme Where Id=@Id ", new
			{
				Id
			});
		}

		public long GetMaxAccountSchemeId()
		{
			return DalContext.GetSingVal<long>(" select MAX(id) from AccountScheme ");
		}

		public List<AccountScheme> GetListAccountScheme()
		{
			return DalContext.GetList<AccountScheme>(" select * from AccountScheme ");
		}

		public long UpdateAccountScheme(AccountScheme Model)
		{
			return DalContext.ExecuteSql(" update AccountScheme set name=@name where Id=@Id ", Model);
		}

		public int DelAccountScheme(string Id)
		{
			return DalContext.ExecuteSql(" delete AccountScheme where Id=@Id;delete AccountSchemeDetail where AccountSchemeId=@Id;", new
			{
				Id
			});
		}

		public List<AccountScheme> GetAccountSchemeList(AccountScheme parm, ref Paging paging)
		{
			string str = " select count(*) from AccountScheme A WHERE 1=1  ";
			string text = "";

            parm.name = DalContext.EscapeString(parm.name);
            if (!string.IsNullOrEmpty(parm.name))
			{
				text = text + " And A.name like '%" + parm.name + "%'  ";
			}
			string sql = " select * from AccountScheme A Where 1=1  " + text;
			str += text;
			return DalContext.GetPage<AccountScheme>(sql, str, " * ", " id asc ", ref paging);
		}

		public List<AccountSchemeDetail> GetAccountSchemeDetail(string AccountSchemeId)
		{
			return DalContext.GetList<AccountSchemeDetail>(" select * from AccountSchemeDetail Where AccountSchemeId=@AccountSchemeId ", new
			{
				AccountSchemeId
			});
		}

		public long GetMaxSchemeDetailEndtime(string AccountSchemeId)
		{
			return DalContext.GetSingVal<long>(" select max(EndTime) from AccountSchemeDetail where AccountSchemeId=@AccountSchemeId ", new
			{
				AccountSchemeId
			});
		}

		public List<AccountSchemeDetailInfo> GetAccountSchemeDetailList(AccountSchemeDetail parm, ref Paging paging)
		{
			string str = " select count(*) from AccountSchemeDetail A WHERE 1=1  ";
			string text = "";
            parm.AccountSchemeId = DalContext.EscapeString(parm.AccountSchemeId);
            if (!string.IsNullOrEmpty(parm.AccountSchemeId))
			{
				text = text + " And A.AccountSchemeId = '" + parm.AccountSchemeId + "'  ";
			}
			string sql = " select * from AccountSchemeDetail A Where 1=1  " + text;
			str += text;
			return DalContext.GetPage<AccountSchemeDetailInfo>(sql, str, " * ", " StarTime asc ", ref paging);
		}

		public AccountSchemeDetailInfo GetAccountSchemeDetailInfo(string Id)
		{
			return DalContext.GetModel<AccountSchemeDetailInfo>(" select * from AccountSchemeDetail where Id=@Id ", new
			{
				Id
			});
		}

		public long AddAccountSchemeDetail(AccountSchemeDetail Model)
		{
			return DalContext.Insert(Model);
		}

		public int UpdateAccountSchemeDetail(AccountSchemeDetail Model)
		{
			return DalContext.ExecuteSql(" update AccountSchemeDetail set SchemeType1=@SchemeType1,TDay1=@TDay1,AmtSingle1=@AmtSingle1,\r\n                           SchemeType2=@SchemeType2,TDay2=@TDay2,AmtSingle2=@AmtSingle2  where Id=@Id ", Model);
		}

		public int DelAccountSchemeDetail(string Id)
		{
			return DalContext.ExecuteSql(" delete AccountSchemeDetail Where Id=@Id ", new
			{
				Id
			});
		}

		public long SetAccountSchemeDetail(string AccountSchemeId, List<AccountSchemeDetail> listModel)
		{
			int num = DalContext.ExecuteSql(" delete AccountSchemeDetail where AccountSchemeId=@AccountSchemeId ", new
			{
				AccountSchemeId
			});
			if (listModel != null && listModel.Count > 0)
			{
				foreach (AccountSchemeDetail item in listModel)
				{
					item.Id = Guid.NewGuid().ToString();
					DalContext.Insert(item);
				}
			}
			return num;
		}

		public List<WithrawUserBaseInfo> LoadWithdrawUser(UserBaseInfo param, ref Paging paging)
		{
			string str = " SELECT COUNT(*) FROM UserBase A WHERE 1=1  ";
			string arg = " And A.UserType=1 And A.AgentPay=0 And A.IsEnabled=1 And A.WithdrawStatus=1 ";
			arg = arg + " And B.Balance>" + param.Balance;
            param.UserId = DalContext.EscapeString(param.UserId);
            param.MerName = DalContext.EscapeString(param.MerName);
            param.MobilePhone = DalContext.EscapeString(param.MobilePhone);
            param.Email = DalContext.EscapeString(param.Email);
            param.AgentId = DalContext.EscapeString(param.AgentId);

            if (!string.IsNullOrEmpty(param.UserId))
			{
				arg += $" AND A.UserId = '{param.UserId}' ";
			}
            if (!string.IsNullOrEmpty(param.MerName))
			{
				arg += $" AND A.MerName LIKE '%{param.MerName}%' ";
			}
			if (!string.IsNullOrEmpty(param.MobilePhone))
			{
				arg += $" AND A.MobilePhone LIKE '%{param.MobilePhone}%' ";
			}
			if (!string.IsNullOrEmpty(param.Email))
			{
				arg += $" AND A.Email LIKE '%{param.Email}%' ";
			}
			if (!string.IsNullOrEmpty(param.AgentId))
			{
				arg += $" AND A.AgentId = '{param.AgentId}' ";
			}
			string sql = "SELECT\r\n                A.* , B.OrderAmt,B.Freeze,B.UnBalance,B.Balance,C.WithdrawAccountType,C.WithdrawBank,C.WithdrawFactName,C.WithdrawBankCode,C.WithdrawBankBranch\r\n                from UserBase AS A  \r\n                JOIN UserAmt  AS B ON A.UserId=B.UserId\r\n                Join UserDetail As C On A.userId=C.userId\r\n                Where 1=1  " + arg;
			str += arg;
			return DalContext.GetPage<WithrawUserBaseInfo>(sql, str, " * ", " RegTime DESC ", ref paging);
		}

		public List<OrderAccount> GetOrderAccountList(OrderAccountQueryParam param, ref Paging paging)
		{
			string str = " SELECT COUNT(*) FROM OrderAccount AS A WHERE 1=1  ";
			string text = "";
            if (!string.IsNullOrEmpty(param.OrderId))
			{
				text += " And A.OrderId =@OrderId  ";
			}
			if (!string.IsNullOrEmpty(param.UserId))
			{
				text += " And A.UserId =@UserId  ";
			}
			if (param.AccountState > -1)
			{
				text += " And A.AccountState =@AccountState  ";
			}
			if (param.AddTimeBgin.HasValue)
			{
				text += " And A.AddTime>=@AddTimeBgin  ";
			}
			if (param.AddTimeEnd.HasValue)
			{
				text += " And A.AddTime<=@AddTimeEnd  ";
			}
			if (param.AccountTimeBegin.HasValue)
			{
				text += " And A.AccountTime>=@AccountTimeBegin  ";
			}
			if (param.AccountTimeEnd.HasValue)
			{
				text += " And A.AccountTime<=@AccountTimeEnd  ";
			}
			string sql = " SELECT * FROM OrderAccount AS A WHERE 1=1  " + text;
			str += text;
			return DalContext.GetPage<OrderAccount>(sql, str, " * ", " AddTime DESC ", ref paging, param);
		}

		public bool ConfirmOrderAccount(OrderAccount model)
		{
			string item = " UPDATE OrderAccount SET EndTime=@EndTime, AccountState=@AccountState,AdminId=@AdminId,LockId=null WHERE OId=@OId ";
			string item2 = " UPDATE UserAmt SET Balance=Balance+@Balance, UnBalance=UnBalance-@UnBalance WHERE UserId=@UserId ";
			return DalContext.ExecuteSqlTransaction(new List<Tuple<string, object>>
			{
				new Tuple<string, object>(item, model),
				new Tuple<string, object>(item2, new
				{
					Balance = model.Amt,
					UnBalance = model.Amt,
					UserId = model.UserId
				})
			}) > 0;
		}

		public List<OrderAccountTarn> GetOrderAccountTarnList(OrderAccountTarn param, ref Paging paging)
		{
			string text = " \r\n            SELECT \r\n\t           COUNT(*)\r\n            FROM  UserBase AS A\r\n            JOIN  UserAmt  AS B ON A.UserId=B.UserId\r\n            JOIN (\r\n            SELECT \r\n\t              UserId\r\n\t             ,SUM(Amt) AS Amt\r\n\t             ,COUNT(1) AS AccountCount\r\n            FROM OrderAccount\r\n            WHERE 1=1 {0}\r\n            GROUP BY UserId\r\n            ) as C ON A.UserId=C.UserId\r\n            WHERE 1=1\r\n            ";
			string text2 = "";
			if (!string.IsNullOrEmpty(param.UserId))
			{
				text2 += " AND UserId=@UserId  ";
			}
			if (param.AccountDate.HasValue)
			{
				text2 += $" AND AccountTime<='{param.AccountDate.Value:yyyy-MM-dd 23:59:59}' ";
			}
			string sql = $"\r\n            SELECT \r\n\t            A.UserId,\r\n\t            A.MerName,\r\n\t            B.Balance,\r\n\t            B.UnBalance,\r\n\t            C.Amt,\r\n\t            C.AccountCount \r\n            FROM  UserBase AS A\r\n            JOIN  UserAmt  AS B ON A.UserId=B.UserId\r\n            JOIN (\r\n            SELECT \r\n\t              UserId\r\n\t             ,SUM(Amt) AS Amt\r\n\t             ,COUNT(1) AS AccountCount\r\n            FROM OrderAccount\r\n            WHERE AccountState=0 {text2}\r\n            GROUP BY UserId\r\n            ) as C ON A.UserId=C.UserId\r\n            WHERE 1=1 ";
			text += string.Format(text, text2);
			return DalContext.GetPage<OrderAccountTarn>(sql, text, " * ", " Amt DESC ", ref paging, param);
		}

		private string GetTradeId()
		{
			return Guid.NewGuid().ToString();
		}

		public bool ConfirmOrderAccountTarnBat(List<OrderAccountTarn> list, OrderAccount model)
		{
			int num = 0;
			StringBuilder stringBuilder = new StringBuilder();
			List<string> list2 = new List<string>();
			for (int i = 0; i < list.Count; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.AppendFormat("'{0}'", list[i].UserId);
				list2.Add(list[i].UserId);
			}
			if (DalContext.ExecuteSql(" UPDATE [OrderAccount] SET LockId=@LockId, AdminId=@AdminId WHERE AccountState=0 AND LockId IS NULL  AND AccountTime<=@AccountTime AND UserId IN (  " + stringBuilder.ToString() + " ) ", model) > 0)
			{
				string item = "\r\n                UPDATE A\r\n                SET A.Balance=   A.Balance + (SELECT SUM(Amt) FROM [OrderAccount] AS B WHERE LockId=@LockId AND A.UserId=B.UserId AND AccountState=0),\r\n\t                A.UnBalance=A.UnBalance- (SELECT SUM(Amt) FROM [OrderAccount] AS B WHERE LockId=@LockId AND A.UserId=B.UserId AND AccountState=0)\r\n                FROM UserAmt AS A\r\n                WHERE EXISTS(SELECT 1 FROM [OrderAccount] AS B WHERE LockId=@LockId AND AccountState=0 AND A.UserId=B.UserId) ";
				string item2 = " UPDATE [OrderAccount] SET [EndTime]=@EndTime,AccountState=@AccountState WHERE LockId=@LockId  ";
				num = DalContext.ExecuteSqlTransaction(new List<Tuple<string, object>>
				{
					new Tuple<string, object>(item, model),
					new Tuple<string, object>(item2, model)
				});
				if (num > 0)
				{
					foreach (string item3 in list2)
					{
						decimal singVal = DalContext.GetSingVal<decimal>(" SELECT SUM(Amt) FROM [OrderAccount] AS B WHERE LockId=@LockId AND B.UserId=@UserId AND AccountState=1 ", new
						{
							LockId = model.LockId,
							UserId = item3
						});
						UserAmt model2 = DalContext.GetModel<UserAmt>(" select top 1 * from UserAmt where UserId=@UserId ", new
						{
							UserId = item3
						});
						DalContext.Insert(new Trade
						{
							BillNo = model.OrderId,
							TradeId = GetTradeId(),
							UserId = item3,
							Type = 1,
							BillType = 7,
							TradeTime = DateTime.Now,
							BeforeAmount = model2.Balance,
							Amount = singVal,
							Balance = model2.Balance + singVal,
							Remark = "系统入账清算"
						});
					}
				}
			}
			return num > 0;
		}

		public WithdrawOrder GetTxOrder()
		{
			return DalContext.GetModel<WithdrawOrder>(" select top 1 * from [dbo].[WithdrawOrder] where (Interfacecode is null or Interfacecode='') \r\n                and paystate=0 and AddTime>dateadd(minute,-5,getdate())\r\n                order by addtime desc  ");
		}
	}
}
