using HM.Framework.Logging;
using HmPMer.Entity;
using Newtonsoft.Json.Linq;
using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Threading;
using System.Collections.Generic;

namespace BankPayWSServer.BankPayServer.SessionHandlers
{
    public class PayHandler : WebSocketBehavior, ITunnel
    {
        public PayHandler()
        {

        }

        public PayManager PayManager
        {
            get;set;
        }

        public IServerHost ServerHost
        {
            get;set;
        }

        public ClientHandlerManager ClientHandlerManager
        {
            get;set;
        }

        public bool CanPay
        {
            get;set;
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            Command command = Command.Parse(e.Data);
            if (command != null)
            {
                try
                {
                    if (command.Cmd == "pay")
                    {
                        this.HandlerPayCmd(command);
                    }
                    else if (command.Cmd == "resetorder")
                    {
                        this.HandlerOrderCheckCmd(command);
                    }
                    else if (command.Cmd == "login")
                    {
                        this.HandlerInterfaceLogin(command);
                    }
                    else if (command.Cmd == "loginstate")
                    {
                        this.HandlerInterfaceLoginState(command);
                    }
                    else if (command.Cmd == "near")
                    {
                        this.HandlerInterfaceQueryNear(command);
                    } else if(command.Cmd == "resetschedule")
                    {
                        this.HandlerResetSchedule(command);
                    } else if(command.Cmd == "loadinterface")
                    {
                        this.HandlerLoadInterface(command);
                    }else if(command.Cmd == "disableinterface")
                    {
                        this.HandlerDisableInterface(command);
                    }
                }
                catch(Exception ex)
                {
                    this.Close(CloseStatusCode.ServerError, ex.Message.Length > 40 ? ex.Message.Substring(0,40):ex.Message);

                }
            }
        }
        public void Send2This(string data)
        {
            if (this.ConnectionState == WebSocketState.Open)
            {
                this.Send(data);
            }
        }

