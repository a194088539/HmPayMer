using HM.Framework.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace HM.Framework.PayApi.ShangDeDeFeng
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
				fail.Message = "此通道暂不支持";
				return fail;
			}
			string postUri = base.Supplier.PostUri;
			SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
			sortedDictionary.Add("mchNo", base.Account.AccountUser);
			sortedDictionary.Add("mchType", "1");
			sortedDictionary.Add("payChannel", "3_bjgq");
			sortedDictionary.Add("payChannelTypeNo", "6");
			sortedDictionary.Add("bankCode", channelCode);
			sortedDictionary.Add("notifyUrl", base.Supplier.NotifyUri);
			sortedDictionary.Add("frontUrl", base.Supplier.ReturnUri);
			sortedDictionary.Add("orderNo", order.OrderNo);
			sortedDictionary.Add("amount", (order.OrderAmt / 100m).ToString("0.00"));
			sortedDictionary.Add("goodsName", order.OrderNo);
			sortedDictionary.Add("goodsDesc", DateTime.Now.ToString("yyyyMMddHHmmss"));
			sortedDictionary.Add("timeStamp", DateTime.Now.ToString("yyyyMMddHHmmss"));
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
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Remove(0, 1);
			}
			string value = EncryUtils.MD5(stringBuilder.ToString() + "&key=" + base.Account.Md5Pwd);
			sortedDictionary.Add("sign", value);
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.AppendFormat("<form name='submit' action='{0}' method='post'>", base.Supplier.PostUri);
			enumerator = sortedDictionary.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<string, string> current2 = enumerator.Current;
					stringBuilder2.AppendFormat("<input type='hidden' name='{0}' value = \"{1}\" />", current2.Key, current2.Value);
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			stringBuilder2.Append("</form>").Append("<script type='text/javascript'>document.forms['submit'].submit();</script>");
			fail.Code = HMPayState.Success;
			fail.Data = stringBuilder2.ToString();
			return fail;
		}

		protected override Dictionary<string, string> GetNotifyParam()
		{
			LogUtil.Debug("杉德德丰四方,GetNotifyParam 进入");
			string request = Utils.GetRequest("mchNo");
			string request2 = Utils.GetRequest("orderNo");
			string request3 = Utils.GetRequest("gwTradeNo");
			string request4 = Utils.GetRequest("amount");
			string request5 = Utils.GetRequest("status");
			string request6 = Utils.GetRequest("timeStamp");
			string request7 = Utils.GetRequest("sign");
			string request8 = Utils.GetRequest("result");
			string request9 = Utils.GetRequest("msg");
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("mchNo", request);
			dictionary.Add("orderNo", request2);
			dictionary.Add("gwTradeNo", request3);
			dictionary.Add("amount", request4);
			dictionary.Add("status", request5);
			dictionary.Add("timeStamp", request6);
			dictionary.Add("sign", request7);
			dictionary.Add("result", request8);
			dictionary.Add("msg", request9);
			LogUtil.Debug("杉德德丰四方,GetNotifyParam=" + dictionary.ToJson());
			return dictionary;
		}

		protected override Dictionary<string, string> GetReturnParam()
		{
			LogUtil.Debug("杉德德丰四方,GetReturnParam 进入");
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string request = Utils.GetRequest("mchNo");
			string request2 = Utils.GetRequest("orderNo");
			string request3 = Utils.GetRequest("gwTradeNo");
			string request4 = Utils.GetRequest("amount");
			string request5 = Utils.GetRequest("status");
			string request6 = Utils.GetRequest("timeStamp");
			string request7 = Utils.GetRequest("sign");
			string request8 = Utils.GetRequest("result");
			string request9 = Utils.GetRequest("msg");
			dictionary = new Dictionary<string, string>();
			dictionary.Add("mchNo", request);
			dictionary.Add("orderNo", request2);
			dictionary.Add("gwTradeNo", request3);
			dictionary.Add("amount", request4);
			dictionary.Add("status", request5);
			dictionary.Add("timeStamp", request6);
			dictionary.Add("sign", request7);
			dictionary.Add("result", request8);
			dictionary.Add("msg", request9);
			LogUtil.Debug("杉德德丰四方,GetReturnParam=" + dictionary.ToJson());
			return dictionary;
		}

		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string notifyRequest = GetNotifyRequest("mchNo");
			string notifyRequest2 = GetNotifyRequest("orderNo");
			string notifyRequest3 = GetNotifyRequest("gwTradeNo");
			string notifyRequest4 = GetNotifyRequest("amount");
			string notifyRequest5 = GetNotifyRequest("status");
			string notifyRequest6 = GetNotifyRequest("sign");
			if (string.IsNullOrEmpty(notifyRequest))
			{
				fail.Message = "参数验证失败";
				return fail;
			}
			if (string.IsNullOrEmpty(notifyRequest2))
			{
				fail.Message = "参数验证失败";
				return fail;
			}
			if (string.IsNullOrEmpty(notifyRequest3))
			{
				fail.Message = "参数验证失败";
				return fail;
			}
			if (string.IsNullOrEmpty(notifyRequest5))
			{
				fail.Message = "参数验证失败";
				return fail;
			}
			if (string.IsNullOrEmpty(notifyRequest6))
			{
				fail.Message = "参数验证失败";
				return fail;
			}
			fail.Code = HMNotifyState.Success;
			fail.Data = new HMOrder
			{
				OrderNo = notifyRequest2,
				SupplierOrderNo = notifyRequest3,
				OrderAmt = Utils.StringToDecimal(notifyRequest4, decimal.Zero) * 100m,
				OrderTime = DateTime.Now
			};
			return fail;
		}

		protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string returnRequest = GetReturnRequest("mchNo");
			string returnRequest2 = GetReturnRequest("orderNo");
			string returnRequest3 = GetReturnRequest("amount");
			string returnRequest4 = GetReturnRequest("result");
			string returnRequest5 = GetReturnRequest("msg");
			if (string.IsNullOrEmpty(returnRequest))
			{
				fail.Message = "参数验证失败";
				return fail;
			}
			if (string.IsNullOrEmpty(returnRequest2))
			{
				fail.Message = "参数验证失败";
				return fail;
			}
			if (string.IsNullOrEmpty(returnRequest4))
			{
				fail.Message = "参数验证失败";
				return fail;
			}
			if (string.IsNullOrEmpty(returnRequest5))
			{
				fail.Message = "参数验证失败";
				return fail;
			}
			fail.Code = HMNotifyState.Success;
			fail.Data = new HMOrder
			{
				OrderNo = returnRequest2,
				OrderAmt = Utils.StringToDecimal(returnRequest3, decimal.Zero) * 100m,
				OrderTime = DateTime.Now
			};
			return fail;
		}

		public override HMNotifyResult<string> NotifySign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			string notifyRequest = GetNotifyRequest("mchNo");
			string notifyRequest2 = GetNotifyRequest("orderNo");
			string notifyRequest3 = GetNotifyRequest("gwTradeNo");
			string notifyRequest4 = GetNotifyRequest("amount");
			string notifyRequest5 = GetNotifyRequest("status");
			string notifyRequest6 = GetNotifyRequest("timeStamp");
			string notifyRequest7 = GetNotifyRequest("sign");
			SortedDictionary<string, string> obj = new SortedDictionary<string, string>
			{
				{
					"mchNo",
					notifyRequest
				},
				{
					"orderNo",
					notifyRequest2
				},
				{
					"gwTradeNo",
					notifyRequest3
				},
				{
					"amount",
					notifyRequest4
				},
				{
					"status",
					notifyRequest5
				},
				{
					"timeStamp",
					notifyRequest6
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
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Remove(0, 1);
			}
			if (!EncryUtils.MD5(stringBuilder.ToString() + "&key=" + base.Account.Md5Pwd).Equals(notifyRequest7))
			{
				LogUtil.DebugFormat("杉德四方签名验证失败！");
				fail.Message = "签名验证失败！";
			}
			else
			{
				fail.Code = HMNotifyState.Success;
				fail.Data = NOTIFY_SUCCESS;
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
			if (!string.IsNullOrEmpty(GetWithdrawChannelCode(withdraw.WithdrawChannel)))
			{
				string agentPayUrl = base.Supplier.AgentPayUrl;
				SortedDictionary<string, string> obj = new SortedDictionary<string, string>
				{
					{
						"versionId",
						"1.0"
					},
					{
						"orderAmount",
						withdraw.Amount.ToString("0")
					},
					{
						"orderDate",
						withdraw.WithdrawTime.ToString("yyyyMMddHHmmss")
					},
					{
						"currency",
						"RMB"
					},
					{
						"asynNotifyUrl",
						base.Supplier.NotifyUri
					},
					{
						"signType",
						"MD5"
					},
					{
						"merId",
						base.Account.AccountUser
					},
					{
						"prdOrdNo",
						withdraw.OrderNo
					},
					{
						"receivableType",
						"D00"
					},
					{
						"isCompay",
						"0"
					},
					{
						"phoneNo",
						withdraw.MobilePhone
					},
					{
						"customerName",
						withdraw.FactName
					},
					{
						"cerdId",
						withdraw.IdCard
					},
					{
						"acctNo",
						withdraw.BankCode
					},
					{
						"rcvBranchCode",
						withdraw.BankCode
					},
					{
						"cityname",
						withdraw.City.Replace("市", "")
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
				string arg = EncryUtils.MD5(stringBuilder.ToString() + "&key=" + base.Account.Md5Pwd);
				string text = stringBuilder.AppendFormat("&signData={0}", arg).ToString();
				string text2 = HttpUtils.SendRequest(agentPayUrl, text, "POST", "UTF-8");
				LogUtil.DebugFormat("提交地址：{0}，提交内容：{1}, 提交结果：{2}", agentPayUrl, text, text2);
				try
				{
					WithdrawResult withdrawResult = text2.FormJson<WithdrawResult>();
					if (withdrawResult.retCode == 1)
					{
						fail.Code = HMPayState.Success;
						fail.Data = withdrawResult.prdOrdNo;
						withdraw.ChannelOrderNo = fail.Data;
						return fail;
					}
					fail.Message = withdrawResult.retMsg;
					return fail;
				}
				catch (Exception exception)
				{
					fail.Message = "系统繁忙，请稍候再试！";
					LogUtil.Error("通联下代付接口出错", exception);
					return fail;
				}
			}
			fail.Message = "接口不支持此银行";
			return fail;
		}

		protected override HMPayResult WithdrawQueryBody(HMWithdraw withdraw)
		{
			HMPayResult fail = HMPayResult.Fail;
			string queryUri = base.Supplier.QueryUri;
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
			string text6 = HttpUtils.SendRequest(base.Supplier.PostUri, text5, "POST", "UTF-8");
			LogUtil.DebugFormat("提交地址：{0}，提交内容：{1}, 提交结果：{2}", base.Supplier.QueryUri, text5, text6);
			try
			{
				WithdrawResult withdrawResult = text6.FormJson<WithdrawResult>();
				if (withdrawResult.retCode == 1)
				{
					fail.Code = HMPayState.Success;
					fail.Data = withdrawResult.prdOrdNo;
					return fail;
				}
				fail.Message = withdrawResult.retMsg;
				return fail;
			}
			catch (Exception exception)
			{
				fail.Message = "系统繁忙，请稍候再试！";
				LogUtil.Error("代付接口出错", exception);
				return fail;
			}
		}
	}
}
