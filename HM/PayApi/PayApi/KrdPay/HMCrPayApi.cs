using HM.Framework.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace HM.Framework.PayApi.KrdPay
{
	public class HMCrPayApi : HMPayApiBase
	{
		public override string NOTIFY_SUCCESS => "success";

		public override string NOTIFY_FAIL => "fail";

		public override bool IsWithdraw => false;

		public override HMMode GetPayMode(HMChannel code)
		{
			return HMMode.输出字符串;
		}

		private string GetChannelCode(HMChannel channel)
		{
			switch (channel)
			{
			case HMChannel.WEIXIN_NATIVE:
				return "wxpay";
			case HMChannel.WEIXIN_H5:
				return "wxh5";
			case HMChannel.ALIPAY_H5:
				return "ALIPAYh5";
			case HMChannel.ALIPAY_NATIVE:
				return "ALIPAY";
			default:
				return string.Empty;
			}
		}

		protected override Dictionary<string, string> GetNotifyParam()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string request = Utils.GetRequest("cgzt");
			string request2 = Utils.GetRequest("orderNo");
			string request3 = Utils.GetRequest("uid");
			string request4 = Utils.GetRequest("totalFee");
			string request5 = Utils.GetRequest("remark");
			string request6 = Utils.GetRequest("sign");
			dictionary.Add("cgzt", request);
			dictionary.Add("orderNo", request2);
			dictionary.Add("uid", request3);
			dictionary.Add("totalFee", request4);
			dictionary.Add("remark", request5);
			dictionary.Add("sign", request6);
			LogUtil.DebugFormat(" KrdPay 异步参数：{0}", dictionary.ToJson());
			return dictionary;
		}

		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string notifyRequest = GetNotifyRequest("cgzt");
			string notifyRequest2 = GetNotifyRequest("orderNo");
			GetNotifyRequest("uid");
			string notifyRequest3 = GetNotifyRequest("totalFee");
			GetNotifyRequest("remark");
			string notifyRequest4 = GetNotifyRequest("sign");
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
			if (string.IsNullOrEmpty(notifyRequest4))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (notifyRequest == "Success")
			{
				fail.Code = HMNotifyState.Success;
				fail.Data = new HMOrder
				{
					OrderNo = notifyRequest2,
					SupplierOrderNo = "",
					OrderAmt = Utils.StringToDecimal(notifyRequest3, decimal.Zero) * 100m,
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
			string notifyRequest = GetNotifyRequest("cgzt");
			string notifyRequest2 = GetNotifyRequest("orderNo");
			string notifyRequest3 = GetNotifyRequest("uid");
			string notifyRequest4 = GetNotifyRequest("totalFee");
			GetNotifyRequest("remark");
			string notifyRequest5 = GetNotifyRequest("sign");
			string text = EncryUtils.MD5($"uid={notifyRequest3}&totalFee={notifyRequest4}&orderNo={notifyRequest2}&orderNo={notifyRequest2}&krdKey={account.Md5Pwd}", "UTF-8").ToLower();
			LogUtil.DebugFormat("KrdPay同步签名：{0}", text);
			if (notifyRequest5.Equals(text))
			{
				decimal num = Utils.StringToDecimal(notifyRequest4, decimal.Zero) * 100m;
				if (notifyRequest.ToLower() == "success")
				{
					fail.Code = HMNotifyState.Success;
					fail.Data = NOTIFY_SUCCESS;
				}
				else if (notifyRequest.ToLower() == "fai")
				{
					fail.Code = HMNotifyState.Fail;
					fail.Data = NOTIFY_FAIL;
				}
				else
				{
					fail.Code = HMNotifyState.WaitAccountInit;
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
			string request = Utils.GetRequest("cgzt");
			string request2 = Utils.GetRequest("orderNo");
			string request3 = Utils.GetRequest("uid");
			string request4 = Utils.GetRequest("totalFee");
			string request5 = Utils.GetRequest("remark");
			string request6 = Utils.GetRequest("sign");
			dictionary.Add("cgzt", request);
			dictionary.Add("orderNo", request2);
			dictionary.Add("uid", request3);
			dictionary.Add("totalFee", request4);
			dictionary.Add("remark", request5);
			dictionary.Add("sign", request6);
			LogUtil.DebugFormat(" KrdPay 同步参数：{0}", dictionary.ToJson());
			return dictionary;
		}

		protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string returnRequest = GetReturnRequest("cgzt");
			string returnRequest2 = GetReturnRequest("orderNo");
			GetReturnRequest("uid");
			string returnRequest3 = GetReturnRequest("totalFee");
			GetReturnRequest("remark");
			string returnRequest4 = GetReturnRequest("sign");
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
			if (string.IsNullOrEmpty(returnRequest4))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (returnRequest == "Success")
			{
				fail.Code = HMNotifyState.Success;
				fail.Data = new HMOrder
				{
					OrderNo = returnRequest2,
					SupplierOrderNo = "",
					OrderAmt = Utils.StringToDecimal(returnRequest3, decimal.Zero) * 100m,
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
			string returnRequest = GetReturnRequest("cgzt");
			string returnRequest2 = GetReturnRequest("orderNo");
			string returnRequest3 = GetReturnRequest("uid");
			string returnRequest4 = GetReturnRequest("totalFee");
			GetReturnRequest("remark");
			string returnRequest5 = GetReturnRequest("sign");
			string text = EncryUtils.MD5($"uid={returnRequest3}&totalFee={returnRequest4}&orderNo={returnRequest2}&orderNo={returnRequest2}&krdKey={account.Md5Pwd}", "UTF-8").ToLower();
			LogUtil.DebugFormat("KrdPay同步签名：{0}", text);
			if (returnRequest5.Equals(text))
			{
				decimal num = Utils.StringToDecimal(returnRequest4, decimal.Zero) * 100m;
				if (returnRequest.ToLower() == "success")
				{
					fail.Code = HMNotifyState.Success;
					fail.Data = NOTIFY_SUCCESS;
				}
				else if (returnRequest.ToLower() == "fai")
				{
					fail.Code = HMNotifyState.Fail;
					fail.Data = NOTIFY_FAIL;
				}
				else
				{
					fail.Code = HMNotifyState.WaitAccountInit;
					fail.Data = NOTIFY_FAIL;
				}
			}
			else
			{
				fail.Data = NOTIFY_FAIL;
			}
			return fail;
		}

		protected override HMPayResult PayGatewayBody(HMOrder order)
		{
			HMPayResult fail = HMPayResult.Fail;
			fail.Mode = GetPayMode(order.ChannelCode);
			string channelCode = GetChannelCode(order.ChannelCode);
			if (!string.IsNullOrEmpty(channelCode))
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary.Add("GuestNo", base.Account.AccountUser);
				dictionary.Add("krdKey", base.Account.Md5Pwd);
				dictionary.Add("orderNo", base.Order.OrderNo);
				dictionary.Add("totalFee", (base.Order.OrderAmt / 100m).ToString("0.00"));
				dictionary.Add("notifyUrl", base.Supplier.NotifyUri);
				dictionary.Add("returnUrl", base.Supplier.ReturnUri);
				dictionary.Add("defaultbank", channelCode);
				dictionary.Add("remark", base.Order.OrderNo);
				try
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendFormat("<body onLoad='document.pay.submit()'><form name='pay' id='pay' action='{0}' method='post'>", base.Supplier.PostUri);
					foreach (KeyValuePair<string, object> item in dictionary)
					{
						stringBuilder.AppendFormat("<input type='hidden' name='{0}' value = '{1}' />", item.Key, item.Value);
					}
					stringBuilder.Append("</form></body>");
					fail.Code = HMPayState.Success;
					fail.Data = stringBuilder.ToString();
					LogUtil.DebugFormat("KrdPay,支付:{0}", stringBuilder.ToString());
					return fail;
				}
				catch (Exception exception)
				{
					fail.Message = "系统繁忙，请稍候再试！";
					LogUtil.Error("KrdPay,订单号:" + order.OrderNo, exception);
					return fail;
				}
			}
			fail.Message = "此通道已关闭！";
			return fail;
		}
	}
}
