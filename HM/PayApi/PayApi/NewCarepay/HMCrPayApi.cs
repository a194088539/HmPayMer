using System;
using System.Collections.Generic;

namespace HM.Framework.PayApi.NewCarepay
{
	public class HMCrPayApi : HMPayApiBase
	{
		public class CrResult
		{
			public string status
			{
				get;
				set;
			}

			public string amount
			{
				get;
				set;
			}

			public string order_no
			{
				get;
				set;
			}

			public string order_status
			{
				get;
				set;
			}

			public string pay_time
			{
				get;
				set;
			}

			public string sign
			{
				get;
				set;
			}
		}

		public override string NOTIFY_SUCCESS => "success";

		public override string NOTIFY_FAIL => "fail";

		public override bool IsWithdraw => false;

		public override HMMode GetPayMode(HMChannel code)
		{
			return HMMode.跳转链接;
		}

		private string GetChannelCode(HMChannel channel)
		{
			switch (channel)
			{
			case HMChannel.ALIPAY_NATIVE:
				return "alipay_pc";
			case HMChannel.WEIXIN_NATIVE:
				return "wechat_pc";
			case HMChannel.QQPAY_NATIVE:
				return "qq_pc";
			case HMChannel.ALIPAY_H5:
				return "alipay_wp";
			case HMChannel.WEIXIN_H5:
				return "wechat_wp";
			case HMChannel.QQPAY_H5:
				return "qq_wp";
			default:
				return string.Empty;
			}
		}

		protected override HMPayResult PayGatewayBody(HMOrder order)
		{
			HMPayResult fail = HMPayResult.Fail;
			fail.Mode = GetPayMode(order.ChannelCode);
			string channelCode = GetChannelCode(order.ChannelCode);
			if (string.IsNullOrEmpty(channelCode))
			{
				fail.Message = "此通道已关闭！";
				return fail;
			}
			string accountUser = base.Account.AccountUser;
			decimal orderAmt = order.OrderAmt;
			string orderNo = order.OrderNo;
			string notifyUri = base.Supplier.NotifyUri;
			string returnUri = base.Supplier.ReturnUri;
			string md5Pwd = base.Account.Md5Pwd;
			string accountUser2 = base.Account.AccountUser;
			string orderNo2 = order.OrderNo;
			string text = "";
			text = EncryUtils.MD5($"app_id={accountUser}&amount={orderAmt}&order_no={orderNo}&device={channelCode}&app_secret={md5Pwd}&notify_url={notifyUri}", "UTF-8").ToLower();
			string data = base.Supplier.PostUri + $"?app_id={accountUser}&amount={orderAmt}&order_no={orderNo}&device={channelCode}&return_url={returnUri}&notify_url={notifyUri}&merchant={accountUser2}&goods={orderNo2}&sign={text}";
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

		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string notifyRequest = GetNotifyRequest("status");
			string notifyRequest2 = GetNotifyRequest("amount");
			string notifyRequest3 = GetNotifyRequest("order_no");
			string notifyRequest4 = GetNotifyRequest("order_status");
			GetNotifyRequest("pay_time");
			string notifyRequest5 = GetNotifyRequest("sign");
			if (string.IsNullOrEmpty(notifyRequest3))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (string.IsNullOrEmpty(notifyRequest2))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (string.IsNullOrEmpty(notifyRequest5))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (notifyRequest == "200" && notifyRequest4.ToLower() == "success")
			{
				fail.Code = HMNotifyState.Success;
				fail.Data = new HMOrder
				{
					OrderNo = notifyRequest3,
					SupplierOrderNo = "",
					OrderAmt = Utils.StringToDecimal(notifyRequest2, decimal.Zero),
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
			string notifyRequest2 = GetNotifyRequest("amount");
			string notifyRequest3 = GetNotifyRequest("order_no");
			string notifyRequest4 = GetNotifyRequest("order_status");
			string notifyRequest5 = GetNotifyRequest("pay_time");
			string notifyRequest6 = GetNotifyRequest("sign");
			string value = EncryUtils.MD5($"status={notifyRequest}&amount={notifyRequest2}&order_no={notifyRequest3}&order_status={notifyRequest4}&pay_time={notifyRequest5}&app_secret={account.Md5Pwd}", "UTF-8").ToLower();
			if (notifyRequest6.Equals(value))
			{
				if (notifyRequest == "200" && notifyRequest4 == "success")
				{
					Utils.StringToDecimal(notifyRequest2, decimal.Zero);
					fail.Code = HMNotifyState.Success;
					fail.Data = NOTIFY_SUCCESS;
				}
				else
				{
					fail.Data = NOTIFY_FAIL;
				}
			}
			else
			{
				fail.Data = NOTIFY_FAIL;
			}
			return fail;
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

		protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string returnRequest = GetReturnRequest("status");
			string returnRequest2 = GetReturnRequest("amount");
			string returnRequest3 = GetReturnRequest("order_no");
			string returnRequest4 = GetReturnRequest("order_status");
			GetReturnRequest("pay_time");
			string returnRequest5 = GetReturnRequest("sign");
			if (string.IsNullOrEmpty(returnRequest3))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (string.IsNullOrEmpty(returnRequest2))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (string.IsNullOrEmpty(returnRequest5))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (returnRequest == "200" && returnRequest4.ToLower() == "success")
			{
				fail.Code = HMNotifyState.Success;
				fail.Data = new HMOrder
				{
					OrderNo = returnRequest3,
					SupplierOrderNo = "",
					OrderAmt = Utils.StringToDecimal(returnRequest2, decimal.Zero),
					OrderTime = DateTime.Now
				};
			}
			else
			{
				fail.Code = HMNotifyState.Fail;
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
			string value = EncryUtils.MD5($"status={returnRequest}&amount={returnRequest2}&order_no={returnRequest3}&order_status={returnRequest4}&pay_time={returnRequest5}&app_secret={account.Md5Pwd}", "UTF-8").ToLower();
			if (returnRequest6.Equals(value))
			{
				if (returnRequest == "200" && returnRequest4 == "success")
				{
					Utils.StringToDecimal(returnRequest2, decimal.Zero);
					fail.Code = HMNotifyState.Success;
					fail.Data = NOTIFY_SUCCESS;
				}
				else
				{
					fail.Data = NOTIFY_FAIL;
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
