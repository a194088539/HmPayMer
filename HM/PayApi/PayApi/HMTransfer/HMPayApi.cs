using System;
using System.Collections.Generic;
using System.Text;

namespace HM.Framework.PayApi.HMTransfer
{
	public class HMPayApi : HMPayApiBase
	{
		public override string NOTIFY_SUCCESS => "SUCCESS";

		public override string NOTIFY_FAIL => "FAIL";

		public override bool IsWithdraw => false;

		private string GetChannelCode(HMChannel channel)
		{
			switch (channel)
			{
			case HMChannel.WEIXIN_NATIVE:
				return "WEIXIN_NATIVE";
			case HMChannel.WEIXIN_H5:
				return "WEIXIN_H5";
			case HMChannel.WEIXIN_JSAPI:
				return "WEIXIN_JSAPI";
			case HMChannel.WEIXIN_MICROPAY:
				return "WEIXIN_MICROPAY";
			case HMChannel.ALIPAY_NATIVE:
				return "ALIPAY_NATIVE";
			case HMChannel.ALIPAY_H5:
				return "ALIPAY_H5";
			case HMChannel.ALIPAY_JSAPI:
				return "ALIPAY_JSAPI";
			case HMChannel.ALIPAY_MICROPAY:
				return "ALIPAY_MICROPAY";
			case HMChannel.QQPAY_NATIVE:
				return "QQPAY_NATIVE";
			case HMChannel.QQPAY_H5:
				return "QQPAY_H5";
			case HMChannel.QQPAY_APP:
				return "QQPAY_APP";
			case HMChannel.JD_NATIVE:
				return "JD_NATIVE";
			case HMChannel.JD_H5:
				return "JD_H5";
			case HMChannel.SPDB:
				return "SPDB";
			case HMChannel.HXB:
				return "HXB";
			case HMChannel.SPABANK:
				return "SPABANK";
			case HMChannel.ECITIC:
				return "ECITIC";
			case HMChannel.CIB:
				return "CIB";
			case HMChannel.CMBC:
				return "CMBC";
			case HMChannel.CMB:
				return "CMB";
			case HMChannel.BOC:
				return "BOC";
			case HMChannel.BCOM:
				return "BCOM";
			case HMChannel.CCB:
				return "CCB";
			case HMChannel.ICBC:
				return "ICBC";
			case HMChannel.ABC:
				return "ABC";
			case HMChannel.GATEWAY_QUICK:
				return "GATEWAY_QUICK";
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
			string text = "V2.0";
			string postUri = base.Supplier.PostUri;
			string accountUser = base.Account.AccountUser;
			string childAccountUser = base.Account.ChildAccountUser;
			string md5Pwd = base.Account.Md5Pwd;
			string text2 = order.OrderAmt.ToString("0");
			string orderNo = order.OrderNo;
			string text3 = channelCode;
			string notifyUri = base.Supplier.NotifyUri;
			string clientIp = order.ClientIp;
			string returnUri = base.Supplier.ReturnUri;
			SortedDictionary<string, string> obj = new SortedDictionary<string, string>
			{
				{
					"app_id",
					accountUser
				},
				{
					"trade_type",
					text3
				},
				{
					"total_amount",
					text2
				},
				{
					"out_trade_no",
					orderNo
				},
				{
					"notify_url",
					notifyUri
				}
			};
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, string> item in obj)
			{
				stringBuilder.AppendFormat("&{0}={1}", item.Key, item.Value);
			}
			stringBuilder.Append(md5Pwd);
			stringBuilder.Remove(0, 1);
			string text4 = EncryUtils.MD5(stringBuilder.ToString()).ToLower();
			fail.Code = HMPayState.Success;
			fail.Data = string.Format(postUri + "?app_id={0}&trade_type={1}&total_amount={2}&out_trade_no={3}&notify_url={4}&return_url={5}&client_ip={6}&extra_return_param={7}&sign={8}&interface_version={9}", accountUser, text3, text2, orderNo, notifyUri, returnUri, clientIp, childAccountUser, text4, text);
			return fail;
		}

		protected override Dictionary<string, string> GetNotifyParam()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string request = Utils.GetRequest("out_trade_no");
			string request2 = Utils.GetRequest("trade_no");
			string request3 = Utils.GetRequest("trade_status");
			string request4 = Utils.GetRequest("extra_return_param");
			string request5 = Utils.GetRequest("total_amount");
			string request6 = Utils.GetRequest("trade_time");
			string request7 = Utils.GetRequest("sign");
			dictionary.Add("out_trade_no", request);
			dictionary.Add("trade_no", request2);
			dictionary.Add("trade_status", request3);
			dictionary.Add("extra_return_param", request4);
			dictionary.Add("total_amount", request5);
			dictionary.Add("trade_time", request6);
			dictionary.Add("sign", request7);
			return dictionary;
		}

