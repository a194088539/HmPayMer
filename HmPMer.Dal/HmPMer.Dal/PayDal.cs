using HM.DAL;
using HM.Framework;
using HmPMer.Entity;
using System;
using System.Collections.Generic;

namespace HmPMer.Dal
{
	public class PayDal
	{
		public List<PayType> LoadRechargePayType(PayType param, ref Paging paging)
		{
			string str = " select count(*) from PayType A WHERE 1=1  ";
			string text = "";

            param.PayName = DalContext.EscapeString(param.PayName);
            if (!string.IsNullOrEmpty(param.PayName))
			{
				text += string.Format(" AND (A.PayName like '%{0}%' or A.PayCode like '%{0}%') ", param.PayName);
			}
			string sql = " select * from PayType A Where 1=1  " + text;
			str += text;
			return DalContext.GetPage<PayType>(sql, str, " * ", " PaySort DESC ", ref paging);
		}

		public PayType GetPayTypeModel(string PayCode)
		{
			return DalContext.GetModel<PayType>(" select * from PayType Where PayCode=@PayCode ", new
			{
				PayCode
			});
		}

		public long AddPayType(PayType Model)
		{
			return DalContext.Insert(Model);
		}

		public int UpdatePayType(PayType Model)
		{
			return DalContext.ExecuteSql(" update PayType set PayName=@PayName,PaySort=@PaySort where PayCode=@PayCode ", new
			{
				Model.PayCode,
				Model.PayName,
				Model.PaySort
			});
		}

		public int DelPayType(string PayCode)
		{
			return DalContext.ExecuteSql(" delete PayType where PayCode=@PayCode;delete InterfaceType where PayCode=@PayCode;delete PayTypeQuota where PayCode=@PayCode; ", new
			{
				PayCode
			});
		}

		public int UpPayTypeEnabled(string PayCode, int IsEnabled)
		{
			return DalContext.ExecuteSql(" update PayType set IsEnable=@IsEnabled where PayCode=@PayCode ", new
			{
				PayCode,
				IsEnabled
			});
		}

		public List<PayType> GetPayTypeList()
		{
			return DalContext.GetList<PayType>(" select * from PayType where IsEnable=1  order by PaySort desc ");
		}

