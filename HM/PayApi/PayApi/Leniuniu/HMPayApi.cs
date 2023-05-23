using HM.Framework.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace HM.Framework.PayApi.Leniuniu
{
	public class HMPayApi : HMPayApiBase
	{
		public override string NOTIFY_SUCCESS => "success";

		public override string NOTIFY_FAIL => "fail";

		public override bool IsWithdraw => true;

		private string GetChannelCode(HMChannel channel)
		{
			switch (channel)
			{
			case HMChannel.WEIXIN_NATIVE:
				return "V1/weChatH5Pay";
			case HMChannel.WEIXIN_H5:
				return "V1/weChatH5Pay";
			case HMChannel.ALIPAY_NATIVE:
				return "V1/aliH5Pay";
			case HMChannel.ALIPAY_H5:
				return "V1/aliH5Pay";
			case HMChannel.QQPAY_NATIVE:
				return "V1/qqPay";
			case HMChannel.QQPAY_H5:
				return "V1/qqPay";
			case HMChannel.JD_NATIVE:
				return "V1/jingdong/pay";
			case HMChannel.JD_H5:
				return "V1/jingdong/pay";
			case HMChannel.SPDB:
			case HMChannel.HXB:
			case HMChannel.SPABANK:
			case HMChannel.ECITIC:
			case HMChannel.CIB:
			case HMChannel.CMBC:
			case HMChannel.CMB:
			case HMChannel.BOC:
			case HMChannel.BCOM:
			case HMChannel.CCB:
			case HMChannel.ICBC:
			case HMChannel.ABC:
				return "V1/unionYun/pay";
			case HMChannel.GATEWAY_QUICK:
				return "V1/swift/pay";
			default:
				return string.Empty;
			}
		}

		private string GetBankCode(HMChannel channel)
		{
			switch (channel)
			{
			case HMChannel.SPDB:
				return "SPDB";
			case HMChannel.HXB:
				return "HXB";
			case HMChannel.SPABANK:
				return "PINGAN";
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
			default:
				return string.Empty;
			}
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
			default:
				return HMMode.跳转链接;
			}
		}

		protected override HMPayResult PayGatewayBody(HMOrder order)
		{
			HMPayResult fail = HMPayResult.Fail;
			fail.Mode = GetPayMode(order.ChannelCode);
			string channelCode = GetChannelCode(order.ChannelCode);
			string accountUser = base.Account.AccountUser;
			string md5Pwd = base.Account.Md5Pwd;
			string value = "1.0";
			string notifyUri = base.Supplier.NotifyUri;
			string value2 = "";
			string value3 = "";
			string value4 = "";
			string text = (order.OrderAmt / 100m).ToString("0.00");
			string returnUri = base.Supplier.ReturnUri;
			string orderNo = order.OrderNo;
			string appId = base.Account.AppId;
			string orderNo2 = order.OrderNo;
			string text2 = order.OrderTime.ToString("yyyyMMddHHmmss");
			string value5 = EncryUtils.MD5(accountUser + md5Pwd + text2 + orderNo2 + text).ToUpper();
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("AppId", accountUser);
			dictionary.Add("version", value);
			dictionary.Add("return_url", notifyUri);
			dictionary.Add("device_info", value2);
			dictionary.Add("mch_app_name", value3);
			dictionary.Add("mch_app_id", value4);
			dictionary.Add("total_fee", text);
			dictionary.Add("front_skip_url", returnUri);
			dictionary.Add("subject", orderNo);
			dictionary.Add("body", appId);
			dictionary.Add("out_order_no", orderNo2);
			dictionary.Add("timestamp", text2);
			dictionary.Add("sign", value5);
			if (channelCode.Equals("V1/unionYun/pay"))
			{
				dictionary.Add("card_type", "1");
				dictionary.Add("user_type", "1");
				dictionary.Add("channel_type", "1");
				dictionary.Add("bank_code", "1");
			}
			StringBuilder stringBuilder = new StringBuilder();
			string text3 = base.Supplier.PostUri;
			if (!text3.EndsWith("/"))
			{
				text3 += "/";
			}
			text3 += channelCode;
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				if (!string.IsNullOrEmpty(item.Value))
				{
					stringBuilder.AppendFormat("&{0}={1}", item.Key, item.Value);
				}
			}
			stringBuilder.Remove(0, 1);
			try
			{
				string text4 = HttpUtils.SendRequest(text3, stringBuilder.ToString(), "POST", "UTF-8");
				LogUtil.DebugFormat("提交地址：{0}，提交内容：{1}, 提交结果：{2}", text3, stringBuilder, text4);
				if (string.IsNullOrEmpty(text4))
				{
					fail.Message = "未获得接口数据!";
					return fail;
				}
				LPayResult lPayResult = text4.FormJson<LPayResult>();
				if (lPayResult.code == 1)
				{
					fail.Code = HMPayState.Success;
					fail.Data = lPayResult.payUrl;
				}
				return fail;
			}
			catch (Exception exception)
			{
				fail.Message = "系统繁忙，请稍候再试！";
				LogUtil.Error("平安接口出错,订单号:" + order.OrderNo, exception);
				return fail;
			}
		}

		protected override Dictionary<string, string> GetNotifyParam()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string request = Utils.GetRequest("code");
			string request2 = Utils.GetRequest("message");
			string request3 = Utils.GetRequest("order_no");
			string request4 = Utils.GetRequest("out_order_no");
			string request5 = Utils.GetRequest("total_fee");
			string request6 = Utils.GetRequest("real_fee");
			string request7 = Utils.GetRequest("type");
			string request8 = Utils.GetRequest("discount_fee");
			string request9 = Utils.GetRequest("refund_fee");
			string request10 = Utils.GetRequest("time_end");
			string request11 = Utils.GetRequest("sign");
			dictionary.Add("code", request);
			dictionary.Add("message", request2);
			dictionary.Add("order_no", request3);
			dictionary.Add("out_order_no", request4);
			dictionary.Add("total_fee", request5);
			dictionary.Add("real_fee", request6);
			dictionary.Add("type", request7);
			dictionary.Add("discount_fee", request8);
			dictionary.Add("refund_fee", request9);
			dictionary.Add("time_end", request10);
			dictionary.Add("sign", request11);
			return dictionary;
		}

		protected override Dictionary<string, string> GetReturnParam()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string[] allKeys = HttpContext.Current.Request.Form.AllKeys;
			foreach (string text in allKeys)
			{
				string request = Utils.GetRequest(text);
				dictionary.Add(text, request);
			}
			return dictionary;
		}

		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string notifyRequest = GetNotifyRequest("out_order_no");
			string notifyRequest2 = GetNotifyRequest("order_no");
			string notifyRequest3 = GetNotifyRequest("total_fee");
			string notifyRequest4 = GetNotifyRequest("time_end");
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
				OrderAmt = Utils.StringToDecimal(notifyRequest3, decimal.Zero) * 100m,
				OrderTime = Utils.StringToDateTime(notifyRequest4, DateTime.Now).Value
			};
			return fail;
		}

		protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string returnRequest = GetReturnRequest("orderid");
			GetReturnRequest("transaction_id");
			string returnRequest2 = GetReturnRequest("amount");
			string returnRequest3 = GetReturnRequest("datetime");
			string notifyRequest = GetNotifyRequest("sign");
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
			if (string.IsNullOrEmpty(notifyRequest))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			fail.Code = HMNotifyState.Success;
			fail.Data = new HMOrder
			{
				OrderNo = "",
				SupplierOrderNo = returnRequest,
				OrderAmt = Utils.StringToDecimal(returnRequest2, decimal.Zero) * 100m,
				OrderTime = Utils.StringToDateTime(returnRequest3, DateTime.Now).Value
			};
			return fail;
		}

		public override HMNotifyResult<string> NotifySign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			string notifyRequest = GetNotifyRequest("code");
			string notifyRequest2 = GetNotifyRequest("order_no");
			string notifyRequest3 = GetNotifyRequest("total_fee");
			GetNotifyRequest("datetime");
			string notifyRequest4 = GetNotifyRequest("sign");
			string text = Utils.StringToDecimal(notifyRequest3, decimal.Zero).ToString("0.00");
			string text2 = EncryUtils.MD5(account.AccountUser + notifyRequest2 + account.Md5Pwd + text + notifyRequest).ToUpper();
			if (notifyRequest4.Equals(text2))
			{
				fail.Code = HMNotifyState.Success;
				fail.Data = NOTIFY_SUCCESS;
			}
			else
			{
				fail.Data = NOTIFY_FAIL;
				fail.Message = "签名失败,sign=" + notifyRequest4 + ",signData=" + text2;
			}
			return fail;
		}

		public override HMNotifyResult<string> ResultSign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			fail.Code = HMNotifyState.Success;
			fail.Data = NOTIFY_SUCCESS;
			return fail;
		}

		private string GetWithdrawChannelCode(HMWithdrawChannel code)
		{
			switch (code)
			{
			case HMWithdrawChannel.ICBC:
				return "01020000";
			case HMWithdrawChannel.BOC:
				return "01040000";
			case HMWithdrawChannel.CCB:
				return "01050000";
			case HMWithdrawChannel.ABC:
				return "01030000";
			case HMWithdrawChannel.CMB:
				return "03080000";
			case HMWithdrawChannel.PSBC:
				return "01000000";
			case HMWithdrawChannel.CEBB:
				return "03030000";
			case HMWithdrawChannel.SPDB:
				return "03100000";
			case HMWithdrawChannel.CIB:
				return "03090000";
			case HMWithdrawChannel.SPABANK:
				return "03070000";
			case HMWithdrawChannel.GDB:
				return "03060000";
			case HMWithdrawChannel.ECITIC:
				return "03020000";
			case HMWithdrawChannel.BCOM:
				return "03010000";
			case HMWithdrawChannel.CMBC:
				return "03050000";
			case HMWithdrawChannel.TZB:
				return "04593450";
			case HMWithdrawChannel.HZBANK:
				return "04233310";
			case HMWithdrawChannel.DRCBANK:
				return "14156020";
			case HMWithdrawChannel.HXB:
				return "03040000";
			case HMWithdrawChannel.北京银行:
				return "04031000";
			case HMWithdrawChannel.南京银行:
				return "04243010";
			case HMWithdrawChannel.东莞银行:
				return "04256020";
			default:
				return "";
			}
		}

		protected override HMPayResult WithdrawGatewayBody(HMWithdraw withdraw)
		{
			HMPayResult fail = HMPayResult.Fail;
			string withdrawChannelCode = GetWithdrawChannelCode(withdraw.WithdrawChannel);
			if (string.IsNullOrEmpty(withdrawChannelCode))
			{
				fail.Message = "代付银行不支持";
				return fail;
			}
			if (withdraw.BankAccountType != 2)
			{
				string accountUser = base.Account.AccountUser;
				string md5Pwd = base.Account.Md5Pwd;
				string value = "1.0";
				string text = DateTime.Now.ToString("yyyyMMddHHmmss");
				string value2 = EncryUtils.MD5(accountUser + md5Pwd + text + withdraw.OrderNo + (withdraw.Amount / 100m).ToString("0.00")).ToUpper();
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary.Add("app_id", accountUser);
				dictionary.Add("sign", value2);
				dictionary.Add("version", value);
				dictionary.Add("card_type", withdraw.BankAccountType.ToString());
				dictionary.Add("real_amt", (withdraw.Amount / 100m).ToString("0.00"));
				dictionary.Add("bank_code", withdrawChannelCode);
				dictionary.Add("bank_name", withdraw.BankName);
				dictionary.Add("bank_no", withdraw.BankCode);
				dictionary.Add("bank_acct_name", withdraw.FactName);
				dictionary.Add("brabank_name", withdraw.BankAddress);
				dictionary.Add("out_order_no", withdraw.OrderNo);
				dictionary.Add("timestamp", text);
				try
				{
					StringBuilder stringBuilder = new StringBuilder();
					foreach (KeyValuePair<string, string> item in dictionary)
					{
						if (!string.IsNullOrEmpty(item.Value))
						{
							stringBuilder.AppendFormat("&{0}={1}", item.Key, item.Value);
						}
					}
					stringBuilder.Remove(0, 1);
					string pubilcKey = EncryUtils.RSAPublicKeyJava2DotNet(base.Account.RsaPublic);
					string str = EncryUtils.RSAEncryptByPublicKey(stringBuilder.ToString(), pubilcKey);
					string text2 = HttpUtils.SendRequest(base.Supplier.AgentPayUrl, "app_id=" + accountUser + "&message=" + str, "POST", "UTF-8");
					LogUtil.Info("新支付宝代付接口httpResult：" + text2);
					NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(EncryUtils.DecryptPublicKeyJava(base.Account.RsaPublic, text2), Encoding.GetEncoding("gb2312"));
					if (!(nameValueCollection["status"] == "1"))
					{
						fail.Code = HMPayState.Fail;
						fail.Message = nameValueCollection["message"];
						return fail;
					}
					fail.Code = HMPayState.Paymenting;
					DfResult dfResult = nameValueCollection["obj"].FormJson<DfResult>();
					withdraw.ChannelOrderNo = dfResult.order_no;
					fail.Message = dfResult.message;
					return fail;
				}
				catch (Exception exception)
				{
					fail.Message = "系统繁忙，请稍候再试！";
					LogUtil.Error("新支付宝代付接口出错：", exception);
					return fail;
				}
			}
			fail.Message = "目前代付只支持对私账号";
			return fail;
		}

		protected override HMPayResult WithdrawQueryBody(HMWithdraw withdraw)
		{
			HMPayResult fail = HMPayResult.Fail;
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string accountUser = base.Account.AccountUser;
			string md5Pwd = base.Account.Md5Pwd;
			string text = DateTime.Now.ToString("yyyyMMddHHmmss");
			string value = EncryUtils.MD5(accountUser + md5Pwd + text + withdraw.OrderNo).ToUpper();
			dictionary.Add("app_id", accountUser);
			dictionary.Add("sign", value);
			dictionary.Add("order_no", withdraw.ChannelOrderNo);
			dictionary.Add("out_order_no", withdraw.OrderNo);
			dictionary.Add("timestamp", text);
			try
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (KeyValuePair<string, string> item in dictionary)
				{
					if (!string.IsNullOrEmpty(item.Value))
					{
						stringBuilder.AppendFormat("&{0}={1}", item.Key, item.Value);
					}
				}
				stringBuilder.Remove(0, 1);
				string pubilcKey = EncryUtils.RSAPublicKeyJava2DotNet(base.Account.RsaPublic);
				string str = EncryUtils.RSAEncryptByPublicKey(stringBuilder.ToString(), pubilcKey);
				string text2 = HttpUtils.SendRequest(base.Supplier.QueryUri, "?app_id=" + accountUser + "&message=" + str, "GET", "UTF-8");
				LogUtil.Info("新支付宝代付查询接口httpResult：" + text2);
				NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(EncryUtils.DecryptPublicKeyJava(base.Account.RsaPublic, text2), Encoding.GetEncoding("gb2312"));
				if (!(nameValueCollection["status"] == "1"))
				{
					fail.Code = HMPayState.Fail;
					fail.Message = nameValueCollection["message"];
					return fail;
				}
				fail.Code = HMPayState.Paymenting;
				DfsResult dfsResult = nameValueCollection["obj"].FormJson<DfsResult>();
				if (dfsResult.pay_status != 4)
				{
					if (dfsResult.pay_status != 5)
					{
						fail.Code = HMPayState.Paymenting;
						return fail;
					}
					fail.Code = HMPayState.Fail;
					return fail;
				}
				fail.Code = HMPayState.Success;
				return fail;
			}
			catch (Exception exception)
			{
				fail.Message = "系统繁忙，请稍候再试！";
				LogUtil.Error("云付宝代付查询接口出错：", exception);
				return fail;
			}
		}
	}
}
