using HM.Framework.Logging;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace HM.Framework.PayApi.ShouFuPay
{
	public class HMPayApi : HMPayApiBase
	{
		public override string NOTIFY_SUCCESS => "success";

		public override string NOTIFY_FAIL => "error";

		public override bool IsWithdraw => false;

		private string GetChannelCode(HMChannel channel)
		{
			switch (channel)
			{
			case HMChannel.WEIXIN_NATIVE:
				return "02";
			case HMChannel.WEIXIN_H5:
				return "08";
			case HMChannel.WEIXIN_JSAPI:
				return "04";
			case HMChannel.ALIPAY_NATIVE:
				return "03";
			case HMChannel.ALIPAY_H5:
				return "03";
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
			string rsaPrivate = base.Account.RsaPrivate;
			string rsaPublic = base.Account.RsaPublic;
			X509Certificate2 privateKeyXmlFromPFX = CryptUtils.getPrivateKeyXmlFromPFX(rsaPrivate, md5Pwd);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("backUrl", base.Supplier.NotifyUri);
			dictionary.Add("certId", "1526523864183656");
			dictionary.Add("merId", base.Account.ChildAccountUser);
			dictionary.Add("merchantNum", base.Account.AppId);
			dictionary.Add("orderId", order.OrderNo);
			dictionary.Add("productCode", "0202");
			dictionary.Add("reqReserved", "0202");
			dictionary.Add("signMethod", "01");
			dictionary.Add("txnAmt", order.OrderAmt.ToString("0"));
			dictionary.Add("txnSubType", channelCode);
			dictionary.Add("txnTime", order.OrderTime.ToString("yyyyMMddHHmmss"));
			dictionary.Add("txnType", "02");
			dictionary.Add("version", "1.0");
			switch (order.ChannelCode)
			{
			case HMChannel.WEIXIN_H5:
			{
				string value = "";
				value = ((!string.IsNullOrEmpty(value)) ? (base.Account.SubDomain.StartsWith("http") ? base.Account.SubDomain : ("http://" + base.Account.SubDomain)) : "http://pay.qq.com");
				dictionary.Add("sceneInfo", "Wap⼁在线支付⼁" + value);
				break;
			}
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				if (!string.IsNullOrEmpty(item.Value))
				{
					stringBuilder.AppendFormat("&{0}={1}", item.Key, item.Value);
				}
			}
			stringBuilder.Remove(0, 1);
			stringBuilder = new StringBuilder(string.Format("backUrl={0}&certId={1}&merId={2}&merchantNum={3}&orderId={4}&productCode=0202&signMethod=01&txnAmt={5}&txnSubType={6}&txnTime={7}&txnType=02&version=1.0", base.Supplier.NotifyUri, base.Account.ChildAccountUser, base.Account.AccountUser, base.Account.AppId, order.OrderNo, order.OrderAmt.ToString("0"), channelCode, DateTime.Now.ToString("yyyyMMddHHmmss")));
			string text = CryptUtils.CreateSignWithPrivateKey(stringBuilder.ToString(), privateKeyXmlFromPFX);
			dictionary.Add("signature", text);
			string text2 = stringBuilder.ToString() + $"&signature={text}";
			try
			{
				string text3 = HttpUtils.SendRequest(base.Supplier.PostUri, text2, "POST", "UTF-8");
				LogUtil.DebugFormat("提交地址：{0}，提交内容：{1}, 提交结果：{2}", base.Supplier.PostUri, text2, text3);
				if (string.IsNullOrEmpty(text3))
				{
					fail.Message = "未获得接口数据!";
					return fail;
				}
				SFPayResult sFPayResult = SFPayResult.FormStr(text3);
				if (string.IsNullOrWhiteSpace(sFPayResult.respCode))
				{
					fail.Message = text3;
					return fail;
				}
				if (sFPayResult.respCode.Equals("00") || sFPayResult.respCode.Equals("66"))
				{
					fail.Code = HMPayState.Success;
					fail.Data = sFPayResult.payInfo;
				}
				else
				{
					fail.Data = sFPayResult.respMsg;
				}
				return fail;
			}
			catch (Exception exception)
			{
				fail.Message = "系统繁忙，请稍候再试！";
				LogUtil.Error("收付宝接口出错,订单号:" + order.OrderNo, exception);
				return fail;
			}
		}

		protected override Dictionary<string, string> GetNotifyParam()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string[] allKeys = HttpContext.Current.Request.Form.AllKeys;
			foreach (string text in allKeys)
			{
				string request = Utils.GetRequest(text.ToString());
				dictionary.Add(text.ToString(), request);
			}
			LogUtil.Debug("dic=" + dictionary.ToJson());
			return dictionary;
		}

		protected override Dictionary<string, string> GetReturnParam()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string[] allKeys = HttpContext.Current.Request.QueryString.AllKeys;
			foreach (string text in allKeys)
			{
				string request = Utils.GetRequest(text.ToString());
				dictionary.Add(text.ToString(), request);
			}
			LogUtil.Debug("dic=" + dictionary.ToJson());
			return dictionary;
		}

		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string notifyRequest = GetNotifyRequest("orderId");
			string notifyRequest2 = GetNotifyRequest("queryId");
			string notifyRequest3 = GetNotifyRequest("txnAmt");
			string notifyRequest4 = GetNotifyRequest("signature");
			if (string.IsNullOrEmpty(notifyRequest))
			{
				fail.Message = "error";
				return fail;
			}
			if (string.IsNullOrEmpty(notifyRequest3))
			{
				fail.Message = "error";
				return fail;
			}
			if (string.IsNullOrEmpty(notifyRequest4))
			{
				fail.Message = "error";
				return fail;
			}
			fail.Code = HMNotifyState.Success;
			fail.Data = new HMOrder
			{
				OrderNo = notifyRequest,
				SupplierOrderNo = notifyRequest2,
				OrderAmt = Utils.StringToDecimal(notifyRequest3, decimal.Zero),
				OrderTime = DateTime.Now
			};
			return fail;
		}

		protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string returnRequest = GetReturnRequest("orderId");
			string returnRequest2 = GetReturnRequest("queryId");
			string returnRequest3 = GetReturnRequest("txnAmt");
			string returnRequest4 = GetReturnRequest("signature");
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
			if (string.IsNullOrEmpty(returnRequest4))
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
				OrderTime = DateTime.Now
			};
			return fail;
		}

		public override HMNotifyResult<string> NotifySign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			string notifyRequest = GetNotifyRequest("respCode");
			string notifyRequest2 = GetNotifyRequest("respMsg");
			if (notifyRequest == "66" || notifyRequest == "00")
			{
				fail.Code = HMNotifyState.Success;
				fail.Data = NOTIFY_SUCCESS;
			}
			else
			{
				fail.Data = NOTIFY_FAIL;
				fail.Message = notifyRequest2;
			}
			return fail;
		}

		public override HMNotifyResult<string> ResultSign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			string returnRequest = GetReturnRequest("respCode");
			string returnRequest2 = GetReturnRequest("respMsg");
			if (returnRequest == "66")
			{
				fail.Code = HMNotifyState.Success;
				fail.Data = NOTIFY_SUCCESS;
			}
			else
			{
				fail.Data = NOTIFY_FAIL;
				fail.Message = returnRequest2;
			}
			return fail;
		}
	}
}
