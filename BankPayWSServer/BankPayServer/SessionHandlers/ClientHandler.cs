using System;
using System.Collections.Generic;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace BankPayWSServer.BankPayServer.SessionHandlers
{
    public class ClientHandler : WebSocketBehavior, ITunnel
    {
        InterfaceInfo _business = null;
        List<IWebSocketBehaviorEvent> _webSocketBehaviorEvent;
        public ClientHandler()
        {

        }
        public ITunnel Tunnel
        {
            get; set;
        }

        public InterfaceInfo InterfaceInfo
        {
            get { return _business; }
            set { _business = value; }
        }

        public List<IWebSocketBehaviorEvent> WebSocketBehaviorEvent
        {
            get { return _webSocketBehaviorEvent; }
            set { _webSocketBehaviorEvent = value; }
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            if (_webSocketBehaviorEvent != null && _webSocketBehaviorEvent.Count > 0)
            {
                foreach (IWebSocketBehaviorEvent ev in _webSocketBehaviorEvent)
                {
                    ev.OnMessageBefore(this, e);
                }
            }
            if (Tunnel != null)
            {
                Tunnel.Send2This(e.Data);
            }

        }
        protected override void OnOpen()
        {
            if (_webSocketBehaviorEvent != null)
            {
                foreach (IWebSocketBehaviorEvent ev in _webSocketBehaviorEvent)
                {
                    ev.OnOpenBefore(this);
                }
            }
        }
        protected override void OnClose(CloseEventArgs e)
        {
            if (_webSocketBehaviorEvent != null)
            {
                foreach (IWebSocketBehaviorEvent ev in _webSocketBehaviorEvent)
                {
                    ev.OnCloseBefore(this, e);
                }
            }
        }

        protected override void OnError(ErrorEventArgs e)
        {
            if (_webSocketBehaviorEvent != null)
            {
                foreach (IWebSocketBehaviorEvent ev in _webSocketBehaviorEvent)
                {
                    ev.OnErrorBefore(this, e);
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
    }
}
