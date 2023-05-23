using System;
using HM;
using System.Web.Mvc;
using HmPMer.Business;
using HmPMer.Entity;
using HM.Framework;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Pay.Controllers
{
    public class CheckWeixinController : Controller
    {

        private ConcurrentDictionary<string, string> checkOrderMaps = new ConcurrentDictionary<string, string>();
        // GET: Check
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult RegisterDevice()
        {
            string code = Request.Form["code"];
            string imei = Request.Form["imei"];
            if (string.IsNullOrWhiteSpace(code)
                || string.IsNullOrWhiteSpace(imei))
            {
                return Content("error:999");
            }
            InterfaceBll interfaceBll = new InterfaceBll();
            InterfaceBusiness interfaceBusiness = interfaceBll.GetInterfaceBusinessModel(code);
            if (interfaceBusiness == null)
            {
                return Content("error:999");
            }
            if(!string.IsNullOrWhiteSpace(interfaceBusiness.ChildAccount))
            {
                return Content("error:990");
            }
            string md5 = Guid.NewGuid().ToString("N");
            interfaceBusiness.MD5Pwd = md5;
            interfaceBusiness.ChildAccount = imei;
            interfaceBll.UpdateInterfaceBusiness(interfaceBusiness);
            return Content("ok:" + md5);
        }

        public ActionResult CheckWeixinMaidanPostOrder()
        {
            string code = Request.Form["code"];
            string shopname = Request.Form["shop"];
            string orderno = Request.Form["orderno"];
            string amount = Request.Form["amount"];
            string time = Request.Form["time"];
            string sign = Request.Form["sign"];
            if(string.IsNullOrWhiteSpace(code)
                || string.IsNullOrWhiteSpace(shopname)
                || string.IsNullOrWhiteSpace(orderno)
                || string.IsNullOrWhiteSpace(amount)
                || string.IsNullOrWhiteSpace(time)
                || string.IsNullOrWhiteSpace(sign))
            {
                return Content("error:999");
            }
            InterfaceBll interfaceBll = new InterfaceBll();
            InterfaceBusiness interfaceBusiness = interfaceBll.GetInterfaceBusinessModel(code);
            if(interfaceBusiness == null)
            {
                return Content("error:998");
            }

            string signstr = code + shopname + amount + orderno + interfaceBusiness.MD5Pwd + time;
            if (EncryUtils.MD5(signstr, "UTF-8").ToLower() != sign)
            {
                return Content("error:997");
            }
            decimal money = 0M;
            if(!decimal.TryParse(amount, out money))
            {
                return Content("error:996");
            }
            string orderId = "";
            OrderBll orderBll = new OrderBll();
            if (checkOrderMaps.TryGetValue(orderno, out orderId))
            {
                OrderBase od = orderBll.GetOrderBase(orderId);
                if(od.PayState == 1)
                {
                    return Content("error:100");
                }
                return Content("ok:" + orderId);
            }
            money = money * 100;
            string s = "";
            List<OrderInfo> orders = orderBll.GetOrderList("", code, System.DateTime.Now.AddMinutes(-6), 1000, 0);
            OrderInfo matchOrder = null;
            foreach(OrderInfo o in orders)
            {
                if(o.ChannelOrderNo == orderno)
                {
                    return Content("error:101");
                }
                if (o.OrderAmt == money)
                {
                    matchOrder = o;
                }
            }
            if(matchOrder != null)
            {
                checkOrderMaps[orderno] = matchOrder.OrderId;
                return Content("ok:" + matchOrder.OrderId);
            }
            return Content("error:995");
        }
    }
}