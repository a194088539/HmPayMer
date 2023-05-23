using System;
using System.Collections.Generic;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace BankPayWSServer.BankPayServer.SessionHandlers
{
    public interface IWebSocketBehaviorEvent
    {
        void OnMessageBefore(WebSocketBehavior webSocketBehavior, MessageEventArgs e);
        void OnCloseBefore(WebSocketBehavior webSocketBehavior, CloseEventArgs e);
        void OnOpenBefore(WebSocketBehavior webSocketBehavior);
        void OnErrorBefore(WebSocketBehavior webSocketBehavior, ErrorEventArgs error);
    }

    public interface ITunnel
    {
        void Send2This(string data);
    }

    public interface IServerHost
    {   
        /// <summary>
        /// 添加新的监听
        /// </summary>
        /// <param name="interfaceCode"></param>
        /// <returns></returns>
        bool AddClientPath(string interfaceCode);
    }
}
