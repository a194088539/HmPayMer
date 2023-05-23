using HM.Framework.Logging;
using HM.Framework.PayApi.ZongHeDianShang.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;

namespace HM.Framework.PayApi.ZongHeDianShang
{
	public class HMPayApi : HMPayApiBase
	{
		public override string NOTIFY_SUCCESS => "success";

		public override string NOTIFY_FAIL => "fail";

		public override bool IsWithdraw => false;

		public override HMMode GetPayMode(HMChannel code)
		{
			if (code == HMChannel.WEIXIN_NATIVE || code == HMChannel.ALIPAY_NATIVE || code == HMChannel.ALIPAY_HB_NATIVE || code == HMChannel.GATEWAY_NATIVE)
			{
				return HMMode.跳转扫码页面;
			}
			return HMMode.输出字符串;
		}

		private string GetChannelCode(HMChannel channel)
		{
			switch (channel)
			{
			case HMChannel.GATEWAY_NATIVE:
				return "00000003";
			case HMChannel.GATEWAY_QUICK:
				return "00000001";
			case HMChannel.SPDB:
			case HMChannel.HXB:
			case HMChannel.SPABANK:
			case HMChannel.ECITIC:
			case HMChannel.CIB:
			case HMChannel.CEBB:
			case HMChannel.CMBC:
			case HMChannel.CMB:
			case HMChannel.BOC:
			case HMChannel.BCOM:
			case HMChannel.CCB:
			case HMChannel.ICBC:
			case HMChannel.ABC:
				return "00000002";
			default:
				return string.Empty;
			}
		}

		private string GetPostUri(HMChannel channel)
		{
			switch (channel)
			{
			case HMChannel.GATEWAY_NATIVE:
				return "api/pay/scanPay.html";
			case HMChannel.GATEWAY_QUICK:
				return "api/pay/h5QuickPay.html";
			case HMChannel.SPDB:
			case HMChannel.HXB:
			case HMChannel.SPABANK:
			case HMChannel.ECITIC:
			case HMChannel.CIB:
			case HMChannel.CEBB:
			case HMChannel.CMBC:
			case HMChannel.CMB:
			case HMChannel.BOC:
			case HMChannel.BCOM:
			case HMChannel.CCB:
			case HMChannel.ICBC:
			case HMChannel.ABC:
				return "api/pay/bankPay.html";
			default:
				return string.Empty;
			}
		}

		private Dictionary<string, string> toDictionary(string content)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(content);
			XmlNodeList childNodes = xmlDocument.SelectSingleNode("xml").ChildNodes;
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (XmlNode item in childNodes)
			{
				dictionary.Add(item.Name, item.InnerText);
			}
			return dictionary;
		}

