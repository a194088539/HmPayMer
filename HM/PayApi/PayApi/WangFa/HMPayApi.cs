using HM.Framework.Logging;
using HM.Framework.PayApi.WangFa.Lib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace HM.Framework.PayApi.WangFa
{
	public class HMPayApi : HMPayApiBase
	{
		public class WFResult
		{
			public string sign
			{
				get;
				set;
			}

			public string rsp_code
			{
				get;
				set;
			}

			public string rsp_msg
			{
				get;
				set;
			}

			public string data
			{
				get;
				set;
			}
		}

		public override string NOTIFY_SUCCESS => "SUCCESS";

		public override string NOTIFY_FAIL => "FAIL";

		public override bool IsWithdraw => false;

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

		private string GetChannelCode(HMChannel channel)
		{
			switch (channel)
			{
			case HMChannel.WEIXIN_NATIVE:
				return "10103";
			case HMChannel.WEIXIN_JSAPI:
				return "10303";
			case HMChannel.WEIXIN_H5:
				return "10203";
			case HMChannel.ALIPAY_H5:
				return "20203";
			case HMChannel.ALIPAY_NATIVE:
				return "20203";
			case HMChannel.QQPAY_NATIVE:
				return "70103";
			case HMChannel.QQPAY_H5:
				return "70203";
			case HMChannel.JD_NATIVE:
				return "80103";
			default:
				return string.Empty;
			}
		}

		protected override Dictionary<string, string> GetNotifyParam()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string request = Utils.GetRequest("trx_key");
			string request2 = Utils.GetRequest("ord_amount");
			string request3 = Utils.GetRequest("request_id");
			string request4 = Utils.GetRequest("trx_status");
			string request5 = Utils.GetRequest("product_type");
			string request6 = Utils.GetRequest("request_time");
			string request7 = Utils.GetRequest("goods_name");
			string request8 = Utils.GetRequest("trx_time");
			string request9 = Utils.GetRequest("pay_request_id");
			string request10 = Utils.GetRequest("remark");
			string request11 = Utils.GetRequest("sign");
			dictionary.Add("trx_key", request);
			dictionary.Add("ord_amount", request2);
			dictionary.Add("request_id", request3);
			dictionary.Add("trx_status", request4);
			dictionary.Add("product_type", request5);
			dictionary.Add("request_time", request6);
			dictionary.Add("goods_name", request7);
			dictionary.Add("trx_time", request8);
			dictionary.Add("pay_request_id", request9);
			dictionary.Add("remark", request10);
			dictionary.Add("sign", request11);
			LogUtil.DebugFormat("旺发异步参数：{0}", dictionary.ToJson());
			return dictionary;
		}

		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string notifyRequest = GetNotifyRequest("request_id");
			string notifyRequest2 = GetNotifyRequest("ord_amount");
			string notifyRequest3 = GetNotifyRequest("trx_status");
			string notifyRequest4 = GetNotifyRequest("pay_request_id");
			string notifyRequest5 = GetNotifyRequest("trx_time");
			string notifyRequest6 = GetNotifyRequest("sign");
			if (string.IsNullOrEmpty(notifyRequest))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (string.IsNullOrEmpty(notifyRequest2))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (string.IsNullOrEmpty(notifyRequest6))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (notifyRequest3 == "SUCCESS")
			{
				fail.Code = HMNotifyState.Success;
				fail.Data = new HMOrder
				{
					OrderNo = notifyRequest,
					SupplierOrderNo = notifyRequest4,
					OrderAmt = Utils.StringToDecimal(notifyRequest2, decimal.Zero) * 100m,
					OrderTime = Utils.StringToDateTime(notifyRequest5, DateTime.Now).Value
				};
			}
			else
			{
				fail.Code = HMNotifyState.Fail;
			}
			return fail;
		}

		public override HMNotifyResult<string> NotifySign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			string notifyRequest = GetNotifyRequest("trx_key");
			string notifyRequest2 = GetNotifyRequest("ord_amount");
			string notifyRequest3 = GetNotifyRequest("request_id");
			string notifyRequest4 = GetNotifyRequest("trx_status");
			string notifyRequest5 = GetNotifyRequest("product_type");
			string notifyRequest6 = GetNotifyRequest("request_time");
			string notifyRequest7 = GetNotifyRequest("goods_name");
			string notifyRequest8 = GetNotifyRequest("trx_time");
			string notifyRequest9 = GetNotifyRequest("pay_request_id");
			string notifyRequest10 = GetNotifyRequest("remark");
			string notifyRequest11 = GetNotifyRequest("sign");
			string sign = MD5Utils.getSign(new Dictionary<string, object>
			{
				{
					"trx_key",
					notifyRequest
				},
				{
					"ord_amount",
					notifyRequest2
				},
				{
					"request_id",
					notifyRequest3
				},
				{
					"trx_status",
					notifyRequest4
				},
				{
					"product_type",
					notifyRequest5
				},
				{
					"request_time",
					notifyRequest6
				},
				{
					"goods_name",
					notifyRequest7
				},
				{
					"trx_time",
					notifyRequest8
				},
				{
					"pay_request_id",
					notifyRequest9
				},
				{
					"remark",
					notifyRequest10
				}
			}, account.Md5Pwd);
			LogUtil.DebugFormat("旺发异步签名：{0}", sign);
			if (notifyRequest11.Equals(sign))
			{
				decimal num = Utils.StringToDecimal(notifyRequest2, decimal.Zero) * 100m;
				if (notifyRequest4 == "SUCCESS")
				{
					fail.Code = HMNotifyState.Success;
					fail.Data = NOTIFY_SUCCESS;
				}
				else if (notifyRequest4 == "FAILED")
				{
					fail.Code = HMNotifyState.Fail;
					fail.Data = NOTIFY_FAIL;
				}
				else
				{
					fail.Code = HMNotifyState.WaitAccountInit;
					fail.Data = NOTIFY_FAIL;
				}
			}
			else
			{
				fail.Data = NOTIFY_FAIL;
			}
			return fail;
		}

		protected override Dictionary<string, string> GetReturnParam()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string request = Utils.GetRequest("trx_key");
			string request2 = Utils.GetRequest("ord_amount");
			string request3 = Utils.GetRequest("request_id");
			string request4 = Utils.GetRequest("trx_status");
			string request5 = Utils.GetRequest("product_type");
			string request6 = Utils.GetRequest("request_time");
			string request7 = Utils.GetRequest("goods_name");
			string request8 = Utils.GetRequest("trx_time");
			string request9 = Utils.GetRequest("pay_request_id");
			string request10 = Utils.GetRequest("remark");
			string request11 = Utils.GetRequest("sign");
			dictionary.Add("trx_key", request);
			dictionary.Add("ord_amount", request2);
			dictionary.Add("request_id", request3);
			dictionary.Add("trx_status", request4);
			dictionary.Add("product_type", request5);
			dictionary.Add("request_time", request6);
			dictionary.Add("goods_name", request7);
			dictionary.Add("trx_time", request8);
			dictionary.Add("pay_request_id", request9);
			dictionary.Add("remark", request10);
			dictionary.Add("sign", request11);
			return dictionary;
		}

		protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			string returnRequest = GetReturnRequest("request_id");
			string returnRequest2 = GetReturnRequest("ord_amount");
			string returnRequest3 = GetReturnRequest("sign");
			string returnRequest4 = GetReturnRequest("trx_status");
			string returnRequest5 = GetReturnRequest("pay_request_id");
			string returnRequest6 = GetReturnRequest("trx_time");
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
			if (string.IsNullOrEmpty(returnRequest3))
			{
				fail.Message = "必要参数为空";
				return fail;
			}
			if (returnRequest4 == "SUCCESS")
			{
				fail.Code = HMNotifyState.Success;
				fail.Data = new HMOrder
				{
					OrderNo = returnRequest,
					SupplierOrderNo = returnRequest5,
					OrderAmt = Utils.StringToDecimal(returnRequest2, decimal.Zero) * 100m,
					OrderTime = Utils.StringToDateTime(returnRequest6, DateTime.Now).Value
				};
			}
			else
			{
				fail.Code = HMNotifyState.Fail;
			}
			return fail;
		}

		public override HMNotifyResult<string> ResultSign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			string returnRequest = GetReturnRequest("trx_key");
			string returnRequest2 = GetReturnRequest("ord_amount");
			string returnRequest3 = GetReturnRequest("request_id");
			string returnRequest4 = GetReturnRequest("trx_status");
			string returnRequest5 = GetReturnRequest("product_type");
			string returnRequest6 = GetReturnRequest("request_time");
			string returnRequest7 = GetReturnRequest("goods_name");
			string returnRequest8 = GetReturnRequest("trx_time");
			string returnRequest9 = GetReturnRequest("pay_request_id");
			string returnRequest10 = GetReturnRequest("remark");
			string returnRequest11 = GetReturnRequest("sign");
			string sign = MD5Utils.getSign(new Dictionary<string, object>
			{
				{
					"trx_key",
					returnRequest
				},
				{
					"ord_amount",
					returnRequest2
				},
				{
					"request_id",
					returnRequest3
				},
				{
					"trx_status",
					returnRequest4
				},
				{
					"product_type",
					returnRequest5
				},
				{
					"request_time",
					returnRequest6
				},
				{
					"goods_name",
					returnRequest7
				},
				{
					"trx_time",
					returnRequest8
				},
				{
					"pay_request_id",
					returnRequest9
				},
				{
					"remark",
					returnRequest10
				}
			}, account.Md5Pwd);
			LogUtil.DebugFormat("旺发同步签名：{0}", sign);
			if (returnRequest11.Equals(sign))
			{
				decimal num = Utils.StringToDecimal(returnRequest2, decimal.Zero) * 100m;
				if (returnRequest4 == "SUCCESS")
				{
					fail.Code = HMNotifyState.Success;
					fail.Data = NOTIFY_SUCCESS;
				}
				else if (returnRequest4 == "FAILED")
				{
					fail.Code = HMNotifyState.Fail;
					fail.Data = NOTIFY_FAIL;
				}
				else
				{
					fail.Code = HMNotifyState.WaitAccountInit;
					fail.Data = NOTIFY_FAIL;
				}
			}
			else
			{
				fail.Data = NOTIFY_FAIL;
			}
			return fail;
		}

		protected override HMPayResult PayGatewayBody(HMOrder order)
		{
			HMPayResult fail = HMPayResult.Fail;
			fail.Mode = GetPayMode(order.ChannelCode);
			string channelCode = GetChannelCode(order.ChannelCode);
			if (!string.IsNullOrEmpty(channelCode))
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary.Add("trx_key", base.Account.OpenId);
				dictionary.Add("ord_amount", (base.Order.OrderAmt / 100m).ToString("0.00"));
				dictionary.Add("request_id", base.Order.OrderNo);
				dictionary.Add("product_type", channelCode);
				string value = DateTime.Now.ToString("yyyyMMddHHmmss");
				dictionary.Add("request_time", value);
				dictionary.Add("goods_name", base.Order.OrderNo);
				dictionary.Add("request_ip", "127.0.0.1");
				dictionary.Add("return_url", base.Supplier.ReturnUri);
				dictionary.Add("callback_url", base.Supplier.NotifyUri);
				dictionary.Add("remark", base.Order.OrderNo);
				string sign = MD5Utils.getSign(dictionary, base.Account.Md5Pwd);
				dictionary.Add("sign", sign);
				try
				{
					string text = HttpUtils.SendRequest(base.Supplier.PostUri, getParamStr(dictionary), "POST", "UTF-8");
					LogUtil.DebugFormat("旺发提交地址：{0}，提交内容：{1}, 提交结果：{2}", base.Supplier.PostUri, getParamStr(dictionary), text);
					if (string.IsNullOrEmpty(text))
					{
						fail.Message = "未获得接口数据!";
						return fail;
					}
					WFResult wFResult = text.FormJson<WFResult>();
					if (wFResult.rsp_code.Equals("0000"))
					{
						fail.Code = HMPayState.Success;
						fail.Data = wFResult.data;
					}
					else
					{
						fail.Message = wFResult.rsp_msg;
					}
					return fail;
				}
				catch (Exception exception)
				{
					fail.Message = "系统繁忙，请稍候再试！";
					LogUtil.Error("旺发提交接口出错,订单号:" + order.OrderNo, exception);
					return fail;
				}
			}
			fail.Message = "此通道已关闭！";
			return fail;
		}

		public string getParamStr(Dictionary<string, object> paramDic)
		{
			string text = "";
			if (paramDic.Count == 0)
			{
				return text;
			}
			foreach (KeyValuePair<string, object> item in paramDic)
			{
				string text2 = UrlEncode((item.Value == null) ? "" : item.Value.ToString(), Encoding.UTF8);
				text = text + item.Key + "=" + text2 + "&";
			}
			return text.Substring(0, text.Length - 1);
		}

		public string UrlEncode(string temp, Encoding encoding)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < temp.Length; i++)
			{
				string text = temp[i].ToString();
				string text2 = HttpUtility.UrlEncode(text, encoding);
				if (text == text2)
				{
					stringBuilder.Append(text);
				}
				else
				{
					stringBuilder.Append(text2.ToUpper());
				}
			}
			return stringBuilder.ToString();
		}
	}
}
