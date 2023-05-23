using HM.Framework;
using HM.Framework.PayApi.DaiFu;
using HmPMer.Business;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Message
    {
        public string ID { get; set; }
        public DateTime DateTime { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            //GenSql();
            TestRandomModel();
        }

        public static void GenSql()
        {
            string dc = @"656075 100161238 印秀服装-张卫卫
185436 100161236 宣捷建材-张泉泉
656075 100162280 卓鼎百货-张卫卫
656075 100162286 靳之豪-张卫卫
185436 100162289 泽之信-张泉泉
185436 100162290 晶之尚-张泉泉
635446 100162308 昱硕日用品-朱光亮
252634 100162310 东之兴-吴桂忠
252634 100162311 逸居卫浴-吴桂忠
100716 100162352 子泰服装-安李明
100716 100162346 瑜之智百货-安李明
100716 100162389 槐荫永晨-安李明
761051 100162377 龙晶尚服装-陈军
146023 100162330 珠颖缘饰品-陈军
033361 100162363 聚安卡服装-陈军
306014 100162365 易呈宇服装-陈军
122087 100162366 均按阎服装店-陈军
031205 100162313 心愿婚庆-陈军
868884 100162312 陈军家具店-陈军

";
            string[] rows = dc.Split('\n');
            int codeindex = 42;
            string co = "";
            string codes = "";
            foreach(string r in rows)
            {
                string[] items = r.Trim().Split(' ');
                if(items.Length < 2)
                {
                    continue;
                }
                string code = string.Format("_bankpayv1{0:D3}", codeindex++);
                string account = items[1];
                string password = items[0];
                string md5 = Guid.NewGuid().ToString("N");
                string name = items[2];
                string gid = Guid.NewGuid().ToString();
                string gid2 = Guid.NewGuid().ToString();
                string sql = @"INSERT [dbo].[InterfaceBusiness] 
([Code], [Name], [AccType], [SubMitUrl], [AgentPayUrl], [QueryUrl], [IsEnabled], [Account], [ChildAccount], [Appid], [OpenId], [OpenPwd], [MD5Pwd], [RSAOpen], [RSAPrivate], [SubDomain], [BindDomain], [OrderNo], [AgentPay], [CallbackDomain]) 
VALUES (N'{{code}}', N'{{name}}{{acount}}', 0, N'wss://127.0.0.1:8011/pay', 
NULL, NULL, 0, N'{{acount}}', NULL, NULL, NULL, N'{{password}}', 
N'{{md5}}', NULL, NULL, NULL, NULL, 0, 0, NULL);

DELETE FROM [dbo].[InterfaceType] where InterfaceCode ='{{code}}' and PayCode='PAYSAPIALIPAY';
DELETE FROM [dbo].[InterfaceType] where InterfaceCode ='{{code}}' and PayCode='PAYSAPIWEIXIN';
DELETE FROM [dbo].[PayRate] where UserId ='{{code}}';

INSERT INTO [dbo].[InterfaceType]([InterfaceCode],[PayCode],[Type],[DefaulInfaceCode],[AccountScheme]) VALUES
           ('{{code}}','PAYSAPIALIPAY',1,'','')
INSERT INTO [dbo].[InterfaceType]([InterfaceCode],[PayCode],[Type],[DefaulInfaceCode],[AccountScheme]) VALUES
           ('{{code}}','PAYSAPIWEIXIN',1,'','')
INSERT INTO [dbo].[PayRate]([Id],[RateType],[UserId],[ChannelId],[Rate],[IsEnabled])VALUES
           ('{{gid}}',1,'{{code}}','PAYSAPIWEIXIN',0.0038,1)
INSERT INTO [dbo].[PayRate]([Id],[RateType],[UserId],[ChannelId],[Rate],[IsEnabled])VALUES
           ('{{gid2}}',1,'{{code}}','PAYSAPIALIPAY',0.0038,1)
";
                sql = sql.Replace("{{code}}", code);
                sql = sql.Replace("{{name}}", name);
                sql = sql.Replace("{{acount}}", account);
                sql = sql.Replace("{{password}}", password);
                sql = sql.Replace("{{gid}}", gid);
                sql = sql.Replace("{{gid2}}", gid2);
                sql = sql.Replace("{{md5}}", md5);
                codes += code;
                codes += "\n";
                co += sql;
            }
            Console.WriteLine(co);
        }

        public static void TestAsync()
        {
            List<Message> messages = new List<Message>();
            Dictionary<string, string> dictest = new Dictionary<string, string>();
            Task task = new Task(() =>
            {
                int remvoeCount = 0;
                while (true)
                {
                    while (messages.Count > 0)
                    {
                        Message m = messages[0];
                        string id = dictest[m.ID];
                        messages.RemoveAt(0);
                        remvoeCount++;
                    }
                    Console.WriteLine("remove count: " + remvoeCount);
                }

            });
            task.Start();
            Task task2 = new Task(() =>
            {
                int addCount = 0;
                while (true)
                {
                    Message m = new Message() { ID = Guid.NewGuid().ToString() };
                    dictest.Add(m.ID, Guid.NewGuid().ToString());
                    messages.Add(m);
                    addCount++;
                    Console.WriteLine("addCount count: " + addCount);
                }

            });
            task2.Start();
            task.Wait();
        }


        public static void TestRandomModel()
        {
            DateTime dt = DateTime.Now;
            for(int i = 0; i < 1000; i++)
            {

                var interfaceBusiness = new InterfaceBll().GetInterfaceBusinessRandomModel("_random_bankpayv1");
                Console.WriteLine(interfaceBusiness.Code);
            }
            Console.WriteLine(DateTime.Now.Subtract(dt));
            dt = DateTime.Now;
            for (int i = 0; i < 1000; i++)
            {

                var interfaceBusiness = new InterfaceBll().GetInterfaceBusinessRandomModel("_random_bankpayv1");
                Console.WriteLine(interfaceBusiness.Code);
            }
            Console.WriteLine(DateTime.Now.Subtract(dt));
            Console.ReadKey();
        }
    }
}
