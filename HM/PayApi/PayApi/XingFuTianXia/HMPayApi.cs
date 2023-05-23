using HM.Framework.Logging;
using HM.Framework.PayApi.XingFuTianXia.Lib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace HM.Framework.PayApi.XingFuTianXia
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
				return "SMZF016";
			case HMChannel.JD_NATIVE:
			case HMChannel.JD_H5:
				return "SMZF021";
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
			string appId = base.Account.AppId;
			string text = channelCode;
			string text2 = "RSA";
			string text3 = DateTime.Now.ToString("yyyyMMddHHmmss");
			string text4 = "";
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			HMChannel channelCode2 = order.ChannelCode;
			if ((uint)(channelCode2 - 9) > 1u)
			{
				if ((uint)(channelCode2 - 12) <= 1u)
				{
					dictionary.Add("merchantCode", base.Account.AccountUser);
					dictionary.Add("terminalCode", base.Account.ChildAccountUser);
					dictionary.Add("orderNum", order.OrderNo);
					dictionary.Add("transMoney", order.OrderAmt.ToString("0"));
					dictionary.Add("notifyUrl", base.Supplier.NotifyUri);
					dictionary.Add("merchantName", base.Account.BindDomain);
					dictionary.Add("merchantNum", base.Account.OpenId);
					dictionary.Add("terminalNum", base.Account.OpenPwd);
				}
			}
			else
			{
				dictionary.Add("merchantCode", base.Account.AccountUser);
				dictionary.Add("terminalCode", base.Account.ChildAccountUser);
				dictionary.Add("orderNum", order.OrderNo);
				dictionary.Add("transMoney", order.OrderAmt.ToString("0"));
				dictionary.Add("notifyUrl", base.Supplier.NotifyUri);
				dictionary.Add("merchantName", base.Account.BindDomain);
				dictionary.Add("merchantNum", base.Account.OpenId);
				dictionary.Add("terminalNum", base.Account.OpenPwd);
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				stringBuilder.AppendFormat("&{0}={1}", item.Key, item.Value);
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Remove(0, 1);
			}
			try
			{
				string pubilcKey = EncryUtils.RSAPublicKeyJava2DotNet(base.Account.RsaPublic);
				text4 = EncryUtils.RSAEncryptByPublicKey(stringBuilder.ToString(), pubilcKey);
				string text5 = $"groupId={appId}&service={text}&signType={text2}&sign={text4}&datetime={text3}";
				string text6 = HttpUtils.SendRequest(base.Supplier.PostUri, text5, "POST", "UTF-8");
				LogUtil.DebugFormat("提交地址：{0}，提交内容：{1}, 提交结果：{2}", base.Supplier.PostUri, text5, text6);
				LogUtil.Debug("signParam.ToString()=" + stringBuilder.ToString());
				if (string.IsNullOrEmpty(text6))
				{
					fail.Message = "未获得接口数据!";
					return fail;
				}
				XfResult xfResult = text6.FormJson<XfResult>();
				if (!(xfResult.pl_code != "0000"))
				{
					fail.Code = HMPayState.Error;
					LogUtil.DebugFormat("公钥：" + base.Account.RsaPublic);
					LogUtil.DebugFormat("加密字符串：" + xfResult.pl_sign);
					string text7 = EncryUtils.DecryptPublicKeyJava(base.Account.RsaPublic, xfResult.pl_sign);
					LogUtil.DebugFormat("解密字符串：" + text7);
					Dictionary<string, string> dictionary2 = XfData.FormString(text7);
					if (!dictionary2.ContainsKey("pl_url"))
					{
						LogUtil.DebugFormat("系统繁忙，获取支付链接失败");
						fail.Message = "系统繁忙，获取支付链接失败";
						return fail;
					}
					fail.Code = HMPayState.Success;
					fail.Data = dictionary2["pl_url"];
					if (fail.Mode == HMMode.跳转链接)
					{
						fail.Data = HttpUtility.UrlDecode(fail.Data);
					}
					LogUtil.DebugFormat("pl_url=：{0}", fail.Data);
					return fail;
				}
				fail.Message = xfResult.pl_message;
				return fail;
			}
			catch (Exception ex)
			{
				fail.Message = "系统繁忙:" + ex.Message;
				LogUtil.Error("支付过程中出现错误", ex);
				return fail;
			}
		}

		protected override Dictionary<string, string> GetNotifyParam()
		{
			Dictionary<string, string> dictionary = null;
			try
			{
				string request = Utils.GetRequest("pl_service");
				string request2 = Utils.GetRequest("pl_code");
				string request3 = Utils.GetRequest("pl_message");
				string request4 = Utils.GetRequest("pl_signType");
				string request5 = Utils.GetRequest("pl_datetime");
				string text = Utils.GetRequest("pl_sign").Replace(" ", "+").Replace(" ", "");
				dictionary = new Dictionary<string, string>();
				dictionary.Add("pl_service", request);
				dictionary.Add("pl_code", request2);
				dictionary.Add("pl_message", request3);
				dictionary.Add("pl_signType", request4);
				dictionary.Add("pl_datetime", request5);
				dictionary.Add("pl_sign", text);
				LogUtil.InfoFormat("pl_service={0}&pl_code={1}&pl_message={2}&pl_signType={3}&pl_datetime={4}&pl_sign={5}", request, request2, request3, request4, request5, text);
				return dictionary;
			}
			catch (Exception exception)
			{
				LogUtil.Error("行付天下获取失败.GetNotifyParam", exception);
				return dictionary;
			}
		}

		protected override Dictionary<string, string> GetReturnParam()
		{
			Dictionary<string, string> dictionary = null;
			try
			{
				string request = Utils.GetRequest("pl_service");
				string request2 = Utils.GetRequest("pl_code");
				string request3 = Utils.GetRequest("pl_message");
				string request4 = Utils.GetRequest("pl_signType");
				string request5 = Utils.GetRequest("pl_datetime");
				string value = Utils.GetRequest("pl_sign").Replace(" ", "+").Replace(" ", "");
				dictionary = new Dictionary<string, string>();
				dictionary.Add("pl_service", request);
				dictionary.Add("pl_code", request2);
				dictionary.Add("pl_message", request3);
				dictionary.Add("pl_signType", request4);
				dictionary.Add("pl_datetime", request5);
				dictionary.Add("pl_sign", value);
				return dictionary;
			}
			catch (Exception exception)
			{
				LogUtil.Error("行付天下获取失败.GetReturnParam", exception);
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
			base.NotifyParams = dic;
			GetNotifyRequest("pl_service");
			string notifyRequest = GetNotifyRequest("pl_code");
			string notifyRequest2 = GetNotifyRequest("pl_message");
			string notifyRequest3 = GetNotifyRequest("pl_sign");
			if (string.IsNullOrEmpty(notifyRequest))
			{
				fail.Message = "参数验证失败";
				return fail;
			}
			if (!notifyRequest.Equals("0000"))
			{
				fail.Message = notifyRequest2;
				return fail;
			}
			if (!string.IsNullOrEmpty(notifyRequest3))
			{
				try
				{
					LogUtil.DebugFormat("Account.RsaPublic={0}&pl_sign={1}", base.Account.RsaPublic, notifyRequest3);
					string text = EncryUtils.DecryptPublicKeyJava(base.Account.RsaPublic, notifyRequest3);
					if (!string.IsNullOrEmpty(text))
					{
						LogUtil.DebugFormat("解密签名：" + text);
						Dictionary<string, string> dictionary = XfData.FormString(text);
						if (dictionary == null)
						{
							return fail;
						}
						HMOrder hMOrder = new HMOrder();
						foreach (KeyValuePair<string, string> item in dictionary)
						{
							if (item.Key.ToLower().EndsWith("ordernum") && !item.Key.ToLower().Equals("pl_ordernum"))
							{
								hMOrder.OrderNo = item.Value.Trim();
							}
							else if (item.Key.ToLower().Equals("pl_ordernum"))
							{
								hMOrder.SupplierOrderNo = item.Value.Trim();
							}
							else if (item.Key.ToLower().Contains("pl_paystate"))
							{
								fail.Code = ((!item.Value.Equals("4")) ? HMNotifyState.Fail : HMNotifyState.Success);
							}
							else if (item.Key.ToLower().Contains("pl_paymessage"))
							{
								fail.Message = item.Value;
							}
						}
						LogUtil.DebugFormat("ApiNotifyState.Success");
						fail.Data = hMOrder;
						return fail;
					}
					LogUtil.DebugFormat("签名验证失败！");
					fail.Message = "签名验证失败！";
					return fail;
				}
				catch (Exception)
				{
					fail.Message = "系统繁忙";
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
			base.NotifyParams = dic;
			GetNotifyRequest("pl_service");
			string notifyRequest = GetNotifyRequest("pl_code");
			string notifyRequest2 = GetNotifyRequest("pl_message");
			string notifyRequest3 = GetNotifyRequest("pl_sign");
			if (string.IsNullOrEmpty(notifyRequest))
			{
				fail.Message = "参数验证失败";
				return fail;
			}
			if (!notifyRequest.Equals("0000"))
			{
				fail.Message = notifyRequest2;
				return fail;
			}
			if (!string.IsNullOrEmpty(notifyRequest3))
			{
				try
				{
					LogUtil.DebugFormat("Account.RsaPublic={0}&pl_sign={1}", base.Account.RsaPublic, notifyRequest3);
					string text = EncryUtils.DecryptPublicKeyJava(base.Account.RsaPublic, notifyRequest3);
					if (!string.IsNullOrEmpty(text))
					{
						LogUtil.DebugFormat("解密签名：" + text);
						Dictionary<string, string> dictionary = XfData.FormString(text);
						LogUtil.DebugFormat("解密后签名：" + dictionary);
						if (dictionary == null)
						{
							return fail;
						}
						HMOrder hMOrder = new HMOrder();
						foreach (KeyValuePair<string, string> item in dictionary)
						{
							if (item.Key.ToLower().EndsWith("ordernum") && !item.Key.ToLower().Equals("pl_ordernum"))
							{
								hMOrder.OrderNo = item.Value.Trim();
							}
							else if (item.Key.ToLower().Equals("pl_ordernum"))
							{
								hMOrder.SupplierOrderNo = item.Value.Trim();
							}
							else if (item.Key.ToLower().Contains("pl_paystate"))
							{
								fail.Code = ((!item.Equals("4")) ? HMNotifyState.Fail : HMNotifyState.Success);
							}
							else if (item.Key.ToLower().Contains("pl_paymessage"))
							{
								fail.Message = item.Value;
							}
						}
						LogUtil.DebugFormat("ApiNotifyState.Success");
						fail.Data = hMOrder;
						return fail;
					}
					LogUtil.DebugFormat("签名验证失败！");
					fail.Message = "签名验证失败！";
					return fail;
				}
				catch (Exception)
				{
					fail.Message = "系统繁忙";
					return fail;
				}
			}
			fail.Message = "参数验证失败";
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
			string withdrawChannelCode = GetWithdrawChannelCode(withdraw.WithdrawChannel);
			if (!string.IsNullOrEmpty(withdrawChannelCode))
			{
				string appId = base.Account.AppId;
				string text = string.IsNullOrEmpty(base.Account.Md5Pwd) ? "DF002" : base.Account.Md5Pwd;
				string text2 = "RSA";
				string text3 = DateTime.Now.ToString("yyyyMMddHHmmss");
				string text4 = "";
				Dictionary<string, string> obj = new Dictionary<string, string>
				{
					{
						"merchantCode",
						base.Account.AccountUser
					},
					{
						"terminalCode",
						base.Account.ChildAccountUser
					},
					{
						"orderNum",
						withdraw.OrderNo
					},
					{
						"transDate",
						withdraw.WithdrawTime.ToString("yyyyMMdd")
					},
					{
						"transTime",
						withdraw.WithdrawTime.ToString("HHmmss")
					},
					{
						"accountName",
						withdraw.FactName
					},
					{
						"bankCard",
						withdraw.BankCode
					},
					{
						"bankName",
						withdrawChannelCode
					},
					{
						"bankLinked",
						new Random().Next(100000, 999999).ToString()
					},
					{
						"transMoney",
						withdraw.Amount.ToString("0")
					}
				};
				StringBuilder stringBuilder = new StringBuilder();
				foreach (KeyValuePair<string, string> item in obj)
				{
					stringBuilder.AppendFormat("&{0}={1}", item.Key, item.Value);
				}
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Remove(0, 1);
				}
				string pubilcKey = EncryUtils.RSAPublicKeyJava2DotNet(base.Account.RsaPublic);
				LogUtil.DebugFormat("提交参数：{0}", stringBuilder);
				text4 = EncryUtils.RSAEncryptByPublicKey(stringBuilder.ToString(), pubilcKey);
				string text5 = $"groupId={appId}&service={text}&signType={text2}&sign={text4}&datetime={text3}";
				string text6 = HttpUtils.SendRequest(base.Supplier.AgentPayUrl, text5, "POST", "UTF-8");
				LogUtil.DebugFormat("提交地址：{0}，提交内容：{1}, 提交结果：{2}", base.Supplier.AgentPayUrl, text5, text6);
				try
				{
					XfResult xfResult = text6.FormJson<XfResult>();
					if (!(xfResult.pl_code != "0000"))
					{
						LogUtil.DebugFormat("公钥：" + base.Account.RsaPublic);
						LogUtil.DebugFormat("加密字符串：" + xfResult.pl_sign);
						fail.Code = HMPayState.Fail;
						string text7 = EncryUtils.DecryptPublicKeyJava(base.Account.RsaPublic, xfResult.pl_sign);
						LogUtil.DebugFormat("解密字符串：" + text7);
						Dictionary<string, string> dictionary = XfData.FormString(text7);
						if (dictionary.ContainsKey("pl_orderNum"))
						{
							fail.Data = dictionary["pl_orderNum"].Trim();
							withdraw.ChannelOrderNo = fail.Data;
						}
						if (dictionary.ContainsKey("pl_transState"))
						{
							string a = dictionary["pl_transState"].Trim();
							if (a == "1")
							{
								fail.Code = HMPayState.Success;
							}
							else if (a == "3")
							{
								fail.Code = HMPayState.Paymenting;
							}
						}
						if (!dictionary.ContainsKey("pl_transMessage"))
						{
							return fail;
						}
						fail.Message = dictionary["pl_transMessage"].ToString();
						return fail;
					}
					fail.Message = xfResult.pl_message;
					return fail;
				}
				catch (Exception exception)
				{
					fail.Message = "系统繁忙，请稍候再试！";
					LogUtil.Error("行付天下代付接口出错", exception);
					return fail;
				}
			}
			fail.Message = "代付银行不支持";
			return fail;
		}

		protected override HMPayResult WithdrawQueryBody(HMWithdraw withdraw)
		{
			HMPayResult fail = HMPayResult.Fail;
			string appId = base.Account.AppId;
			string text = "DF003";
			string text2 = "RSA";
			string text3 = DateTime.Now.ToString("yyyyMMddHHmmss");
			string text4 = "";
			Dictionary<string, string> obj = new Dictionary<string, string>
			{
				{
					"merchantCode",
					base.Account.AccountUser
				},
				{
					"orderNum",
					withdraw.OrderNo
				}
			};
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, string> item in obj)
			{
				stringBuilder.AppendFormat("&{0}={1}", item.Key, item.Value);
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Remove(0, 1);
			}
			string pubilcKey = EncryUtils.RSAPublicKeyJava2DotNet(base.Account.RsaPublic);
			LogUtil.DebugFormat("提交参数：{0}", stringBuilder);
			text4 = EncryUtils.RSAEncryptByPublicKey(stringBuilder.ToString(), pubilcKey);
			string text5 = $"groupId={appId}&service={text}&signType={text2}&sign={text4}&datetime={text3}";
			string text6 = HttpUtils.SendRequest(base.Supplier.QueryUri, text5, "POST", "UTF-8");
			LogUtil.DebugFormat("提交地址：{0}，提交内容：{1}, 提交结果：{2}", base.Supplier.QueryUri, text5, text6);
			try
			{
				XfResult xfResult = text6.FormJson<XfResult>();
				if (!(xfResult.pl_code == "0000"))
				{
					if (!(xfResult.pl_code == "2028"))
					{
						fail.Code = HMPayState.Paymenting;
						fail.Message = xfResult.pl_message;
						return fail;
					}
					fail.Message = xfResult.pl_message;
					return fail;
				}
				LogUtil.DebugFormat("公钥：" + base.Account.RsaPublic);
				LogUtil.DebugFormat("加密字符串：" + xfResult.pl_sign);
				fail.Code = HMPayState.Fail;
				string text7 = EncryUtils.DecryptPublicKeyJava(base.Account.RsaPublic, xfResult.pl_sign);
				LogUtil.DebugFormat("解密字符串：" + text7);
				Dictionary<string, string> dictionary = XfData.FormString(text7);
				if (dictionary.ContainsKey("pl_orderNum"))
				{
					fail.Data = dictionary["pl_orderNum"].Trim();
					withdraw.ChannelOrderNo = fail.Data;
				}
				if (dictionary.ContainsKey("pl_transState"))
				{
					string a = dictionary["pl_transState"].Trim();
					if (a == "1")
					{
						fail.Code = HMPayState.Success;
					}
					else if (a == "2")
					{
						fail.Code = HMPayState.Fail;
					}
					else
					{
						fail.Code = HMPayState.Paymenting;
					}
				}
				if (!dictionary.ContainsKey("pl_transMessage"))
				{
					return fail;
				}
				fail.Message = dictionary["pl_transMessage"].ToString();
				return fail;
			}
			catch (Exception exception)
			{
				fail.Message = "系统繁忙，请稍候再试！";
				LogUtil.Error("行付天下代付接口出错", exception);
				return fail;
			}
		}
	}
}
