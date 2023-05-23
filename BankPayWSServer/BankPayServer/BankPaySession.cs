using System;
using System.Collections.Generic;
using HmPMer.Entity;
using WebSocketSharp.Server;
using WebSocketSharp;
using HM.Framework.Logging;
using System.Threading.Tasks;
using BankPayWSServer.BankPayServer.SessionHandlers;
using System.IO;

namespace BankPayWSServer.BankPayServer
{
 
    //todo 登录问题
    //todo 错误处理
    public class BankPaySession : IServerHost
    {
        int _port = 0;
        const string InterfaceCodeStarts = "_bankpayv1";
        //ConcurrentDictionary<string, List<InterfaceOrderInfo>> _orderInfoMaps = null;//key 为接口代码
        
        
        WebSocketServer _wssv;

        ClientHandlerManager _clientManager;
        PayManager _payManager;
        TimeSpan _startTime = TimeSpan.MinValue;
        TimeSpan _endTime = TimeSpan.MinValue;
        public BankPaySession(int port, string startTime)
        {
            _port = port;
            if (!string.IsNullOrEmpty(startTime))
            {
                string[] sp = startTime.Split('-');
                if (sp.Length == 2)
                {
                    _startTime = TimeSpan.Parse(sp[0]);
                    _endTime = TimeSpan.Parse(sp[1]);
                }
            }
            
        }


        public ClientHandlerManager ClientHandlerManager { get { return _clientManager; } }

        public PayManager PayManager { get { return _payManager; } }
        public bool Start(string scheduleRulePath)
        {
            if(_wssv != null)
            {
                return false;
            }
            _clientManager = new ClientHandlerManager();
            Console.WriteLine("Load Interface...");
            List<InterfaceBusiness> allBusinesses = DBOp.GetInterfaceBusinesses(InterfaceCodeStarts);
            if(allBusinesses.Count == 0)
            {
                Console.WriteLine("No Interface.Please check");
                return false;
            }
            Console.WriteLine("Load Interface Count:" + allBusinesses.Count);
            InterfaceInfo[] allBusinessesInfo = new InterfaceInfo[allBusinesses.Count];
            //_orderInfoMaps = GetInterfaceOrderInfo();
            
            _wssv = new WebSocketServer(_port, true);
            _wssv.SslConfiguration.ServerCertificate =
                new System.Security.Cryptography.X509Certificates.X509Certificate2("server.pfx", "Aa123.com");
            LogUtil.Debug("BankPay Start: interface count " + allBusinesses.Count);
            for (int i = 0; i < allBusinesses.Count; i++)
            {
                var b = allBusinesses[i];
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
                interfaceInfo.Enable = b.IsEnabled == 1;
                //
                allBusinessesInfo[i] = interfaceInfo;
                //开启路由
                _wssv.AddWebSocketService<ClientHandler>("/agent" + b.Account, (ClientHandler handler) =>
                {
                    handler.WebSocketBehaviorEvent = new List<IWebSocketBehaviorEvent>();
                    handler.WebSocketBehaviorEvent.Add(_clientManager);
                    handler.InterfaceInfo = interfaceInfo;
                });
            }
            this._clientManager.Init(allBusinessesInfo);
            //初始化订单相关
            
            this._payManager = new PayManager(this._clientManager);
            this._payManager.StartOrderCheck();
            //开启支付方调用监听
            _wssv.AddWebSocketService<PayHandler>("/pay", (PayHandler handler) =>
            {
                bool canpay = true;
                if(_startTime != _endTime)
                {
                    if(DateTime.Now.TimeOfDay.CompareTo(_startTime) < 0
                    || DateTime.Now.TimeOfDay.CompareTo(_endTime) >= 0)
                    {
                        canpay = false;
                    }
                }
                handler.PayManager = _payManager;
                handler.ClientHandlerManager = _clientManager;
                handler.CanPay = canpay;
                handler.ServerHost = this;
            });

            //调度规则
            if(!string.IsNullOrWhiteSpace(scheduleRulePath) && File.Exists(scheduleRulePath))
            {
                string scontent = File.ReadAllText(scheduleRulePath, System.Text.Encoding.UTF8);
                ScheduleRule[] rules = ScheduleRule.ParseRules(scontent);
                if(rules != null && rules.Length > 0)
                {
                    ClientHandlerManager.ScheduleRules = rules;
                }
            }
            _wssv.Start();
            Console.WriteLine("Server Start OK! Port:" + _port);
            //登录检测
            LoginTask();
            //接口启用状态检测
            InterfaceTask();
            return true;
        }

