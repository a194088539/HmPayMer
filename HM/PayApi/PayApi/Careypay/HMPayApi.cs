using System;
using System.Collections.Generic;
using System.Web;

namespace HM.Framework.PayApi.Careypay
{
	public class HMPayApi : HMPayApiBase
	{
		public override string NOTIFY_SUCCESS => "success";

		public override string NOTIFY_FAIL => "fail";

		public override bool IsWithdraw => false;

		private string GetChannelCode(HMChannel channel)
		{
			switch (channel)
			{
			case HMChannel.ALIPAY_NATIVE:
				return "V1/aliH5Pay";
			case HMChannel.ALIPAY_H5:
				return "V1/aliH5Pay";
			default:
				return string.Empty;
			}
		}

		private bool IsMoblie(string userAgent)
		{
			if (userAgent == "" || userAgent.Contains("mobile") || userAgent.Contains("mobi") || userAgent.Contains("nokia") || userAgent.Contains("samsung") || userAgent.Contains("sonyericsson") || userAgent.Contains("mot") || userAgent.Contains("blackberry") || userAgent.Contains("lg") || userAgent.Contains("htc") || userAgent.Contains("j2me") || userAgent.Contains("ucweb") || userAgent.Contains("opera mini") || userAgent.Contains("mobi") || userAgent.Contains("android") || userAgent.Contains("iphone"))
			{
				return true;
			}
			return false;
		}

		public override HMMode GetPayMode(HMChannel code)
		{
			return HMMode.跳转链接;
		}

		protected override HMPayResult PayGatewayBody(HMOrder order)
		{
			HMPayResult fail = HMPayResult.Fail;
			fail.Mode = GetPayMode(order.ChannelCode);
			GetChannelCode(order.ChannelCode);
			string userAgent = HttpContext.Current.Request.UserAgent;
			string md5Pwd = base.Account.Md5Pwd;
			string accountUser = base.Account.AccountUser;
			string text = order.OrderAmt.ToString("0");
			string orderNo = order.OrderNo;
			string text2 = IsMoblie(userAgent.ToLower()) ? "alipay_wp" : "alipay_pc";
			string notifyUri = base.Supplier.NotifyUri;
			string returnUri = base.Supplier.ReturnUri;
			string childAccountUser = base.Account.ChildAccountUser;
			string orderNo2 = order.OrderNo;
			string text3 = EncryUtils.MD5($"app_id={accountUser}&amount={text}&order_no={orderNo}&device={text2}&app_secret={md5Pwd}&notify_url={notifyUri}").ToLower();
			string data = base.Supplier.PostUri + "?" + $"app_id={accountUser}&amount={text}&order_no={orderNo}&device={text2}&notify_url={notifyUri}&return_url={returnUri}&merchant={childAccountUser}&goods={orderNo2}&sign={text3}";
			fail.Code = HMPayState.Success;
			fail.Data = data;
			return fail;
		}

		protected override Dictionary<string, string> GetNotifyParam()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string request = Utils.GetRequest("status");
			string request2 = Utils.GetRequest("amount");
			string request3 = Utils.GetRequest("order_no");
			string request4 = Utils.GetRequest("order_status");
			string request5 = Utils.GetRequest("pay_time");
			string request6 = Utils.GetRequest("sign");
			dictionary.Add("status", request);
			dictionary.Add("amount", request2);
			dictionary.Add("order_no", request3);
			dictionary.Add("order_status", request4);
			dictionary.Add("pay_time", request5);
			dictionary.Add("sign", request6);
			return dictionary;
		}

