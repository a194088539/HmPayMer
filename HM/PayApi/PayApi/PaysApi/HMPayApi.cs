using HM.Framework.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace HM.Framework.PayApi.PaysApi
{
	public class HMPayApi : HMPayApiBase
	{
		public override string NOTIFY_SUCCESS => "success";

		public override string NOTIFY_FAIL => "fail";

		public override bool IsWithdraw => false;

		private string GetChannelCode(HMChannel channel)
		{
            if(channel == HMChannel.PAYSAPI_ALIPAY)
            {
                return "1";
            } else if(channel == HMChannel.PAYSAPI_WEIXIN)
            {
                return "2";
            }
            return "1";
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
            string orderNo = order.OrderNo;
            string price = (order.OrderAmt / 100m).ToString("#0.00");
            SortedDictionary<string, string> obj = new SortedDictionary<string, string>
                {
                    {
                        "uid",
                        base.Account.AccountUser
                    },
                    {
                        "price",
                        price
                    },
                    {
                        "istype",
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
                        "orderid",
                        orderNo
                    },
                    {
                        "orderuid",
                        order.MerOrderNo
                    },
                    {
                        "goodsname",
                        "onlinepay"
                    },
                };
            string signstr = obj["goodsname"] + obj["istype"] + obj["notify_url"]
                + obj["orderid"] + obj["orderuid"] + obj["price"] + obj["return_url"] + base.Supplier.Account.Md5Pwd + obj["uid"];
            string key = Alipay.Core.GetAbstractToMD5(System.Text.Encoding.UTF8.GetBytes(signstr));
            obj.Add("key", key);
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat("<form name='submit' action='{0}' _input_charset='utf-8' method='get'>", base.Supplier.PostUri);
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
                LogUtil.Error("paysapi,订单号:" + order.OrderNo, exception);
                return fail;
            }
            fail.Message = "此支付接口不支持此通道!";
			return fail;
		}

		protected override Dictionary<string, string> GetNotifyParam()
		{
			LogUtil.Debug("paysapi,GetNotifyParam 进入");
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
				LogUtil.Error("paysapi：GetNotifyParam", exception);
				return dictionary;
			}
		}

		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
            LogUtil.Debug("paysapi,NotifyParamToOrder=" + dic.ToJson());
            HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
            fail.Code = HMNotifyState.Success;
            fail.Data = new HMOrder
            {
                OrderNo = dic["orderid"],
                SupplierOrderNo = dic["paysapi_id"],
                OrderAmt = Utils.StringToDecimal(dic["price"], decimal.Zero) * 100, // 转换元为分
                OrderTime = DateTime.Now
            };

            return fail;
		}

		public override HMNotifyResult<string> NotifySign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
            LogUtil.Debug("paysapi,NotifySign=" + order.OrderNo);
            HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			Dictionary<string, string> notifyParams = base.NotifyParams;
            string signStr = "";
            if(notifyParams.ContainsKey("orderid"))
            {
                signStr += notifyParams["orderid"];
            }

            if (notifyParams.ContainsKey("orderuid"))
            {
                signStr += notifyParams["orderuid"];
            }

            if (notifyParams.ContainsKey("paysapi_id"))
            {
                signStr += notifyParams["paysapi_id"];
            }

            if (notifyParams.ContainsKey("price"))
            {
                signStr += notifyParams["price"];
            }

            if (notifyParams.ContainsKey("realprice"))
            {
                signStr += notifyParams["realprice"];
            }

            signStr += account.Md5Pwd;

            string key = Alipay.Core.GetAbstractToMD5(System.Text.Encoding.UTF8.GetBytes(signStr));

			string notifyRequest2 = GetNotifyRequest("key");
			if (key == notifyRequest2)
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
			LogUtil.Debug("paysapi,GetReturnParam 进入");
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
				LogUtil.Error("paysapi：GetReturnParam", exception);
				return dictionary;
			}
		}

		protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
            fail.Code = HMNotifyState.Success;
            fail.Data = new HMOrder
            {
                OrderNo = dic["orderid"],
                OrderTime = DateTime.Now
            };
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
