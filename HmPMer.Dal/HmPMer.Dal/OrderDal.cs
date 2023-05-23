using HM.DAL;
using HmPMer.Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace HmPMer.Dal
{
	public class OrderDal
	{
		public List<OrderInfo> GetOrderList(OrderSearchParam param, ref Paging paging, out OrderListCount countData)
		{
			string str = " SELECT  COUNT(*)\r\n                            FROM[OrderBase](nolock) as A\r\n                            LEFT JOIN[OrderNotity](nolock)AS B on A.[OrderId] = B.[OrderId]\r\n                            WHERE 1=1 ";
			string orderWhere = GetOrderWhere(param);
			string sql = " SELECT  A.*,\r\n                        B.[NotityState] as [OrderState],\r\n                        C.PayName,D.Name InterfaceName ,F.ChannelName\r\n                        FROM [OrderBase](nolock) as A\r\n                        LEFT JOIN [OrderNotity](nolock) AS B on A.[OrderId]=B.[OrderId]\r\n                        Left Join PayType C On A.PayCode=C.PayCode\r\n                        Left Join InterfaceBusiness D On A.[InterfaceCode]=D.Code\r\n                        Left Join PayChannel F On A.ChannelCode=F.Code\r\n                        WHERE 1=1 " + orderWhere;
			str += orderWhere;
			string str2 = "\r\n            SELECT\t \r\n                     SUM(A.[OrderAmt]) AS [TotalOrderAmt]                    \r\n                    ,SUM(A.[CostAmt]) AS [TotalCostAmt]       \r\n                    ,SUM(A.[PromAmt]) AS [TotalPromAmt]\r\n                    ,SUM(A.[AgentAmt]) AS [TotalAgentAmt]\r\n                    ,SUM(A.[Profits]) AS [TotalProfits]         \r\n\t\t            ,SUM(CASE WHEN A.[PayState]=1 AND ISNULL(B.[NotityState],0)<>1 THEN 1 ELSE 0 end) AS OutOrderCount\r\n\t\t             FROM [OrderBase]   AS A\t\r\n\t            LEFT JOIN [OrderNotity] AS B on A.[OrderId]=B.[OrderId] \r\n            WHERE 1=1 ";
			str2 += orderWhere;
			List<OrderInfo> page = DalContext.GetPage<OrderInfo>(sql, str, " * ", " OrderTime DESC ", ref paging, param);
			countData = DalContext.GetModel<OrderListCount>(str2, param);
			return page;
		}


        public List<OrderInfo> GetOrderList(string userId, string code, DateTime dt, int topN, int payState  = -1, decimal startAmt = -1, decimal endAmt = -1)
        {
            string sql = "";
            if(topN <= 0)
            {
                sql = "SELECT A.* FROM OrderBase(nolock) as A  WHERE 1=1";
            } else
            {
                sql = "SELECT TOP " + topN + " A.* FROM OrderBase(nolock) as A  WHERE 1=1";
            }
            if(!string.IsNullOrWhiteSpace(userId))
            {
                sql += " AND A.UserId='" + DalContext.EscapeString(userId) + "'";
            }
            if (!string.IsNullOrWhiteSpace(code))
            {
                sql += " AND A.InterfaceCode like '" + DalContext.EscapeString(code) + "%'";
            }
            if(payState >= 0)
            {
                sql += " AND A.PayState = " + payState;
            }
            if (startAmt >= 0)
            {
                sql += " AND A.OrderAmt >= " + startAmt;
            }
            if (endAmt >= 0)
            {
                sql += " AND A.OrderAmt < " + endAmt;
            }
            sql += " AND OrderTime >= '" + dt.ToString("yyyy-MM-dd HH:mm:ss") + "' ORDER BY OrderTime DESC";
            List<OrderInfo> page = DalContext.GetList<OrderInfo>(sql);
            return page;
        }

        public DataTable GetOrderListTable(OrderSearchParam param)
		{
			return DalContext.GetDataTable(" SELECT A.UserId 商户ID,A.MerOrderNo 商户订单号,A.OrderId 系统订单号,A.ChannelOrderNo 接口商订单号,D.Name 接口商,C.PayName 支付类型,\r\n            F.ChannelName 通道,\r\n            Convert(decimal(10, 2), A.OrderAmt / 100) 订单金额,Convert(decimal(10, 2), A.MerAmt / 100) 商户金额,\r\n            Convert(decimal(10, 2), A.Profits / 100) 利润,Convert(decimal(10, 2), A.AgentAmt / 100) 代理提成,\r\n            Convert(decimal(10, 2), A.PromAmt / 100) 业务员提成,\r\n            A.OrderTime 下单时间,\r\n            case A.PayState\r\n                when 0 then '处理中' when 1 then '成功'\r\n                when 2 then '失败' when 3 then '已过期' else '' end 支付状态,\r\n            case B.NotityState\r\n                when 0 then '处理中' when 1 then '成功'\r\n                when 2 then '失败' else '' end 通知状态\r\n            FROM[OrderBase](nolock) as A\r\n            LEFT JOIN[OrderNotity](nolock)AS B on A.[OrderId] = B.[OrderId]\r\n            Left Join PayType C On A.PayCode = C.PayCode\r\n            Left Join InterfaceBusiness D On A.[InterfaceCode]=D.Code\r\n            Left Join PayChannel F On A.ChannelCode=F.Code\r\n            WHERE 1=1 " + GetOrderWhere(param) + " order by A.OrderTime desc  ");
		}

		public DataTable GetOrderListTableUI(OrderSearchParam param)
		{
			return DalContext.GetDataTable(" SELECT A.OrderId 订单号,C.PayName 支付方式,F.ChannelName 通道,\r\n            Convert(decimal(10, 2), A.OrderAmt / 100) 金额,Convert(decimal(10, 2), A.PayFlow / 100) 手续费,\r\n            case A.PayState\r\n            when 0 then '待支付' when 1 then '支付成功'\r\n            when 2 then '支付失败' when 3 then '支付过期' else '' end 支付状态,\r\n            A.OrderTime 下单时间,A.PayTime 支付时间\r\n            FROM[OrderBase](nolock) as A\r\n            LEFT JOIN[OrderNotity](nolock)AS B on A.[OrderId] = B.[OrderId]\r\n            Left Join PayType C On A.PayCode = C.PayCode\r\n            Left Join InterfaceBusiness D On A.[InterfaceCode]=D.Code\r\n            Left Join PayChannel F On A.ChannelCode=F.Code\r\n            WHERE 1=1 " + GetOrderWhere(param) + " order by A.OrderTime desc  ");
		}

		private string GetOrderWhere(OrderSearchParam param)
		{
			string text = "";
            param.UserId = DalContext.EscapeString(param.UserId);
            param.OrderId = DalContext.EscapeString(param.OrderId);
            param.MerOrderNo = DalContext.EscapeString(param.MerOrderNo);
            param.AgentId = DalContext.EscapeString(param.AgentId);
            param.PromId = DalContext.EscapeString(param.PromId);
            param.ChannelOrderNo = DalContext.EscapeString(param.ChannelOrderNo);
            param.InterfaceCode = DalContext.EscapeString(param.InterfaceCode);
            param.Channel = DalContext.EscapeString(param.Channel);
            param.PayCode = DalContext.EscapeString(param.PayCode);
            if (!string.IsNullOrEmpty(param.UserId))
			{
				text = text + " AND A.[UserId]='" + param.UserId + "' ";
			}
			if (!string.IsNullOrEmpty(param.OrderId))
			{
				text = text + " AND A.[OrderId]='" + param.OrderId + "' ";
			}
			if (!string.IsNullOrEmpty(param.MerOrderNo))
			{
				text = text + " AND A.[MerOrderNo]='" + param.MerOrderNo + "' ";
			}
			if (!string.IsNullOrEmpty(param.AgentId))
			{
				text = text + " AND A.[AgentId]='" + param.AgentId + "' ";
			}
			if (!string.IsNullOrEmpty(param.PromId))
			{
				text = text + " AND A.[PromId]='" + param.PromId + "' ";
			}
			if (!string.IsNullOrEmpty(param.ChannelOrderNo))
			{
				text = text + " AND A.[ChannelOrderNo]='" + param.ChannelOrderNo + "' ";
			}
			if (!string.IsNullOrEmpty(param.InterfaceCode))
			{
				text = text + " AND A.[InterfaceCode]='" + param.InterfaceCode + "' ";
			}
			if (!string.IsNullOrEmpty(param.Channel))
			{
				text = text + " AND A.[ChannelCode]='" + param.Channel + "' ";
			}
			if (!string.IsNullOrEmpty(param.PayCode))
			{
				text = text + " AND A.[PayCode]='" + param.PayCode + "' ";
			}
			if (param.OrderAmt > decimal.Zero)
			{
				text = text + " AND A.[OrderAmt]=" + param.OrderAmt;
			}
			if (param.OrderBeginTime.HasValue)
			{
				text += $" AND A.[OrderTime]>='{param.OrderBeginTime.Value:yyyy-MM-dd HH:mm:ss}' ";
			}
			if (param.OrderEndTime.HasValue)
			{
				text += $" AND A.[OrderTime]<='{param.OrderEndTime.Value:yyyy-MM-dd HH:mm:ss}' ";
			}
			if (param.PayBeginTime.HasValue)
			{
				text += $" AND A.[PayTime]>='{param.PayBeginTime.Value:yyyy-MM-dd HH:mm:ss}' ";
			}
			if (param.PayEndTime.HasValue)
			{
				text += $" AND A.[PayTime]<='{param.PayBeginTime.Value:yyyy-MM-dd HH:mm:ss}' ";
			}
			if (param.PayState > -1)
			{
				text = text + " AND A.[PayState]= " + param.PayState;
			}
			if (param.OrderState > -1)
			{
				text = ((param.OrderState != 0) ? (text + " AND B.[NotityState]= " + param.OrderState) : (text + " AND B.[NotityState] is null "));
			}
			return text;
		}

		public OrderDetail GetOrderDetail(string orderId)
		{
			return DalContext.GetModel<OrderDetail>("\r\n            SELECT A.*     \r\n                  ,B.[NotityState] as [OrderState]\r\n                  ,B.[NotityState]\r\n                  ,B.[NotityCount]\r\n                  ,B.[NotityUrl] as [NotityAddress]\r\n                  ,B.[NotityContext]\r\n                  ,B.[NotityTime],\r\n                  C.PayName,D.Name InterfaceName ,F.ChannelName\r\n            FROM [OrderBase](nolock) as A\r\n            LEFT JOIN [OrderNotity](nolock) AS B on A.[OrderId]=B.[OrderId]\r\n            Left Join PayType C On A.PayCode=C.PayCode\r\n            Left Join InterfaceBusiness D On A.[InterfaceCode]=D.Code\r\n            Left Join PayChannel F On A.ChannelCode=F.Code\r\n            WHERE  A.OrderId=@OrderId  ", new
			{
				OrderId = orderId
			});
		}

		public OrderBase GetOrderBase(string orderId)
		{
			return DalContext.GetModel<OrderBase>(" SELECT * FROM OrderBase WHERE OrderId=@OrderId ", new
			{
				OrderId = orderId
			});
		}

		public OrderNotity GetOrderNotity(string orderId)
		{
			return DalContext.GetModel<OrderNotity>(" SELECT * FROM OrderNotity WHERE OrderId=@OrderId ", new
			{
				OrderId = orderId
			});
		}

		public bool InsertOrderNotity(OrderNotity model)
		{
			return DalContext.Insert(model) > 0;
		}

		public List<OrderNotityInfo> GetOrderNotityPageList(OrderNotityInfo parm, ref Paging paging)
		{
			string str = " select count(*) from OrderNotity A WHERE 1=1  ";
			string text = "";

            parm.UserId = DalContext.EscapeString(parm.UserId);
            parm.OrderId = DalContext.EscapeString(parm.OrderId);
            if (!string.IsNullOrEmpty(parm.UserId))
			{
				text = text + " And B.UserId='" + parm.UserId + "'";
			}
			if (!string.IsNullOrEmpty(parm.OrderId))
			{
				text = text + " And A.OrderId='" + parm.OrderId + "'";
			}
			if (parm.NotityState > -1)
			{
				text = text + " And A.NotityState=" + parm.NotityState;
			}
			if (parm.BeginTime.HasValue)
			{
				text += $" AND A.[addtime]>='{parm.BeginTime.Value:yyyy-MM-dd HH:mm:ss}' ";
			}
			if (parm.EndTime.HasValue)
			{
				text += $" AND A.[addtime]<='{parm.EndTime.Value:yyyy-MM-dd HH:mm:ss}' ";
			}
			string sql = " select A.*,B.UserId from OrderNotity A inner Join OrderBase B On A.OrderId=B.OrderId Where 1=1  " + text;
			str += text;
			return DalContext.GetPage<OrderNotityInfo>(sql, str, " * ", " AddTime desc ", ref paging);
		}

		public bool EditOrderNotity(OrderNotity model)
		{
			return DalContext.ExecuteSql("UPDATE [OrderNotity] SET NotityState=@NotityState, NotityCount=@NotityCount , NotityContext=@NotityContext, NotityTime=@NotityTime\r\n            WHERE [NotityId]=@NotityId ", model) > 0;
		}

        public bool EditOrderInterfaceCode(string orderId, string interfaceCode, string channelOrderNo)
        {
            return DalContext.ExecuteSql("UPDATE [OrderBase] SET InterfaceCode=@InterfaceCode ,ChannelOrderNo=@ChannelOrderNo WHERE [OrderId]=@OrderId ",
                new { OrderId = orderId, InterfaceCode = interfaceCode, ChannelOrderNo = channelOrderNo }) > 0;
        }

        public OrderStateGet GetOrderPayState(string orderId)
		{
			return DalContext.GetModel<OrderStateGet>("SELECT a.OrderId, a.OrderTime, a.ExpiredTime, a.PayTime, a.PayState,b.ReturnUrl FROM OrderBase as a left join OrderNotity(nolock) as b on a.OrderId=b.OrderId WHERE a.OrderId=@OrderId ", new
			{
				OrderId = orderId
			});
		}

		public OrderCountInfo CountTodayOrder(string UserId, string Datetime)
		{
			return DalContext.GetModel<OrderCountInfo>(" select count(*) TodayOrderCount,SUM(OrderAmt) TodayOrderAmt from OrderBase \r\n                 where PayState=1 and CONVERT(varchar(10),PayTime,20)=CONVERT(varchar(10),@Datetime,20) and UserId=@UserId ", new
			{
				UserId,
				Datetime
			});
		}

		public List<OrderCountInfo> GetCountOrderList(string MinDate, string Maxdate, string UserId)
		{
			return DalContext.GetList<OrderCountInfo>(" select CONVERT(varchar(10),OrderTime,20) CountDate,\r\n                    SUM(OrderAmt) CountOrderAmt,\r\n                    SUM(case when PayState=1 then OrderAmt else 0 end ) CountPayOrderAmt\r\n                    from OrderBase \r\n                     where CONVERT(varchar(10),OrderTime,20)>=CONVERT(varchar(10),@MinDate,20) and \r\n                    CONVERT(varchar(10),OrderTime,20)<=CONVERT(varchar(10),@Maxdate,20) and UserId=@UserId\r\n                    group by CONVERT(varchar(10),OrderTime,20) \r\n                     ", new
			{
				MinDate,
				Maxdate,
				UserId
			});
		}

		public List<OrderCountInfo> GetCountOrderList(string UserId)
		{
			return DalContext.GetList<OrderCountInfo>(" select D.PayName, SUM(case when PayState=1 and CONVERT(varchar(10),A.PayTime,20)= CONVERT(varchar(10),getdate(),20) And userid=@UserId then OrderAmt else 0 end ) CountPayOrderAmt,\r\n        count(case when PayState=1 and CONVERT(varchar(10),A.PayTime,20)= CONVERT(varchar(10),getdate(),20) And userid=@UserId then 1 else null end) PayOrderCount\r\n         from  InterfaceType C \r\n         left Join OrderBase A On A.PayCode=C.PayCode \r\n         left join  PayType D on C.PayCode=D.PayCode\r\n        where C.[Type]=2 And C.InterfaceCode=@UserId\r\n        group by D.PayName\r\n        order by D.PayName asc ", new
			{
				UserId
			});
		}

		public OrderCountInfo AgentCountTodayOrder(string UserId)
		{
			return DalContext.GetModel<OrderCountInfo>(" select count(*) TodayOrderCount,SUM(OrderAmt) TodayOrderAmt,SUM(AgentAmt) CountPayOrderAmt from OrderBase \r\n                 where PayState=1 and CONVERT(varchar(10),PayTime,20)=CONVERT(varchar(10),GETDATE(),20) and AgentId=@UserId ", new
			{
				UserId
			});
		}

		public OrderCountInfo AgentCountTodayOrder(string UserId, string Datetime)
		{
			return DalContext.GetModel<OrderCountInfo>(" select count(*) TodayOrderCount,SUM(OrderAmt) TodayOrderAmt,SUM(AgentAmt) CountPayOrderAmt from OrderBase \r\n                 where PayState=1 and CONVERT(varchar(10),PayTime,20)=CONVERT(varchar(10),@Datetime,20) and AgentId=@UserId ", new
			{
				UserId,
				Datetime
			});
		}

		public List<OrderCountInfo> GetAgentCountOrderList(string MinDate, string Maxdate, string UserId)
		{
			return DalContext.GetList<OrderCountInfo>(" select CONVERT(varchar(10),OrderTime,20) CountDate,\r\n                    SUM(OrderAmt) CountOrderAmt,\r\n                    SUM(case when PayState=1 then OrderAmt else 0 end ) CountPayOrderAmt\r\n                    from OrderBase \r\n                     where CONVERT(varchar(10),OrderTime,20)>=CONVERT(varchar(10),@MinDate,20) and \r\n                    CONVERT(varchar(10),OrderTime,20)<=CONVERT(varchar(10),@Maxdate,20) and AgentId=@UserId\r\n                    group by CONVERT(varchar(10),OrderTime,20) \r\n                     ", new
			{
				MinDate,
				Maxdate,
				UserId
			});
		}

		public int DelOrder(OrderSearchParam param)
		{
			List<Tuple<string, object>> list = new List<Tuple<string, object>>();
			string delOrderWhere = GetDelOrderWhere(param);
			list.Add(new Tuple<string, object>(" Insert into OrderBaseBack select * from OrderBase where 1=1 " + delOrderWhere, null));
			list.Add(new Tuple<string, object>(" Insert into OrderNotityBack select * from OrderNotity where orderId in (select orderid from orderbase where 1=1" + delOrderWhere + ") ", null));
			list.Add(new Tuple<string, object>(" delete OrderNotity where orderId in (select orderid from orderbase where 1=1" + delOrderWhere + ") ", null));
			list.Add(new Tuple<string, object>(" delete orderbase where 1=1 " + delOrderWhere, null));
			return DalContext.ExecuteSqlTransaction(list);
		}

		private string GetDelOrderWhere(OrderSearchParam param)
		{
			string text = "";
            param.UserId = DalContext.EscapeString(param.UserId);
            param.OrderId = DalContext.EscapeString(param.OrderId);
            param.Channel = DalContext.EscapeString(param.Channel);
            param.PayCode = DalContext.EscapeString(param.PayCode);
            param.InterfaceCode = DalContext.EscapeString(param.InterfaceCode);
            if (!string.IsNullOrEmpty(param.UserId))
			{
				text = text + " AND [UserId]='" + param.UserId + "' ";
			}
			if (!string.IsNullOrEmpty(param.OrderId))
			{
				text = text + " AND [OrderId]='" + param.OrderId + "' ";
			}
			if (param.OrderBeginTime.HasValue)
			{
				text += $" AND [OrderTime]>='{param.OrderBeginTime.Value:yyyy-MM-dd HH:mm:ss}' ";
			}
			if (param.OrderEndTime.HasValue)
			{
				text += $" AND [OrderTime]<='{param.OrderEndTime.Value:yyyy-MM-dd HH:mm:ss}' ";
			}
			if (!string.IsNullOrEmpty(param.Channel))
			{
				text = text + " AND [ChannelCode]='" + param.Channel + "' ";
			}
			if (!string.IsNullOrEmpty(param.PayCode))
			{
				text = text + " AND [PayCode]='" + param.PayCode + "' ";
			}
			if (!string.IsNullOrEmpty(param.InterfaceCode))
			{
				text = text + " AND [InterfaceCode]='" + param.InterfaceCode + "' ";
			}
			if (param.PayState > -1)
			{
				text = text + " AND [PayState]= " + param.PayState;
			}
			return text;
		}

		public AdminOrderCount GetAdminOrderCount()
		{
			return DalContext.GetModel<AdminOrderCount>(" select \r\n                (select count(*) from UserBase where UserType=1 And IsEnabled=1  ) CountBusiness,\r\n                (select Sum(A.Balance) from UserAmt A Inner Join UserBase B  On A.UserId=B.UserId  where B.UserType=1 ) SumBusinessAmt,\r\n                (select count(*) from UserBase where UserType=2  ) CountAgent,\r\n                (select Sum(Agentamt) from OrderBase(nolock) where convert(varchar(10),PayTime,20)= convert(varchar(10),getdate(),20) ) SumAgentamt,\r\n                (select Count(*) from HmAdmin where IsEnable=1) CountProm, \r\n                (select Sum(PromAmt) from OrderBase(nolock) where convert(varchar(10),PayTime,20)= convert(varchar(10),getdate(),20)) SumPromAmt,\r\n                (select count(*) from OrderBase(nolock) where convert(varchar(10),ordertime,20)= convert(varchar(10),getdate(),20)) CountOrder,\r\n                (select count(*) from OrderBase(nolock) where PayState=1 and convert(varchar(10),PayTime,20)= convert(varchar(10),getdate(),20)) CountOrderPay,\r\n                (select Sum(OrderAmt) from OrderBase(nolock) where convert(varchar(10),ordertime,20)= convert(varchar(10),getdate(),20)) SumOrderAmt,\r\n                (select Sum(OrderAmt) from OrderBase(nolock) where PayState=1 and convert(varchar(10),PayTime,20)= convert(varchar(10),getdate(),20)) SumOrderPayAmt,\r\n                (select Sum(Profits) from OrderBase(nolock) where PayState=1 and convert(varchar(10),PayTime,20)= convert(varchar(10),getdate(),20)) SumProfits ");
		}

		public List<OrderPayScale> GetOrderPayScale(string MinDate, string Maxdate)
		{
			return DalContext.GetList<OrderPayScale>("select B.PayName,sum(orderamt) sumamt,sum(orderamt)*100/(select sum(orderamt) from OrderBase where PayState=1  And\r\n            CONVERT(varchar(10),paytime,20)>=CONVERT(varchar(10),@MinDate,20) and \r\n            CONVERT(varchar(10),paytime,20)<=CONVERT(varchar(10),@Maxdate,20)  ) payscale \r\n            from OrderBase A inner join PayType B On A.PayCode=B.PayCode\r\n\t\t\t where A.PayState=1 And\r\n            CONVERT(varchar(10),paytime,20)>=CONVERT(varchar(10),@MinDate,20) and \r\n            CONVERT(varchar(10),paytime,20)<=CONVERT(varchar(10),@Maxdate,20) \r\n            group by B.PayName ", new
			{
				MinDate,
				Maxdate
			});
		}

		public List<OrderCountInfo> GetOrderPayTime(string MinDate, string Maxdate)
		{
			return DalContext.GetList<OrderCountInfo>(" \r\n            select  CONVERT(varchar(10),paytime,20) CountDate, count(*) TodayOrderCount,count(case when PayState=1 then 1 else null end) TodayPayOrderCount from OrderBase\r\n            where CONVERT(varchar(10),paytime,20)>=CONVERT(varchar(10),@MinDate,20) and\r\n            CONVERT(varchar(10),paytime,20)<=CONVERT(varchar(10),@Maxdate,20)\r\n            group by CONVERT(varchar(10),paytime,20)  ", new
			{
				MinDate,
				Maxdate
			});
		}

		public List<OrderNotity> GetSerOrderNotity(int NotityCount)
		{
			return DalContext.GetList<OrderNotity>(" select * from OrderNotity where NotityCount<@NotityCount And NotityState!=1 And NotityTime<=GETDATE() ", new
			{
				NotityCount
			});
		}
	}
}
