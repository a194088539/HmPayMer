using System;
using System.Collections.Generic;
using System.Web;

namespace HM.Framework.PayApi.NewCarepay
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
				return "ALIPAY";
			case HMChannel.ALIPAY_H5:
				return "ALIH5";
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
			string channelCode = GetChannelCode(order.ChannelCode);
			string userAgent = HttpContext.Current.Request.UserAgent;
			string accountUser = base.Account.AccountUser;
			string md5Pwd = base.Account.Md5Pwd;
			string orderNo = order.OrderNo;
			string text = (order.OrderAmt / 100m).ToString("0.00");
			string returnUri = base.Supplier.ReturnUri;
			string notifyUri = base.Supplier.NotifyUri;
			string text2 = channelCode;
			string text3 = "";
			text3 = EncryUtils.MD5($"merNo={accountUser}&merSecret={md5Pwd}&orderNo={orderNo}&amount={text}&payType={text2}", "UTF-8").ToLower();
			string data = base.Supplier.PostUri + "?" + $"merNo={accountUser}&orderNo={orderNo}&amount={text}&returnUrl={returnUri}&notifyUrl={notifyUri}&payType={text2}&sign={text3}";
			fail.Code = HMPayState.Success;
			fail.Data = data;
			return fail;
		}

		protected override Dictionary<string, string> GetNotifyParam()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string request = Utils.GetRequest("status");
			string request2 = Utils.GetRequest("orderStatus");
			string request3 = Utils.GetRequest("orderAmount");
			string request4 = Utils.GetRequest("payType");
			string request5 = Utils.GetRequest("payoverTime");
			string request6 = Utils.GetRequest("orderNo");
			string request7 = Utils.GetRequest("sign");
			dictionary.Add("status", request);
			dictionary.Add("orderStatus", request2);
			dictionary.Add("orderAmount", request3);
			dictionary.Add("payType", request4);
			dictionary.Add("payoverTime", request5);
			dictionary.Add("orderNo", request6);
			dictionary.Add("sign", request7);
			return dictionary;
		}

		protected override Dictionary<string, string> GetReturnParam()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string request = Utils.GetRequest("status");
			string request2 = Utils.GetRequest("orderStatus");
			string request3 = Utils.GetRequest("orderAmount");
			string request4 = Utils.GetRequest("payType");
			string request5 = Utils.GetRequest("payoverTime");
			string request6 = Utils.GetRequest("orderNo");
			string request7 = Utils.GetRequest("sign");
			dictionary.Add("status", request);
			dictionary.Add("orderStatus", request2);
			dictionary.Add("orderAmount", request3);
			dictionary.Add("payType", request4);
			dictionary.Add("payoverTime", request5);
			dictionary.Add("orderNo", request6);
			dictionary.Add("sign", request7);
			return dictionary;
		}

		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string notifyRequest = GetNotifyRequest("orderNo");
			string notifyRequest2 = GetNotifyRequest("orderAmount");
			string notifyRequest3 = GetNotifyRequest("sign");
			string notifyRequest4 = GetNotifyRequest("status");
			if (string.IsNullOrEmpty(notifyRequest))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (string.IsNullOrEmpty(notifyRequest2))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (string.IsNullOrEmpty(notifyRequest3))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (notifyRequest4 == "200")
			{
				fail.Code = HMNotifyState.Success;
				fail.Data = new HMOrder
				{
					OrderNo = notifyRequest,
					SupplierOrderNo = "",
					OrderAmt = Utils.StringToDecimal(notifyRequest2, decimal.Zero) * 100m,
					OrderTime = DateTime.Now
				};
			}
			else
			{
				fail.Code = HMNotifyState.Fail;
			}
			return fail;
		}

		protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string returnRequest = GetReturnRequest("orderNo");
			string returnRequest2 = GetReturnRequest("orderAmount");
			string returnRequest3 = GetReturnRequest("sign");
			string returnRequest4 = GetReturnRequest("status");
			if (string.IsNullOrEmpty(returnRequest))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (string.IsNullOrEmpty(returnRequest2))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (string.IsNullOrEmpty(returnRequest3))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (returnRequest4 == "200")
			{
				fail.Code = HMNotifyState.Success;
				fail.Data = new HMOrder
				{
					OrderNo = returnRequest,
					SupplierOrderNo = "",
					OrderAmt = Utils.StringToDecimal(returnRequest2, decimal.Zero) * 100m,
					OrderTime = DateTime.Now
				};
			}
			else
			{
				fail.Code = HMNotifyState.Fail;
			}
			return fail;
		}

		public override HMNotifyResult<string> NotifySign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			string notifyRequest = GetNotifyRequest("status");
			string notifyRequest2 = GetNotifyRequest("orderStatus");
			string notifyRequest3 = GetNotifyRequest("orderAmount");
			string notifyRequest4 = GetNotifyRequest("payType");
			string notifyRequest5 = GetNotifyRequest("payoverTime");
			string notifyRequest6 = GetNotifyRequest("orderNo");
			string notifyRequest7 = GetNotifyRequest("sign");
			string value = EncryUtils.MD5($"status={notifyRequest}&payType={notifyRequest4}&orderNo={notifyRequest6}&orderStatus={notifyRequest2}&orderAmount={notifyRequest3}&payoverTime={notifyRequest5}&merSecret={account.Md5Pwd}", "UTF-8").ToLower();
			if (notifyRequest7.Equals(value))
			{
				decimal num = Utils.StringToDecimal(notifyRequest3, decimal.Zero) * 100m;
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
			string returnRequest2 = GetReturnRequest("orderStatus");
			string returnRequest3 = GetReturnRequest("orderAmount");
			string notifyRequest = GetNotifyRequest("payType");
			string returnRequest4 = GetReturnRequest("payoverTime");
			string returnRequest5 = GetReturnRequest("orderNo");
			string returnRequest6 = GetReturnRequest("sign");
			string value = EncryUtils.MD5($"status={returnRequest}&payType={notifyRequest}&orderNo={returnRequest5}&orderStatus={returnRequest2}&orderAmount={returnRequest3}&payoverTime={returnRequest4}&merSecret={account.Md5Pwd}", "UTF-8").ToLower();
			if (returnRequest6.Equals(value))
			{
				decimal num = Utils.StringToDecimal(returnRequest3, decimal.Zero) * 100m;
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