		public List<PayType> GetInterfaceType(string Code, int Type)
		{
			return DalContext.GetList<PayType>(" select B.*,A.DefaulInfaceCode,A.AccountScheme from InterfaceType A inner join PayType B On A.PayCode=B.PayCode \r\n                            where A.InterfaceCode=@Code And A.Type=@Type order by B.PaySort desc ", new
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

		public List<PayChannelInfo> LoadRechargePayChannel(PayChannelInfo param, ref Paging paging)
		{
			string str = " select count(*) from PayChannel A Left join PayType B On A.payCode=B.payCode Where 1=1  ";
			string text = "";
            param.ChannelName = DalContext.EscapeString(param.ChannelName);
            if (!string.IsNullOrEmpty(param.ChannelName))
			{
				text += string.Format(" AND (A.ChannelName like '%{0}%' or A.Code like '%{0}%') ", param.ChannelName);
			}
			if (!string.IsNullOrEmpty(param.PayCode))
			{
				text += $" AND A.PayCode='{param.PayCode}' ";
			}
			string sql = " select A.*,B.PayName,C.Name InterfaceName from PayChannel A\r\n                        Left join PayType B On A.payCode=B.payCode\r\n                        Left Join InterfaceBusiness C On A.InterfaceCode=C.Code\r\n                        Where 1=1  " + text;
			str += text;
			return DalContext.GetPage<PayChannelInfo>(sql, str, " * ", " ChannelSort DESC ", ref paging);
		}

		public PayChannel GetPayChannelModel(string Code)
		{
			return DalContext.GetModel<PayChannel>(" select * from PayChannel Where Code=@Code ", new
			{
				Code
			});
		}

		public long AddPayChannel(PayChannel Model)
		{
			return DalContext.Insert(Model);
		}

		public int UpdatePayChannel(PayChannel Model)
		{
			return DalContext.ExecuteSql(" update PayChannel set ChannelName=@ChannelName,ChannelSort=@ChannelSort,PayCode=@PayCode,InterfaceCode=@InterfaceCode where Code=@Code ", new
			{
				Model.Code,
				Model.ChannelName,
				Model.ChannelSort,
				Model.InterfaceCode,
				Model.PayCode
			});
		}

		public int UpPayChannelEnabled(string Code, int IsEnabled)
		{
			return DalContext.ExecuteSql(" update PayChannel set IsEnable=@IsEnabled where Code=@Code ", new
			{
				Code,
				IsEnabled
			});
		}

		public int DelChannel(string Code)
		{
			return DalContext.ExecuteSql(" delete PayChannel where Code=@Code ", new
			{
				Code
			});
		}

		public List<PayChannel> GetPayChannelList()
		{
			return DalContext.GetList<PayChannel>(" select * from PayChannel order by ChannelSort desc ");
		}

		public List<PayChannel> GetUserPayChannelList(string UserId)
		{
			return DalContext.GetList<PayChannel>("  select * from PayChannel where PayCode in( select DISTINCT PayCode from InterfaceType where InterfaceCode=@UserId and Type=2 ) ", new
			{
				UserId
			});
		}

		public List<PayChannelSetting> LoadChannelSettingEnable()
		{
			return DalContext.GetList<PayChannelSetting>("\r\n            SELECT  A.PayCode, A.PayName, B.Code as [ChannelCode], B.ChannelName\r\n            FROM PayType    AS A\r\n            JOIN PayChannel AS B ON A.PayCode=B.PayCode\r\n            WHERE A.IsEnable=1 AND B.IsEnable=1 ");
		}

		public InterfaceType GetPayTypeUser(string userId, string payCode)
		{
			return DalContext.GetModel<InterfaceType>(" SELECT TOP 1 * FROM InterfaceType WHERE [Type]=2 AND PayCode=@PayCode AND InterfaceCode=@InterfaceCode   ", new
			{
				PayCode = payCode,
				InterfaceCode = userId
			});
		}

		public List<OrderBase> GetPayOrderList(string userId, string channel, decimal amt, DateTime time, DateTime expiredTime)
		{
			return DalContext.GetList<OrderBase>(" SELECT * FROM OrderBase WHERE  UserId=@UserId and ChannelCode=@ChannelCode and MerOrderAmt=@MerOrderAmt and PayState=@PayState and OrderTime<= @Time and @Time <= ExpiredTime ", new
			{
				UserId = userId,
				ChannelCode = channel,
				MerOrderAmt = amt,
				PayState = 0,
				Time = time,
				ExpiredTime = expiredTime
			});
		}

		public bool InsertOrderInfo(OrderBase model)
		{
			int num = 0;
			try
			{
				model.OrderId = Utils.CreateOrderId();
				num = (int)DalContext.Insert(model);
			}
			catch (Exception)
			{
				model.OrderId = Utils.CreateOrderId();
				num = (int)DalContext.Insert(model);
			}
			return num > 0;
		}

		public bool InsertOrderPayCode(OrderPayCode model)
		{
			return DalContext.Insert(model) > 0;
		}

		public OrderPayCode GetOrderPayCodeByMerOrderNo(string merOrderNo, string userId)
		{
			return DalContext.GetModel<OrderPayCode>("\r\n            SELECT \r\n\t              A.*\r\n                 ,B.OrderAmt\r\n\t             ,B.PayState\r\n\t             ,B.OrderTime\r\n\t             ,B.ExpiredTime\r\n            FROM OrderBase         AS B \r\n            LEFT JOIN OrderPayCode AS A ON A.OrderId=B.OrderId           \r\n            WHERE B.MerOrderNo=@MerOrderNo and B.userId=@userId", new
			{
				MerOrderNo = merOrderNo,
				userId = userId
			});
		}

		private string GetTradeId()
		{
			return Guid.NewGuid().ToString();
		}

		public bool CompleteOrder(OrderBase model, List<OrderAccount> orderAccountList)
		{
			int num = DalContext.ExecuteSql(" \r\n            UPDATE [OrderBase]\r\n               SET\r\n                   [ChannelOrderNo] = @ChannelOrderNo\r\n                  ,[PayState] = @PayState\r\n                  ,[PayTime] = @PayTime\r\n                  ,[PayFlow] = @PayFlow\r\n                  ,[MerAmt] = @MerAmt\r\n                  ,[CostRate] = @CostRate \r\n                  ,[CostAmt] = @CostAmt\r\n                  ,[PromRate] = @PromRate\r\n                  ,[PromAmt] = @PromAmt\r\n                  ,[AgentRate] = @AgentRate\r\n                  ,[AgentAmt] = @AgentAmt\r\n                  ,[Profits] = @Profits\r\n            WHERE [OrderId] = @OrderId   ", model);
			if (num > 0)
			{
				if (orderAccountList == null || orderAccountList.Count == 0)
				{
					if (DalContext.ExecuteSql(" UPDATE UserAmt SET OrderAmt=OrderAmt+@OrderAmt, Balance=Balance+@Balance  WHERE UserId=@UserId ", new
					{
						OrderAmt = model.MerOrderAmt,
						Balance = model.MerAmt,
						UserId = model.UserId
					}) > 0)
					{
						UserAmt model2 = DalContext.GetModel<UserAmt>(" select top 1 * from UserAmt where UserId=@UserId ", new
						{
							model.UserId
						});
						DalContext.Insert(new Trade
						{
							BillNo = model.OrderId,
							TradeId = GetTradeId(),
							UserId = model.UserId,
							Type = 1,
							BillType = 2,
							TradeTime = DateTime.Now,
							BeforeAmount = model2.Balance,
							Amount = model.MerAmt,
							Balance = model2.Balance + model.MerAmt,
							Remark = "订单入金"
						});
					}
				}
				else
				{
					decimal num2 = default(decimal);
					decimal num3 = default(decimal);
					foreach (OrderAccount orderAccount in orderAccountList)
					{
						if (orderAccount.AccountState == 1)
						{
							num2 += orderAccount.Amt;
						}
						else
						{
							num3 += orderAccount.Amt;
						}
					}
					if (DalContext.InsertBat(orderAccountList) > 0 && DalContext.ExecuteSql(" UPDATE UserAmt SET OrderAmt=OrderAmt+@OrderAmt, Balance=Balance+@Balance, UnBalance=UnBalance+@UnBalance  WHERE UserId=@UserId ", new
					{
						OrderAmt = model.MerOrderAmt,
						Balance = num2,
						UnBalance = num3,
						UserId = model.UserId
					}) > 0 && num2 > decimal.Zero)
					{
						UserAmt model3 = DalContext.GetModel<UserAmt>(" select top 1 * from UserAmt where UserId=@UserId ", new
						{
							model.UserId
						});
						DalContext.Insert(new Trade
						{
							BillNo = model.OrderId,
							TradeId = GetTradeId(),
							UserId = model.UserId,
							Type = 1,
							BillType = 2,
							TradeTime = DateTime.Now,
							BeforeAmount = model3.Balance,
							Amount = num2,
							Balance = model3.Balance + num2,
							Remark = "订单入金"
						});
					}
				}
				if (!string.IsNullOrEmpty(model.PromId))
				{
					DalContext.ExecuteSql(" UPDATE HmAdminAmt SET OrderAmt=OrderAmt+@OrderAmt,OrderNum=OrderNum+1, Balance=Balance+@Balance, TotalBalance=TotalBalance+@Balance WHERE AdminId=@AdminId ", new HmAdminAmt
					{
						AdminId = model.PromId,
						OrderAmt = model.MerOrderAmt,
						Balance = model.PromAmt
					});
				}
				if (!string.IsNullOrEmpty(model.AgentId) && DalContext.ExecuteSql(" UPDATE UserAmt SET OrderAmt=OrderAmt+@OrderAmt, Balance=Balance+@AgentAmt  WHERE UserId=@AgentId ", model) > 0)
				{
					UserAmt model4 = DalContext.GetModel<UserAmt>(" select top 1 * from UserAmt where UserId=@UserId ", new
					{
						UserId = model.AgentId
					});
					DalContext.Insert(new Trade
					{
						BillNo = model.OrderId,
						TradeId = GetTradeId(),
						UserId = model.UserId,
						Type = 1,
						BillType = 6,
						TradeTime = DateTime.Now,
						BeforeAmount = model4.Balance,
						Amount = model.AgentAmt,
						Balance = model4.Balance + model.AgentAmt,
						Remark = "订单入金代理收益"
					});
				}
			}
			return num > 0;
		}

		public List<OrderSettlement> GetOrderSettlementStateList()
		{
			return DalContext.GetList<OrderSettlement>("  select * from OrderSettlement where SettlementState = 0 and CONVERT(varchar(10),PaymentTime,20)<= CONVERT(varchar(10), getdate(), 20) ");
		}

		public int SettlementUserAmt(OrderSettlement Model)
		{
			return DalContext.ExecuteSqlTransaction(new List<Tuple<string, object>>
			{
				new Tuple<string, object>(" update UserAmt set Balance=Balance+@Balance,Freeze=Freeze-@Freeze where UserId=@UserId ", new
				{
					Balance = Model.SettlementAmt,
					Freeze = Model.SettlementAmt,
					UserId = Model.UserId
				}),
				new Tuple<string, object>("update OrderSettlement set SettlementState=1,AccountingTime=GETDATE() where Id=@Id ", new
				{
					Model.Id
				})
			});
		}
	}
}