        public void Close2()
        {
            this.Close();
        }
        /// <summary>
        /// 支付调用
        /// </summary>
        /// <param name="command"></param>
        public void HandlerPayCmd(Command command)
        {
            //if(command == null || command.Cmd != "pay")
            //{
            //    return;
            //}
            if(!CanPay)
            {
                this.Send(command.ToReplyError("不可用时间"));
                Thread.Sleep(500);
                this.Close();
                return;
            }
            JObject json = command.GetData<JObject>();
            int amount = (int)json["total_fee"];
            string orderno = json["out_trade_no"].ToString();
            string trade_type = (string)json["trade_type"];
            OrderInfo.PayType payType = OrderInfo.PayType.Alipay;
            if (trade_type == "WEIXIN_NATIVE")
            {
                payType = OrderInfo.PayType.WeixinPay;
            }

            ClientHandler socketHandler = ClientHandlerManager.Schedule(amount / 100M, payType);
            if (socketHandler != null)
            {
                Console.WriteLine("{0} Order {1} transfer to {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), orderno, socketHandler.InterfaceInfo.InterfaceCode);
                PayManager.InterfacePay(socketHandler, this, command);
            }
            else//无可用调度
            {
                Console.WriteLine("{0} Order {1} Not Find Schedule", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), orderno);
                LogUtil.Debug("Pay Not Find Schedule: " + command.ToString());
                this.Send(command.ToReplyError("无可用接口"));
                Thread.Sleep(500);
                this.Close();
            }
        }
        /// <summary>
        /// 重新检查订单
        /// </summary>
        /// <param name="command"></param>
        public void HandlerOrderCheckCmd(Command command)
        {
            //if (command == null || command.Cmd != "queryorder")
            //{
            //    return;
            //}
            JObject json = command.GetData<JObject>();
            string orderno = json["out_trade_no"].ToString();
            OrderBase orderBase = DBOp.GetOrder(orderno);
            if(orderBase != null)
            {
                OrderInfo orderInfo = new OrderInfo();
                orderInfo.Amount = orderInfo.Amount;
                orderInfo.OrderId = orderBase.OrderId;
                orderInfo.OrderState = orderBase.PayState;
                orderInfo.InterfaceCode = orderBase.InterfaceCode;
                orderInfo.OrderTime = orderBase.OrderTime.Value;
                orderInfo.TradeOrderNo = orderBase.ChannelOrderNo;
                orderInfo.InterfaceIndex = -1;
                for (int i = 0; i < ClientHandlerManager.Count(); i++)
                {
                    var interfaceInfo = ClientHandlerManager.GetInterfaceInfo(i);
                    if (interfaceInfo != null && orderInfo.InterfaceCode == interfaceInfo.InterfaceCode)
                    {
                        orderInfo.InterfaceIndex = i;
                        orderInfo.SignMd5 = interfaceInfo.InterfaceMd5;
                    }
                }
                if(orderInfo.InterfaceIndex == -1)
                {
                    this.Send(command.ToReplyError("找不到可用接口"));
                }
                else
                {
                    PayManager.AddOrUpdateOrder(orderInfo);
                    this.Send(command.ToReply("SUCCESS"));
                }
            }
            else
            {
                this.Send(command.ToReplyError("订单不存在"));
            }
            Thread.Sleep(500);
            this.Close();
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="command"></param>
        public void HandlerInterfaceLogin(Command command)
        {
            JObject json = command.GetData<JObject>();
            string account = json["account"].ToString();
            for (int i = 0; i < ClientHandlerManager.Count(); i++)
            {
                var handler = ClientHandlerManager.GetHandler(i);
                if(handler != null && handler.InterfaceInfo.account == account)
                {
                    PayManager.InterfaceLogin(handler, null);
                    this.Send(command.ToReply("SUCCESS"));
                    break;
                }
            }
            Thread.Sleep(500);
            this.Close();
        }

        /// <summary>
        /// 获得登录状态
        /// </summary>
        /// <param name="command"></param>
        public void HandlerInterfaceLoginState(Command command)
        {
            JArray array = new JArray();
            for (int i = 0; i < ClientHandlerManager.Count(); i++)
            {
                var handler = ClientHandlerManager.GetHandler(i);
                if (handler == null)
                {
                    continue;
                }
                JObject json = new JObject();
                json.Add("interface", handler.InterfaceInfo.InterfaceCode);
                json.Add("key", handler.InterfaceInfo.key);
                json.Add("account", handler.InterfaceInfo.account);
                json.Add("realname", handler.InterfaceInfo.cashName);
                json.Add("logintime", handler.InterfaceInfo.logintime);
                json.Add("wsstate", (int)handler.ConnectionState);
                array.Add(json);
            }
            this.Send(command.ToReply(array));
            Thread.Sleep(500);
            this.Close();
        }

        /// <summary>
        /// 查询最近成功订单 5条
        /// </summary>
        /// <param name="command"></param>
        public void HandlerInterfaceQueryNear(Command command)
        {
            JObject json = command.GetData<JObject>();
            string account = (string)json["account"];
            string msg = "找不到接口或接口已断开";
            for (int i = 0; i < ClientHandlerManager.Count(); i++)
            {
                var handler = ClientHandlerManager.GetHandler(i);
                if (handler == null || handler.ConnectionState != WebSocketState.Open)
                {
                    continue;
                }
                if (account == handler.InterfaceInfo.InterfaceAccount)
                {
                    msg = "";
                    PayManager.InterfaceQueryNearlyOrder(handler, (m) => {
                        if (m.Success && m.Recv.Count > 0)
                        {
                            Command rscmd = m.Recv[0];
                            if (rscmd.Code == 0)
                            {
                                Send2This(command.ToReply(rscmd.GetData<JObject>()));
                            }
                            else
                            {
                                Send2This(command.ToReplyError("接口命令失败:" + rscmd.Code));
                            }
                        }
                        else
                        {
                            Send2This(command.ToReplyError("接口返回失败:" + m.Code));
                        }
                        m.CanDestroy = true;//设置释放标记
                    });
                }
            }
            if(!string.IsNullOrWhiteSpace(msg))
            {
                this.Send(command.ToReplyError(msg));
                Thread.Sleep(500);
                this.Close();
            }
        }

        public void HandlerResetSchedule(Command command)
        {
            ClientHandlerManager.ResetSchedule();
            Send2This(command.ToReply("Reset"));
            Thread.Sleep(500);
            this.Close();
        }

        public void HandlerLoadInterface(Command command)
        {
            JObject json = command.GetData<JObject>();
            string code = "";
            if(json["code"] != null)
            {
                code = (string)json["code"];
            }
            List<InterfaceBusiness> allBusinesses = DBOp.GetInterfaceBusinesses("_bankpayv1");
            if (!string.IsNullOrWhiteSpace(code))
            {
                foreach(InterfaceBusiness b in allBusinesses)
                {
                    if(b.Code == code)
                    {
                        InterfaceInfo interfaceInfo = new InterfaceInfo();
                        interfaceInfo.InterfaceCode = b.Code;
                        interfaceInfo.InterfaceAccount = b.Account;
                        interfaceInfo.InterfaceMd5 = b.MD5Pwd;
                        interfaceInfo.InterfaceRsaPrivate = b.RSAPrivate;
                        interfaceInfo.InterfaceRsaPublic = b.RSAOpen;
                        interfaceInfo.account = b.Account;
                        interfaceInfo.password = b.OpenPwd;
                        interfaceInfo.LocalIp = "127.0.0.1";
                        interfaceInfo.logintime = DateTime.Now.AddDays(-1);
                        interfaceInfo.loginchecktime = DateTime.Now.AddDays(-1);

                        ClientHandlerManager.LoadMoreInterface(new InterfaceInfo[] { interfaceInfo });
                        if(ServerHost != null)
                        {
                            ServerHost.AddClientPath(interfaceInfo.account);
                        }
                        break;
                    }
                }
            }
            else
            {
                InterfaceInfo[] ibs = new InterfaceInfo[allBusinesses.Count];
                int index = 0;
                foreach (InterfaceBusiness b in allBusinesses)
                {
                    InterfaceInfo interfaceInfo = new InterfaceInfo();
                    interfaceInfo.InterfaceCode = b.Code;
                    interfaceInfo.InterfaceAccount = b.Account;
                    interfaceInfo.InterfaceMd5 = b.MD5Pwd;
                    interfaceInfo.InterfaceRsaPrivate = b.RSAPrivate;
                    interfaceInfo.InterfaceRsaPublic = b.RSAOpen;
                    interfaceInfo.account = b.Account;
                    interfaceInfo.password = b.OpenPwd;
                    interfaceInfo.LocalIp = "127.0.0.1";
                    interfaceInfo.logintime = DateTime.Now.AddDays(-1);
                    interfaceInfo.loginchecktime = DateTime.Now.AddDays(-1);
                    ibs[index++] = interfaceInfo;
                }
                ClientHandlerManager.LoadMoreInterface(ibs);
                for (int i = 0; i < ibs.Length; i++)
                {
                    ServerHost.AddClientPath(ibs[i].InterfaceCode);
                }
            }
            Thread.Sleep(500);
            this.Close();
        }

        public void HandlerDisableInterface(Command command)
        {
            JObject json = command.GetData<JObject>();
            string code = (string)json["code"];
            for (int i = 0; i < ClientHandlerManager.Count(); i++)
            {
                var interfaceInfo = ClientHandlerManager.GetInterfaceInfo(i);
                if (interfaceInfo.InterfaceCode == code)
                {
                    interfaceInfo.Enable = false;
                    Send2This(command.ToReply("code:" + code + " disabled"));
                    break;
                }
            }
            Thread.Sleep(500);
            this.Close();
        }

        public void HandlerEnableInterface(Command command)
        {
            JObject json = command.GetData<JObject>();
            string code = (string)json["code"];
            for (int i = 0; i < ClientHandlerManager.Count(); i++)
            {
                var interfaceInfo = ClientHandlerManager.GetInterfaceInfo(i);
                if (interfaceInfo.InterfaceCode == code)
                {
                    interfaceInfo.Enable = true;
                    Send2This(command.ToReply("code:" + code + " enabled"));
                    break;
                }
            }
            Thread.Sleep(500);
            this.Close();
        }
    }
}
