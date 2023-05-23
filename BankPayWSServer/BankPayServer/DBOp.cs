using HmPMer.Business;
using HmPMer.Entity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankPayWSServer.BankPayServer
{
    public class DBOp
    {
        static OrderBll orderBll = new OrderBll();
        static InterfaceBll interfaceBll = new InterfaceBll();
        public static List<InterfaceBusiness> GetInterfaceBusinesses(string codeStart)
        {
            List<InterfaceBusiness> business = interfaceBll.GetInterfaceBusinessList(-1);
            List<InterfaceBusiness> bankpays = new List<InterfaceBusiness>();
            foreach (InterfaceBusiness b in business)
            {
                if (!string.IsNullOrWhiteSpace(b.Code) && b.Code.StartsWith(codeStart))
                {
                    
                    bankpays.Add(b);
                }
            }
            return bankpays;
        }

        public static List<InterfaceType> GetInterfaceTypes(string code)
        {
            List<InterfaceType> interfaceTypes = interfaceBll.GetInterfaceTypeOnly(code, 1);
            return interfaceTypes;
        }

        public static List<OrderInfo> GetInterfaceOrderInfo(string codeStart)
        {
            List<OrderInfo> iof = new List<OrderInfo>();
            List<HmPMer.Entity.OrderInfo> orders = orderBll.GetOrderList("", codeStart, System.DateTime.Now.AddMinutes(-10), 0);
            foreach (HmPMer.Entity.OrderInfo o in orders)
            {
                if(o.OrderState == 0)
                {
                    iof.Add(new OrderInfo()
                    {
                        InterfaceCode = o.InterfaceCode,
                        Amount = o.OrderAmt,
                        OrderState = o.PayState,
                        OrderTime = o.OrderTime.Value,
                        OrderId = o.OrderId,
                        TradeOrderNo = o.ChannelOrderNo,
                        ChannelCode = o.ChannelCode,
                        CheckTime = o.OrderTime.Value
                    });
                }
                
            }
            return iof;
        }

        //public static ConcurrentDictionary<string, List<OrderInfo>> GetInterfaceOrderInfo(string codeStart)
        //{
        //    List<HmPMer.Entity.OrderInfo> orders = orderBll.GetOrderList("", codeStart, System.DateTime.Now.Date, 0);
        //    ConcurrentDictionary<string, List<OrderInfo>> orderMap = new ConcurrentDictionary<string, List<OrderInfo>>();
        //    foreach (HmPMer.Entity.OrderInfo o in orders)
        //    {
        //        List<OrderInfo> iof = new List<OrderInfo>();
        //        if (!orderMap.ContainsKey(o.InterfaceCode))
        //        {
        //            orderMap[o.InterfaceCode] = iof;
        //        }
        //        else
        //        {
        //            iof = orderMap[o.InterfaceCode];
        //        }
        //        iof.Add(new OrderInfo() { InterfaceCode = o.InterfaceCode, Amount = o.OrderAmt, OrderState = o.PayState, OrderTime = o.OrderTime.Value });
        //    }
        //    return orderMap;
        //}

        /// <summary>
        /// 修改订单信息
        /// </summary>
        /// <param name="orderId">主键</param>
        /// <param name="code">通道</param>
        /// <param name="out_trade_no">支付平台订单</param>
        public static bool UpdateOrderCode(string orderId, string code, string out_trade_no)
        {
            return orderBll.EditOrderInterfaceCode(orderId, code, out_trade_no);
        }

        public static OrderBase GetOrder(string orderId)
        {
            return orderBll.GetOrderBase(orderId);
        }
    }
}
