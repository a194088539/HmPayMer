using HM.Framework.Logging;
using System;
using System.Collections.Generic;

namespace HM.Framework.PayApi.YiZhiFu
{
	public class HMPayApi : HMPayApiBase
	{
		public override string NOTIFY_SUCCESS => "SUCCESS";

		public override string NOTIFY_FAIL => "FAIL";

		public override bool IsWithdraw => true;

		private string GetChannelCode(HMChannel channel)
		{
			switch (channel)
			{
			case HMChannel.WEIXIN_NATIVE:
				return "wxpay";
			case HMChannel.WEIXIN_H5:
				return "wxpayh5";
			case HMChannel.ALIPAY_NATIVE:
				return "alipay";
			case HMChannel.ALIPAY_H5:
				return "alipayh5";
			case HMChannel.QQPAY_NATIVE:
			case HMChannel.QQPAY_H5:
				return "qqpay";
			default:
				return string.Empty;
			}
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
			if (string.IsNullOrEmpty(channelCode))
			{
				fail.Message = "此接口通道暂不支持";
				return fail;
			}
			string accountUser = base.Account.AccountUser;
			string text = "pay";
			string text2 = channelCode;
			string orderNo = order.OrderNo;
			string orderNo2 = order.OrderNo;
			string text3 = (order.OrderAmt / 100m).ToString("0.00");
			string notifyUri = base.Supplier.NotifyUri;
			string returnUri = base.Supplier.ReturnUri;
			string text4 = EncryUtils.MD5(accountUser + base.Account.Md5Pwd).ToUpper();
			fail.Code = HMPayState.Success;
			fail.Data = $"{base.Supplier.PostUri}?pid={accountUser}&act={text}&type={text2}&out_trade_no={orderNo}&name={orderNo2}&money={text3}&notify_url={notifyUri}&return_url={returnUri}&sign={text4}";
			return fail;
		}

		protected override Dictionary<string, string> GetNotifyParam()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			try
			{
				string request = Utils.GetRequest("out_trade_no");
				string request2 = Utils.GetRequest("trade_no");
				string request3 = Utils.GetRequest("result_code");
				string request4 = Utils.GetRequest("money");
				string request5 = Utils.GetRequest("type");
				string request6 = Utils.GetRequest("sign");
				dictionary.Add("out_trade_no", request);
				dictionary.Add("trade_no", request2);
				dictionary.Add("result_code", request3);
				dictionary.Add("money", request4);
				dictionary.Add("type", request5);
				dictionary.Add("sign", request6);
				LogUtil.Debug("易支付.GetNotifyParam=" + dictionary.ToJson());
				return dictionary;
			}
			catch (Exception exception)
			{
				LogUtil.Error("行付天下获取失败.GetNotifyParam", exception);
				return dictionary;
			}
		}

		protected override Dictionary<string, string> GetReturnParam()
		{
			return new Dictionary<string, string>();
		}

		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string notifyRequest = GetNotifyRequest("out_trade_no");
			string notifyRequest2 = GetNotifyRequest("trade_no");
			string notifyRequest3 = GetNotifyRequest("money");
			string notifyRequest4 = GetNotifyRequest("result_code");
			string notifyRequest5 = GetNotifyRequest("sign");
			if (string.IsNullOrEmpty(notifyRequest))
			{
				fail.Message = "参数验证失败";
				return fail;
			}
			if (string.IsNullOrEmpty(notifyRequest4))
			{
				fail.Message = "参数验证失败";
				return fail;
			}
			if (string.IsNullOrEmpty(notifyRequest5))
			{
				fail.Message = "参数验证失败";
				return fail;
			}
			fail.Code = HMNotifyState.Success;
			fail.Data = new HMOrder
			{
				OrderNo = notifyRequest,
				SupplierOrderNo = notifyRequest2,
				OrderAmt = Utils.StringToDecimal(notifyRequest3, decimal.Zero) * 100m,
				OrderTime = DateTime.Now
			};
			return fail;
		}

		protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			fail.Code = HMNotifyState.WaitAccountInit;
			return fail;
		}

		public override HMNotifyResult<string> NotifySign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			fail.Data = NOTIFY_FAIL;
			string notifyRequest = GetNotifyRequest("sign");
			string request = Utils.GetRequest("result_code");
			string returnRequest = GetReturnRequest("money");
			string value = EncryUtils.MD5(account.AccountUser + account.Md5Pwd).ToUpper();
			if (request.Equals("TRADE_SUCCESS") && notifyRequest.ToUpper().Equals(value))
			{
				decimal d = Utils.StringToDecimal(returnRequest, decimal.Zero) * 100m;
				if (order.OrderAmt == d)
				{
					fail.Code = HMNotifyState.Success;
					fail.Data = NOTIFY_SUCCESS;
				}
				else
				{
					fail.Message = "金额验证失败！";
				}
			}
			else
			{
				fail.Message = "签名失败!result_code=" + request;
			}
			fail.Code = HMNotifyState.Success;
			fail.Data = NOTIFY_SUCCESS;
			return fail;
		}

		public override HMNotifyResult<string> ResultSign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			fail.Code = HMNotifyState.Success;
			fail.Data = NOTIFY_SUCCESS;
			return fail;
		}
	}
}
