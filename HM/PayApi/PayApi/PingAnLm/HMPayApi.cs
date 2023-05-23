using HM.Framework.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace HM.Framework.PayApi.PingAnLm
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
			case HMChannel.WEIXIN_JSAPI:
				return "weixin.mppay";
			case HMChannel.WEIXIN_NATIVE:
			case HMChannel.WEIXIN_H5:
				return "weixin.pay";
			case HMChannel.ALIPAY_NATIVE:
			case HMChannel.ALIPAY_H5:
				return "alipay.h5_pay";
			case HMChannel.GATEWAY_QUICK:
				return "unionpay.wap";
			case HMChannel.JD_NATIVE:
			case HMChannel.JD_H5:
				return "jd.pay";
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
				return "unionpay.gateway";
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
			case HMChannel.GATEWAY_QUICK:
				return HMMode.跳转链接;
			default:
				return HMMode.输出字符串;
			}
		}

		private string GetBankCodeByChannelCode(HMChannel channelCode)
		{
			switch (channelCode)
			{
			case HMChannel.SPDB:
				return "03100000";
			case HMChannel.HXB:
				return "03040000";
			case HMChannel.SPABANK:
				return "03070000";
			case HMChannel.ECITIC:
				return "03020000";
			case HMChannel.CIB:
				return "03090000";
			case HMChannel.CEBB:
				return "03030000";
			case HMChannel.CMBC:
				return "03050000";
			case HMChannel.CMB:
				return "03080000";
			case HMChannel.BOC:
				return "01040000";
			case HMChannel.BCOM:
				return "03010000";
			case HMChannel.CCB:
				return "01050000";
			case HMChannel.ICBC:
				return "01020000";
			case HMChannel.ABC:
				return "01030000";
			default:
				return "";
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
				string accountUser = base.Supplier.Account.AccountUser;
				string arg = channelCode;
				string arg2 = "MD5";
				string arg3 = "1.0";
				string text = "";
				string text2 = "";
				SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
				switch (order.ChannelCode)
				{
				case HMChannel.WEIXIN_JSAPI:
				{
					string str = $"{order.OrderNo}_{order.OrderAmt}";
					string appId = base.Account.AppId;
					fail.Code = HMPayState.Success;
					fail.Data = string.Format("http://hub.trd46.cn/wx/openid?callback={0}", HttpUtility.UrlEncode(base.Supplier.AuthUri + "?state=" + str));
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
				case HMChannel.WEIXIN_NATIVE:
					sortedDictionary.Add("merchant_no", base.Account.ChildAccountUser);
					sortedDictionary.Add("out_trade_no", order.OrderNo);
					sortedDictionary.Add("order_name", order.OrderNo);
					sortedDictionary.Add("total_amount", (order.OrderAmt / 100m).ToString("0.00"));
					sortedDictionary.Add("spbill_create_ip", order.ClientIp);
					sortedDictionary.Add("notify_url", base.Supplier.NotifyUri);
					sortedDictionary.Add("pay_uid", EncryUtils.MD5(order.ClientIp + DateTime.Now.ToString("yyyyMMddHH")).ToLower());
					break;
				case HMChannel.WEIXIN_H5:
					sortedDictionary.Add("merchant_no", base.Account.ChildAccountUser);
					sortedDictionary.Add("out_trade_no", order.OrderNo);
					sortedDictionary.Add("order_name", order.OrderNo);
					sortedDictionary.Add("total_amount", (order.OrderAmt / 100m).ToString("0.00"));
					sortedDictionary.Add("spbill_create_ip", order.ClientIp);
					sortedDictionary.Add("notify_url", base.Supplier.NotifyUri);
					sortedDictionary.Add("pay_uid", EncryUtils.MD5(order.ClientIp + DateTime.Now.ToString("yyyyMMddHH")).ToLower());
					break;
				case HMChannel.ALIPAY_NATIVE:
				case HMChannel.ALIPAY_H5:
					sortedDictionary.Add("merchant_no", base.Account.ChildAccountUser);
					sortedDictionary.Add("out_trade_no", order.OrderNo);
					sortedDictionary.Add("order_name", order.OrderNo);
					sortedDictionary.Add("total_amount", (order.OrderAmt / 100m).ToString("0.00"));
					sortedDictionary.Add("notify_url", base.Supplier.NotifyUri);
					break;
				case HMChannel.JD_NATIVE:
				case HMChannel.JD_H5:
					sortedDictionary.Add("merchant_no", base.Account.AccountUser);
					sortedDictionary.Add("out_trade_no", order.OrderNo);
					sortedDictionary.Add("order_name", order.OrderNo);
					sortedDictionary.Add("total_amount", (order.OrderAmt / 100m).ToString("0.00"));
					sortedDictionary.Add("order_type", "1");
					sortedDictionary.Add("notify_url", base.Supplier.NotifyUri);
					break;
				case HMChannel.QQPAY_NATIVE:
				case HMChannel.QQPAY_H5:
					sortedDictionary.Add("merchant_no", base.Account.AccountUser);
					sortedDictionary.Add("out_trade_no", order.OrderNo);
					sortedDictionary.Add("order_name", order.OrderNo);
					sortedDictionary.Add("spbill_create_ip", order.ClientIp);
					sortedDictionary.Add("total_amount", (order.OrderAmt / 100m).ToString("0.00"));
					sortedDictionary.Add("notify_url", base.Supplier.NotifyUri);
					break;
				case HMChannel.GATEWAY_QUICK:
					fail.Code = HMPayState.Success;
					fail.Data = base.Supplier.AuthUri;
					fail.Mode = HMMode.跳转扫码页面;
					return fail;
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
					sortedDictionary.Add("out_trade_no", order.OrderNo);
					sortedDictionary.Add("total_amount", (order.OrderAmt / 100m).ToString("0.00"));
					sortedDictionary.Add("channel_type", "07");
					sortedDictionary.Add("subjec", order.OrderNo);
					sortedDictionary.Add("body", "付款订单");
					sortedDictionary.Add("client_ip", Utils.GetClientIp());
					sortedDictionary.Add("pay_type", "3");
					sortedDictionary.Add("bank_code", GetBankCodeByChannelCode(order.ChannelCode));
					sortedDictionary.Add("notify_url", base.Supplier.NotifyUri);
					sortedDictionary.Add("success_url", base.Supplier.ReturnUri);
					break;
				}
				text = sortedDictionary.ToJson();
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendFormat("app_id={0}", accountUser).AppendFormat("&content={0}", text).AppendFormat("&method={0}", arg)
					.AppendFormat("&version={0}", arg3);
				text2 = EncryUtils.MD5(stringBuilder.ToString() + $"&key={base.Supplier.Account.Md5Pwd}", "UTF-8").ToLower();
				stringBuilder.AppendFormat("&sign_type={0}", arg2);
				stringBuilder.AppendFormat("&sign={0}", text2);
				string text3 = base.Account.SubDomain;
				if (!string.IsNullOrEmpty(text3))
				{
					if (!text3.StartsWith("http"))
					{
						text3 = "http://" + text3;
					}
					if (!text3.EndsWith("/"))
					{
						text3 += "/";
					}
				}
				try
				{
					string text4 = stringBuilder.ToString();
					string text5 = HttpUtils.SendRequest(base.Supplier.PostUri, text4, "POST", "UTF-8");
					LogUtil.DebugFormat("提交地址：{0}，提交内容：{1}, 提交结果：{2}", base.Supplier.PostUri, text4, text5);
					if (string.IsNullOrEmpty(text5))
					{
						fail.Message = "接口商返回结果为空！";
						return fail;
					}
					PaResult paResult = text5.FormJson<PaResult>();
					if (paResult != null)
					{
						if (!string.IsNullOrEmpty(paResult.pay_url))
						{
							if (!paResult.pay_url.StartsWith("http"))
							{
								paResult.pay_url = paResult.pay_url.Substring(paResult.pay_url.IndexOf("http"), paResult.pay_url.Length - paResult.pay_url.IndexOf("http"));
							}
							fail.Code = HMPayState.Success;
							fail.Data = paResult.pay_url;
							if (!string.IsNullOrEmpty(text3))
							{
								text3 = ((text3.IndexOf('?') != -1) ? (text3 + "&") : (text3 + "?"));
								fail.Data = text3 + "url=" + HttpUtility.UrlEncode(fail.Data);
							}
						}
						else if (!string.IsNullOrEmpty(paResult.code_url))
						{
							fail.Code = HMPayState.Success;
							fail.Data = paResult.code_url;
						}
						else if (!string.IsNullOrEmpty(paResult.qr_code))
						{
							fail.Code = HMPayState.Success;
							fail.Data = paResult.qr_code;
						}
					}
					if (fail.Code == HMPayState.Success)
					{
						return fail;
					}
					fail.Message = text5;
					return fail;
				}
				catch (Exception exception)
				{
					fail.Message = "系统繁忙，请稍候再试！";
					LogUtil.Error("平安接口出错,通道编码:" + channelCode, exception);
					return fail;
				}
			}
			fail.Message = "缺少必须参数，支付失败！";
			return fail;
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
				string accountUser = base.Supplier.Account.AccountUser;
				string arg = channelCode;
				string arg2 = "MD5";
				string arg3 = "1.0";
				string text = "";
				string text2 = "";
				SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
				switch (order.ChannelCode)
				{
				case HMChannel.WEIXIN_JSAPI:
					if (string.IsNullOrEmpty(code))
					{
						fail.Message = "未能授权成功，请重新支付！";
						return fail;
					}
					if (code.StartsWith("error:"))
					{
						fail.Message = code;
						return fail;
					}
					sortedDictionary.Add("merchant_no", base.Account.AccountUser);
					sortedDictionary.Add("out_trade_no", order.OrderNo);
					sortedDictionary.Add("order_name", order.OrderNo);
					sortedDictionary.Add("total_amount", (order.OrderAmt / 100m).ToString("0.00"));
					sortedDictionary.Add("sub_openid", code);
					sortedDictionary.Add("spbill_create_ip", Utils.GetClientIp());
					sortedDictionary.Add("notify_url", base.Supplier.NotifyUri);
					break;
				case HMChannel.GATEWAY_QUICK:
					fail.Mode = HMMode.跳转链接;
					sortedDictionary.Add("out_trade_no", order.OrderNo);
					sortedDictionary.Add("total_amount", (order.OrderAmt / 100m).ToString("0.00"));
					sortedDictionary.Add("channel_type", "07");
					sortedDictionary.Add("subject", order.OrderNo);
					sortedDictionary.Add("body", order.OrderNo);
					sortedDictionary.Add("client_ip", Utils.GetClientIp());
					sortedDictionary.Add("card_no", code);
					sortedDictionary.Add("notify_url", base.Supplier.NotifyUri);
					sortedDictionary.Add("success_url", base.Supplier.ReturnUri);
					break;
				}
				text = sortedDictionary.ToJson();
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendFormat("app_id={0}", accountUser).AppendFormat("&content={0}", text).AppendFormat("&method={0}", arg)
					.AppendFormat("&version={0}", arg3);
				text2 = EncryUtils.MD5(stringBuilder.ToString() + $"&key={base.Supplier.Account.Md5Pwd}").ToLower();
				stringBuilder.AppendFormat("&sign_type={0}", arg2);
				stringBuilder.AppendFormat("&sign={0}", text2);
				try
				{
					string text3 = stringBuilder.ToString();
					string text4 = HttpUtils.SendRequest(base.Supplier.PostUri, text3, "POST", "UTF-8");
					LogUtil.DebugFormat("提交地址：{0}，提交内容：{1}, 提交结果：{2}", base.Supplier.PostUri, text3, text4);
					if (string.IsNullOrEmpty(text4))
					{
						fail.Message = "接口商返回结果为空！";
						return fail;
					}
					PaResult paResult = text4.FormJson<PaResult>();
					if (paResult != null)
					{
						if (!string.IsNullOrEmpty(paResult.pay_url))
						{
							fail.Code = HMPayState.Success;
							fail.Data = paResult.pay_url;
						}
						else if (!string.IsNullOrEmpty(paResult.code_url))
						{
							fail.Code = HMPayState.Success;
							fail.Data = paResult.code_url;
						}
						else if (!string.IsNullOrEmpty(paResult.qr_code))
						{
							fail.Code = HMPayState.Success;
							fail.Data = paResult.qr_code;
						}
						else if (paResult.pay_info != null)
						{
							fail.Code = HMPayState.Success;
							fail.Data = paResult.pay_info.ToJson();
						}
					}
					if (fail.Code == HMPayState.Success)
					{
						return fail;
					}
					fail.Message = text4;
					return fail;
				}
				catch (Exception exception)
				{
					fail.Message = "系统繁忙，请稍候再试！";
					LogUtil.Error("平安接口出错,订单号:" + order.OrderNo, exception);
					return fail;
				}
			}
			fail.Message = "此通道已关闭！";
			return fail;
		}

		protected override Dictionary<string, string> GetNotifyParam()
		{
			LogUtil.InfoFormat("平安老马进入GetNotifyParam");
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			try
			{
				string request = Utils.GetRequest("out_trade_no");
				string request2 = Utils.GetRequest("trade_no");
				string request3 = Utils.GetRequest("total_amount");
				string request4 = Utils.GetRequest("bank_type");
				string request5 = Utils.GetRequest("cash_fee");
				string request6 = Utils.GetRequest("fee_type");
				string request7 = Utils.GetRequest("is_subscribe");
				string request8 = Utils.GetRequest("openid");
				string request9 = Utils.GetRequest("pay_time");
				string request10 = Utils.GetRequest("trade_type");
				string request11 = Utils.GetRequest("trade_status");
				string request12 = Utils.GetRequest("status");
				string request13 = Utils.GetRequest("desc");
				string request14 = Utils.GetRequest("sign_type");
				string request15 = Utils.GetRequest("sign");
				dictionary.Add("out_trade_no", request);
				dictionary.Add("trade_no", request2);
				dictionary.Add("total_amount", request3);
				dictionary.Add("bank_type", request4);
				dictionary.Add("cash_fee", request5);
				dictionary.Add("fee_type", request6);
				dictionary.Add("is_subscribe", request7);
				dictionary.Add("openid", request8);
				dictionary.Add("pay_time", request9);
				dictionary.Add("trade_type", request10);
				dictionary.Add("trade_status", request11);
				dictionary.Add("status", request12);
				dictionary.Add("desc", request13);
				dictionary.Add("sign_type", request14);
				dictionary.Add("sign", request15);
				LogUtil.Debug("平安银行老马H5.GetNotifyParam=" + dictionary.ToJson());
				return dictionary;
			}
			catch (Exception exception)
			{
				LogUtil.Error("平安银行老马H5", exception);
				return dictionary;
			}
		}

		protected override Dictionary<string, string> GetReturnParam()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			try
			{
				string request = Utils.GetRequest("out_trade_no");
				string request2 = Utils.GetRequest("trade_no");
				string request3 = Utils.GetRequest("total_amount");
				string request4 = Utils.GetRequest("bank_type");
				string request5 = Utils.GetRequest("cash_fee");
				string request6 = Utils.GetRequest("fee_type");
				string request7 = Utils.GetRequest("is_subscribe");
				string request8 = Utils.GetRequest("openid");
				string request9 = Utils.GetRequest("pay_time");
				string request10 = Utils.GetRequest("trade_type");
				string request11 = Utils.GetRequest("trade_status");
				string request12 = Utils.GetRequest("status");
				string request13 = Utils.GetRequest("desc");
				string request14 = Utils.GetRequest("sign_type");
				string request15 = Utils.GetRequest("sign");
				dictionary.Add("out_trade_no", request);
				dictionary.Add("trade_no", request2);
				dictionary.Add("total_amount", request3);
				dictionary.Add("bank_type", request4);
				dictionary.Add("cash_fee", request5);
				dictionary.Add("fee_type", request6);
				dictionary.Add("is_subscribe", request7);
				dictionary.Add("openid", request8);
				dictionary.Add("pay_time", request9);
				dictionary.Add("trade_type", request10);
				dictionary.Add("trade_status", request11);
				dictionary.Add("status", request12);
				dictionary.Add("desc", request13);
				dictionary.Add("sign_type", request14);
				dictionary.Add("sign", request15);
				return dictionary;
			}
			catch (Exception exception)
			{
				LogUtil.Error("平安银行老马H5", exception);
				return dictionary;
			}
		}

		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string notifyRequest = GetNotifyRequest("out_trade_no");
			string notifyRequest2 = GetNotifyRequest("trade_no");
			string notifyRequest3 = GetNotifyRequest("total_amount");
			fail.Code = HMNotifyState.Success;
			fail.Data = new HMOrder
			{
				OrderNo = notifyRequest,
				SupplierOrderNo = notifyRequest2,
				OrderAmt = Utils.StringToDecimal(notifyRequest3, decimal.Zero) * 100m
			};
			return fail;
		}

		protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string returnRequest = GetReturnRequest("out_trade_no");
			string returnRequest2 = GetReturnRequest("trade_no");
			string returnRequest3 = GetReturnRequest("total_amount");
			fail.Code = HMNotifyState.Success;
			fail.Data = new HMOrder
			{
				OrderNo = returnRequest,
				SupplierOrderNo = returnRequest2,
				OrderAmt = Utils.StringToDecimal(returnRequest3, decimal.Zero) * 100m
			};
			return fail;
		}

		public override HMNotifyResult<string> NotifySign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			fail.Data = NOTIFY_FAIL;
			string notifyRequest = GetNotifyRequest("out_trade_no");
			string notifyRequest2 = GetNotifyRequest("trade_no");
			string notifyRequest3 = GetNotifyRequest("total_amount");
			string notifyRequest4 = GetNotifyRequest("bank_type");
			string notifyRequest5 = GetNotifyRequest("cash_fee");
			string notifyRequest6 = GetNotifyRequest("fee_type");
			string notifyRequest7 = GetNotifyRequest("is_subscribe");
			string notifyRequest8 = GetNotifyRequest("openid");
			string notifyRequest9 = GetNotifyRequest("pay_time");
			string notifyRequest10 = GetNotifyRequest("trade_type");
			string notifyRequest11 = GetNotifyRequest("trade_status");
			string notifyRequest12 = GetNotifyRequest("status");
			string notifyRequest13 = GetNotifyRequest("desc");
			GetNotifyRequest("sign_type");
			string notifyRequest14 = GetNotifyRequest("sign");
			if (string.IsNullOrEmpty(notifyRequest))
			{
				fail.Message = "参数不正确";
				return fail;
			}
			if (string.IsNullOrEmpty(notifyRequest3))
			{
				fail.Message = "参数不正确";
				return fail;
			}
			if (string.IsNullOrEmpty(notifyRequest14))
			{
				fail.Message = "参数不正确";
				return fail;
			}
			SortedDictionary<string, string> obj = new SortedDictionary<string, string>
			{
				{
					"out_trade_no",
					notifyRequest
				},
				{
					"trade_no",
					notifyRequest2
				},
				{
					"total_amount",
					notifyRequest3
				},
				{
					"bank_type",
					notifyRequest4
				},
				{
					"cash_fee",
					notifyRequest5
				},
				{
					"fee_type",
					notifyRequest6
				},
				{
					"is_subscribe",
					notifyRequest7
				},
				{
					"openid",
					notifyRequest8
				},
				{
					"pay_time",
					notifyRequest9
				},
				{
					"trade_type",
					notifyRequest10
				},
				{
					"trade_status",
					notifyRequest11
				},
				{
					"status",
					notifyRequest12
				},
				{
					"desc",
					notifyRequest13
				}
			};
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, string> item in obj)
			{
				if (!string.IsNullOrEmpty(item.Value))
				{
					stringBuilder.AppendFormat("{0}={1}&", item.Key, item.Value);
				}
			}
			stringBuilder.AppendFormat("key={0}", supplier.Account.Md5Pwd);
			string text = EncryUtils.MD5(stringBuilder.ToString(), "UTF-8").ToLower();
			LogUtil.DebugFormat("sb={0} \n strSign={1} \n sign={2}", stringBuilder, text, notifyRequest14);
			if (text.Equals(notifyRequest14))
			{
				fail.Code = HMNotifyState.Success;
				fail.Data = NOTIFY_SUCCESS;
			}
			else
			{
				fail.Message = "签名验证失败";
			}
			return fail;
		}

		public override HMNotifyResult<string> ResultSign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			fail.Data = NOTIFY_FAIL;
			string returnRequest = GetReturnRequest("out_trade_no");
			string returnRequest2 = GetReturnRequest("trade_no");
			string returnRequest3 = GetReturnRequest("total_amount");
			string returnRequest4 = GetReturnRequest("bank_type");
			string returnRequest5 = GetReturnRequest("cash_fee");
			string returnRequest6 = GetReturnRequest("fee_type");
			string returnRequest7 = GetReturnRequest("is_subscribe");
			string returnRequest8 = GetReturnRequest("openid");
			string returnRequest9 = GetReturnRequest("pay_time");
			string returnRequest10 = GetReturnRequest("trade_type");
			string returnRequest11 = GetReturnRequest("trade_status");
			string returnRequest12 = GetReturnRequest("status");
			string returnRequest13 = GetReturnRequest("desc");
			GetReturnRequest("sign_type");
			string returnRequest14 = GetReturnRequest("sign");
			if (string.IsNullOrEmpty(returnRequest))
			{
				fail.Message = "参数不正确";
				return fail;
			}
			if (string.IsNullOrEmpty(returnRequest3))
			{
				fail.Message = "参数不正确";
				return fail;
			}
			if (string.IsNullOrEmpty(returnRequest14))
			{
				fail.Message = "参数不正确";
				return fail;
			}
			SortedDictionary<string, string> obj = new SortedDictionary<string, string>
			{
				{
					"out_trade_no",
					returnRequest
				},
				{
					"trade_no",
					returnRequest2
				},
				{
					"total_amount",
					returnRequest3
				},
				{
					"bank_type",
					returnRequest4
				},
				{
					"cash_fee",
					returnRequest5
				},
				{
					"fee_type",
					returnRequest6
				},
				{
					"is_subscribe",
					returnRequest7
				},
				{
					"openid",
					returnRequest8
				},
				{
					"pay_time",
					returnRequest9
				},
				{
					"trade_type",
					returnRequest10
				},
				{
					"trade_status",
					returnRequest11
				},
				{
					"status",
					returnRequest12
				},
				{
					"desc",
					returnRequest13
				}
			};
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, string> item in obj)
			{
				if (!string.IsNullOrEmpty(item.Value))
				{
					stringBuilder.AppendFormat("{0}={1}&", item.Key, item.Value);
				}
			}
			stringBuilder.AppendFormat("key={0}", supplier.Account.Md5Pwd);
			if (EncryUtils.MD5(stringBuilder.ToString(), "UTF-8").ToLower().Equals(returnRequest14))
			{
				fail.Code = HMNotifyState.Success;
				fail.Data = NOTIFY_SUCCESS;
			}
			else
			{
				fail.Message = "签名验证失败";
			}
			return fail;
		}

		protected override HMPayResult WithdrawGatewayBody(HMWithdraw withdraw)
		{
			HMPayResult fail = HMPayResult.Fail;
			string accountUser = base.Account.AccountUser;
			string arg = "unionpay.withdraw";
			string arg2 = "MD5";
			string arg3 = "1.0";
			string text = "";
			string text2 = "";
			text = new SortedDictionary<string, string>
			{
				{
					"out_trade_no",
					withdraw.OrderNo
				},
				{
					"amount",
					(withdraw.Amount / 100m).ToString("0.00")
				},
				{
					"bakn_account_no",
					withdraw.BankCode
				},
				{
					"bank_account_name",
					withdraw.FactName
				},
				{
					"bank_mobile",
					withdraw.MobilePhone
				},
				{
					"account_attr",
					withdraw.BankAccountType.ToString()
				},
				{
					"bank_code",
					withdraw.BankLasalleCode
				},
				{
					"bank_name",
					withdraw.BankName
				},
				{
					"remark",
					"银行转账"
				}
			}.ToJson();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("app_id={0}", accountUser).AppendFormat("&content={0}", text).AppendFormat("&method={0}", arg)
				.AppendFormat("&version={0}", arg3);
			text2 = EncryUtils.MD5(stringBuilder.ToString() + $"&key={base.Account.Md5Pwd}", "UTF-8").ToLower();
			stringBuilder.AppendFormat("&sign_type={0}", arg2);
			stringBuilder.AppendFormat("&sign={0}", text2);
			try
			{
				string text3 = stringBuilder.ToString();
				string text4 = HttpUtils.SendRequest(base.Supplier.AgentPayUrl, text3, "POST", "UTF-8");
				LogUtil.DebugFormat(" 平安代付 提交地址：{0}，提交内容：{1}, 提交结果：{2}", base.Supplier.AgentPayUrl, text3, text4);
				if (string.IsNullOrEmpty(text4))
				{
					fail.Message = "接口商返回结果为空！";
					return fail;
				}
				DfResult dfResult = text4.FormJson<DfResult>();
				if (!dfResult.error_code.Equals("0"))
				{
					fail.Code = HMPayState.Fail;
					fail.Message = dfResult.error_msg;
					return fail;
				}
				fail.Code = HMPayState.Paymenting;
				return fail;
			}
			catch (Exception exception)
			{
				fail.Message = "系统繁忙，请稍候再试！";
				LogUtil.Error("平安代付接口出错:", exception);
				return fail;
			}
		}

		protected override HMPayResult WithdrawQueryBody(HMWithdraw withdraw)
		{
			HMPayResult fail = HMPayResult.Fail;
			string accountUser = base.Account.AccountUser;
			string arg = "unionpay.withdraw_query";
			string arg2 = "MD5";
			string arg3 = "1.0";
			string text = "";
			string text2 = "";
			text = new SortedDictionary<string, string>
			{
				{
					"out_trade_no",
					withdraw.OrderNo
				}
			}.ToJson();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("app_id={0}", accountUser).AppendFormat("&content={0}", text).AppendFormat("&method={0}", arg)
				.AppendFormat("&version={0}", arg3);
			text2 = EncryUtils.MD5(stringBuilder.ToString() + $"&key={base.Account.Md5Pwd}", "UTF-8").ToLower();
			stringBuilder.AppendFormat("&sign_type={0}", arg2);
			stringBuilder.AppendFormat("&sign={0}", text2);
			try
			{
				string text3 = stringBuilder.ToString();
				string text4 = HttpUtils.SendRequest(base.Supplier.QueryUri, text3, "POST", "UTF-8");
				LogUtil.DebugFormat(" 平安代付 提交地址：{0}，提交内容：{1}, 提交结果：{2}", base.Supplier.QueryUri, text3, text4);
				if (string.IsNullOrEmpty(text4))
				{
					fail.Message = "接口商返回结果为空！";
					return fail;
				}
				DfSResult dfSResult = text4.FormJson<DfSResult>();
				withdraw.ChannelOrderNo = dfSResult.trade_no;
				if (dfSResult.trade_status != 1)
				{
					if (dfSResult.trade_status != 2)
					{
						fail.Code = HMPayState.Paymenting;
						fail.Message = dfSResult.remark;
						return fail;
					}
					fail.Code = HMPayState.Fail;
					fail.Message = dfSResult.remark;
					return fail;
				}
				fail.Code = HMPayState.Success;
				return fail;
			}
			catch (Exception exception)
			{
				fail.Message = "系统繁忙，请稍候再试！";
				LogUtil.Error("平安代付查询接口出错：", exception);
				return fail;
			}
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

		protected override HMPayResult QueryCallbackBody(HMOrder order)
		{
			HMPayResult fail = HMPayResult.Fail;
			string accountUser = base.Supplier.Account.AccountUser;
			string arg = "trade.query";
			string arg2 = "MD5";
			string arg3 = "1.0";
			string text = "";
			string text2 = "";
			text = new SortedDictionary<string, string>
			{
				{
					"out_trade_no",
					order.OrderNo
				}
			}.ToJson();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("app_id={0}", accountUser).AppendFormat("&content={0}", text).AppendFormat("&method={0}", arg)
				.AppendFormat("&version={0}", arg3);
			text2 = EncryUtils.MD5(stringBuilder.ToString() + $"&key={base.Supplier.Account.Md5Pwd}", "UTF-8").ToLower();
			stringBuilder.AppendFormat("&sign_type={0}", arg2);
			stringBuilder.AppendFormat("&sign={0}", text2);
			try
			{
				string text3 = stringBuilder.ToString();
				string text4 = HttpUtils.SendRequest(base.Supplier.QueryUri, text3, "POST", "UTF-8");
				LogUtil.DebugFormat("查询地址：{0}，提交内容：{1}, 提交结果：{2}", base.Supplier.QueryUri, text3, text4);
				if (string.IsNullOrEmpty(text4))
				{
					fail.Message = "接口商返回结果为空！";
					return fail;
				}
				PaResult paResult = text4.FormJson<PaResult>();
				if (paResult != null && string.IsNullOrEmpty(paResult.trade_status) && (paResult.trade_status.ToUpper().Equals("SUCCESS") || paResult.trade_status.ToUpper().Equals("00")))
				{
					if (!(paResult.total_amount * 100m == order.OrderAmt))
					{
						return fail;
					}
					fail.Code = HMPayState.Success;
					fail.Data = paResult.trade_no;
					return fail;
				}
				fail.Message = text4;
				return fail;
			}
			catch (Exception exception)
			{
				fail.Message = "系统繁忙，请稍候再试！";
				LogUtil.Error("平安接口出错,通道编码:" + order.OrderNo, exception);
				return fail;
			}
		}
	}
}
