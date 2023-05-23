using HM.Framework.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace HM.Framework.PayApi.SuPay
{
	public class HMPayApi : HMPayApiBase
	{
		public override string NOTIFY_SUCCESS => "SUCCESS";

		public override string NOTIFY_FAIL => "FAIL";

		public override bool IsWithdraw => false;

		private string GetChannelCode(HMChannel channel)
		{
			if (channel == HMChannel.WEIXIN_NATIVE)
			{
				return "weixin";
			}
			if ((uint)(channel - 2) <= 1u)
			{
				return "wxwap";
			}
			return string.Empty;
		}

		public override HMMode GetPayMode(HMChannel code)
		{
			switch (code)
			{
			case HMChannel.WEIXIN_NATIVE:
			case HMChannel.ALIPAY_NATIVE:
			case HMChannel.QQPAY_NATIVE:
			case HMChannel.JD_NATIVE:
				return HMMode.跳转扫码页面;
			case HMChannel.WEIXIN_H5:
			case HMChannel.ALIPAY_H5:
			case HMChannel.QQPAY_H5:
			case HMChannel.JD_H5:
				return HMMode.跳转链接;
			default:
				return HMMode.输出字符串;
			}
		}

		public override HMNotifyResult<string> NotifySign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			string accountUser = account.AccountUser;
			string notifyRequest = GetNotifyRequest("r0_Cmd");
			string notifyRequest2 = GetNotifyRequest("r1_Code");
			string notifyRequest3 = GetNotifyRequest("r2_TrxId");
			string notifyRequest4 = GetNotifyRequest("r3_Amt");
			string notifyRequest5 = GetNotifyRequest("r4_Cur");
			string notifyRequest6 = GetNotifyRequest("r5_Pid");
			string notifyRequest7 = GetNotifyRequest("r6_Order");
			string notifyRequest8 = GetNotifyRequest("r7_Uid");
			string notifyRequest9 = GetNotifyRequest("r8_MP");
			string notifyRequest10 = GetNotifyRequest("r9_BType");
			GetNotifyRequest("rp_PayDate");
			string notifyRequest11 = GetNotifyRequest("hmac");
			string aValue = "" + accountUser + notifyRequest + notifyRequest2 + notifyRequest3 + notifyRequest4 + notifyRequest5 + notifyRequest6 + notifyRequest7 + notifyRequest8 + notifyRequest9 + notifyRequest10;
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			if (Encrypt.HmacSign(aValue, account.Md5Pwd).Equals(notifyRequest11))
			{
				decimal d = Utils.StringToDecimal(notifyRequest4, decimal.Zero) * 100m;
				if (order.OrderAmt == d)
				{
					fail.Code = HMNotifyState.Success;
					fail.Data = NOTIFY_SUCCESS;
					if (notifyRequest10.Equals("1"))
					{
						fail.Code = HMNotifyState.ReturnUrl;
					}
				}
				else
				{
					fail.Message = "订单金额验证失败";
				}
			}
			else
			{
				fail.Code = HMNotifyState.Fail;
				fail.Data = NOTIFY_FAIL;
			}
			return fail;
		}

		public override HMNotifyResult<string> ResultSign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			string accountUser = account.AccountUser;
			string returnRequest = GetReturnRequest("r0_Cmd");
			string returnRequest2 = GetReturnRequest("r1_Code");
			string returnRequest3 = GetReturnRequest("r2_TrxId");
			string returnRequest4 = GetReturnRequest("r3_Amt");
			string returnRequest5 = GetReturnRequest("r4_Cur");
			string returnRequest6 = GetReturnRequest("r5_Pid");
			string returnRequest7 = GetReturnRequest("r6_Order");
			string returnRequest8 = GetReturnRequest("r7_Uid");
			string returnRequest9 = GetReturnRequest("r8_MP");
			string returnRequest10 = GetReturnRequest("r9_BType");
			GetReturnRequest("rp_PayDate");
			string returnRequest11 = GetReturnRequest("hmac");
			string aValue = "" + accountUser + returnRequest + returnRequest2 + returnRequest3 + returnRequest4 + returnRequest5 + returnRequest6 + returnRequest7 + returnRequest8 + returnRequest9 + returnRequest10;
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			if (Encrypt.HmacSign(aValue, account.Md5Pwd).Equals(returnRequest11))
			{
				decimal d = Utils.StringToDecimal(returnRequest4, decimal.Zero) * 100m;
				if (order.OrderAmt == d)
				{
					fail.Code = HMNotifyState.Success;
					fail.Data = NOTIFY_SUCCESS;
				}
				else
				{
					fail.Message = "订单金额验证失败";
				}
			}
			else
			{
				fail.Code = HMNotifyState.Fail;
				fail.Data = NOTIFY_FAIL;
			}
			return fail;
		}

		protected override Dictionary<string, string> GetNotifyParam()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			try
			{
				string request = Utils.GetRequest("r0_Cmd");
				string request2 = Utils.GetRequest("r1_Code");
				string request3 = Utils.GetRequest("r2_TrxId");
				string request4 = Utils.GetRequest("r3_Amt");
				string request5 = Utils.GetRequest("r4_Cur");
				string request6 = Utils.GetRequest("r5_Pid");
				string request7 = Utils.GetRequest("r6_Order");
				string request8 = Utils.GetRequest("r7_Uid");
				string request9 = Utils.GetRequest("r8_MP");
				string request10 = Utils.GetRequest("r9_BType");
				string request11 = Utils.GetRequest("rp_PayDate");
				string request12 = Utils.GetRequest("hmac");
				dictionary.Add("r0_Cmd", request);
				dictionary.Add("r1_Code", request2);
				dictionary.Add("r2_TrxId", request3);
				dictionary.Add("r3_Amt", request4);
				dictionary.Add("r4_Cur", request5);
				dictionary.Add("r5_Pid", request6);
				dictionary.Add("r6_Order", request7);
				dictionary.Add("r7_Uid", request8);
				dictionary.Add("r8_MP", request9);
				dictionary.Add("r9_BType", request10);
				dictionary.Add("rp_PayDate", request11);
				dictionary.Add("hmac", request12);
				LogUtil.Debug("GetNotifyParam=" + dictionary.ToJson());
				return dictionary;
			}
			catch (Exception exception)
			{
				LogUtil.Error("吉诚 速支付：GetNotifyParam", exception);
				return dictionary;
			}
		}

		protected override Dictionary<string, string> GetReturnParam()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			try
			{
				string request = Utils.GetRequest("r0_Cmd");
				string request2 = Utils.GetRequest("r1_Code");
				string request3 = Utils.GetRequest("r2_TrxId");
				string request4 = Utils.GetRequest("r3_Amt");
				string request5 = Utils.GetRequest("r4_Cur");
				string request6 = Utils.GetRequest("r5_Pid");
				string request7 = Utils.GetRequest("r6_Order");
				string request8 = Utils.GetRequest("r7_Uid");
				string request9 = Utils.GetRequest("r8_MP");
				string request10 = Utils.GetRequest("r9_BType");
				string request11 = Utils.GetRequest("rp_PayDate");
				string request12 = Utils.GetRequest("hmac");
				dictionary.Add("r0_Cmd", request);
				dictionary.Add("r1_Code", request2);
				dictionary.Add("r2_TrxId", request3);
				dictionary.Add("r3_Amt", request4);
				dictionary.Add("r4_Cur", request5);
				dictionary.Add("r5_Pid", request6);
				dictionary.Add("r6_Order", request7);
				dictionary.Add("r7_Uid", request8);
				dictionary.Add("r8_MP", request9);
				dictionary.Add("r9_BType", request10);
				dictionary.Add("rp_PayDate", request11);
				dictionary.Add("hmac", request12);
				LogUtil.Debug("GetReturnParam=" + dictionary.ToJson());
				return dictionary;
			}
			catch (Exception exception)
			{
				LogUtil.Error("吉诚 速支付：GetReturnParam", exception);
				return dictionary;
			}
		}

		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			GetNotifyRequest("r9_BType");
			string notifyRequest = GetNotifyRequest("r6_Order");
			string notifyRequest2 = GetNotifyRequest("r2_TrxId");
			string notifyRequest3 = GetNotifyRequest("r3_Amt");
			string notifyRequest4 = GetNotifyRequest("rp_PayDate");
			fail.Code = HMNotifyState.Success;
			fail.Data = new HMOrder
			{
				OrderNo = notifyRequest,
				SupplierOrderNo = notifyRequest2,
				OrderAmt = Utils.StringToDecimal(notifyRequest3, decimal.Zero) * 100m,
				OrderTime = Utils.StringToDateTime(notifyRequest4, DateTime.Now).Value
			};
			return fail;
		}

		protected override HMPayResult PayGatewayBody(HMOrder order)
		{
			HMPayResult fail = HMPayResult.Fail;
			fail.Mode = GetPayMode(order.ChannelCode);
			string channelCode = GetChannelCode(order.ChannelCode);
			if (!string.IsNullOrEmpty(channelCode))
			{
				string str = "Buy";
				string accountUser = base.Account.AccountUser;
				string md5Pwd = base.Account.Md5Pwd;
				string str2 = base.Order.OrderNo ?? "";
				string str3 = (base.Order.OrderAmt / 100m).ToString("0.00") ?? "";
				string str4 = "CNY";
				string str5 = "";
				string str6 = "";
				string str7 = "";
				string notifyUri = base.Supplier.NotifyUri;
				string text = "";
				string text2 = "";
				string str8 = channelCode ?? "";
				string text3 = "";
				string text4 = "";
				text4 = Encrypt.HmacSign("" + str + accountUser + str2 + str3 + str4 + str5 + str6 + str7 + notifyUri + text + text2 + str8 + text3, md5Pwd);
				SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
				sortedDictionary.Add("PostUrl", base.Supplier.PostUri);
				sortedDictionary.Add("p0_Cmd", "Buy");
				sortedDictionary.Add("p1_MerId", base.Account.AccountUser);
				sortedDictionary.Add("p2_Order", base.Order.OrderNo);
				sortedDictionary.Add("p3_Amt", (base.Order.OrderAmt / 100m).ToString("0.00"));
				sortedDictionary.Add("p4_Cur", "CNY");
				sortedDictionary.Add("p5_Pid", "");
				sortedDictionary.Add("p6_Pcat", "");
				sortedDictionary.Add("p7_Pdesc", "");
				sortedDictionary.Add("p8_Url", base.Supplier.NotifyUri);
				sortedDictionary.Add("p9_SAF", text);
				sortedDictionary.Add("pa_MP", text2);
				sortedDictionary.Add("pd_FrpId", channelCode);
				sortedDictionary.Add("pr_NeedResponse", text3);
				sortedDictionary.Add("hmac", text4);
				try
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendFormat("<form name='submit' action='{0}' _input_charset='utf-8' method='get'>", base.Supplier.PostUri);
					foreach (KeyValuePair<string, string> item in sortedDictionary)
					{
						stringBuilder.AppendFormat("<input type='hidden' name='{0}' value = '{1}' />", item.Key, item.Value);
					}
					stringBuilder.Append("</form>").Append("<script type='text/javascript'>document.forms['submit'].submit();</script>");
					fail.Code = HMPayState.Success;
					fail.Data = stringBuilder.ToString();
					fail.Mode = HMMode.输出字符串;
					return fail;
				}
				catch (Exception ex)
				{
					fail.Message = "系统繁忙:" + ex.Message;
					LogUtil.Error("支付过程中出现错误", ex);
					return fail;
				}
			}
			fail.Message = "此通道已关闭！";
			return fail;
		}

		protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			GetReturnRequest("r9_BType");
			string returnRequest = GetReturnRequest("r6_Order");
			string returnRequest2 = GetReturnRequest("r2_TrxId");
			string returnRequest3 = GetReturnRequest("r3_Amt");
			string returnRequest4 = GetReturnRequest("rp_PayDate");
			fail.Code = HMNotifyState.Success;
			fail.Data = new HMOrder
			{
				OrderNo = returnRequest,
				SupplierOrderNo = returnRequest2,
				OrderAmt = Utils.StringToDecimal(returnRequest3, decimal.Zero) * 100m,
				OrderTime = Utils.StringToDateTime(returnRequest4, DateTime.Now).Value
			};
			return fail;
		}
	}
}
