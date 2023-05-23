using HM.Framework;
using HmPMer.Business;
using HmPMer.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;

namespace HmPMer.Pay.Controllers
{
	public class DemoController : Controller
	{
		public ActionResult Index()
		{
			return Content("暂无");
		}

		public ActionResult Test()
		{
			SysConfig forKey = new SysConfigBll().GetForKey("ApiUrl");
			string text = "";
			text = (forKey.Value = (forKey.Value.EndsWith("/") ? (forKey.Value + "/") : forKey.Value));
			return View(forKey);
		}

		public ActionResult PayTest()
		{
			string text = "V2.0";
			string request = Utils.GetRequest("post_url");
			string request2 = Utils.GetRequest("app_id");
			string request3 = Utils.GetRequest("apiKey");
			decimal requestToDecimal = Utils.GetRequestToDecimal("total_amount");
			string text2 = Guid.NewGuid().ToString().Substring(0, 20)
				.Replace("-", "");
			string request4 = Utils.GetRequest("trade_type");
			string request5 = Utils.GetRequest("extra_return_param");
			string request6 = Utils.GetRequest("notify_url");
			string clientIp = Utils.GetClientIp();
			string request7 = Utils.GetRequest("return_url");
			requestToDecimal *= 100m;
			SortedDictionary<string, string> obj = new SortedDictionary<string, string>
			{
				{
					"app_id",
					request2
				},
				{
					"trade_type",
					request4
				},
				{
					"total_amount",
					requestToDecimal.ToString("0")
				},
				{
					"out_trade_no",
					text2
				},
				{
					"notify_url",
					request6
				}
			};
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, string> item in obj)
			{
				stringBuilder.AppendFormat("&{0}={1}", item.Key, item.Value);
			}
			stringBuilder.Append(request3);
			stringBuilder.Remove(0, 1);
			string text3 = EncryUtils.MD5(stringBuilder.ToString()).ToLower();
			return new RedirectResult(string.Format(request + "?app_id={0}&trade_type={1}&total_amount={2}&out_trade_no={3}&notify_url={4}&return_url={5}&client_ip={6}&extra_return_param={7}&sign={8}&interface_version={9}", request2, request4, requestToDecimal.ToString("0"), text2, request6, request7, clientIp, request5, text3, text));
		}

		public ActionResult PayTestNotity()
		{
			return Content("SUCCESS");
		}

		public ActionResult PayTestReturn()
		{
			return Content("SUCCESS");
		}

		public ActionResult Notity()
		{
			return Content("SUCCESS");
		}
	}
}
