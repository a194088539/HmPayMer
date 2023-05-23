using BankPayWSServer.BankPayServer;
using HmPMer.Entity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BankPayWSServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DateTime startDate = DateTime.Now;
            string startTime = AooFu.Tools.Config.GetConfigurationValue("OnlineTime");
            BankPaySession session = new BankPaySession(8011, startTime);
            session.Start("schedule_rules.txt");
            Task task = new Task(() =>
            {
                bool g = true;
                while (g)
                {
                    string cmd = Console.ReadLine().Trim();
                    g = HandleCommand(session, cmd);
                }
            });
            task.Start();
            while (true)
            {
                if (task.IsCompleted)
                {
                    break;
                }
                if(DateTime.Now.Subtract(startDate).TotalHours > 2 && DateTime.Now.Hour == 1)
                {
                    session.End();
                    System.Threading.Thread.Sleep(2000);
                    Process.Start(typeof(Program).Assembly.Location);
                    break;
                }
                System.Threading.Thread.Sleep(2000);

            }
        }

        public static bool HandleCommand(BankPaySession session, string cmd)
        {
            if (cmd == "state")
            {
                for (int i = 0; i < session.ClientHandlerManager.Count(); i++)
                {
                    var handler = session.ClientHandlerManager.GetHandler(i);
                    if (handler == null)
                    {
                        continue;
                    }
                    Console.WriteLine("interface " + handler.InterfaceInfo.InterfaceCode);
                    Console.WriteLine("key " + handler.InterfaceInfo.key);
                    Console.WriteLine("account " + handler.InterfaceInfo.account);
                    Console.WriteLine("realname " + handler.InterfaceInfo.cashName);
                    Console.WriteLine("logintime " + handler.InterfaceInfo.logintime);
                    Console.WriteLine("wsstate " + (int)handler.ConnectionState);
                    Console.WriteLine("-------------------------------");
                }
            }
            else if (cmd == "exit")
            {
                session.End();
                return false;
            }
            else if (cmd == "restart")
            {
                session.End();
                Process.Start(typeof(Program).Assembly.Location);
                return false;
            }
            else if (cmd.StartsWith("login"))
            {
                string account = cmd.Substring("login".Length + 1);
                for (int i = 0; i < session.ClientHandlerManager.Count(); i++)
                {
                    var handler = session.ClientHandlerManager.GetHandler(i);
                    if (handler != null && handler.InterfaceInfo.account == account)
                    {
                        session.PayManager.InterfaceLogin(handler, null);
                        break;
                    }
                }
            }
            else if (cmd.StartsWith("checkorder"))
            {
                string orderno = cmd.Substring("checkorder".Length + 1);
                OrderBase orderBase = DBOp.GetOrder(orderno);
                if (orderBase != null)
                {
                    BankPayServer.OrderInfo orderInfo = new BankPayServer.OrderInfo();
                    orderInfo.Amount = orderInfo.Amount;
                    orderInfo.OrderId = orderBase.OrderId;
                    orderInfo.OrderState = orderBase.PayState;
                    orderInfo.InterfaceCode = orderBase.InterfaceCode;
                    orderInfo.OrderTime = orderBase.OrderTime.Value;
                    orderInfo.TradeOrderNo = orderBase.ChannelOrderNo;
                    orderInfo.InterfaceIndex = -1;
                    for (int i = 0; i < session.ClientHandlerManager.Count(); i++)
                    {
                        var interfaceInfo = session.ClientHandlerManager.GetInterfaceInfo(i);
                        if (interfaceInfo != null && orderInfo.InterfaceCode == interfaceInfo.InterfaceCode)
                        {
                            orderInfo.InterfaceIndex = i;
                            orderInfo.SignMd5 = interfaceInfo.InterfaceMd5;
                        }
                    }
                    if (orderInfo.InterfaceIndex == -1)
                    {
                        Console.WriteLine("找不到可用接口");
                    }
                    else
                    {
                        session.PayManager.AddOrUpdateOrder(orderInfo);
                        Console.WriteLine("重置成功");
                    }
                }
                else
                {
                    Console.WriteLine("订单不存在");
                }

            }
            else if (cmd == "loadinterface")
            {
                List<InterfaceBusiness> allBusinesses = DBOp.GetInterfaceBusinesses("_bankpayv1");
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
                session.ClientHandlerManager.LoadMoreInterface(ibs);
                for (int i = 0; i < ibs.Length; i++)
                {
                    session.AddClientPath(ibs[i].InterfaceCode);
                }
            }
            return true;
        }
    }
}
