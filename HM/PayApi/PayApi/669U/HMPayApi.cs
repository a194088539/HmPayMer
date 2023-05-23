using HM.Framework.Logging;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace HM.Framework.PayApi._669U
{
	public class HMPayApi : HMPayApiBase
	{
		public override string NOTIFY_SUCCESS => "success";

		public override string NOTIFY_FAIL => "fail";

		public override bool IsWithdraw => false;

		private string GetChannelCode(HMChannel channel)
		{
            if(channel == HMChannel._669U_ALIPAY)
            {
                return "alipay_auto";
            } else if(channel == HMChannel._669U_WEIXIN)
            {
                return "wechat_auto";
            } else if(channel == HMChannel._669U_SALIPAY)
            {
                return "service_auto";
            }
            else if (channel == HMChannel._669U_SWEIXIN)
            {
                return "service_auto";
            }
            return "alipay_auto";
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
                        "account_id",
                        base.Account.AccountUser
                    },
                    {
                        "amount",
                        price
                    },
                    {
                        "thoroughfare",
                        channelCode
                    },
                    {
                        "callback_url",
                        base.Supplier.NotifyUri
                    },
                    {
                        "success_url",
                        base.Supplier.ReturnUri + "?success=true_" + orderNo
                    },
                    {
                        "error_url",
                        base.Supplier.ReturnUri + "?success=false"
                    },
                    {
                        "out_trade_no",
                        orderNo
                    },
                    {
                        "robin",
                        "2"
                    },
                    {
                        "keyId",
                        ""
                    },
                    {
                        "type",
                        order.ChannelCode == HMChannel._669U_SWEIXIN ? "1":"2"
                    },
                    {
                        "content_type",
                        "text"
                    },
                };
            string signstr = obj["amount"] + obj["out_trade_no"];
            string md5Str = GetMd5Str(signstr);
            RC4Crypto rc4 = new RC4Crypto();
            byte[] byts = rc4.EncryptEx(Encoding.UTF8.GetBytes(md5Str), base.Supplier.Account.Md5Pwd);
            signstr = Alipay.Core.GetAbstractToMD5(System.Text.Encoding.UTF8.GetBytes(signstr));
            string rc4Str = GetMd5Str(byts);
            obj.Add("sign", rc4Str);
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
                LogUtil.Error("paysapi,订单号:" + order.OrderNo, exception);
                return fail;
            }
            fail.Message = "此支付接口不支持此通道!";
			return fail;
		}
        
        public static string GetMd5Str(string ConvertString)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            string t2 = BitConverter.ToString(md5.ComputeHash(UTF8Encoding.Default.GetBytes(ConvertString)));
            t2 = t2.Replace("-", "").ToLower();
            return t2;
        }

        public static string GetMd5Str(byte[] byts)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            string t2 = BitConverter.ToString(md5.ComputeHash(byts));
            t2 = t2.Replace("-", "").ToLower();
            return t2;
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
                OrderNo = dic["out_trade_no"],
                SupplierOrderNo = dic["trade_no"],
                OrderAmt = Utils.StringToDecimal(dic["amount"], decimal.Zero) * 100, // 转换元为分
                OrderTime = Utils.StringToDateTime(dic["pay_time"], DateTime.Now).Value
            };

            return fail;
		}

		public override HMNotifyResult<string> NotifySign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
            LogUtil.Debug("paysapi,NotifySign=" + order.OrderNo);
            HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			Dictionary<string, string> notifyParams = base.NotifyParams;
            string signstr = notifyParams["amount"] + notifyParams["out_trade_no"];
            string md5Str = GetMd5Str(signstr);
            RC4Crypto rc4 = new RC4Crypto();
            byte[] byts = rc4.EncryptEx(Encoding.UTF8.GetBytes(md5Str), supplier.Account.Md5Pwd);
            signstr = Alipay.Core.GetAbstractToMD5(System.Text.Encoding.UTF8.GetBytes(signstr));
            string rc4Str = GetMd5Str(byts);

            string notifyRequest2 = GetNotifyRequest("sign");
			if (rc4Str == notifyRequest2)
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
            if(dic.ContainsKey("success") && dic["success"].StartsWith("true"))
            {
                string o = dic["success"].Substring(5);
                fail.Code = HMNotifyState.Success;
                fail.Data = new HMOrder
                {
                    OrderNo = o,
                    OrderTime = DateTime.Now
                };
                return fail;
            } 
            fail.Message = "支付失败";
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
