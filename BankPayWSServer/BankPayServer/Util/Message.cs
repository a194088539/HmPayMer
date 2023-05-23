using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Server;

namespace BankPayWSServer.BankPayServer
{
    /// <summary>
    /// 通信消息体
    /// </summary>
    public class Message
    {
        public Message(Command send)
        {
            this.Send = send;
            this.SendId = send.ID;
            this.Recv = new List<Command>();
            this.exception = null;
            this.Success = false;
            this.DateTime = DateTime.Now;
            this.OnComplete = null;
            this.CanDestroy = false;
        }
        public string SendId { get; set; }
        public Command Send { get; set; }
        public List<Command> Recv { get; set; }
        public Exception exception { get; set; }
        public bool Success { get; set; }

        /// <summary>
        /// 错误指示，1已完成，2关闭,3超时,4错误
        /// </summary>
        public int Code { get; set; }
        public int SubCode { get; set; }
        public DateTime DateTime { get; set; }

        public Action<Message> OnComplete { get; set; }
        public WebSocketBehavior WsHandler { get; set; }

        public DateTime LastRecvTime { get; set; }
        /// <summary>
        /// 主动设置，是否可用移除
        /// </summary>
        public bool CanDestroy { get; set; }
    }
}
