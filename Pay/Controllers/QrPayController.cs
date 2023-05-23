using HM.Framework;
using System;
using System.Web;
using System.Web.Mvc;
using HmPMer.Business;
using HmPMer.Entity;

namespace Pay.Controllers
{
    public class QrPayController : Controller
    {
        // GET: QrPay
        public ActionResult Index()
        {
            string qr = Utils.GetRequest("qr");
            if (!string.IsNullOrEmpty(qr))
            {
                qr = HttpUtility.UrlDecode(qr);
            }
            string paytype = Utils.GetRequest("type");
            string orderno = Utils.GetRequest("trade_no");
            string merchantno = Utils.GetRequest("out_trade_no");
            string amount = Utils.GetRequest("total_fee");
            decimal total_fee = 0;
            if (string.IsNullOrWhiteSpace(qr)
                || string.IsNullOrWhiteSpace(paytype)
                || string.IsNullOrWhiteSpace(orderno)
                || string.IsNullOrWhiteSpace(merchantno)
                || !decimal.TryParse(amount, out total_fee)
                )
            {
                return Content("参数有误");
            }
            OrderBll orderBll = new OrderBll();
            OrderBase orderBase = orderBll.GetOrderBase(orderno);
            if(orderBase == null)
            {
                return Content("订单不存在或过期");
            }
            if (orderBase.MerOrderNo != merchantno)
            {
                return Content("参数有误");
            }
            if (orderBase.OrderAmt != total_fee * 100)
            {
                return Content("参数有误");
            }
            if (orderBase.PayState == 1)
            {
                return Content("订单已支付");
            }
            int time = (int)orderBase.OrderTime.Value.AddMinutes(5.1).Subtract(DateTime.Now).TotalSeconds;
            if (time <= 0)
            {
                return Content("订单已失效");
            }
            ViewBag.qr = qr;
            ViewBag.paytype = paytype;
            ViewBag.merchantno = merchantno;
            ViewBag.orderno = orderno;
            ViewBag.total_fee = total_fee.ToString("#0.00");
            
            ViewBag.time = time;
            return View();
        }

        public ActionResult Qrimg()
        {
            string qr = Utils.GetRequest("qr");
            if (!string.IsNullOrEmpty(qr))
            {
                qr = HttpUtility.UrlDecode(qr);
            }
            string paytype = Utils.GetRequest("type");
            string orderno = Utils.GetRequest("trade_no");
            string merchantno = Utils.GetRequest("out_trade_no");
            string amount = Utils.GetRequest("total_fee");
            decimal total_fee = 0;
            if (string.IsNullOrWhiteSpace(qr)
                || string.IsNullOrWhiteSpace(paytype)
                || string.IsNullOrWhiteSpace(orderno)
                || string.IsNullOrWhiteSpace(merchantno)
                || !decimal.TryParse(amount, out total_fee)
                )
            {
                return Content("参数有误");
            }
            OrderBll orderBll = new OrderBll();
            OrderBase orderBase = orderBll.GetOrderBase(orderno);
            if (orderBase == null)
            {
                return Content("订单不存在或过期");
            }
            if (orderBase.MerOrderNo != merchantno)
            {
                return Content("参数有误");
            }
            if (orderBase.OrderAmt != total_fee * 100)
            {
                return Content("参数有误");
            }
            if (orderBase.PayState == 1)
            {
                return Content("订单已支付");
            }
            int time = (int)orderBase.OrderTime.Value.AddMinutes(5.1).Subtract(DateTime.Now).TotalSeconds;
            if (time <= 0)
            {
                return Content("订单已失效");
            }
            ViewBag.qr = qr;
            ViewBag.paytype = paytype;
            ViewBag.merchantno = merchantno;
            ViewBag.orderno = orderno;
            ViewBag.total_fee = total_fee.ToString("#0.00");

            ViewBag.time = time;
            return View();
        }

        public ActionResult check()
        {
            string paytype = Utils.GetRequest("type");
            string orderno = Utils.GetRequest("trade_no");
            string merchantno = Utils.GetRequest("out_trade_no");
            string amount = Utils.GetRequest("total_fee");
            decimal total_fee = 0;
            if (string.IsNullOrWhiteSpace(paytype)
                || string.IsNullOrWhiteSpace(orderno)
                || string.IsNullOrWhiteSpace(merchantno)
                || !decimal.TryParse(amount, out total_fee)
                )
            {
                return Json(new { code = 1 });
            }
            OrderBll orderBll = new OrderBll();
            OrderBase orderBase = orderBll.GetOrderBase(orderno);
            if (orderBase == null)
            {
                return Json(new { code = 1 });
            }
            if (orderBase.MerOrderNo != merchantno)
            {
                return Json(new { code = 1 });
            }
            if (orderBase.OrderAmt != total_fee * 100)
            {
                return Json(new { code = 1 });
            }
            if (orderBase.PayState == 1)
            {
                return Json(new { code = 0, url = orderBase.ReturnUrl });
            }
            int time = (int)orderBase.OrderTime.Value.AddMinutes(5.1).Subtract(DateTime.Now).TotalSeconds;
            if (time <= 0)
            {
                return Json(new { code = 1 });
            }
            return Json(new { code = 2 });//
        }
    }
}