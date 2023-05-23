using HM.Framework.Logging;
using HM.Framework.PayApi.Swiftpass.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace HM.Framework.PayApi.Swiftpass
{
	public class CiticBankPay : HMPayApiBase
	{
		public override string NOTIFY_SUCCESS => "SUCCESS";

		public override string NOTIFY_FAIL => "FAIL";

		public override bool IsWithdraw => true;

		private string GetChannelCode(HMChannel channel)
		{
			switch (channel)
			{
			default:
				if ((uint)(channel - 28) <= 1u)
				{
					return "pay.unionpay.native";
				}
				break;
			case HMChannel.WEIXIN_NATIVE:
				return "pay.weixin.native";
			case HMChannel.WEIXIN_JSAPI:
				return "pay.weixin.jspay";
			case HMChannel.WEIXIN_H5:
				return "pay.weixin.wappay";
			case HMChannel.ALIPAY_NATIVE:
			case HMChannel.ALIPAY_H5:
				return "pay.alipay.native";
			case HMChannel.QQPAY_NATIVE:
			case HMChannel.QQPAY_H5:
				return "pay.tenpay.native";
			case HMChannel.JD_NATIVE:
			case HMChannel.JD_H5:
				return "pay.jdpay.native";
			case HMChannel.WEIXIN_MICROPAY:
			case HMChannel.ALIPAY_MICROPAY:
				return "unified.trade.micropay";
			case HMChannel.ALIPAY_JSAPI:
			case HMChannel.QQPAY_APP:
				break;
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
			case HMChannel.GATEWAY_NATIVE:
				return HMMode.跳转扫码页面;
			case HMChannel.WEIXIN_H5:
			case HMChannel.ALIPAY_H5:
			case HMChannel.QQPAY_H5:
			case HMChannel.JD_H5:
			case HMChannel.GATEWAY_QUICK:
				return HMMode.跳转链接;
			case HMChannel.WEIXIN_MICROPAY:
			case HMChannel.ALIPAY_MICROPAY:
				return HMMode.输出Json;
			default:
				return HMMode.输出字符串;
			}
		}

		protected override HMPayResult PayGatewayBody(HMOrder order)
		{
			HMPayResult fail = HMPayResult.Fail;
			switch (order.ChannelCode)
			{
			case HMChannel.WEIXIN_MICROPAY:
			case HMChannel.ALIPAY_MICROPAY:
				fail.Code = HMPayState.Success;
				fail.Mode = HMMode.跳转扫码页面;
				fail.Data = base.Supplier.AuthUri;
				return fail;
			case HMChannel.WEIXIN_JSAPI:
			{
				fail.Code = HMPayState.Success;
				string arg = $"{order.OrderNo}_{order.OrderAmt}";
				string appId = base.Account.AppId;
				fail.Data = $"https://open.weixin.qq.com/connect/oauth2/authorize?appid={appId}&redirect_uri={HttpUtility.UrlEncode(base.Supplier.AuthUri)}&response_type=code&scope=snsapi_base&state={arg}";
				if (HttpContext.Current.Request.UserAgent.ToLower().Contains("micromessenger"))
				{
					fail.Mode = HMMode.跳转链接;
				}
				else
				{
					fail.Mode = HMMode.跳转扫码页面;
				}
				return fail;
			}
			default:
			{
				fail.Mode = GetPayMode(order.ChannelCode);
				string channelCode = GetChannelCode(order.ChannelCode);
				if (!string.IsNullOrEmpty(channelCode))
				{
					string text = base.Account.SubDomain;
					if (!string.IsNullOrEmpty(text))
					{
						if (!text.StartsWith("http"))
						{
							text = "http://" + text;
						}
						if (!text.EndsWith("/"))
						{
							text += "/";
						}
					}
					SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
					sortedDictionary.Add("out_trade_no", base.Order.OrderNo);
					sortedDictionary.Add("body", base.Order.OrderNo);
					sortedDictionary.Add("total_fee", base.Order.OrderAmt.ToString("0"));
					sortedDictionary.Add("mch_create_ip", base.Order.ClientIp);
					sortedDictionary.Add("service", channelCode);
					sortedDictionary.Add("mch_id", base.Account.AccountUser);
					sortedDictionary.Add("version", "2.0");
					sortedDictionary.Add("notify_url", base.Supplier.NotifyUri);
					sortedDictionary.Add("nonce_str", base.Order.OrderNo);
					sortedDictionary.Add("charset", "UTF-8");
					sortedDictionary.Add("sign_type", "RSA_1_256");
					if (order.ChannelCode == HMChannel.WEIXIN_H5)
					{
						sortedDictionary.Add("device_info", "iOS_WAP");
						sortedDictionary.Add("mch_app_name", "云70");
						sortedDictionary.Add("mch_app_id", "com.tencent.tmgp.sgame");
					}
					StringBuilder stringBuilder = new StringBuilder();
					foreach (KeyValuePair<string, string> item in sortedDictionary)
					{
						stringBuilder.AppendFormat("&{0}={1}", item.Key, item.Value);
					}
					stringBuilder.Remove(0, 1);
					string privateKey = EncryUtils.RSAPrivateKeyJava2DotNet(base.Account.RsaPrivate);
					string value = EncryUtils.RSASignByPrivateKey(stringBuilder.ToString(), privateKey, "SHA256");
					sortedDictionary.Add("sign", value);
					try
					{
						string text2 = Tools.toXml(sortedDictionary);
						string text3 = HttpService.Post(text2, base.Supplier.PostUri, isUseCert: false, 3600);
						LogUtil.DebugFormat("提交地址：{0}，提交内容：{1}, 提交结果：{2}", base.Supplier.PostUri, text2, text3);
						if (string.IsNullOrEmpty(text3))
						{
							fail.Message = "未获得接口数据!";
							return fail;
						}
						Dictionary<string, string> dictionary = Tools.toDictionary(text3);
						if (dictionary.ContainsKey("status") && dictionary.ContainsKey("result_code"))
						{
							string text4 = dictionary["status"].Trim();
							string text5 = dictionary["result_code"].Trim();
							string key = "code_url";
							string key2 = "err_msg";
							HMChannel channelCode2 = order.ChannelCode;
							if (channelCode2 == HMChannel.WEIXIN_H5)
							{
								key = "pay_info";
							}
							if (text4.Equals("0") && text5.Equals("0"))
							{
								string data = dictionary[key].Trim();
								if (order.ChannelCode == HMChannel.WEIXIN_H5 && !string.IsNullOrEmpty(text))
								{
									text = ((text.IndexOf('?') != -1) ? (text + "&") : (text + "?"));
									data = text + "url=" + HttpUtility.UrlEncode(fail.Data);
								}
								fail.Code = HMPayState.Success;
								fail.Data = data;
								return fail;
							}
							if (!dictionary.ContainsKey(key2))
							{
								return fail;
							}
							fail.Message = dictionary[key2];
							return fail;
						}
						fail.Message = text3;
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
			}
		}

		protected override Dictionary<string, string> GetNotifyParam()
		{
			Dictionary<string, string> dictionary = null;
			try
			{
				using (StreamReader streamReader = new StreamReader(HttpContext.Current.Request.InputStream))
				{
					string text = streamReader.ReadToEnd();
					LogUtil.InfoFormat("中信银行威付通.Notify({0})", text);
					dictionary = Tools.toDictionary(text);
					LogUtil.InfoFormat("中信银行威付通.Notify.Dic({0})", dictionary.ToJson());
					return dictionary;
				}
			}
			catch (Exception exception)
			{
				LogUtil.Error("中信银行威付通", exception);
				return dictionary;
			}
		}

		protected override Dictionary<string, string> GetReturnParam()
		{
			Dictionary<string, string> result = null;
			try
			{
				using (StreamReader streamReader = new StreamReader(HttpContext.Current.Request.InputStream))
				{
					string text = streamReader.ReadToEnd();
					LogUtil.InfoFormat("中信银行威付通.Return({0})", text);
					result = Tools.toDictionary(text);
					return result;
				}
			}
			catch (Exception exception)
			{
				LogUtil.Error("中信银行威付通", exception);
				return result;
			}
		}

		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			base.NotifyParams = dic;
			GetNotifyRequest("version");
			GetNotifyRequest("charset");
			GetNotifyRequest("sign_type");
			GetNotifyRequest("status");
			GetNotifyRequest("message");
			GetNotifyRequest("result_code");
			string notifyRequest = GetNotifyRequest("out_trade_no");
			string notifyRequest2 = GetNotifyRequest("total_fee");
			string notifyRequest3 = GetNotifyRequest("transaction_id");
			string notifyRequest4 = GetNotifyRequest("out_transaction_id");
			fail.Code = HMNotifyState.Success;
			fail.Data = new HMOrder
			{
				OrderNo = notifyRequest,
				SupplierOrderNo = notifyRequest3,
				TarnNo = notifyRequest4,
				OrderAmt = Utils.StringToDecimal(notifyRequest2, decimal.Zero)
			};
			return fail;
		}

		protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			GetReturnRequest("version");
			GetReturnRequest("charset");
			GetReturnRequest("sign_type");
			GetReturnRequest("status");
			GetReturnRequest("message");
			GetReturnRequest("result_code");
			string returnRequest = GetReturnRequest("out_trade_no");
			string returnRequest2 = GetReturnRequest("total_fee");
			string returnRequest3 = GetReturnRequest("transaction_id");
			string returnRequest4 = GetReturnRequest("out_transaction_id");
			fail.Code = HMNotifyState.Success;
			fail.Data = new HMOrder
			{
				OrderNo = returnRequest,
				SupplierOrderNo = returnRequest3,
				TarnNo = returnRequest4,
				OrderAmt = Utils.StringToDecimal(returnRequest2, decimal.Zero)
			};
			return fail;
		}

		public override HMNotifyResult<string> NotifySign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			string notifyRequest = GetNotifyRequest("sign");
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, string> notifyParam in base.NotifyParams)
			{
				if (!string.IsNullOrEmpty(notifyParam.Value) && !notifyParam.Key.Equals("sign") && !notifyParam.Key.Equals("key"))
				{
					stringBuilder.AppendFormat("&{0}={1}", notifyParam.Key, notifyParam.Value);
				}
			}
			stringBuilder.Remove(0, 1);
			string publicKey = EncryUtils.RSAPublicKeyJava2DotNet(account.RsaPublic);
			if (EncryUtils.RsaVerifyByPublicKey(stringBuilder.ToString(), publicKey, notifyRequest, "SHA256"))
			{
				fail.Code = HMNotifyState.Success;
				fail.Data = NOTIFY_SUCCESS;
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
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			string returnRequest = GetReturnRequest("sign");
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, string> returnParam in base.ReturnParams)
			{
				if (!string.IsNullOrEmpty(returnParam.Value) && !returnParam.Key.Equals("sign") && !returnParam.Key.Equals("key"))
				{
					stringBuilder.AppendFormat("&{0}={1}", returnParam.Key, returnParam.Value);
				}
			}
			stringBuilder.Remove(0, 1);
			string publicKey = EncryUtils.RSAPublicKeyJava2DotNet(account.RsaPublic);
			if (EncryUtils.RsaVerifyByPublicKey(stringBuilder.ToString(), publicKey, returnRequest, "SHA256"))
			{
				fail.Code = HMNotifyState.Success;
				fail.Data = NOTIFY_SUCCESS;
			}
			else
			{
				fail.Code = HMNotifyState.Fail;
				fail.Data = NOTIFY_FAIL;
			}
			return fail;
		}

		protected override HMPayResult QueryCallbackBody(HMOrder order)
		{
			HMPayResult fail = HMPayResult.Fail;
			fail.Code = HMPayState.Paymenting;
			string value = "unified.trade.query";
			SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
			sortedDictionary.Add("out_trade_no", base.Order.OrderNo);
			sortedDictionary.Add("body", base.Order.OrderNo);
			sortedDictionary.Add("service", value);
			sortedDictionary.Add("mch_id", base.Account.AccountUser);
			sortedDictionary.Add("version", "2.0");
			sortedDictionary.Add("nonce_str", base.Order.OrderNo);
			sortedDictionary.Add("charset", "UTF-8");
			sortedDictionary.Add("sign_type", "RSA_1_256");
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, string> item in sortedDictionary)
			{
				stringBuilder.AppendFormat("&{0}={1}", item.Key, item.Value);
			}
			stringBuilder.Remove(0, 1);
			string privateKey = EncryUtils.RSAPrivateKeyJava2DotNet(base.Account.RsaPrivate);
			string value2 = EncryUtils.RSASignByPrivateKey(stringBuilder.ToString(), privateKey, "SHA256");
			sortedDictionary.Add("sign", value2);
			try
			{
				string text = Tools.toXml(sortedDictionary);
				string text2 = HttpService.Post(text, base.Supplier.PostUri, isUseCert: false, 3600);
				LogUtil.DebugFormat("提交地址：{0}，提交内容：{1}, 提交结果：{2}", base.Supplier.PostUri, text, text2);
				if (string.IsNullOrEmpty(text2))
				{
					fail.Message = "未获得接口数据!";
					return fail;
				}
				Dictionary<string, string> dictionary = Tools.toDictionary(text2);
				if (dictionary.ContainsKey("status") && dictionary.ContainsKey("result_code"))
				{
					string text3 = dictionary["status"].Trim();
					string text4 = dictionary["result_code"].Trim();
					if (text3.Equals("0"))
					{
						if (!text4.Equals("0"))
						{
							if (dictionary.ContainsKey("need_query") && dictionary["need_query"].Equals("Y"))
							{
								fail.Code = HMPayState.PaymentingQueryResult;
								return fail;
							}
							fail.Message = dictionary["err_msg"];
							return fail;
						}
						string text5 = dictionary["trade_state"].Trim();
						if (!text5.Equals("SUCCESS"))
						{
							if (!text5.Equals("CLOSED") && !text5.Equals("REVOKED") && !text5.Equals("REVOKED"))
							{
								if (dictionary.ContainsKey("need_query") && !dictionary["need_query"].Equals("Y"))
								{
									return fail;
								}
								fail.Code = HMPayState.PaymentingQueryResult;
								return fail;
							}
							fail.Code = HMPayState.Fail;
							return fail;
						}
						fail.Code = HMPayState.Success;
						fail.Data = dictionary["transaction_id"].Trim();
						return fail;
					}
					fail.Message = dictionary["message"];
					return fail;
				}
				fail.Message = text2;
				return fail;
			}
			catch (Exception ex)
			{
				fail.Message = "系统繁忙:" + ex.Message;
				LogUtil.Error("中信银行威付通反扫支付查询过程中出现错误", ex);
				return fail;
			}
		}

		private string getWxOpenId(string code)
		{
			string text = HttpService.Get($"https://api.weixin.qq.com/sns/oauth2/access_token?appid={base.Account.OpenId}&secret={base.Account.OpenPwd}&code={code}&grant_type=authorization_code");
			if (!string.IsNullOrEmpty(text))
			{
				try
				{
					Dictionary<string, object> dictionary = text.FormJson<Dictionary<string, object>>();
					if (!dictionary.ContainsKey("openid"))
					{
						if (!dictionary.ContainsKey("errcode"))
						{
							return text;
						}
						if (!dictionary.ContainsKey("errmsg"))
						{
							return text;
						}
						text = "error:" + dictionary["errmsg"];
						return text;
					}
					text = dictionary["openid"].ToString().Trim();
					return text;
				}
				catch (Exception)
				{
					return text;
				}
			}
			return text;
		}

		public override HMPayResult AuthPayGateway(string code, HMOrder order)
		{
			HMPayResult fail = HMPayResult.Fail;
			if (string.IsNullOrEmpty(code))
			{
				fail.Message = "授权编码为空！";
				return fail;
			}
			string channelCode = GetChannelCode(order.ChannelCode);
			if (!string.IsNullOrEmpty(channelCode))
			{
				SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
				HMChannel channelCode2 = order.ChannelCode;
				if (channelCode2 == HMChannel.WEIXIN_JSAPI)
				{
					string wxOpenId = getWxOpenId(code);
					if (string.IsNullOrEmpty(wxOpenId))
					{
						fail.Message = "未能授权成功，请重新支付！";
						return fail;
					}
					if (wxOpenId.StartsWith("error:"))
					{
						fail.Message = wxOpenId;
						return fail;
					}
					sortedDictionary.Add("out_trade_no", base.Order.OrderNo);
					sortedDictionary.Add("body", base.Order.OrderNo);
					sortedDictionary.Add("total_fee", base.Order.OrderAmt.ToString("0"));
					sortedDictionary.Add("mch_create_ip", base.Order.ClientIp);
					sortedDictionary.Add("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
					sortedDictionary.Add("time_expire", DateTime.Now.AddMinutes(15.0).ToString("yyyyMMddHHmmss"));
					sortedDictionary.Add("service", channelCode);
					sortedDictionary.Add("mch_id", base.Account.AccountUser);
					sortedDictionary.Add("version", "2.0");
					sortedDictionary.Add("notify_url", base.Supplier.NotifyUri);
					sortedDictionary.Add("nonce_str", base.Order.OrderNo);
					sortedDictionary.Add("charset", "UTF-8");
					sortedDictionary.Add("sign_type", "RSA_1_256");
					sortedDictionary.Add("sub_openid", wxOpenId);
					sortedDictionary.Add("sub_appid", base.Account.AppId);
					sortedDictionary.Add("is_raw", "1");
				}
				else
				{
					sortedDictionary.Add("auth_code", code);
					sortedDictionary.Add("out_trade_no", base.Order.OrderNo);
					sortedDictionary.Add("body", base.Order.OrderNo);
					sortedDictionary.Add("total_fee", base.Order.OrderAmt.ToString("0"));
					sortedDictionary.Add("mch_create_ip", base.Order.ClientIp);
					sortedDictionary.Add("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
					sortedDictionary.Add("time_expire", DateTime.Now.AddMinutes(15.0).ToString("yyyyMMddHHmmss"));
					sortedDictionary.Add("service", channelCode);
					sortedDictionary.Add("mch_id", base.Account.AccountUser);
					sortedDictionary.Add("version", "2.0");
					sortedDictionary.Add("notify_url", base.Supplier.NotifyUri);
					sortedDictionary.Add("nonce_str", base.Order.OrderNo);
					sortedDictionary.Add("charset", "UTF-8");
					sortedDictionary.Add("sign_type", "RSA_1_256");
				}
				StringBuilder stringBuilder = new StringBuilder();
				foreach (KeyValuePair<string, string> item in sortedDictionary)
				{
					stringBuilder.AppendFormat("&{0}={1}", item.Key, item.Value);
				}
				stringBuilder.Remove(0, 1);
				string privateKey = EncryUtils.RSAPrivateKeyJava2DotNet(base.Account.RsaPrivate);
				string value = EncryUtils.RSASignByPrivateKey(stringBuilder.ToString(), privateKey, "SHA256");
				sortedDictionary.Add("sign", value);
				try
				{
					string text = Tools.toXml(sortedDictionary);
					string text2 = HttpService.Post(text, base.Supplier.PostUri, isUseCert: false, 3600);
					LogUtil.DebugFormat("提交地址：{0}，提交内容：{1}, 提交结果：{2}", base.Supplier.PostUri, text, text2);
					if (string.IsNullOrEmpty(text2))
					{
						fail.Message = "未获得接口数据!";
						return fail;
					}
					Dictionary<string, string> dictionary = Tools.toDictionary(text2);
					if (dictionary.ContainsKey("status") && dictionary.ContainsKey("result_code"))
					{
						string text3 = dictionary["status"].Trim();
						string text4 = dictionary["result_code"].Trim();
						if (order.ChannelCode != HMChannel.WEIXIN_JSAPI)
						{
							if (text3.Equals("0"))
							{
								if (!text4.Equals("0"))
								{
									if (dictionary.ContainsKey("need_query") && dictionary["need_query"].Equals("Y"))
									{
										fail.Code = HMPayState.PaymentingQueryResult;
										return fail;
									}
									fail.Message = dictionary["err_msg"];
									return fail;
								}
								fail.Code = HMPayState.Paymenting;
								if (dictionary.ContainsKey("need_query") && !dictionary["need_query"].Equals("Y"))
								{
									return fail;
								}
								fail.Code = HMPayState.PaymentingQueryResult;
								return fail;
							}
							fail.Message = dictionary["message"];
							return fail;
						}
						if (text3.Equals("0") && text4.Equals("0"))
						{
							fail.Code = HMPayState.Success;
							fail.Data = dictionary["pay_info"].Trim();
							return fail;
						}
						fail.Message = dictionary["err_msg"];
						return fail;
					}
					fail.Message = text2;
					return fail;
				}
				catch (Exception ex)
				{
					fail.Message = "系统繁忙:" + ex.Message;
					LogUtil.Error("中信银行威付通反扫支付过程中出现错误", ex);
					return fail;
				}
			}
			fail.Message = "此通道已关闭！";
			return fail;
		}
	}
}
