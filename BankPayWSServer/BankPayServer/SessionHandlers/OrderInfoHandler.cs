using System;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace BankPayWSServer.BankPayServer.SessionHandlers
{
    class OrderInfoHandler : WebSocketBehavior, ITunnel
    {
        public ITunnel Tunnel
        {
            get; set;
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
    }
}
