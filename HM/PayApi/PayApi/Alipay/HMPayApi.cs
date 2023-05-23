using HM.Framework.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace HM.Framework.PayApi.Alipay
{
	public class HMPayApi : HMPayApiBase
	{
		public override string NOTIFY_SUCCESS => "success";

		public override string NOTIFY_FAIL => "fail";

		public override bool IsWithdraw => false;

		private string GetChannelCode(HMChannel channel)
		{
			if (channel == HMChannel.ALIPAY_NATIVE)
			{
				return "create_direct_pay_by_user";
			} else if (channel == HMChannel.ALIPAY_H5)
            {
                return "alipay.wap.create.direct.pay.by.user";
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
				string value = "在线支付";
				string value2 = (order.OrderAmt / 100m).ToString();
				string value3 = "";
				string value4 = "";
				SortedDictionary<string, string> obj = new SortedDictionary<string, string>
				{
					{
						"partner",
						base.Account.AccountUser
					},
					{
						"seller_id",
						base.Account.AccountUser
					},
					{
						"_input_charset",
						"utf-8"
					},
					{
						"service",
						channelCode
					},
					{
						"payment_type",
						"1"
					},
					{
						"notify_url",
						base.Supplier.NotifyUri
					},
					{
						"return_url",
						base.Supplier.ReturnUri
					},
					{
						"out_trade_no",
						orderNo
					},
					{
						"subject",
						value
					},
					{
						"total_fee",
						value2
					},
					{
						"show_url",
						value3
					},
					{
						"app_pay",
						"Y"
					},
					{
						"body",
						value4
					}
				};
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary = Core.FilterPara(obj);
				string value5 = AlipayMD5.Sign(Core.CreateLinkString(dictionary), base.Account.Md5Pwd, "utf-8");
				dictionary.Add("sign", value5);
				dictionary.Add("sign_type", "MD5");
				obj.ToJson();
				try
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendFormat("<form name='submit' action='{0}' _input_charset='utf-8' method='get'>", base.Supplier.PostUri);
					foreach (KeyValuePair<string, string> item in dictionary)
					{
						stringBuilder.AppendFormat("<input type='hidden' name='{0}' value = '{1}' />", item.Key, item.Value);
					}
					stringBuilder.Append("</form>").Append("<script type='text/javascript'>document.forms['submit'].submit();</script>");
					fail.Code = HMPayState.Success;
					fail.Data = stringBuilder.ToString();
					fail.Mode = HMMode.输出字符串;
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
			LogUtil.Debug("支付宝扫码,GetNotifyParam 进入");
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
				LogUtil.Error("支付宝：GetNotifyParam", exception);
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
				string notifyRequest4 = GetNotifyRequest("total_fee");
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
			string notifyRequest = GetNotifyRequest("notify_id");
			string notifyRequest2 = GetNotifyRequest("sign");
			if (new Notify
			{
				_partner = account.AccountUser,
				_key = account.Md5Pwd,
				_input_charset = "utf-8"
			}.Verify(sortedDictionary, notifyRequest, notifyRequest2))
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
