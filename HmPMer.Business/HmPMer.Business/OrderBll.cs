using HmPMer.Dal;
using HmPMer.Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace HmPMer.Business
{
	public class OrderBll
	{
		private OrderDal _dal = new OrderDal();

		public List<OrderInfo> GetOrderList(OrderSearchParam param, ref Paging paging, out OrderListCount countData)
		{
			return _dal.GetOrderList(param, ref paging, out countData);
		}

        public List<OrderInfo> GetOrderList(string userId, string code, DateTime dt, int topN, int payState = -1, decimal startAmt = -1, decimal endAmt = -1)
        {
            return _dal.GetOrderList(userId, code, dt, topN, payState, startAmt, endAmt);
        }

        public DataTable GetOrderListTable(OrderSearchParam param)
		{
			return _dal.GetOrderListTable(param);
		}

		public DataTable GetOrderListTableUI(OrderSearchParam param)
		{
			return _dal.GetOrderListTableUI(param);
		}

		public OrderDetail GetOrderDetail(string orderId)
		{
			return _dal.GetOrderDetail(orderId);
		}

		public OrderBase GetOrderBase(string orderId)
		{
			return _dal.GetOrderBase(orderId);
		}

		public List<OrderNotityInfo> GetOrderNotityPageList(OrderNotityInfo parm, ref Paging paging)
		{
			return _dal.GetOrderNotityPageList(parm, ref paging);
		}

		public OrderNotity GetOrderNotity(string orderId)
		{
			return _dal.GetOrderNotity(orderId);
		}

		public bool InsertOrderNotity(OrderNotity model)
		{
			return _dal.InsertOrderNotity(model);
		}

		public bool EditOrderNotity(OrderNotity model)
		{
			return _dal.EditOrderNotity(model);
		}

		public OrderStateGet GetOrderPayState(string orderId)
		{
			return _dal.GetOrderPayState(orderId);
		}

		public OrderCountInfo CountTodayOrder(string UserId, string DateTime)
		{
			return _dal.CountTodayOrder(UserId, DateTime);
		}

		public List<OrderCountInfo> GetCountOrderList(string MinDate, string Maxdate, string UserId)
		{
			if (string.IsNullOrEmpty(MinDate))
			{
				MinDate = DateTime.Now.AddDays(-7.0).ToString("yyyy-MM-dd");
			}
			if (string.IsNullOrEmpty(Maxdate))
			{
				Maxdate = DateTime.Now.ToString("yyyy-MM-dd");
			}
			return _dal.GetCountOrderList(MinDate, Maxdate, UserId);
		}

		public OrderCountInfo AgentCountTodayOrder(string UserId)
		{
			return _dal.AgentCountTodayOrder(UserId);
		}

		public OrderCountInfo AgentCountTodayOrder(string UserId, string Datetime)
		{
			return _dal.AgentCountTodayOrder(UserId, Datetime);
		}

		public List<OrderCountInfo> GetAgentCountOrderList(string MinDate, string Maxdate, string UserId)
		{
			if (string.IsNullOrEmpty(MinDate))
			{
				MinDate = DateTime.Now.AddDays(-7.0).ToString("yyyy-MM-dd");
			}
			if (string.IsNullOrEmpty(Maxdate))
			{
				Maxdate = DateTime.Now.ToString("yyyy-MM-dd");
			}
			return _dal.GetAgentCountOrderList(MinDate, Maxdate, UserId);
		}

		public List<OrderCountInfo> GetCountOrderList(string UserId)
		{
			return _dal.GetCountOrderList(UserId);
		}

		public int DelOrder(OrderSearchParam param)
		{
			return _dal.DelOrder(param);
		}

		public AdminOrderCount GetAdminOrderCount()
		{
			return _dal.GetAdminOrderCount();
		}

		public List<OrderPayScale> GetOrderPayScale(string MinDate, string Maxdate)
		{
			return _dal.GetOrderPayScale(MinDate, Maxdate);
		}

		public List<OrderCountInfo> GetOrderPayTime(string MinDate, string Maxdate)
		{
			return _dal.GetOrderPayTime(MinDate, Maxdate);
		}

		public List<OrderNotity> GetSerOrderNotity(int NotityCount)
		{
			return _dal.GetSerOrderNotity(NotityCount);
		}

        public bool EditOrderInterfaceCode(string orderId, string interfaceCode, string channelOrderNo)
        {
            return _dal.EditOrderInterfaceCode(orderId, interfaceCode, channelOrderNo);
        }
    }
}
