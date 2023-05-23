using BankPayWSServer.BankPayServer.SessionHandlers;
using HM.Framework.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace BankPayWSServer.BankPayServer
{
    //todo 心跳检测
    /// <summary>
    /// 通信管理
    /// </summary>
    public class ClientHandlerManager : IWebSocketBehaviorEvent
    {
        ClientHandler[] _socketHandlers = null;//处理对象，下标和_allBusinesses对应
        Dictionary<string, Dictionary<string, Message>> _msgRecords = null;
        List<Message> _dropMessages = new List<Message>();
        InterfaceInfo[] _allBusinesses = null;//所有的接口商
        List<InterfaceInfo> _moreBusinesses = new List<InterfaceInfo>();//动态运行中添加的接口商
        List<ClientHandler> _moreSocketHandlers = new List<ClientHandler>();//动态运行中添加的接口商
        List<int> _schedules = new List<int>();// 调度顺序列表，_schedules[0]为最新使用的接口商对应的_allBusinesses的下标
        int _nullScheduleIndex = 0;//为空的调度下标
        public ScheduleRule[] ScheduleRules
        {
            get; set;
        }

        public void Init(InterfaceInfo[] allBusinesse)
        {
            this._allBusinesses = allBusinesse;
            this._socketHandlers = new ClientHandler[this._allBusinesses.Length];
            this._msgRecords = new Dictionary<string, Dictionary<string, Message>>();
            this._dropMessages.Clear();
            int index = 0;
            foreach (InterfaceInfo i in allBusinesse)
            {
                //加入调度
                _schedules.Add(index++);
                this._msgRecords.Add(i.InterfaceCode, new Dictionary<string, Message>());
            }
            _nullScheduleIndex = 0;//所有的调度都为null,不可使用
            //开启消息检查，定时移除
            (new Task(() =>
            {
                while (true)
                {
                    while (_dropMessages.Count > 0)
                    {
                        Message m = _dropMessages[0];
                        _dropMessages.RemoveAt(0);
                        Dictionary<string, Message> msgs = null;
                        ClientHandler handler = m.WsHandler as ClientHandler;
                        if (handler != null && _msgRecords.TryGetValue(handler.InterfaceInfo.InterfaceCode, out msgs))
                        {
                            lock (msgs)
                            {
                                msgs.Remove(m.SendId);
                            }
                        }
                    }
                    System.Threading.Thread.Sleep(5000);
                }

            })).Start();
        }

        public void LoadMoreInterface(InterfaceInfo[] allBusinesse)
        {
            lock (_moreBusinesses)
            {
                Dictionary<string, InterfaceInfo> mps = new Dictionary<string, InterfaceInfo>();
                for (int i = 0; i < _allBusinesses.Length; i++)
                {
                    mps[_allBusinesses[i].InterfaceCode] = _allBusinesses[i];
                }
                for (int i = 0; i < _moreBusinesses.Count; i++)
                {
                    mps[_moreBusinesses[i].InterfaceCode] = _moreBusinesses[i];
                }
                int index = Count();
                foreach (InterfaceInfo i in allBusinesse)
                {
                    if (mps.ContainsKey(i.InterfaceCode))
                    {
                        InterfaceInfo interfaceInfo = mps[i.InterfaceCode];
                        interfaceInfo.InterfaceAccount = i.InterfaceAccount;
                        interfaceInfo.InterfaceMd5 = i.InterfaceMd5;
                        interfaceInfo.InterfaceRsaPrivate = i.InterfaceRsaPrivate;
                        interfaceInfo.InterfaceRsaPublic = i.InterfaceRsaPublic;
                        interfaceInfo.account = i.account;
                        interfaceInfo.password = i.password;
                        interfaceInfo.LocalIp = "127.0.0.1";
                    }
                    else
                    { //加入调度
                        _schedules.Add(index++);
                        this._msgRecords.Add(i.InterfaceCode, new Dictionary<string, Message>());
                        _moreSocketHandlers.Add(null);
                        _moreBusinesses.Add(i);
                        mps[i.InterfaceCode] = i;
                    }

                }
            }

        }

        public int Count()
        {
            return _allBusinesses.Length + _moreBusinesses.Count;
        }

        public ClientHandler GetHandler(int i)
        {
            if (i < _socketHandlers.Length)
            {
                return _socketHandlers[i];
            }
            else
            {
                return _moreSocketHandlers[i - _socketHandlers.Length];
            }
        }

        public void SetHandler(int i, ClientHandler handler)
        {
            if (i < _socketHandlers.Length)
            {
                _socketHandlers[i] = handler;
            }
            else
            {
                _moreSocketHandlers[i - _socketHandlers.Length] = handler;
            }
        }

        public InterfaceInfo GetInterfaceInfo(int i)
        {
            if (i < _allBusinesses.Length)
            {
                return _allBusinesses[i];
            }
            else
            {
                return _moreBusinesses[i - _allBusinesses.Length];
            }
        }

        public void OnCloseBefore(WebSocketBehavior webSocketBehavior, CloseEventArgs e)
        {
            ClientHandler socketHandler = webSocketBehavior as ClientHandler;
            if (socketHandler != null)
            {
                LogUtil.Debug("ClientHandlerManager DisConnected:" + socketHandler.InterfaceInfo.InterfaceCode);
                //发出错误
                Dictionary<string, Message> msgs = null;
                if (_msgRecords.TryGetValue(socketHandler.InterfaceInfo.InterfaceCode, out msgs))
                {
                    _msgRecords[socketHandler.InterfaceInfo.InterfaceCode] = new Dictionary<string, Message>();
                    lock (msgs)
                    {
                        foreach (var p in msgs)
                        {
                            if (p.Value.Code == 0)
                            {
                                p.Value.exception = new Exception(e.Reason);
                                p.Value.Success = false;
                                p.Value.Code = 2;//关闭
                                p.Value.SubCode = e.Code;
                                Task t = new Task(() =>
                                {
                                    try
                                    {
                                        p.Value.OnComplete(p.Value);
                                        lock (_dropMessages)
                                        {
                                            _dropMessages.Add(p.Value);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LogUtil.Debug("ClientHandlerManager OnCloseBefore Callback error", ex);
                                        lock (_dropMessages)
                                        {
                                            _dropMessages.Add(p.Value);
                                        }
                                    }
                                });
                                t.Start();
                            }
                        }
                        msgs.Clear();
                    }
                }
            }
        }

        public void OnErrorBefore(WebSocketBehavior webSocketBehavior, ErrorEventArgs error)
        {

        }

        public void OnMessageBefore(WebSocketBehavior webSocketBehavior, MessageEventArgs e)
        {
            ClientHandler socketHandler = webSocketBehavior as ClientHandler;
            if (socketHandler != null)
            {
                //
                Command command = Command.Parse(e.Data);
                if (command == null)
                {
                    return;
                }
                if (command.IsRequest)
                {

                }
                else
                {
                    string rid = command.ReplyId;
                    Dictionary<string, Message> msgs = null;
                    if (!_msgRecords.TryGetValue(socketHandler.InterfaceInfo.InterfaceCode, out msgs))
                    {
                        return;
                    }
                    Message m = null;
                    if (!msgs.TryGetValue(rid, out m))
                    {
                        return;
                    }
                    lock (m.Recv)
                    {
                        m.Recv.Add(command);
                    }
                    m.Code = 1;
                    m.LastRecvTime = DateTime.Now;
                    Task t = new Task(() =>
                    {
                        try
                        {
                            //回调
                            m.Success = true;
                            m.OnComplete(m);
                            if (m.CanDestroy)
                            {
                                lock (_dropMessages)
                                {
                                    _dropMessages.Add(m);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogUtil.Debug("ClientHandlerManager OnMessageBefore Callback error", ex);
                            lock (_dropMessages)
                            {
                                _dropMessages.Add(m);
                            }
                        }
                    });
                    t.Start();
                }
            }
        }

        public void OnOpenBefore(WebSocketBehavior webSocketBehavior)
        {
            ClientHandler socketHandler = webSocketBehavior as ClientHandler;

            if (socketHandler != null)
            {
                LogUtil.Debug("WebSocket Connected:" + socketHandler.InterfaceInfo.InterfaceCode);
                int bc = this.Count();
                for (int i = 0; i < bc; i++)
                {
                    InterfaceInfo interfaceInfo = this.GetInterfaceInfo(i);
                    if (interfaceInfo.InterfaceCode == socketHandler.InterfaceInfo.InterfaceCode)
                    {
                        SetHandler(i, socketHandler);
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public bool Send(ClientHandler socketHandler, Command cmd, Action<Message> recvAction)
        {
            Message m = new Message(cmd);
            m.OnComplete = recvAction;
            m.WsHandler = socketHandler;
            m.DateTime = DateTime.Now;
            Dictionary<string, Message> msgs = null;
            try
            {
                if (_msgRecords.TryGetValue(socketHandler.InterfaceInfo.InterfaceCode, out msgs))
                {
                    lock (msgs)
                    {
                        msgs[m.SendId] = m;
                    }
                    socketHandler.Send2This(cmd.ToString());
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogUtil.Debug("ClientHandlerManager Send error", ex);
                try
                {

                    m.Code = 4;
                    m.exception = ex;
                    m.Success = false;
                    recvAction(m);
                    if (m.CanDestroy)
                    {
                        lock (_dropMessages)
                        {
                            _dropMessages.Add(m);
                        }
                    }
                }
                catch (Exception ex2)
                {
                    LogUtil.Debug("ClientHandlerManager Send Callback error", ex2);
                    lock (_dropMessages)
                    {
                        _dropMessages.Add(m);
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// 获得对应金额的接口商
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        private string[] InterfaceCodesByScheduleRule(decimal amount)
        {
            ScheduleRule[] scheduleRules = this.ScheduleRules;
            if (scheduleRules != null && scheduleRules.Length > 0)
            {
                foreach (ScheduleRule s in scheduleRules)
                {
                    if (s.Name == ScheduleRule.dispatch_range
                        && s.Start < amount
                        && amount <= s.End)
                    {
                        if (s.AllInterface)
                        {
                            return null;
                        }
                        else
                        {
                            return s.InterfaceCodes;
                        }
                    }
                }
            }
            return null;
        }
        private ScheduleRule GetLimitScheduleRule(OrderInfo.PayType payType)
        {
            ScheduleRule[] scheduleRules = this.ScheduleRules;
            if (scheduleRules != null && scheduleRules.Length > 0)
            {
                foreach (ScheduleRule s in scheduleRules)
                {
                    if (s.Name == ScheduleRule.limit_alipay && payType == OrderInfo.PayType.Alipay)
                    {
                        return s;
                    }
                    else if (s.Name == ScheduleRule.limit_weixin && payType == OrderInfo.PayType.WeixinPay)
                    {
                        return s;
                    }

                }
            }
            return null;
        }
        /// <summary>
        /// 检查所有接口商，重置调度
        /// </summary>
        public void ResetSchedule()
        {
            lock (_schedules)
            {
                _nullScheduleIndex = _schedules.Count;
                int c = this.Count();
                for (int i = 0; i < c; i++)
                {
                    ClientHandler handler = this.GetHandler(i);
                    if (handler == null
                        || handler.ConnectionState != WebSocketState.Open
                        )
                    {
                        int index = _schedules.IndexOf(i);
                        if (index > -1)
                        {
                            _nullScheduleIndex -= 1;
                            _schedules.RemoveAt(index);
                        }
                        _schedules.Add(i);
                    }
                }
            }
        }
        /// <summary>
        /// 调度任务
        /// </summary>
        /// <returns></returns>
        public ClientHandler Schedule(decimal amount, OrderInfo.PayType payType)
        {
            string[] interfaceCodes = InterfaceCodesByScheduleRule(amount);
            ScheduleRule payLimit = GetLimitScheduleRule(payType);
            ClientHandler handler = null;
            if (_nullScheduleIndex == 0)
            {
                ResetSchedule();
                if (_nullScheduleIndex == 0)
                {
                    LogUtil.Debug("Schedule ResetSchedule not find enable schedule");
                    return handler;//找不到可用接口
                }
            }
            int i = -1;
            int firstEnable = -1;
            int ii = 0;//接口商下标
            lock (_schedules)
            {
                for (i = _nullScheduleIndex - 1; i >= 0; i--)
                {
                    ii = _schedules[i];//接口商下标
                    ClientHandler chandler = this.GetHandler(ii);
                    decimal am = 0;
                    if (chandler != null && payLimit != null)
                    {
                        if (chandler.InterfaceInfo.SuccessOrderAmount.TryGetValue(payType, out am))
                        {
                            if (am > payLimit.Start)
                            {
                                continue;
                            }
                        }
                    }
                    if (chandler == null
                        || chandler.ConnectionState != WebSocketState.Open
                        //|| DateTime.Now.Subtract(chandler.InterfaceInfo.logintime).TotalSeconds > 4 * 3500
                        || string.IsNullOrWhiteSpace(chandler.InterfaceInfo.key)
                        || !chandler.InterfaceInfo.Enable
                        || (!chandler.InterfaceInfo.WeixinEnable && payType == OrderInfo.PayType.WeixinPay)
                        || (!chandler.InterfaceInfo.AlipayEnable && payType == OrderInfo.PayType.Alipay)
                        )
                    {
                        //if(chandler.ConnectionState == WebSocketState.Open && DateTime.Now.Subtract(chandler.InterfaceInfo.logintime).TotalSeconds > 4 * 3600)
                        //{
                        //    InterfaceLogin(handler, null);
                        //}
                        continue;
                    }
                    if (firstEnable == -1)
                    {
                        firstEnable = i;
                    }
                    bool find = false;
                    if (interfaceCodes != null)
                    {
                        for (int j = 0; j < interfaceCodes.Length; j++)
                        {
                            if (interfaceCodes[j] == this.GetInterfaceInfo(ii).InterfaceCode)
                            {
                                find = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                    if (find)
                        break;
                }
                if (i == -1 && firstEnable != -1)
                {
                    i = firstEnable;
                }
                if (i == -1)
                {
                    return null;
                }
                ii = _schedules[i];
                handler = this.GetHandler(ii);
                _schedules.RemoveAt(i);
                _schedules.Insert(0, ii);
                LogUtil.Debug("Schedule find enable schedule index: " + ii + " interface code " + handler.InterfaceInfo.InterfaceCode);
            }
            return handler;
        }
    }
}