        private void LoginTask()
        {
            Task task = new Task(() =>
            {
                Random random = new Random();
                bool open = true;
                while (true)
                {
                    if (_startTime != _endTime)
                    {
                        if (DateTime.Now.TimeOfDay.CompareTo(_startTime.Subtract(TimeSpan.FromMinutes(10))) < 0//提前十分钟登录
                        || DateTime.Now.TimeOfDay.CompareTo(_endTime) >= 0)
                        {
                            open = false;
                        }
                        else
                        {
                            open = true;
                        }
                    }

                    int c = _clientManager.Count();
                    if (!open)
                    {
                        for (int i = 0; i < c; i++)
                        {
                            InterfaceInfo ifinfo = _clientManager.GetInterfaceInfo(i);
                            if (ifinfo != null)
                            {
                                ifinfo.key = "";
                                ifinfo.SuccessOrderAmount.Clear();
                            }
                        }
                    }
                    else
                    {
                        int counter = 0;
                        for (int i = 0; i < c; i++)
                        {
                            ClientHandler handler = _clientManager.GetHandler(i);
                            if (handler != null
                            && handler.ConnectionState == WebSocketState.Open
                            && (
                                //DateTime.Now.Subtract(handler.InterfaceInfo.logintime).TotalHours > 14
                                string.IsNullOrWhiteSpace(handler.InterfaceInfo.key))
                                && handler.InterfaceInfo.loginErrorNum < 2
                            //&& DateTime.Now.Subtract(handler.InterfaceInfo.loginchecktime).TotalSeconds > 10 * 60 + random.Next(60)
                            )
                            {
                                if (counter < 10)
                                {
                                    this._payManager.InterfaceLogin(handler, null);
                                    handler.InterfaceInfo.loginchecktime = DateTime.Now;
                                    counter++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }


                    System.Threading.Thread.Sleep(1000 + random.Next(5000));
                }
            });
            task.Start();
        }

        private void InterfaceTask()
        {
            Task task = new Task(() =>
            {
                while (true)
                {
                    List<InterfaceBusiness> allBusinesses = DBOp.GetInterfaceBusinesses(InterfaceCodeStarts);
                    int c = _clientManager.Count();
                    for (int i = 0; i < c; i++)
                    {
                        InterfaceInfo ifinfo = _clientManager.GetInterfaceInfo(i);
                        if (allBusinesses.Count == 0)
                        {
                            ifinfo.Enable = false;
                            continue;
                        }
                        bool flag = false;
                        foreach (InterfaceBusiness ib in allBusinesses)
                        {
                            if (ib.Code != ifinfo.InterfaceCode)
                            {
                                continue;
                            }
                            flag = true;
                            if (ib.IsEnabled == 0)
                            {
                                ifinfo.Enable = false;
                            }
                            else
                            {
                                List<InterfaceType> interfaceTypes = DBOp.GetInterfaceTypes(ib.Code);
                                if (interfaceTypes != null && interfaceTypes.Count > 0)
                                {
                                    bool findweixin = false;
                                    bool findalpay = false;
                                    foreach (InterfaceType t in interfaceTypes)
                                    {
                                        if (t.PayCode == "PAYSAPIWEIXIN")
                                        {
                                            findweixin = true;
                                        }
                                        else if (t.PayCode == "PAYSAPIALIPAY")
                                        {
                                            findalpay = true;
                                        }
                                    }
                                    ifinfo.WeixinEnable = findweixin;
                                    ifinfo.AlipayEnable = findalpay;
                                    ifinfo.Enable = true;
                                }
                                else
                                {
                                    ifinfo.Enable = false;
                                    ifinfo.WeixinEnable = false;
                                    ifinfo.AlipayEnable = false;
                                }
                            }
                        }
                        if (!flag)
                        {
                            ifinfo.Enable = false;
                        }
                    }
                    System.Threading.Thread.Sleep(20000);
                }
                
            });
            task.Start();
        }

        public void End()
        {
            _wssv.Stop();
            _wssv = null;
        }

        public bool AddClientPath(string interfaceCode)
        {
            InterfaceInfo interfaceInfo = null;
            for (int i = 0; i < ClientHandlerManager.Count(); i++)
            {
                if(ClientHandlerManager.GetInterfaceInfo(i).InterfaceCode == interfaceCode)
                {
                    interfaceInfo = ClientHandlerManager.GetInterfaceInfo(i);
                }
            }
            if(interfaceInfo != null)
            {
                IEnumerable<string> paths = _wssv.WebSocketServices.Paths;
                string path = "/agent" + interfaceInfo.account;
                foreach (string p in paths)
                {
                    if(p == path)
                    {
                        return false;
                    }
                }
                //开启路由
                _wssv.AddWebSocketService<ClientHandler>(path, (ClientHandler handler) =>
                {
                    handler.WebSocketBehaviorEvent = new List<IWebSocketBehaviorEvent>();
                    handler.WebSocketBehaviorEvent.Add(_clientManager);
                    handler.InterfaceInfo = interfaceInfo;
                });
            }
            
            return false;
        }

    }
}
