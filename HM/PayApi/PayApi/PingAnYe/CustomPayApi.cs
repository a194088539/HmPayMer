using HM.Framework.Logging;
using HM.Framework.PayApi.PingAnYe.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace HM.Framework.PayApi.PingAnYe
{
	public class CustomPayApi : HMPayApiBase
	{
		public override string NOTIFY_SUCCESS => "SUCCESS";

		public override string NOTIFY_FAIL => "FAIL";

		public override bool IsWithdraw => true;

		private string GetChannelCode(HMChannel channel)
		{
			switch (channel)
			{
			case HMChannel.WEIXIN_H5:
				return "qrc/wxh5/order/v0";
			case HMChannel.ALIPAY_H5:
				return "qrc/alipayh5/order/v0";
			case HMChannel.QQPAY_H5:
				return "qrc/c2b/order/v0";
			case HMChannel.JD_H5:
				return "qrc/jdh5/order/v0";
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
			case HMChannel.WEIXIN_H5:
			case HMChannel.ALIPAY_H5:
			case HMChannel.QQPAY_H5:
			case HMChannel.JD_H5:
				return HMMode.跳转链接;
			default:
				return HMMode.输出字符串;
			}
		}

		protected override HMPayResult PayGatewayBody(HMOrder order)
		{
			HMPayResult fail = HMPayResult.Fail;
			fail.Mode = GetPayMode(order.ChannelCode);
			string channelCode = GetChannelCode(order.ChannelCode);
			if (string.IsNullOrEmpty(channelCode))
			{
				fail.Message = "此支付接口不支持此支付通道！";
				return fail;
			}
			if (base.Supplier.Account != null && !string.IsNullOrEmpty(base.Supplier.Account.AccountUser))
			{
				string text = base.Supplier.PostUri;
				if (!text.EndsWith("/"))
				{
					text += "/";
				}
				text += channelCode;
				string accountUser = base.Supplier.Account.AccountUser;
				string text2 = "";
				string text3 = "";
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				switch (order.ChannelCode)
				{
				case HMChannel.WEIXIN_H5:
					if (string.IsNullOrEmpty(order.Attach))
					{
						fail.Message = "必填参数未填，请填写订单备注！";
						return fail;
					}
					dictionary.Add("merchantId", order.Attach);
					dictionary.Add("wechatSubMerId", base.Account.AccountUser);
					dictionary.Add("orderId", order.OrderNo);
					dictionary.Add("orderDate", order.OrderTime.ToString("yyyyMMdd"));
					dictionary.Add("mediaType", "H5");
					dictionary.Add("mediaIdentify", base.Account.SubDomain);
					dictionary.Add("mediaName", order.OrderNo);
					dictionary.Add("goodsDesc", order.OrderNo);
					dictionary.Add("orderAmount", order.OrderAmt.ToString("0"));
					dictionary.Add("terminalIp", order.ClientIp);
					dictionary.Add("callBackUrl", base.Supplier.NotifyUri);
					break;
				case HMChannel.ALIPAY_H5:
				case HMChannel.JD_H5:
					dictionary.Add("merchantId", base.Account.AccountUser);
					dictionary.Add("orderId", order.OrderNo);
					dictionary.Add("orderDate", order.OrderTime.ToString("yyyyMMdd"));
					dictionary.Add("goodsDesc", order.OrderNo);
					dictionary.Add("orderAmount", order.OrderAmt.ToString("0"));
					dictionary.Add("callBackUrl", base.Supplier.NotifyUri);
					break;
				}
				text2 = dictionary.ToJson();
				text3 = EncryUtils.MD5_16(text2 + base.Supplier.Account.Md5Pwd);
				RequestBody obj = new RequestBody
				{
					platformId = accountUser,
					businessBody = text2,
					businessBodySign = text3
				};
				try
				{
					string text4 = obj.ToJson();
					string text5 = HttpService.Post(text4, text, isUseCert: false, 3600);
					LogUtil.DebugFormat("提交地址：{0}，提交内容：{1}, 提交结果：{2}", text, text4, text5);
					if (string.IsNullOrEmpty(text5))
					{
						fail.Message = "未获得接口数据!";
						return fail;
					}
					ResponseBody responseBody = text5.FormJson<ResponseBody>();
					if (!responseBody.code.Equals("200"))
					{
						fail.Message = responseBody.message;
						return fail;
					}
					Dictionary<string, string> dictionary2 = responseBody.data.FormJson<Dictionary<string, string>>();
					if (dictionary2 == null)
					{
						fail.Message = text5;
					}
					else if (dictionary2.ContainsKey("payStatus") && dictionary2["payStatus"].Equals("FAIL"))
					{
						if (dictionary2.ContainsKey("respMessage"))
						{
							fail.Message = dictionary2["respMessage"];
						}
						else
						{
							fail.Message = text5;
						}
					}
					else if (dictionary2.ContainsKey("wxPayUrl"))
					{
						fail.Code = HMPayState.Success;
						fail.Data = dictionary2["wxPayUrl"].Trim();
					}
					else if (dictionary2.ContainsKey("jdPayUrl"))
					{
						fail.Code = HMPayState.Success;
						fail.Data = dictionary2["jdPayUrl"].Trim();
					}
					else if (dictionary2.ContainsKey("alipayUrl"))
					{
						fail.Code = HMPayState.Success;
						fail.Data = dictionary2["alipayUrl"].Trim();
					}
					else
					{
						fail.Message = text5;
					}
					if (fail.Code != 0)
					{
						return fail;
					}
					if (fail.Mode != 0)
					{
						return fail;
					}
					if (string.IsNullOrEmpty(base.Account.SubDomain))
					{
						return fail;
					}
					string text6 = base.Account.SubDomain;
					if (!text6.StartsWith("http"))
					{
						text6 = "http://" + text6;
					}
					text6 = ((text6.IndexOf('?') != -1) ? (text6 + "&") : (text6 + "?"));
					fail.Data = text6 + "url=" + HttpUtility.UrlEncode(fail.Data);
					return fail;
				}
				catch (Exception ex)
				{
					fail.Message = "系统繁忙:" + ex.Message;
					LogUtil.Error("支付过程中出现错误", ex);
					return fail;
				}
			}
			fail.Message = "缺少必须参数，支付失败！";
			return fail;
		}

		protected override Dictionary<string, string> GetNotifyParam()
		{
			Dictionary<string, string> dictionary = null;
			try
			{
				HttpContext.Current.Request.InputStream.Position = 0L;
				using (StreamReader streamReader = new StreamReader(HttpContext.Current.Request.InputStream))
				{
					string text = streamReader.ReadToEnd();
					LogUtil.InfoFormat("平安银行叶总H5.Notify({0})", text);
					dictionary = text.FormJson<Dictionary<string, string>>();
					LogUtil.InfoFormat("平安银行叶总H5.Notify.Dic({0})", dictionary.ToJson());
					return dictionary;
				}
			}
			catch (Exception exception)
			{
				LogUtil.Error("平安银行叶总H5", exception);
				return dictionary;
			}
		}

		protected override Dictionary<string, string> GetReturnParam()
		{
			Dictionary<string, string> result = null;
			try
			{
				HttpContext.Current.Request.InputStream.Position = 0L;
				using (StreamReader streamReader = new StreamReader(HttpContext.Current.Request.InputStream))
				{
					string text = streamReader.ReadToEnd();
					LogUtil.InfoFormat("平安银行叶总H5.Return({0})", text);
					result = text.FormJson<Dictionary<string, string>>();
					return result;
				}
			}
			catch (Exception exception)
			{
				LogUtil.Error("平安银行叶总H5", exception);
				return result;
			}
		}

		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			base.NotifyParams = dic;
			string notifyRequest = GetNotifyRequest("orderId");
			string notifyRequest2 = GetNotifyRequest("systemOrderId");
			string notifyRequest3 = GetNotifyRequest("orderAmount");
			fail.Code = HMNotifyState.Success;
			fail.Data = new HMOrder
			{
				OrderNo = notifyRequest,
				SupplierOrderNo = notifyRequest2,
				OrderAmt = Utils.StringToDecimal(notifyRequest3, decimal.Zero)
			};
			return fail;
		}

		protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string returnRequest = GetReturnRequest("orderId");
			string returnRequest2 = GetReturnRequest("systemOrderId");
			string returnRequest3 = GetReturnRequest("orderAmount");
			fail.Code = HMNotifyState.Success;
			fail.Data = new HMOrder
			{
				OrderNo = returnRequest,
				SupplierOrderNo = returnRequest2,
				OrderAmt = Utils.StringToDecimal(returnRequest3, decimal.Zero)
			};
			return fail;
		}

		public override HMNotifyResult<string> NotifySign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			fail.Code = HMNotifyState.Success;
			fail.Data = NOTIFY_SUCCESS;
			return fail;
		}

		public override HMNotifyResult<string> ResultSign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			string returnRequest = GetReturnRequest("merchantId");
			string returnRequest2 = GetReturnRequest("orderDate");
			string returnRequest3 = GetReturnRequest("orderId");
			string returnRequest4 = GetReturnRequest("orderAmount");
			string returnRequest5 = GetReturnRequest("payStatus");
			string returnRequest6 = GetReturnRequest("systemOrderId");
			string md5Pwd = supplier.Account.Md5Pwd;
			string notifyRequest = GetNotifyRequest("sign");
			string value = EncryUtils.MD5_16($"{returnRequest}&{returnRequest2}&{returnRequest3}&{returnRequest4}&{returnRequest5}&{returnRequest6}{md5Pwd}");
			if (notifyRequest.Equals(value))
			{
				fail.Code = HMNotifyState.Success;
				fail.Data = NOTIFY_SUCCESS;
			}
			else
			{
				fail.Data = NOTIFY_FAIL;
				fail.Message = "签名失败！";
			}
			return fail;
		}

		private string GetQueryCode(HMChannel channel, string platformId)
		{
			switch (channel)
			{
			case HMChannel.WEIXIN_H5:
				return "qrc/wxh5/order/" + platformId;
			case HMChannel.ALIPAY_H5:
				return "qrc/alipayh5/order/" + platformId;
			case HMChannel.JD_H5:
				return "qrc/jdh5/order/" + platformId;
			default:
				return string.Empty;
			}
		}
	}
}
