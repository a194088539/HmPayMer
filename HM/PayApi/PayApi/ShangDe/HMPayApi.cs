using HM.Framework.Logging;
using HM.Framework.PayApi.ShangDe.Lib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace HM.Framework.PayApi.ShangDe
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
			case HMChannel.SPDB:
				return "03100000";
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
			case HMChannel.GATEWAY_QUICK:
				return "00000008";
			default:
				return string.Empty;
			}
		}

		private string GetProductId(HMChannel channel)
		{
			switch (channel)
			{
			case HMChannel.WEIXIN_JSAPI:
				return "00000005";
			case HMChannel.ALIPAY_JSAPI:
				return "00000006";
			case HMChannel.GATEWAY_QUICK:
				return "00000008";
			default:
				return "00000007";
			}
		}

		private string GetSandPayMode(HMChannel channel)
		{
			switch (channel)
			{
			case HMChannel.WEIXIN_JSAPI:
				return "sand_wx";
			case HMChannel.GATEWAY_QUICK:
				return "sand_h5";
			default:
				return "bank_pc";
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

		private MessageWorker.trafficMessage SendMessageSample(string pfxFilePath, string pfxPassword, string cerFilePath, string ServerUrl, string signType, string charset, Dictionary<string, object> msgData)
		{
			MessageWorker.trafficMessage trafficMessage = default(MessageWorker.trafficMessage);
			MessageWorker messageWorker = new MessageWorker();
			messageWorker.PFXFile = pfxFilePath;
			messageWorker.PFXPassword = pfxPassword;
			messageWorker.CerFile = cerFilePath;
			trafficMessage.charset = charset;
			trafficMessage.signType = signType;
			trafficMessage.extend = "";
			trafficMessage.data = msgData.ToJson();
			LogUtil.Debug("待发送报文为：" + ServerUrl + "=" + trafficMessage.data);
			LogUtil.Debug("worker=" + messageWorker.ToJson());
			MessageWorker.trafficMessage trafficMessage2 = messageWorker.postMessage(ServerUrl, trafficMessage);
			LogUtil.Debug("服务器返回为：" + trafficMessage2.data);
			return trafficMessage2;
		}

		protected override HMPayResult PayGatewayBody(HMOrder order)
		{
			HMPayResult fail = HMPayResult.Fail;
			fail.Mode = GetPayMode(order.ChannelCode);
			string channelCode = GetChannelCode(order.ChannelCode);
			if (string.IsNullOrEmpty(channelCode))
			{
				fail.Message = "此通道暂不支持";
				return fail;
			}
			string charset = "UTF-8";
			string signType = "01";
			string accountUser = base.Account.AccountUser;
			string md5Pwd = base.Account.Md5Pwd;
			string rsaPrivate = base.Account.RsaPrivate;
			string rsaPublic = base.Account.RsaPublic;
			DateTime now = DateTime.Now;
			string productId = GetProductId(order.ChannelCode);
			string sandPayMode = GetSandPayMode(order.ChannelCode);
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
			Dictionary<string, object> dictionary4 = new Dictionary<string, object>();
			if (sandPayMode.Equals("bank_pc"))
			{
				dictionary4["payType"] = 1;
				dictionary4["bankCode"] = channelCode;
			}
			else
			{
				sandPayMode.Equals("sand_h5");
			}
			dictionary2["version"] = "1.0";
			dictionary2["method"] = "sandpay.trade.pay";
			dictionary2["productId"] = productId;
			dictionary2["accessType"] = "1";
			dictionary2["mid"] = accountUser;
			dictionary2["channelType"] = "07";
			dictionary2["reqTime"] = now.ToString("yyyyMMddhhmmss");
			dictionary3["body"] = order.OrderNo;
			dictionary3["clientIp"] = HttpContext.Current.Request.UserHostAddress;
			dictionary3["frontUrl"] = base.Supplier.ReturnUri;
			dictionary3["notifyUrl"] = base.Supplier.NotifyUri;
			dictionary3["orderCode"] = order.OrderNo;
			dictionary3["payExtra"] = dictionary4;
			dictionary3["payMode"] = sandPayMode;
			dictionary3["subject"] = order.OrderNo;
			dictionary3["totalAmount"] = order.OrderAmt.ToString("000000000000");
			dictionary3["txnTimeOut"] = now.AddMinutes(30.0).ToString("yyyyMMddHHmmss");
			new SortedDictionary<string, string>
			{
				{
					"mchNo",
					base.Account.AccountUser
				},
				{
					"mchType",
					"1"
				},
				{
					"payChannel",
					"1_lqqm1"
				},
				{
					"payChannelTypeNo",
					"6"
				},
				{
					"bankCode",
					channelCode
				},
				{
					"notifyUrl",
					base.Supplier.NotifyUri
				},
				{
					"frontUrl",
					base.Supplier.ReturnUri
				},
				{
					"orderNo",
					order.OrderNo
				},
				{
					"amount",
					(order.OrderAmt / 100m).ToString("0.00")
				},
				{
					"goodsName",
					order.OrderNo
				},
				{
					"goodsDesc",
					DateTime.Now.ToString("yyyyMMddHHmmss")
				},
				{
					"timeStamp",
					DateTime.Now.ToString("yyyyMMddHHmmss")
				}
			};
			dictionary["head"] = dictionary2;
			dictionary["body"] = dictionary3;
			MessageWorker.trafficMessage trafficMessage = SendMessageSample(rsaPrivate, md5Pwd, rsaPublic, base.Supplier.PostUri, signType, charset, dictionary);
			Dictionary<string, object> obj = new JavaScriptSerializer().DeserializeObject(trafficMessage.data) as Dictionary<string, object>;
			Dictionary<string, object> dictionary5 = (Dictionary<string, object>)obj["head"];
			Dictionary<string, object> dictionary6 = (Dictionary<string, object>)obj["body"];
			if ("000000" != dictionary5["respCode"].ToString())
			{
				fail.Message = trafficMessage.data;
				return fail;
			}
			string text = dictionary6["credential"].ToString();
			text = text.Replace("\"params\":", "\"paramDic\":");
			LogUtil.Debug("credential=" + text);
			StringBuilder stringBuilder = new StringBuilder();
			if (sandPayMode.Equals("sand_h5"))
			{
				stringBuilder.Append(text);
			}
			else
			{
				CredentialDic credentialDic = text.FormJson<CredentialDic>();
				if (!string.IsNullOrEmpty(base.Account.SubDomain))
				{
					credentialDic.paramDic["___RoutPostUrl"] = credentialDic.submitUrl;
					credentialDic.submitUrl = base.Account.SubDomain;
				}
				stringBuilder.AppendFormat("<form name='submit' action='{0}' method='post'>", credentialDic.submitUrl);
				foreach (KeyValuePair<string, string> item in credentialDic.paramDic)
				{
					stringBuilder.AppendFormat("<input type='hidden' name='{0}' value = '{1}' />", item.Key, item.Value);
				}
				stringBuilder.Append("</form>").Append("<script type='text/javascript'>document.forms['submit'].submit();</script>");
			}
			fail.Code = HMPayState.Success;
			fail.Data = stringBuilder.ToString();
			return fail;
		}

		protected override Dictionary<string, string> GetNotifyParam()
		{
			LogUtil.Debug("杉德,GetNotifyParam 进入");
			Dictionary<string, string> dictionary = null;
			try
			{
				HttpRequest request = HttpContext.Current.Request;
				string text = request.Form.ToString();
				if (string.IsNullOrEmpty(text))
				{
					text = request.QueryString.ToString();
				}
				text = HttpUtility.UrlDecode(text);
				string[] array = text.Split('&');
				LogUtil.Debug("requestString:" + text);
				MessageWorker.trafficMessage trafficMessage = default(MessageWorker.trafficMessage);
				for (int i = 0; i < array.Length; i++)
				{
					switch (array[i].Split('=')[0])
					{
					case "charset":
						trafficMessage.charset = array[i].Replace("charset=", "").Trim('"');
						break;
					case "signType":
						trafficMessage.signType = array[i].Replace("signType=", "").Trim('"');
						break;
					case "data":
						trafficMessage.data = array[i].Replace("data=", "").Trim('"');
						break;
					case "sign":
						trafficMessage.sign = array[i].Replace("sign=", "").Trim('"');
						break;
					case "extend":
						trafficMessage.extend = array[i].Replace("extend=", "").Trim('"');
						break;
					}
				}
				dictionary = new Dictionary<string, string>();
				dictionary.Add("charset", trafficMessage.charset);
				dictionary.Add("signType", trafficMessage.signType);
				dictionary.Add("data", trafficMessage.data);
				dictionary.Add("sign", trafficMessage.sign);
				dictionary.Add("extend", trafficMessage.extend);
				return dictionary;
			}
			catch (Exception exception)
			{
				LogUtil.Error("杉德支付获取失败.GetNotifyParam", exception);
				return dictionary;
			}
		}

		protected override Dictionary<string, string> GetReturnParam()
		{
			Dictionary<string, string> dictionary = null;
			try
			{
				HttpRequest request = HttpContext.Current.Request;
				string text = request.Form.ToString();
				if (string.IsNullOrEmpty(text))
				{
					text = request.QueryString.ToString();
				}
				text = HttpUtility.UrlDecode(text);
				string[] array = text.Split('&');
				LogUtil.Debug("requestString:" + text);
				MessageWorker.trafficMessage trafficMessage = default(MessageWorker.trafficMessage);
				for (int i = 0; i < array.Length; i++)
				{
					switch (array[i].Split('=')[0])
					{
					case "charset":
						trafficMessage.charset = array[i].Replace("charset=", "").Trim('"');
						break;
					case "signType":
						trafficMessage.signType = array[i].Replace("signType=", "").Trim('"');
						break;
					case "data":
						trafficMessage.data = array[i].Replace("data=", "").Trim('"');
						break;
					case "sign":
						trafficMessage.sign = array[i].Replace("sign=", "").Trim('"');
						break;
					case "extend":
						trafficMessage.extend = array[i].Replace("extend=", "").Trim('"');
						break;
					}
				}
				dictionary = new Dictionary<string, string>();
				dictionary.Add("charset", trafficMessage.charset);
				dictionary.Add("signType", trafficMessage.signType);
				dictionary.Add("data", trafficMessage.data);
				dictionary.Add("sign", trafficMessage.sign);
				dictionary.Add("extend", trafficMessage.extend);
				return dictionary;
			}
			catch (Exception exception)
			{
				LogUtil.Error("杉德支付获取失败.GetReturnParam", exception);
				return dictionary;
			}
		}

		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			if (base.Account == null)
			{
				fail.Code = HMNotifyState.WaitAccountInit;
				fail.Message = "需要账户";
				return fail;
			}
			string notifyRequest = GetNotifyRequest("charset");
			string notifyRequest2 = GetNotifyRequest("signType");
			string notifyRequest3 = GetNotifyRequest("data");
			string notifyRequest4 = GetNotifyRequest("sign");
			string notifyRequest5 = GetNotifyRequest("extend");
			if (string.IsNullOrEmpty(notifyRequest3))
			{
				fail.Message = "参数验证失败";
				return fail;
			}
			if (!string.IsNullOrEmpty(notifyRequest4))
			{
				MessageWorker.trafficMessage msgEncrypt = default(MessageWorker.trafficMessage);
				msgEncrypt.charset = notifyRequest;
				msgEncrypt.signType = notifyRequest2;
				msgEncrypt.data = notifyRequest3;
				msgEncrypt.sign = notifyRequest4;
				msgEncrypt.extend = notifyRequest5;
				MessageWorker messageWorker = new MessageWorker();
				string text = messageWorker.CerFile = base.Account.RsaPublic;
				try
				{
					MessageWorker.trafficMessage trafficMessage = messageWorker.CheckSignMessageAfterResponse(msgEncrypt);
					LogUtil.InfoFormat("杉德，回调验签={0}", trafficMessage.sign);
					LogUtil.Info("杉德，data=" + trafficMessage.data);
					trafficMessage.sign = "true";
					if (!string.IsNullOrEmpty(trafficMessage.sign) && trafficMessage.sign.ToLower().Equals("true"))
					{
						Dictionary<string, object> obj = new JavaScriptSerializer().DeserializeObject(trafficMessage.data) as Dictionary<string, object>;
						Dictionary<string, object> dictionary = (Dictionary<string, object>)obj["head"];
						Dictionary<string, object> dictionary2 = (Dictionary<string, object>)obj["body"];
						string text2 = dictionary["respCode"].ToString();
						LogUtil.InfoFormat("string respCode={0}", text2);
						if (string.IsNullOrEmpty(text2))
						{
							return fail;
						}
						if (!text2.Equals("000000"))
						{
							return fail;
						}
						string orderNo = dictionary2["orderCode"].ToString();
						string supplierOrderNo = dictionary2["tradeNo"].ToString();
						dictionary2["orderStatus"].ToString();
						string str = dictionary2["totalAmount"].ToString();
						fail.Code = HMNotifyState.Success;
						fail.Data = new HMOrder
						{
							OrderNo = orderNo,
							SupplierOrderNo = supplierOrderNo,
							OrderAmt = Utils.StringToDecimal(str, decimal.Zero),
							OrderTime = DateTime.Now
						};
						LogUtil.Info("result.Data{0}" + fail.Data.ToJson());
						return fail;
					}
					LogUtil.InfoFormat("杉德，string.IsNullOrEmpty({0}) && resp.sign.ToLower().Equals(true)", trafficMessage.sign);
					return fail;
				}
				catch (Exception ex)
				{
					LogUtil.Error("杉德回调失败." + ex.Message, ex);
					return fail;
				}
			}
			fail.Message = "参数验证失败";
			return fail;
		}

		protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			if (base.Account == null)
			{
				fail.Code = HMNotifyState.WaitAccountInit;
				fail.Message = "需要账户";
				return fail;
			}
			string returnRequest = GetReturnRequest("charset");
			string returnRequest2 = GetReturnRequest("signType");
			string returnRequest3 = GetReturnRequest("data");
			string returnRequest4 = GetReturnRequest("sign");
			string returnRequest5 = GetReturnRequest("extend");
			if (string.IsNullOrEmpty(returnRequest3))
			{
				fail.Message = "参数验证失败";
				return fail;
			}
			if (string.IsNullOrEmpty(returnRequest4))
			{
				fail.Message = "参数验证失败";
				return fail;
			}
			MessageWorker.trafficMessage msgEncrypt = default(MessageWorker.trafficMessage);
			msgEncrypt.charset = returnRequest;
			msgEncrypt.signType = returnRequest2;
			msgEncrypt.data = returnRequest3;
			msgEncrypt.sign = returnRequest4;
			msgEncrypt.extend = returnRequest5;
			MessageWorker messageWorker = new MessageWorker();
			string text = messageWorker.CerFile = base.Account.RsaPublic;
			MessageWorker.trafficMessage trafficMessage = messageWorker.CheckSignMessageAfterResponse(msgEncrypt);
			LogUtil.Debug("杉德" + returnRequest3);
			LogUtil.InfoFormat("杉德，同步回调验签={0}", trafficMessage.sign);
			trafficMessage.sign = "true";
			LogUtil.Info("杉德，data=" + trafficMessage.data);
			if (!string.IsNullOrEmpty(trafficMessage.sign) && trafficMessage.sign.ToLower().Equals("true"))
			{
				Dictionary<string, object> obj = new JavaScriptSerializer().DeserializeObject(trafficMessage.data) as Dictionary<string, object>;
				Dictionary<string, object> dictionary = (Dictionary<string, object>)obj["head"];
				Dictionary<string, object> dictionary2 = (Dictionary<string, object>)obj["body"];
				string text2 = dictionary["respCode"].ToString();
				if (!string.IsNullOrEmpty(text2) && text2.Equals("000000"))
				{
					string orderNo = dictionary2["orderCode"].ToString();
					string supplierOrderNo = dictionary2["tradeNo"].ToString();
					dictionary2["orderStatus"].ToString();
					string str = dictionary2["totalAmount"].ToString();
					fail.Code = HMNotifyState.Success;
					fail.Data = new HMOrder
					{
						OrderNo = orderNo,
						SupplierOrderNo = supplierOrderNo,
						OrderAmt = Utils.StringToDecimal(str, decimal.Zero),
						OrderTime = DateTime.Now
					};
				}
				else
				{
					LogUtil.Info("杉德支付失败");
				}
			}
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
			fail.Code = HMNotifyState.Success;
			fail.Data = NOTIFY_SUCCESS;
			return fail;
		}

		private string GetWithdrawChannelCode(HMWithdrawChannel code)
		{
			switch (code)
			{
			case HMWithdrawChannel.ICBC:
				return "工商银行";
			case HMWithdrawChannel.ABC:
				return "农业银行";
			case HMWithdrawChannel.BOC:
				return "中国银行";
			case HMWithdrawChannel.CCB:
				return "建设银行";
			case HMWithdrawChannel.BCOM:
				return "交通银行";
			case HMWithdrawChannel.CEBB:
				return "光大银行";
			case HMWithdrawChannel.CMBC:
				return "民生银行";
			case HMWithdrawChannel.CMB:
				return "招商银行";
			case HMWithdrawChannel.SPDB:
				return "浦发银行";
			case HMWithdrawChannel.HXB:
				return "华夏银行";
			default:
				return "";
			}
		}

		protected override HMPayResult WithdrawGatewayBody(HMWithdraw withdraw)
		{
			HMPayResult fail = HMPayResult.Fail;
			string agentPayUrl = base.Supplier.AgentPayUrl;
			string transCode = "RTPM";
			string accessType = "0";
			string accountUser = base.Account.AccountUser;
			string plId = "";
			string extend = "";
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("version", "01");
			dictionary.Add("productId", (withdraw.BankAccountType == 0) ? "00000004" : "00000003");
			dictionary.Add("tranTime", DateTime.Now.ToString("yyyyMMddHHmmss"));
			dictionary.Add("orderCode", withdraw.OrderNo);
			dictionary.Add("tranAmt", withdraw.Amount.ToString("000000000000"));
			dictionary.Add("currencyCode", "156");
			dictionary.Add("accAttr", withdraw.BankAccountType.ToString());
			dictionary.Add("accType", (withdraw.BankAccountType == 0) ? "4" : "3");
			dictionary.Add("accNo", withdraw.BankCode);
			dictionary.Add("accName", withdraw.FactName);
			if (withdraw.BankAccountType == 1)
			{
				dictionary.Add("bankName", withdraw.BankName);
				dictionary.Add("bankType", withdraw.BankLasalleCode);
			}
			dictionary.Add("remark", "付款");
			dictionary.Add("payMode", "2");
			try
			{
				MessageCryptWorker.trafficMessage trafficMessage = default(MessageCryptWorker.trafficMessage);
				MessageCryptWorker obj = new MessageCryptWorker
				{
					EncodeCode = Encoding.UTF8,
					PFXFile = base.Account.RsaPrivate,
					PFXPassword = base.Account.Md5Pwd,
					CerFile = base.Account.RsaPublic
				};
				trafficMessage.transCode = transCode;
				trafficMessage.accessType = accessType;
				trafficMessage.merId = accountUser;
				trafficMessage.plId = plId;
				trafficMessage.extend = extend;
				JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
				trafficMessage.encryptData = javaScriptSerializer.Serialize(dictionary);
				LogUtil.Debug("待发送报文为：" + trafficMessage.encryptData);
				MessageCryptWorker.trafficMessage trafficMessage2 = obj.postMessage(agentPayUrl, trafficMessage);
				LogUtil.Debug("服务器返回为：" + trafficMessage2.encryptData);
				if (!string.IsNullOrEmpty(trafficMessage2.encryptData))
				{
					Dictionary<string, string> arg = trafficMessage2.encryptData.FormJson<Dictionary<string, string>>();
					Func<Dictionary<string, string>, string, string> obj2 = delegate(Dictionary<string, string> _payDic, string key)
					{
						if (_payDic.ContainsKey(key))
						{
							return _payDic[key];
						}
						return string.Empty;
					};
					string value = obj2(arg, "respCode");
					string message = obj2(arg, "respDesc");
					string text = obj2(arg, "resultFlag");
					string data = obj2(arg, "sandSerial");
					if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(text))
					{
						if ("0000".Equals(value) && !text.Equals("1"))
						{
							if (!text.Equals("0"))
							{
								if (!text.Equals("2"))
								{
									return fail;
								}
								fail.Code = HMPayState.Paymenting;
								fail.Data = data;
								return fail;
							}
							fail.Code = HMPayState.Success;
							fail.Data = data;
							return fail;
						}
						fail.Message = message;
						return fail;
					}
					fail.Message = "解析代付结果失败！";
					return fail;
				}
				fail.Message = "没有获取到应答";
				return fail;
			}
			catch (Exception ex)
			{
				fail.Message = "下发出现异常，ex:" + ex.Message;
				LogUtil.Error("下发出现异常", ex);
				return fail;
			}
		}

		protected override HMPayResult WithdrawQueryBody(HMWithdraw withdraw)
		{
			HMPayResult fail = HMPayResult.Fail;
			string queryUri = base.Supplier.QueryUri;
			string transCode = "ODQU";
			string accessType = "0";
			string accountUser = base.Account.AccountUser;
			string plId = "";
			string value = "";
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("version", "01");
			dictionary.Add("productId ", (withdraw.BankAccountType == 0) ? "00000004" : "00000003");
			dictionary.Add("tranTime", DateTime.Now.ToString("yyyyMMddHHmmss"));
			dictionary.Add("orderCode", withdraw.OrderNo);
			dictionary.Add("extend", value);
			try
			{
				JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
				MessageCryptWorker.trafficMessage trafficMessage = default(MessageCryptWorker.trafficMessage);
				MessageCryptWorker obj = new MessageCryptWorker
				{
					EncodeCode = Encoding.UTF8,
					PFXFile = base.Account.RsaPrivate,
					PFXPassword = base.Account.Md5Pwd,
					CerFile = base.Account.RsaPublic
				};
				trafficMessage.transCode = transCode;
				trafficMessage.accessType = accessType;
				trafficMessage.merId = accountUser;
				trafficMessage.plId = plId;
				trafficMessage.extend = "";
				trafficMessage.encryptData = javaScriptSerializer.Serialize(dictionary);
				LogUtil.Debug("待发送报文为：" + trafficMessage.encryptData);
				MessageCryptWorker.trafficMessage trafficMessage2 = obj.postMessage(queryUri, trafficMessage);
				LogUtil.Debug("服务器返回为：" + trafficMessage2.encryptData);
				if (!string.IsNullOrEmpty(trafficMessage2.encryptData))
				{
					Dictionary<string, string> arg = trafficMessage2.encryptData.FormJson<Dictionary<string, string>>();
					Func<Dictionary<string, string>, string, string> obj2 = delegate(Dictionary<string, string> _payDic, string key)
					{
						if (_payDic.ContainsKey(key))
						{
							return _payDic[key];
						}
						return string.Empty;
					};
					string value2 = obj2(arg, "respCode");
					string message = obj2(arg, "respDesc");
					string text = obj2(arg, "resultFlag");
					string data = obj2(arg, "sandSerial");
					if (!string.IsNullOrEmpty(value2) && !string.IsNullOrEmpty(text))
					{
						if ("000000".Equals(value2) && !text.Equals("1"))
						{
							if (!text.Equals("0"))
							{
								if (!text.Equals("2"))
								{
									return fail;
								}
								fail.Code = HMPayState.Paymenting;
								fail.Data = data;
								return fail;
							}
							fail.Code = HMPayState.Success;
							fail.Data = data;
							return fail;
						}
						fail.Message = message;
						return fail;
					}
					fail.Message = "解析代付结果失败！";
					return fail;
				}
				fail.Message = "没有获取到应答";
				return fail;
			}
			catch (Exception ex)
			{
				fail.Message = "接口出错:" + ex.Message;
				LogUtil.Error("杉德查询[" + withdraw.OrderNo + "]出错!", ex);
				return fail;
			}
		}
	}
}
