using Newtonsoft.Json.Linq;
using System;

namespace BankPayWSServer.BankPayServer
{
    
    public class Command
    {
        public Command()
        {
        }

        public Command(string data)
        {
            Flag = "json";
            Data = data;
        }

        public Command(string flag, string data)
        {
            Flag = flag;
            Data = data;
        }
        public string Flag { get; set; }

        public string Data { get; set; }

        public uint Code { get; set; }

        public bool IsRequest { get; set; }
        public string Cmd { get; set; }

        public string Msg { get; set; }

        /// <summary>
        /// 消息id
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 相应的消息id
        /// </summary>
        public string ReplyId { get; set; }

        public long Timestamp { get; set; }

        public static Command Parse(string str)
        {
            if (str.StartsWith("json:"))
            {
                JObject json = JObject.Parse(str.Substring("json:".Length));
                if(json["id"] == null || json["code"] == null)
                {
                    return null;
                }
                Command c = new Command();
                c.Flag = "json";
                uint cd = (uint)json["code"];
                //code 标记，最高位为0为请求，为1 为应答
                if((cd & 0x80000000) == 0x80000000)
                {
                    c.IsRequest = false;
                }
                else
                {
                    c.IsRequest = true;
                }
                uint mark = 2147483647;
                c.Code = cd & mark;
                c.Cmd = (string)json["cmd"];
                c.ID = (string)json["id"];
                if(json["msg"] != null)
                {
                    c.Msg = (string)json["msg"];
                }
                if (json["rid"] != null)
                {
                    c.ReplyId = (string)json["rid"];
                }
                c.Timestamp = (long)json["time"];
                c.Data = (string)json["data"];
                return c;
            }
            return null;
        }

        public T GetData<T>()
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Data);
        }

        public static long GetTimestamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }

        public override string ToString()
        {
            JObject json = new JObject();
            json.Add("id", this.ID);
            uint c = this.Code;
            if(this.IsRequest)
            {
                c = c & 2147483647;//0x7FFFFFFF;
            }
            else
            {
                c = c | 0x80000000;
            }
            json.Add("code", c);
            
            json.Add("time", this.Timestamp);

            if (!string.IsNullOrWhiteSpace(this.Cmd))
            {
                json.Add("cmd", this.Cmd);
            }
            if (!string.IsNullOrWhiteSpace(this.Msg))
            {
                json.Add("msg", this.Msg);
            }
            if (!string.IsNullOrWhiteSpace(this.ReplyId))
            {
                json.Add("rid", this.ReplyId);
            }
            json.Add("data", Data);
            return Flag + ":" + Newtonsoft.Json.JsonConvert.SerializeObject(json);
        }

        public static string ToJson(uint code, string cmd, object obj, string replyId = "", string msg = "")
        {
            
            return ToCmd(code, cmd, obj, replyId, msg).ToString();
        }

        public static Command ToCmd(uint code, string cmd, object obj, string replyId = "", string msg = "")
        {
            Command c = new Command();
            c.Data = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            c.Flag = "json";
            c.Code = code;
            c.Cmd = cmd;
            c.Msg = msg;
            c.Timestamp = GetTimestamp();
            c.ID = Guid.NewGuid().ToString("N");
            c.ReplyId = replyId;
            if (string.IsNullOrWhiteSpace(replyId))
            {
                c.IsRequest = true;
            }
            else
            {
                c.IsRequest = false;
            }
            return c;
        }

        public string ToReplyError(string msg)
        {
            return ToJson(10000, "", null, this.ID, msg);
        }
        public string ToReply(uint code, object obj)
        {
            return ToJson(code, "", obj, this.ID);
        }
        public string ToReply(object obj)
        {
            return ToReply(0, obj);
        }
    }

    /// <summary>
    /// 指令消息
    /// </summary>
    public class InstructionCmd : Command
    {

    }

    /// <summary>
    /// 相应消息
    /// </summary>
    public class ReplyCmd : Command
    {
        public static string Reply(string id)
        {
            return ToJson(0, "", null, id, "");
        }
    }


    public class HttpCmd: Command
    {
        public static Command CreateGet(string url)
        {
            return ToCmd(0, "GET", new { url = url });
        }

        public static Command CreatePost(string url, string content, string contentType)
        {
            return ToCmd(0, "POST", new { url = url, content =content, contentType= contentType });
        }
    }
}
