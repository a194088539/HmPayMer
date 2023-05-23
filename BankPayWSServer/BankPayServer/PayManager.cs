using BankPayWSServer.BankPay;
using BankPayWSServer.BankPay.Models;
using BankPayWSServer.BankPayServer.SessionHandlers;
using HM.Framework.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BankPayWSServer.BankPayServer
{
    /// <summary>
    /// 支付接口相关调用
    /// </summary>
    public class PayManager
    {
        ClientHandlerManager _clientManager = null;

        List<OrderInfo> _orderInfos = new List<OrderInfo>();

        Dictionary<string, OrderInfo> _successOrderNos = new Dictionary<string, OrderInfo>();

        public PayManager(ClientHandlerManager clientHandlerManager)
        {
            _clientManager = clientHandlerManager;
            
        }

        public void StartOrderCheck()
        {
            int c = _clientManager.Count();
            _orderInfos.Clear();
            _successOrderNos.Clear();
            //开启订单检查
            Task task = new Task(() =>
            {
                while (true)
                {
                    lock (_orderInfos)
                    {
                        int count2 = _orderInfos.Count;
                        for (int i = 0; i < count2; i++)
                        {
                            if(i >= _orderInfos.Count)
                            {
                                break;
                            }
                            if(_orderInfos[i].CallBackState == 1 || 
                            DateTime.Now.Subtract(_orderInfos[i].OrderTime).TotalMinutes > 6 )
                            {
                                _orderInfos.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                     //检查
                    int count = _orderInfos.Count;
                    for (int i = 0; i < count; i++)
                    {
                        OrderInfo orderInfo = _orderInfos[i];
                        if (DateTime.Now.Subtract(orderInfo.CheckTime).TotalSeconds > 2)
                        {
                            //Console.WriteLine("{0} Check Order {1} PayState: {2}, CallState: {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), orderInfo.OrderId, orderInfo.OrderState, orderInfo.CallBackState);
                            ClientHandler handler = _clientManager.GetHandler(orderInfo.InterfaceIndex);
                            if(handler!= null && orderInfo.OrderState == 0 
                            && handler.ConnectionState == WebSocketSharp.WebSocketState.Open
                            && !string.IsNullOrWhiteSpace(handler.InterfaceInfo.key))
                            {
                                InterfaceOrderState(orderInfo, handler);
                            }
                            //回调检查
                            if (orderInfo.OrderState == 1 && orderInfo.CallBackState == 0)
                            {
                                try
                                {
                                    if (orderInfo.CallbackTask == null
                                        || ((orderInfo.CallbackTask.IsFaulted
                                        || orderInfo.CallbackTask.IsCanceled)
                                        && DateTime.Now.Subtract(orderInfo.CallBackTime).TotalMinutes > 5)
                                        )
                                    {
                                        orderInfo.CallbackTask = CallBack(orderInfo, orderInfo.SignMd5);
                                    }
                                    else if (orderInfo.CallbackTask.IsCompleted)
                                    {
                                        if (orderInfo.CallbackTask.Result.IsSuccessStatusCode)
                                        {
                                            orderInfo.CallBackState = 1;
                                        }
                                        else
                                        {
                                            LogUtil.Error("CallBack " + orderInfo.OrderId + " 返回状态异常:" + orderInfo.CallbackTask.Result.StatusCode);
                                        }
                                    }
                                }catch(Exception ex)
                                {
                                    orderInfo.CallbackTask = null;
                                }
                                
                            }

                            orderInfo.CheckTime = DateTime.Now;
                        }
                    }
                    System.Threading.Thread.Sleep(1000);
                }
            });
            task.Start();
        }

        public OrderInfo FindOrder(string orderId)
        {
            lock(_orderInfos)
            {
                for (int i = 0; i < _orderInfos.Count; i++)
                {
                    if(_orderInfos[i].OrderId == orderId)
                    {
                        return _orderInfos[i];
                    }
                }
            }
            return null;
        }

        public void AddOrUpdateOrder(OrderInfo order)
        {
            lock (_orderInfos)
            {
                OrderInfo orderInfo = null;
                for (int i = 0; i < _orderInfos.Count; i++)
                {
                    if (_orderInfos[i].OrderId == order.OrderId)
                    {
                        orderInfo = _orderInfos[i];
                    }
                }
                if (orderInfo == null)
                {
                    order.OrderTime = DateTime.Now.AddMinutes(-1);
                    _orderInfos.Add(order);
                }
                else
                {
                    orderInfo.OrderTime = DateTime.Now.AddMinutes(-1);
                    orderInfo.OrderState = order.OrderState;
                    orderInfo.CallBackState = 0;
                    orderInfo.CallbackTask = null;
                }
            }
        }
        /// <summary>
        /// 发送生成二维码
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="payHandler"></param>
        /// <param name="command"></param>
        public void InterfacePay(ClientHandler handler, ITunnel payHandler, Command command)
        {
            JObject json = command.GetData<JObject>();
            int amount = (int)json["total_fee"];
            string orderno = (string)json["out_trade_no"];
            string trade_type = (string)json["trade_type"];
            int payType = 2;
            if (trade_type == "WEIXIN_NATIVE")
            {
                payType = 1;
            }
            string orderId = "";
            PABPay payData = BankPayHelper.QrcodePayData(payType, amount / 100.0, ref orderId,
                handler.InterfaceInfo.cashid, handler.InterfaceInfo.LocalIp,
                handler.InterfaceInfo.cashName, handler.InterfaceInfo.key);

            string apiAddress = string.Empty;
            string configurationValue = AooFu.Tools.Config.GetConfigurationValue("hostIPMiddleware");
            if (string.IsNullOrEmpty(configurationValue))
            {
                apiAddress = "https://bkapi.payweipan.com/" + ((payType == 1) ? "/api/Bank/wxpay_precreate" : "/api/Bank/alipay_precreate");
            }
            else
            {
                apiAddress = configurationValue + ((payType == 1) ? "/api/Bank/wxpay_precreate" : "/api/Bank/alipay_precreate");
            }
            string text = JsonConvert.SerializeObject(payData);
            _clientManager.Send(handler, HttpCmd.CreatePost(apiAddress, text, "application/json"), (m) =>
            {
                if (m.Success && m.Recv.Count > 0)
                {
                    Command rscmd = m.Recv[0];
                    LogUtil.Debug("InterfacePay " + orderId + " transfer to " + handler.InterfaceInfo.InterfaceCode + ":" + rscmd.Data);
                    if (rscmd.Code == 0)
                    {
                        JObject payRs = rscmd.GetData<JObject>();
                        if (payRs["Result"].ToString() == "0")
                        {
                            m.CanDestroy = true;//设置释放标记
                            string outTradeNo = "";
                            string qrno = "";
                            if(payRs["Data"]["outTradeNo"] != null)
                            {
                                outTradeNo = payRs["Data"]["outTradeNo"].ToString();
                                qrno = payRs["Data"]["qrCode"].ToString();
                            } else if(payRs["Data"]["out_trade_no"] != null)
                            {
                                outTradeNo = payRs["Data"]["out_trade_no"].ToString();
                                qrno = payRs["Data"]["qr_code"].ToString();
                            }
                            if(string.IsNullOrWhiteSpace(outTradeNo))
                            {
                                Console.WriteLine("{0} Order {1} transfer to {2} Error: {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), orderno, handler.InterfaceInfo.InterfaceCode, payRs["Error"].ToString());
                                payHandler.Send2This(command.ToReplyError("无订单号"));
                                return;
                            }
                            DBOp.UpdateOrderCode(orderno, handler.InterfaceInfo.InterfaceCode, outTradeNo);
                            Console.WriteLine("{0} Order {1} transfer to {2} SUCCESS!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), orderno, handler.InterfaceInfo.InterfaceCode);
                            LogUtil.Debug("Order " + orderId +" transfer to "+ handler.InterfaceInfo.InterfaceCode + " SUCCESS");
                            
                            payHandler.Send2This(command.ToReply(new { outTradeNo= outTradeNo, qrCode = qrno }));
                            OrderInfo order = new OrderInfo();
                            order.Amount = amount;
                            order.ChannelCode = trade_type;
                            order.InterfaceCode = handler.InterfaceInfo.InterfaceCode;
                            order.OrderId = orderno;
                            order.OrderState = 0;
                            order.OrderTime = DateTime.Now;
                            order.TradeOrderNo = outTradeNo;
                            order.SignMd5 = handler.InterfaceInfo.InterfaceMd5;
                            int c = _clientManager.Count();
                            for (int i = 0; i < c; i++)
                            {
                                if (_clientManager.GetInterfaceInfo(i).InterfaceCode == order.InterfaceCode)
                                {
                                    order.InterfaceIndex = i;
                                    break;
                                }
                            }
                            order.CheckTime = DateTime.Now;
                            lock (_orderInfos)
                            {
                                _orderInfos.Add(order);
                            }
                        }
                        else
                        {
                            if (payRs["Error"].ToString().Contains("签名错误"))
                            {
                                handler.InterfaceInfo.key = "";//重置登录
                            }
                            Console.WriteLine("{0} Order {1} transfer to {2} Error: {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), orderno, handler.InterfaceInfo.InterfaceCode, payRs["Error"].ToString());
                            payHandler.Send2This(command.ToReplyError(payRs["Error"].ToString()));
                        }
                    }
                    else
                    {
                        Console.WriteLine("{0} Order {1} transfer to {2} Error: {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), orderno, handler.InterfaceInfo.InterfaceCode, rscmd.Msg);
                        payHandler.Send2This(command.ToReplyError(rscmd.Msg));
                    }
                }
                else
                {
                    Console.WriteLine("{0} Order {1} transfer to {2} Error: {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), orderno, handler.InterfaceInfo.InterfaceCode, "接收消息失败");
                    payHandler.Send2This(command.ToReplyError("接收消息失败,Code: 999"));
                }
            });
        }

        /// <summary>
        /// 发送登录
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="onLogin"></param>
        public void InterfaceLogin(ClientHandler handler, Action<ClientHandler> onLogin)
        {
            string sign = BankPayHelper.getSign("", new object[]
            {
                "username",
                handler.InterfaceInfo.account,
                "userpassword",
                handler.InterfaceInfo.password,
                "client",
                3
            });
            string apiAddress = "https://bkapi.payweipan.com//api/User/Login";
            string configurationValue = AooFu.Tools.Config.GetConfigurationValue("hostIPMiddleware");
            if (!string.IsNullOrEmpty(configurationValue))
            {
                apiAddress = configurationValue + "/api/User/Login";
            }
            string text = JsonConvert.SerializeObject(new CasherInfo
            {
                sign = sign,
                username = handler.InterfaceInfo.account,
                userpassword = handler.InterfaceInfo.password,
                deviceid = "",
                client = 3,
                version = 0f
            });
            _clientManager.Send(handler, HttpCmd.CreatePost(apiAddress, text, "application/json"), (m) =>
            {
                if (m.Success && m.Recv.Count > 0)
                {
                    Command rscmd = m.Recv[0];
                    if (rscmd.Code == 0)
                    {
                        JObject loginRs = rscmd.GetData<JObject>();
                        if (loginRs["Result"].ToString() == "0")
                        {
                            Console.WriteLine("{0} {1} 登录成功", handler.InterfaceInfo.InterfaceCode, loginRs["cashid"]);
                            int cashId = (int)loginRs["cashid"];
                            string key = (string)loginRs["workkey"];
                            string cashierName = (string)loginRs["realname"];
                            handler.InterfaceInfo.cashid = cashId;
                            handler.InterfaceInfo.cashName = cashierName;
                            handler.InterfaceInfo.key = key;
                            handler.InterfaceInfo.logintime = DateTime.Now;
                            handler.InterfaceInfo.loginErrorNum = 0;
                            if(onLogin != null)
                            {
                                onLogin(handler);
                            }
                        }
                        else
                        {
                            handler.InterfaceInfo.loginErrorNum = handler.InterfaceInfo.loginErrorNum + 1;
                        }
                        m.CanDestroy = true;
                    }
                }
            });
        }

        public void InterfaceOrderState(OrderInfo orderInfo, ClientHandler handler)
        {
            int payType = 2;
            if (orderInfo.ChannelCode == "WEIXIN_NATIVE")
            {
                payType = 1;
            }
            string url = BankPayHelper.OrderStateData(
                payType,
                orderInfo.TradeOrderNo,
                handler.InterfaceInfo.cashid,
                handler.InterfaceInfo.key
                );
            _clientManager.Send(handler, HttpCmd.CreateGet(url), (m) =>
            {
                if (!(m.Success && m.Recv.Count > 0))
                {
                    return;
                }
                Command rscmd = m.Recv[0];
                if (rscmd.Code != 0)
                {
                    return;
                }
                JObject json = rscmd.GetData<JObject>();
                if (json["Result"].ToString() == "0")
                {
                    lock (_orderInfos)
                    {
                        JToken jToken = json["Data"];
                        m.CanDestroy = true;//设置释放标记
                        LogUtil.Error("CallBack Data:" + JsonConvert.SerializeObject(jToken));
                        string orderno = jToken["out_trade_no"].ToString();//接口商订单号
                        if (orderno != orderInfo.TradeOrderNo)//重置order
                        {
                            foreach (OrderInfo o in _orderInfos)
                            {
                                if (o.TradeOrderNo == orderno)
                                {
                                    orderInfo = o;
                                    break;
                                }
                            }
                        }
                        orderInfo.OrderState = 1;
                        if (orderInfo.ChannelCode == "WEIXIN_NATIVE")
                        {
                            orderInfo.PayTime = DateTime.Parse(jToken["time_end"].ToString());
                            orderInfo.Amount = Decimal.Parse(jToken["total_fee"].ToString()) * 100;
                        }
                        else
                        {
                            orderInfo.PayTime = DateTime.Parse(jToken["send_pay_date"].ToString());
                            orderInfo.Amount = Decimal.Parse(jToken["receipt_amount"].ToString()) * 100;
                        }
                        if (!_successOrderNos.ContainsKey(orderno))
                        {
                            _successOrderNos[orderno] = orderInfo;
                            int count = _clientManager.Count();
                            for (int i = 0; i < count; i++)
                            {
                                InterfaceInfo interfaceInfo = _clientManager.GetInterfaceInfo(i);
                                if (interfaceInfo != null && interfaceInfo.InterfaceCode == orderInfo.InterfaceCode)
                                {
                                    decimal am = 0;
                                    interfaceInfo.SuccessOrderAmount.TryGetValue(orderInfo.GetPayType(), out am);
                                    interfaceInfo.SuccessOrderAmount[orderInfo.GetPayType()] = am + orderInfo.Amount;
                                    break;
                                }
                            }
                        }
                        
                    }

                }
                else
                {
                    if (json["Error"] != null && json["Error"].ToString().Contains("签名错误"))
                    {
                        handler.InterfaceInfo.key = "";//重置登录
                    }
                }
            });
        }
        /// <summary>
        /// 异步返回
        /// </summary>
        /// <param name="order"></param>
        /// <param name="interfaceInfo"></param>
        public Task<HttpResponseMessage> CallBack(OrderInfo order, string signMd5)
        {
            try
            {
                Console.WriteLine("{0} Callback Order:{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), order.OrderId);
                order.CallBackTime = DateTime.Now;
                string apiUrl = AooFu.Tools.Config.GetConfigurationValue("ApiUrl").Trim().TrimEnd('/');
                apiUrl += "/OrderNotity/" + order.InterfaceCode + "/" + order.OrderId;
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                dictionary.Add("out_trade_no", order.OrderId);
                dictionary.Add("trade_no", order.TradeOrderNo);
                dictionary.Add("pay_time", order.PayTime.ToString("yyyy-MM-dd HH:mm:ss"));
                dictionary.Add("total_fee", order.Amount.ToString());

                string signstr = BankPayHelper.getSign(signMd5,
                    "out_trade_no", dictionary["out_trade_no"],
                    "pay_time", dictionary["pay_time"],
                    "total_fee", dictionary["total_fee"],
                    "trade_no", dictionary["trade_no"]);
                dictionary.Add("sign", signstr);
                return BankPayHelper.CreatePost(apiUrl, dictionary);
                
            }catch(Exception ex)
            {
                Console.WriteLine(ex);
                LogUtil.Debug("CallBack Ex:", ex);
            }
            return null;
        }
        public void InterfaceQueryNearlyOrder(ClientHandler handler, Action<Message> onComplete)
        {
            string sign = BankPayHelper.getSign(handler.InterfaceInfo.key, new object[]
                    {
                        "cashid",
                        handler.InterfaceInfo.cashid,
                        "page",
                        1,
                        "pagesize",
                        5
                    });
            string text2 = "https://bkapi.payweipan.com/";
            string configurationValue = AooFu.Tools.Config.GetConfigurationValue("hostIPMiddleware");
            if (!string.IsNullOrEmpty(configurationValue))
            {
                text2 = configurationValue;
            }
            string url = string.Concat(new object[]
                    {
                        text2,
                        "/api/BankReport/GetNearlyOrder?client=0&version=0&cashid=",
                        handler.InterfaceInfo.cashid,
                        "&sign=",
                        sign,
                        "&out_trade_no=",
                        "&page=",
                        1,
                        "&pagesize=",
                        5
                    });
            _clientManager.Send(handler, HttpCmd.CreateGet(url), onComplete);
        }

    }
}
