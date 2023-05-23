using HM.Framework.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace HM.Framework.PayApi.Alipay
{
	public class HMF2FPayApi : HMPayApiBase
	{
		public override string NOTIFY_SUCCESS => "success";

		public override string NOTIFY_FAIL => "fail";

		public override bool IsWithdraw => false;


        public string _charset = "GBK";

        public string _ALRSA_PUBLIC = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAvqWbylqJsa0bUbEVePkmWKNFLyqO7K9dv3bPnS5yrcmOb3+XBBiu/7TuGC4s9+g5Q5rWnFffOqpj2CPNWNNymRkFFDIcKIn0pnEBRS+A9AB/xQGSq7rExTfgU0YCMex7o/7jsZn2ppZBzuRTiaSg6tStfwuAZbEChxk3rJcvHTdgs26onvMW7sovq0kXWJN5QZSD3h5zatRCdRZiFi3eu7CgYP3KQgzcz2nvc1TfQzqkTP9Zq0zvUSOCubHYYQKq3w3PfilELFbNhBYL1OdBUtniOHwIG9Q/Zjtjap54hGABMC0w8hj/qQ211le5DghzLxflpTmrRfyWzTiHx1Sa6wIDAQAB";

		private string GetChannelCode(HMChannel channel)
		{
			if (channel == HMChannel.ALIPAY_F2F)
			{
				return "alipay.trade.precreate";
			}
			return string.Empty;
		}

		public override HMMode GetPayMode(HMChannel code)
		{
			return HMMode.输出字符串;
		}

		protected override HMPayResult PayGatewayBody(HMOrder order)
		{
			HMPayResult fail = HMPayResult.Fail;
			fail.Mode = GetPayMode(order.ChannelCode);
			string channelCode = GetChannelCode(order.ChannelCode);
			if (!string.IsNullOrEmpty(channelCode))
			{
				string orderNo = order.OrderNo;
				string value = "面对面支付";
				string value2 = (order.OrderAmt / 100m).ToString("#0.00");
				string value3 = "";
				string value4 = "";
                Dictionary<string, string> biz_content = new Dictionary<string, string>
                {
                    {
                        "out_trade_no",
                        orderNo
                    },
                    {
                        "total_amount",
                        value2
                    },
                    {
                        "subject",
                        value
                    },
                    {
                        "qr_code_timeout_express",
                        "90m"
                    }
                };

                Dictionary<string, string> obj = new Dictionary<string, string>
				{
					{
						"seller_id",
						base.Account.AccountUser
					},
					{
                        "charset",
                        _charset
                    },
					{
                        "method",
						channelCode
					},
					{
						"sign_type",
                        "RSA2"
                    },
                    {
                        "format",
                        "json"
                    },
                    {
                        "timestamp",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    },
                    {
                        "version",
                        "1.0"
                    },
                    {
                        "app_id",
                        base.Account.AppId
                    },
                    {
						"notify_url",
						base.Supplier.NotifyUri
					},
                    {
                        "biz_content",
                        biz_content.ToJson()
                    }
				};
				
				try
				{
                    string sign = AlipaySignature.RSASign(obj, base.Account.RsaPrivate, _charset, "RSA2");
                    obj.Add("sign", sign);

                    string result = HttpUtils.SendRequest(base.Supplier.PostUri, obj, "POST", _charset, "");
                    LogUtil.DebugFormat("提交地址：{0}，提交内容：{1}, 提交结果：{2}", base.Supplier.PostUri, obj.ToJson(), result);
                    if (string.IsNullOrEmpty(result))
                    {
                        fail.Message = "未获得接口数据!";
                        return fail;
                    }

                    string sign2 = "";
                    string content = "";
                    string text3 = result.Trim(' ', '\t', '\n', '{', '}');
                    sign2 = text3.Substring(text3.IndexOf("\"sign\":") + 8).Trim('"');
                    int index = text3.IndexOf("\"alipay_trade_precreate_response\":") + "\"alipay_trade_precreate_response\":".Length;
                    content = text3.Substring(index, text3.LastIndexOf("}") + 1 - index);

                    bool b = AlipaySignature.RSACheckContent(content, sign2, _ALRSA_PUBLIC, _charset, "RSA2", false);
                    if(!b)
                    {
                        fail.Message = "支付宝签名效验错误!";
                        return fail;
                    }
                    //Dictionary<string, object> rs = result.FormJson<Dictionary<string, object>>();
                    fail.Code = HMPayState.Success;
                    fail.Data = content;

                    return fail;
				}
				catch (Exception exception)
				{
					fail.Message = "系统繁忙，请稍候再试！";
					LogUtil.Error("支付宝,订单号:" + order.OrderNo, exception);
					return fail;
				}
			}
			fail.Message = "此支付接口不支持此通道!";
			return fail;
		}

		protected override Dictionary<string, string> GetNotifyParam()
		{
			LogUtil.Debug("支付宝当面付,GetNotifyParam 进入");
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			try
			{
				string[] allKeys = HttpContext.Current.Request.Form.AllKeys;
				foreach (string text in allKeys)
				{
					string request = Utils.GetRequest(text.ToString());
					dictionary.Add(text.ToString(), request);
				}
				LogUtil.Debug("GetNotifyParam=" + dictionary.ToJson());
				return dictionary;
			}
			catch (Exception exception)
			{
				LogUtil.Error("支付宝当面付：GetNotifyParam", exception);
				return dictionary;
			}
		}

		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string notifyRequest = GetNotifyRequest("trade_status");
			LogUtil.Debug("支付宝：NotifyParamToOrder:" + notifyRequest);
			if (notifyRequest.Equals("TRADE_SUCCESS"))
			{
				string notifyRequest2 = GetNotifyRequest("out_trade_no");
				string notifyRequest3 = GetNotifyRequest("trade_no");
				string notifyRequest4 = GetNotifyRequest("total_amount");
				string notifyRequest5 = GetNotifyRequest("gmt_create");
				fail.Code = HMNotifyState.Success;
				fail.Data = new HMOrder
				{
					OrderNo = notifyRequest2,
					SupplierOrderNo = notifyRequest3,
					OrderAmt = Utils.StringToDecimal(notifyRequest4, decimal.Zero) * 100, // 转换元为分
					OrderTime = Utils.StringToDateTime(notifyRequest5, DateTime.Now).Value
				};
			}
			return fail;
		}

		public override HMNotifyResult<string> NotifySign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			Dictionary<string, string> notifyParams = base.NotifyParams;
			SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
			foreach (KeyValuePair<string, string> item in notifyParams)
			{
				sortedDictionary.Add(item.Key, item.Value);
			}
            if(sortedDictionary.ContainsKey("sign_type"))
            {
                sortedDictionary.Remove("sign_type");
            }
            if(AlipaySignature.RSACheckV2(sortedDictionary, _ALRSA_PUBLIC, _charset, "RSA2", false))
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

		protected override Dictionary<string, string> GetReturnParam()
		{
			LogUtil.Debug("支付宝扫码,GetNotifyParam 进入");
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			try
			{
				string[] allKeys = HttpContext.Current.Request.QueryString.AllKeys;
				foreach (string text in allKeys)
				{
					string request = Utils.GetRequest(text.ToString());
					dictionary.Add(text.ToString(), request);
				}
				LogUtil.Debug("GetReturnParam=" + dictionary.ToJson());
				return dictionary;
			}
			catch (Exception exception)
			{
				LogUtil.Error("支付宝：GetReturnParam", exception);
				return dictionary;
			}
		}

		protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string returnRequest = GetReturnRequest("trade_status");
			LogUtil.Debug("支付宝 ReturnParamToOrder:" + returnRequest);
			if (returnRequest.Equals("TRADE_SUCCESS"))
			{
				string returnRequest2 = GetReturnRequest("out_trade_no");
				string returnRequest3 = GetReturnRequest("trade_no");
				string returnRequest4 = GetReturnRequest("total_fee");
				string returnRequest5 = GetReturnRequest("gmt_create");
				fail.Code = HMNotifyState.Success;
				fail.Data = new HMOrder
				{
					OrderNo = returnRequest2,
					SupplierOrderNo = returnRequest3,
					OrderAmt = Utils.StringToDecimal(returnRequest4, decimal.Zero),
					OrderTime = Utils.StringToDateTime(returnRequest5, DateTime.Now).Value
				};
			}
			return fail;
		}

		public override HMNotifyResult<string> ResultSign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			decimal d = Utils.StringToDecimal(GetReturnRequest("total_fee"), decimal.Zero) * 100m;
			if (order.OrderAmt == d)
			{
				fail.Code = HMNotifyState.Success;
				fail.Data = NOTIFY_SUCCESS;
			}
			else
			{
				fail.Message = "订单金额验证失败";
			}
			return fail;
		}
	}
}