		protected override Dictionary<string, string> GetReturnParam()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string request = Utils.GetRequest("out_trade_no");
			string request2 = Utils.GetRequest("trade_no");
			string request3 = Utils.GetRequest("trade_status");
			string request4 = Utils.GetRequest("extra_return_param");
			string request5 = Utils.GetRequest("total_amount");
			string request6 = Utils.GetRequest("trade_time");
			string request7 = Utils.GetRequest("sign");
			dictionary.Add("out_trade_no", request);
			dictionary.Add("trade_no", request2);
			dictionary.Add("trade_status", request3);
			dictionary.Add("extra_return_param", request4);
			dictionary.Add("total_amount", request5);
			dictionary.Add("trade_time", request6);
			dictionary.Add("sign", request7);
			return dictionary;
		}

		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string notifyRequest = GetNotifyRequest("out_trade_no");
			string notifyRequest2 = GetNotifyRequest("trade_no");
			GetNotifyRequest("trade_status");
			string notifyRequest3 = GetNotifyRequest("total_amount");
			string notifyRequest4 = GetNotifyRequest("trade_time");
			string notifyRequest5 = GetNotifyRequest("sign");
			if (string.IsNullOrEmpty(notifyRequest))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (string.IsNullOrEmpty(notifyRequest3))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (string.IsNullOrEmpty(notifyRequest5))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			fail.Code = HMNotifyState.Success;
			fail.Data = new HMOrder
			{
				OrderNo = notifyRequest,
				SupplierOrderNo = notifyRequest2,
				OrderAmt = Utils.StringToDecimal(notifyRequest3, decimal.Zero),
				OrderTime = Utils.StringToDateTime(notifyRequest4, DateTime.Now).Value
			};
			return fail;
		}

		protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string returnRequest = GetReturnRequest("out_trade_no");
			string returnRequest2 = GetReturnRequest("trade_no");
			GetReturnRequest("trade_status");
			string returnRequest3 = GetReturnRequest("total_amount");
			string returnRequest4 = GetReturnRequest("trade_time");
			string returnRequest5 = GetReturnRequest("sign");
			if (string.IsNullOrEmpty(returnRequest))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (string.IsNullOrEmpty(returnRequest3))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (string.IsNullOrEmpty(returnRequest5))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			fail.Code = HMNotifyState.Success;
			fail.Data = new HMOrder
			{
				OrderNo = returnRequest,
				SupplierOrderNo = returnRequest2,
				OrderAmt = Utils.StringToDecimal(returnRequest3, decimal.Zero),
				OrderTime = Utils.StringToDateTime(returnRequest4, DateTime.Now).Value
			};
			return fail;
		}

		public override HMNotifyResult<string> NotifySign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			string notifyRequest = GetNotifyRequest("out_trade_no");
			GetNotifyRequest("trade_no");
			string notifyRequest2 = GetNotifyRequest("trade_status");
			string notifyRequest3 = GetNotifyRequest("total_amount");
			GetNotifyRequest("trade_time");
			string notifyRequest4 = GetNotifyRequest("sign");
			string value = EncryUtils.MD5($"out_trade_no={notifyRequest}&total_amount={notifyRequest3}&trade_status={notifyRequest2}{account.Md5Pwd}");
			if (notifyRequest4.Equals(value))
			{
				fail.Code = HMNotifyState.Success;
				fail.Data = NOTIFY_SUCCESS;
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
			string returnRequest = GetReturnRequest("out_trade_no");
			GetReturnRequest("trade_no");
			string returnRequest2 = GetReturnRequest("trade_status");
			string returnRequest3 = GetReturnRequest("total_amount");
			GetReturnRequest("trade_time");
			string returnRequest4 = GetReturnRequest("sign");
			string value = EncryUtils.MD5($"out_trade_no={returnRequest}&total_amount={returnRequest3}&trade_status={returnRequest2}{account.Md5Pwd}");
			if (returnRequest4.Equals(value))
			{
				fail.Code = HMNotifyState.Success;
				fail.Data = NOTIFY_SUCCESS;
			}
			else
			{
				fail.Data = NOTIFY_FAIL;
			}
			return fail;
		}
	}
}