		protected override HMPayResult PayGatewayBody(HMOrder order)
		{
			HMPayResult fail = HMPayResult.Fail;
			fail.Mode = GetPayMode(order.ChannelCode);
			string channelCode = GetChannelCode(order.ChannelCode);
			string text = base.Supplier.PostUri;
			if (!text.EndsWith("/"))
			{
				text += "/";
			}
			text += GetPostUri(order.ChannelCode);
			SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
			sortedDictionary.Add("version", "1.0.0");
			sortedDictionary.Add("charset", "utf-8");
			sortedDictionary.Add("signType", "MD5");
			sortedDictionary.Add("mchId", base.Account.AccountUser);
			sortedDictionary.Add("outTradeNo", order.OrderNo);
			sortedDictionary.Add("productCode", channelCode);
			sortedDictionary.Add("body", order.OrderNo);
			sortedDictionary.Add("amount", (order.OrderAmt / 100m).ToString("0.00"));
			sortedDictionary.Add("notifyUrl", base.Supplier.NotifyUri);
			sortedDictionary.Add("frontUrl", base.Supplier.ReturnUri);
			sortedDictionary.Add("nonce", DateTime.Now.ToString("yyyyMMddHHmmss"));
			switch (order.ChannelCode)
			{
			case HMChannel.GATEWAY_NATIVE:
				sortedDictionary.Add("clientIp", Utils.GetClientIp());
				sortedDictionary.Add("businessType", "UNION_SCAN");
				break;
			case HMChannel.GATEWAY_QUICK:
				sortedDictionary.Add("clientIp", Utils.GetClientIp());
				break;
			default:
				sortedDictionary.Add("bankCode", "01030000");
				sortedDictionary.Add("clientIp", Utils.GetClientIp());
				break;
			}
			StringBuilder stringBuilder = new StringBuilder();
			SortedDictionary<string, string>.Enumerator enumerator = sortedDictionary.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<string, string> current = enumerator.Current;
					if (!string.IsNullOrEmpty(current.Value))
					{
						stringBuilder.AppendFormat("&{0}={1}", current.Key, current.Value);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			stringBuilder.Remove(0, 1);
			string value = EncryUtils.MD5(stringBuilder.ToString() + $"&key={base.Account.Md5Pwd}", "UTF-8").ToUpper();
			switch (fail.Mode)
			{
			case HMMode.跳转链接:
			case HMMode.跳转扫码页面:
				try
				{
					sortedDictionary.Add("sign", value);
					string text2 = sortedDictionary.ToJson();
					string text3 = HttpService.Post(text, text2, "application/json", 6);
					LogUtil.DebugFormat("提交地址：{0}，提交内容：{1}, 提交结果：{2}", text, text2, text3);
					if (string.IsNullOrEmpty(text3))
					{
						fail.Message = "未获得接口数据!";
						return fail;
					}
					Dictionary<string, string> dictionary = text3.FormJson<Dictionary<string, string>>();
					string text4 = "";
					string message = "";
					string data = "";
					if (dictionary.ContainsKey("respCode"))
					{
						text4 = dictionary["respCode"];
					}
					if (dictionary.ContainsKey("respMsg"))
					{
						message = dictionary["respMsg"];
					}
					if (dictionary.ContainsKey("payUrl"))
					{
						data = dictionary["payUrl"];
					}
					if (!text4.Equals("0000"))
					{
						fail.Message = message;
						return fail;
					}
					fail.Code = HMPayState.Success;
					fail.Data = data;
					return fail;
				}
				catch (Exception ex)
				{
					fail.Message = "系统繁忙:" + ex.Message;
					LogUtil.Error("支付过程中出现错误", ex);
					return fail;
				}
			case HMMode.输出字符串:
			{
				sortedDictionary.Add("sign", value);
				StringBuilder stringBuilder2 = new StringBuilder();
				stringBuilder2.AppendFormat("<form name='submit' action='{0}' _input_charset='utf-8' method='post'>", text);
				enumerator = sortedDictionary.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<string, string> current2 = enumerator.Current;
						stringBuilder2.AppendFormat("<input type='hidden' name='{0}' value = '{1}' />", current2.Key, current2.Value);
					}
				}
				finally
				{
					((IDisposable)enumerator).Dispose();
				}
				stringBuilder2.Append("</form>").Append("<script type='text/javascript'>document.forms['submit'].submit();</script>");
				fail.Code = HMPayState.Success;
				fail.Data = stringBuilder2.ToString();
				break;
			}
			}
			return fail;
		}

		protected override Dictionary<string, string> GetNotifyParam()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			try
			{
				HttpContext.Current.Request.InputStream.Position = 0L;
				using (StreamReader streamReader = new StreamReader(HttpContext.Current.Request.InputStream))
				{
					string text = streamReader.ReadToEnd();
					LogUtil.InfoFormat("综合电商.Notify({0})", text);
					dictionary = text.FormJson<Dictionary<string, string>>();
					LogUtil.InfoFormat("综合电商.Notify.Dic({0})", dictionary.ToJson());
					return dictionary;
				}
			}
			catch (Exception exception)
			{
				LogUtil.Error("综合电商", exception);
				return dictionary;
			}
		}

		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string notifyRequest = GetNotifyRequest("orderNo");
			string notifyRequest2 = GetNotifyRequest("outTradeNo");
			string notifyRequest3 = GetNotifyRequest("amount");
			fail.Code = HMNotifyState.Success;
			fail.Data = new HMOrder
			{
				OrderNo = notifyRequest2,
				SupplierOrderNo = notifyRequest,
				OrderAmt = Utils.StringToDecimal(notifyRequest3, decimal.Zero) * 100m
			};
			return fail;
		}

		public override HMNotifyResult<string> NotifySign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			fail.Data = NOTIFY_FAIL;
			string notifyRequest = GetNotifyRequest("tradeStatus");
			string notifyRequest2 = GetNotifyRequest("sign");
			if (!notifyRequest.Equals("SUCCESS"))
			{
				string text = fail.Message = GetNotifyRequest("message");
			}
			else
			{
				SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
				foreach (KeyValuePair<string, string> notifyParam in base.NotifyParams)
				{
					sortedDictionary.Add(notifyParam.Key, notifyParam.Value);
				}
				StringBuilder stringBuilder = new StringBuilder();
				foreach (KeyValuePair<string, string> item in sortedDictionary)
				{
					if (!string.IsNullOrEmpty(item.Value) && !item.Key.Equals("charset") && !item.Key.Equals("version") && !item.Key.Equals("signType") && !item.Key.Equals("sign") && !item.Key.Equals("key"))
					{
						stringBuilder.AppendFormat("{0}={1}&", item.Key, item.Value);
					}
				}
				if (EncryUtils.MD5(stringBuilder.ToString() + "key=" + account.Md5Pwd, "UTF-8").ToUpper().Equals(notifyRequest2))
				{
					fail.Code = HMNotifyState.Success;
					fail.Data = NOTIFY_SUCCESS;
				}
			}
			return fail;
		}

		protected override Dictionary<string, string> GetReturnParam()
		{
			return new Dictionary<string, string>
			{
				{
					"version",
					Utils.GetRequest("version")
				},
				{
					"charset",
					Utils.GetRequest("charset")
				},
				{
					"signType",
					Utils.GetRequest("signType")
				},
				{
					"tradeStatus",
					Utils.GetRequest("tradeStatus")
				},
				{
					"message",
					Utils.GetRequest("message")
				},
				{
					"mchId",
					Utils.GetRequest("mchId")
				},
				{
					"orderNo",
					Utils.GetRequest("orderNo")
				},
				{
					"outTradeNo",
					Utils.GetRequest("outTradeNo")
				},
				{
					"amount",
					Utils.GetRequest("amount")
				},
				{
					"sign",
					Utils.GetRequest("sign")
				}
			};
		}

		protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string returnRequest = GetReturnRequest("orderNo");
			string returnRequest2 = GetReturnRequest("outTradeNo");
			string returnRequest3 = GetReturnRequest("amount");
			fail.Code = HMNotifyState.Success;
			fail.Data = new HMOrder
			{
				OrderNo = returnRequest2,
				SupplierOrderNo = returnRequest,
				OrderAmt = Utils.StringToDecimal(returnRequest3, decimal.Zero) * 100m
			};
			return fail;
		}

		public override HMNotifyResult<string> ResultSign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			fail.Data = NOTIFY_FAIL;
			string returnRequest = GetReturnRequest("tradeStatus");
			string returnRequest2 = GetReturnRequest("sign");
			if (!returnRequest.Equals("SUCCESS"))
			{
				string text = fail.Message = GetNotifyRequest("message");
			}
			else
			{
				SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
				foreach (KeyValuePair<string, string> notifyParam in base.NotifyParams)
				{
					sortedDictionary.Add(notifyParam.Key, notifyParam.Value);
				}
				StringBuilder stringBuilder = new StringBuilder();
				foreach (KeyValuePair<string, string> item in sortedDictionary)
				{
					if (!string.IsNullOrEmpty(item.Value) && !item.Key.Equals("sign") && !item.Key.Equals("key"))
					{
						stringBuilder.AppendFormat("{0}={1}&", item.Key, item.Value);
					}
				}
				if (EncryUtils.MD5(stringBuilder.ToString() + "key=" + account.Md5Pwd, "UTF-8").ToUpper().Equals(returnRequest2))
				{
					fail.Code = HMNotifyState.Success;
					fail.Data = NOTIFY_SUCCESS;
				}
			}
			return fail;
		}
	}
}
