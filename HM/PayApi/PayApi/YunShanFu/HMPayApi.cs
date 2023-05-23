using HM.Framework.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace HM.Framework.PayApi.YunShanFu
{
	public class HMPayApi : HMPayApiBase
	{
		public override string NOTIFY_SUCCESS => "SUCCESS";

		public override string NOTIFY_FAIL => "FAIL";

		public override bool IsWithdraw => false;

		private string GetChannelCode(HMChannel channel)
		{
			switch (channel)
			{
			case HMChannel.WEIXIN_NATIVE:
				return "WXSCAN";
			case HMChannel.WEIXIN_H5:
				return "WXH5";
			case HMChannel.ALIPAY_NATIVE:
				return "ALISCAN";
			case HMChannel.ALIPAY_H5:
				return "ALIH5";
			case HMChannel.QQPAY_NATIVE:
			case HMChannel.QQPAY_H5:
				return "QQH5";
			default:
				return string.Empty;
			}
		}

		public override HMMode GetPayMode(HMChannel code)
		{
			return HMMode.跳转链接;
		}

		protected override HMPayResult PayGatewayBody(HMOrder order)
		{
			HMPayResult fail = HMPayResult.Fail;
			fail.Mode = GetPayMode(order.ChannelCode);
			string channelCode = GetChannelCode(order.ChannelCode);
			if (string.IsNullOrEmpty(channelCode))
			{
				fail.Message = "此接口通道暂不支持";
				return fail;
			}
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            SortedDictionary<string, string> obj = new SortedDictionary<string, string>
                {
                    {
                        "mch_id",
                        base.Account.AccountUser
                    },
                {
                    "timestamp",
                    Convert.ToInt64(ts.TotalSeconds).ToString()
                },
                {
                    "subject",
                    "shop"
                },
                    {
                        "total_fee",
                        ((int)order.OrderAmt).ToString()
                    },
                {
                    "spbill_create_ip",
                    order.ClientIp
                },
                    {
                        "trade_type",
                        channelCode
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
                        order.OrderNo
                    },
                    {
                        "nonce",
                        Guid.NewGuid().ToString("N")
                    }
                };
            string signstr = Alipay.Core.CreateLinkString(obj) + "&key=" + base.Account.Md5Pwd;
            string sign = EncryUtils.MD5(signstr).ToUpper();
            obj.Add("sign", sign);
            //obj.Add("sign_type", "MD5");
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat("<form name='submit' action='{0}' _input_charset='utf-8' method='post'>", base.Supplier.PostUri);
                foreach (KeyValuePair<string, string> item in obj)
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
                LogUtil.Error("云闪付,订单号:" + order.OrderNo, exception);
                return fail;
            }

		}

		protected override Dictionary<string, string> GetNotifyParam()
		{
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            try
            {
                string[] allKeys = HttpContext.Current.Request.Form.AllKeys;
                foreach (string text in allKeys)
                {
                    string request = Utils.GetRequest(text.ToString());
                    dictionary.Add(text.ToString(), request);
                }
                LogUtil.Debug("云闪付GetNotifyParam=" + dictionary.ToJson());
                return dictionary;
            }
            catch (Exception exception)
			{
				LogUtil.Error("云闪付获取失败.GetNotifyParam", exception);
				return dictionary;
			}
		}

		protected override Dictionary<string, string> GetReturnParam()
		{
			return new Dictionary<string, string>();
		}

		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
            LogUtil.Debug("云闪付,NotifyParamToOrder=" + dic.ToJson());
            HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
            
            if(dic["result_code"] != "SUCCESS")
            {
                return fail;
            }
            fail.Code = HMNotifyState.Success;
            fail.Data = new HMOrder
            {
                OrderNo = dic["out_trade_no"],
                SupplierOrderNo = dic["trade_no"],
                OrderAmt = Utils.StringToDecimal(dic["total_fee"], decimal.Zero), 
                OrderTime = Utils.StringToDateTime(dic["pay_time"], DateTime.Now).Value
            };

            return fail;
        }

		protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			fail.Code = HMNotifyState.WaitAccountInit;
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
            string sign = GetNotifyRequest("sign");
            if (true)
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
			fail.Code = HMNotifyState.Success;
			fail.Data = NOTIFY_SUCCESS;
			return fail;
		}
	}
}