		protected override Dictionary<string, string> GetReturnParam()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string request = Utils.GetRequest("status");
			string request2 = Utils.GetRequest("amount");
			string request3 = Utils.GetRequest("order_no");
			string request4 = Utils.GetRequest("order_status");
			string request5 = Utils.GetRequest("pay_time");
			string request6 = Utils.GetRequest("sign");
			dictionary.Add("status", request);
			dictionary.Add("amount", request2);
			dictionary.Add("order_no", request3);
			dictionary.Add("order_status", request4);
			dictionary.Add("pay_time", request5);
			dictionary.Add("sign", request6);
			return dictionary;
		}

		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			GetNotifyRequest("status");
			string notifyRequest = GetNotifyRequest("amount");
			string notifyRequest2 = GetNotifyRequest("order_no");
			GetNotifyRequest("order_status");
			string notifyRequest3 = GetNotifyRequest("pay_time");
			string notifyRequest4 = GetNotifyRequest("sign");
			if (string.IsNullOrEmpty(notifyRequest2))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (string.IsNullOrEmpty(notifyRequest))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (string.IsNullOrEmpty(notifyRequest4))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			fail.Code = HMNotifyState.Success;
			fail.Data = new HMOrder
			{
				OrderNo = notifyRequest2,
				SupplierOrderNo = "",
				OrderAmt = Utils.StringToDecimal(notifyRequest, decimal.Zero),
				OrderTime = Utils.StringToDateTime(notifyRequest3, DateTime.Now).Value
			};
			return fail;
		}

		protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			GetReturnRequest("status");
			string returnRequest = GetReturnRequest("amount");
			string returnRequest2 = GetReturnRequest("order_no");
			GetReturnRequest("order_status");
			string returnRequest3 = GetReturnRequest("pay_time");
			string returnRequest4 = GetReturnRequest("sign");
			if (string.IsNullOrEmpty(returnRequest2))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (string.IsNullOrEmpty(returnRequest))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (string.IsNullOrEmpty(returnRequest4))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			fail.Code = HMNotifyState.Success;
			fail.Data = new HMOrder
			{
				OrderNo = returnRequest2,
				SupplierOrderNo = "",
				OrderAmt = Utils.StringToDecimal(returnRequest, decimal.Zero),
				OrderTime = Utils.StringToDateTime(returnRequest3, DateTime.Now).Value
			};
			return fail;
		}

		public override HMNotifyResult<string> NotifySign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			string notifyRequest = GetNotifyRequest("status");
			string notifyRequest2 = GetNotifyRequest("amount");
			string notifyRequest3 = GetNotifyRequest("order_no");
			string notifyRequest4 = GetNotifyRequest("order_status");
			string notifyRequest5 = GetNotifyRequest("pay_time");
			string notifyRequest6 = GetNotifyRequest("sign");
			string value = EncryUtils.MD5($"status={notifyRequest}&amount={notifyRequest2}&order_no={notifyRequest3}&order_status={notifyRequest4}&pay_time={notifyRequest5}&app_secret={account.Md5Pwd}").ToLower();
			if (notifyRequest6.Equals(value))
			{
				decimal num = Utils.StringToDecimal(notifyRequest2, decimal.Zero);
				if (order.OrderAmt == num)
				{
					fail.Code = HMNotifyState.Success;
					fail.Data = NOTIFY_SUCCESS;
				}
				else
				{
					fail.Message = string.Format("回调金额不一致，订单金额{0.00}，回调金额:{1.00}", order.OrderAmt / 100m, num / 100m);
				}
			}
			else
			{
				fail.Data = NOTIFY_FAIL;
			}
			return fail;
		}

		public override HMNotifyResult<string> ResultSign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			string returnRequest = GetReturnRequest("status");
			string returnRequest2 = GetReturnRequest("amount");
			string returnRequest3 = GetReturnRequest("order_no");
			string returnRequest4 = GetReturnRequest("order_status");
			string returnRequest5 = GetReturnRequest("pay_time");
			string returnRequest6 = GetReturnRequest("sign");
			string value = EncryUtils.MD5($"status={returnRequest}&amount={returnRequest2}&order_no={returnRequest3}&order_status={returnRequest4}&pay_time={returnRequest5}&app_secret={account.Md5Pwd}").ToLower();
			if (returnRequest6.Equals(value))
			{
				decimal num = Utils.StringToDecimal(returnRequest2, decimal.Zero);
				if (order.OrderAmt == num)
				{
					fail.Code = HMNotifyState.Success;
					fail.Data = NOTIFY_SUCCESS;
				}
				else
				{
					fail.Message = string.Format("回调金额不一致，订单金额{0.00}，回调金额:{1.00}", order.OrderAmt / 100m, num / 100m);
				}
			}
			else
			{
				fail.Data = NOTIFY_FAIL;
			}
			return fail;
		}
	}
}
