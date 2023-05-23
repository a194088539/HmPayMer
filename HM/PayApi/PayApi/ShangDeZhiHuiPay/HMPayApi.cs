using HM.Framework.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace HM.Framework.PayApi.ShangDeZhiHuiPay
{
	public class HMPayApi : HMPayApiBase
	{
		public override string NOTIFY_SUCCESS => "SUCCESS";

		public override string NOTIFY_FAIL => "Fail";

		public override bool IsWithdraw => false;

		private string GetChannelCode(HMChannel channel)
		{
			switch (channel)
			{
			case HMChannel.WEIXIN_NATIVE:
				return "1000010";
			case HMChannel.WEIXIN_JSAPI:
				return "1000030";
			case HMChannel.ALIPAY_NATIVE:
				return "2000030";
			case HMChannel.ALIPAY_H5:
				return "2000020";
			case HMChannel.QQPAY_NATIVE:
			case HMChannel.QQPAY_H5:
				return "7000020";
			case HMChannel.JD_NATIVE:
			case HMChannel.JD_H5:
				return "8000020";
			default:
				return string.Empty;
			}
		}

		public override HMMode GetPayMode(HMChannel code)
		{
			return HMMode.输出字符串;
		}

		protected override HMPayResult PayGatewayBody(HMOrder order)
		{
			HMPayResult fail = HMPayResult.Fail;
			fail.Mode = GetPayMode(order.ChannelCode);
			GetChannelCode(order.ChannelCode);
			string md5Pwd = base.Account.Md5Pwd;
			string accountUser = base.Account.AccountUser;
			string value = (order.OrderAmt / 100m).ToString("0.00");
			string orderNo = order.OrderNo;
			string value2 = order.OrderTime.ToString("yyyyMMddHHmmss");
			string orderNo2 = order.OrderNo;
			string clientIp = order.ClientIp;
			string returnUri = base.Supplier.ReturnUri;
			string notifyUri = base.Supplier.NotifyUri;
			string childAccountUser = base.Account.ChildAccountUser;
			string appId = base.Account.AppId;
			string value3 = "";
			SortedDictionary<string, string> obj = new SortedDictionary<string, string>
			{
				{
					"payKey",
					accountUser
				},
				{
					"paySecret",
					md5Pwd
				},
				{
					"orderPrice",
					value
				},
				{
					"outTradeNo",
					orderNo
				},
				{
					"orderTime",
					value2
				},
				{
					"productName",
					orderNo2
				},
				{
					"orderIp",
					clientIp
				},
				{
					"returnUrl",
					returnUri
				},
				{
					"notifyUrl",
					notifyUri
				},
				{
					"subPayKey",
					childAccountUser
				},
				{
					"appId",
					appId
				},
				{
					"openId",
					value3
				}
			};
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, string> item in obj)
			{
				if (!string.IsNullOrEmpty(item.Value))
				{
					stringBuilder.AppendFormat("&{0}={1}", item.Key, item.Value);
				}
			}
			stringBuilder.Remove(0, 1);
			try
			{
				string text = stringBuilder.ToString();
				string pubilcKey = EncryUtils.RSAPublicKeyJava2DotNet(base.Account.RsaPublic);
				List<string> list = new List<string>();
				for (int i = 0; i < text.Length; i += 117)
				{
					int num = 117;
					if (i + num > text.Length)
					{
						num = text.Length - i;
					}
					list.Add(text.Substring(i, num));
				}
				StringBuilder stringBuilder2 = new StringBuilder();
				stringBuilder2.AppendFormat("<form name='submit' action='{0}' _input_charset='utf-8' method='POST'>", base.Supplier.PostUri);
				for (int j = 0; j < list.Count; j++)
				{
					stringBuilder2.AppendFormat("<input type='hidden' name='{0}' value = '{1}' />", j, EncryUtils.RSAEncryptByPublicKey(list[j], pubilcKey));
				}
				stringBuilder2.AppendFormat("<input type='hidden' name='payKey' value = '{0}' />", accountUser);
				stringBuilder2.Append("</form>").Append("<script type='text/javascript'>document.forms['submit'].submit();</script>");
				fail.Code = HMPayState.Success;
				fail.Data = stringBuilder2.ToString();
				fail.Mode = HMMode.输出字符串;
				return fail;
			}
			catch (Exception exception)
			{
				fail.Message = "系统繁忙，请稍候再试！";
				LogUtil.Error("杉德智慧收银系统出错,订单号:" + order.OrderNo, exception);
				return fail;
			}
		}

		protected override Dictionary<string, string> GetNotifyParam()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string request = Utils.GetRequest("payKey");
			string request2 = Utils.GetRequest("productName");
			string request3 = Utils.GetRequest("outTradeNo");
			string request4 = Utils.GetRequest("orderPrice");
			string request5 = Utils.GetRequest("productType");
			string request6 = Utils.GetRequest("tradeStatus");
			string request7 = Utils.GetRequest("successTime");
			string request8 = Utils.GetRequest("orderTime");
			string request9 = Utils.GetRequest("trxNo");
			string request10 = Utils.GetRequest("remitRequestNo");
			string request11 = Utils.GetRequest("remitStatus");
			string request12 = Utils.GetRequest("sign");
			dictionary.Add("payKey", request);
			dictionary.Add("productName", request2);
			dictionary.Add("outTradeNo", request3);
			dictionary.Add("orderPrice", request4);
			dictionary.Add("productType", request5);
			dictionary.Add("tradeStatus", request6);
			dictionary.Add("successTime", request7);
			dictionary.Add("orderTime", request8);
			dictionary.Add("trxNo", request9);
			dictionary.Add("remitRequestNo", request10);
			dictionary.Add("remitStatus", request11);
			dictionary.Add("sign", request12);
			LogUtil.Info("智慧收银.GetNotifyParam=" + dictionary.ToJson());
			return dictionary;
		}

		protected override Dictionary<string, string> GetReturnParam()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string request = Utils.GetRequest("payKey");
			string request2 = Utils.GetRequest("productName");
			string request3 = Utils.GetRequest("outTradeNo");
			string request4 = Utils.GetRequest("orderPrice");
			string request5 = Utils.GetRequest("productType");
			string request6 = Utils.GetRequest("tradeStatus");
			string request7 = Utils.GetRequest("successTime");
			string request8 = Utils.GetRequest("orderTime");
			string request9 = Utils.GetRequest("trxNo");
			string request10 = Utils.GetRequest("remitRequestNo");
			string request11 = Utils.GetRequest("remitStatus");
			string request12 = Utils.GetRequest("sign");
			dictionary.Add("payKey", request);
			dictionary.Add("productName", request2);
			dictionary.Add("outTradeNo", request3);
			dictionary.Add("orderPrice", request4);
			dictionary.Add("productType", request5);
			dictionary.Add("tradeStatus", request6);
			dictionary.Add("successTime", request7);
			dictionary.Add("orderTime", request8);
			dictionary.Add("trxNo", request9);
			dictionary.Add("remitRequestNo", request10);
			dictionary.Add("remitStatus", request11);
			dictionary.Add("sign", request12);
			LogUtil.Info("智慧收银.GetReturnParam=" + dictionary.ToJson());
			return dictionary;
		}

		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string notifyRequest = GetNotifyRequest("tradeStatus");
			string notifyRequest2 = GetNotifyRequest("orderPrice");
			string notifyRequest3 = GetNotifyRequest("outTradeNo");
			string notifyRequest4 = GetNotifyRequest("trxNo");
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
			if (string.IsNullOrEmpty(notifyRequest))
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
				OrderNo = notifyRequest3,
				SupplierOrderNo = notifyRequest4,
				OrderAmt = Utils.StringToDecimal(notifyRequest2, decimal.Zero) * 100m,
				OrderTime = DateTime.Now
			};
			return fail;
		}

		protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string returnRequest = GetReturnRequest("tradeStatus");
			string returnRequest2 = GetReturnRequest("orderPrice");
			string returnRequest3 = GetReturnRequest("outTradeNo");
			string returnRequest4 = GetReturnRequest("trxNo");
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
			if (string.IsNullOrEmpty(returnRequest))
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
				OrderNo = returnRequest3,
				SupplierOrderNo = returnRequest4,
				OrderAmt = Utils.StringToDecimal(returnRequest2, decimal.Zero) * 100m,
				OrderTime = DateTime.Now
			};
			return fail;
		}

		public override HMNotifyResult<string> NotifySign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			fail.Data = NOTIFY_FAIL;
			string notifyRequest = GetNotifyRequest("tradeStatus");
			string notifyRequest2 = GetNotifyRequest("sign");
			SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
			foreach (KeyValuePair<string, string> notifyParam in base.NotifyParams)
			{
				sortedDictionary.Add(notifyParam.Key, notifyParam.Value);
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, string> item in sortedDictionary)
			{
				if (!string.IsNullOrEmpty(item.Value) && !item.Key.Equals("sign"))
				{
					stringBuilder.AppendFormat("{0}={1}&", item.Key, item.Value);
				}
			}
			string value = EncryUtils.MD5(stringBuilder.ToString() + "paySecret=" + account.Md5Pwd).ToUpper();
			if (notifyRequest2.Equals(value))
			{
				if (notifyRequest.ToUpper().Equals("SUCCESS"))
				{
					fail.Code = HMNotifyState.Success;
					fail.Data = NOTIFY_SUCCESS;
				}
				else
				{
					fail.Message = "交易状态:" + notifyRequest;
				}
			}
			else
			{
				fail.Message = "签名失败";
			}
			return fail;
		}

		public override HMNotifyResult<string> ResultSign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			fail.Data = NOTIFY_FAIL;
			string returnRequest = GetReturnRequest("tradeStatus");
			string returnRequest2 = GetReturnRequest("sign");
			SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
			foreach (KeyValuePair<string, string> returnParam in base.ReturnParams)
			{
				sortedDictionary.Add(returnParam.Key, returnParam.Value);
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, string> item in sortedDictionary)
			{
				if (!string.IsNullOrEmpty(item.Value) && !item.Key.Equals("sign"))
				{
					stringBuilder.AppendFormat("{0}={1}&", item.Key, item.Value);
				}
			}
			string value = EncryUtils.MD5(stringBuilder.ToString() + "paySecret=" + account.Md5Pwd).ToUpper();
			if (returnRequest2.Equals(value))
			{
				if (returnRequest.ToUpper().Equals("SUCCESS"))
				{
					fail.Code = HMNotifyState.Success;
					fail.Data = NOTIFY_SUCCESS;
				}
				else
				{
					fail.Message = "交易状态:" + returnRequest;
				}
			}
			else
			{
				fail.Message = "签名失败";
			}
			return fail;
		}
	}
}
