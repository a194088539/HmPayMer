using HM.Framework.Logging;
using HM.Framework.PayApi.Alipay;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace HM.Framework.PayApi.WeiXin
{
	public class HMMaidanPayApi : HMPayApiBase
	{
		public override string NOTIFY_SUCCESS => "success";

		public override string NOTIFY_FAIL => "fail";

		public override bool IsWithdraw => false;

		private string GetChannelCode(HMChannel channel)
		{
            if(channel == HMChannel.WEIXIN_MAIDAN)
            {
                return "WEIXIN_NATIVE";
            }
            return "WEIXIN_NATIVE";
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

            Dictionary<string, string> dictionary = new Dictionary<string, string>()
                    {
                        {
                            "total_fee",
                            (order.OrderAmt / 100).ToString("#0.00")
                        },
                        {
                            "trade_no",
                            order.OrderNo
                        },
                        {
                            "out_trade_no",
                            order.MerOrderNo
                        },
                        {
                            "type",
                            channelCode == "WEIXIN_NATIVE" ? "1":"0"
                        },
                        {
                            "qr",
                            Account.AppId
                        }
                    };
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("<form name='submit' action='{0}' _input_charset='utf-8' method='post'>", Supplier.PostUri);
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
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            try
            {
                string[] allKeys = HttpContext.Current.Request.Form.AllKeys;
                foreach (string text in allKeys)
                {
                    string request = Utils.GetRequest(text.ToString());
                    dictionary.Add(text.ToString(), request);
                }
                LogUtil.Debug("微信买单GetNotifyParam=" + dictionary.ToJson());
                return dictionary;
            }
            catch (Exception exception)
            {
                LogUtil.Error("微信买单获取失败.GetNotifyParam", exception);
                return dictionary;
            }
        }

		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
            LogUtil.Debug("微信买单,NotifyParamToOrder=" + dic.ToJson());
            HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
            long paytime = Utils.StringToLong(dic["pay_time"], 0);
            if(paytime == 0)
            {
                fail.Message = "参数有误";
                return fail;
            }
            fail.Code = HMNotifyState.Success;
            
            fail.Data = new HMOrder
            {
                OrderNo = dic["out_trade_no"],
                SupplierOrderNo = dic["trade_no"],
                OrderAmt = Utils.StringToDecimal(dic["total_fee"], decimal.Zero) * 100,
                OrderTime = (new DateTime(1970, 1, 1)).AddSeconds(paytime)
            };

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
            string signstr = notifyParams["total_fee"] + notifyParams["pay_time"] +
                    notifyParams["trade_no"] +
                    notifyParams["out_trade_no"];

            string sign = notifyParams["sign"];
            LogUtil.Debug("微信买单,supplier=" + supplier.ToJson());
            if (AlipayMD5.Sign(signstr, account.Md5Pwd, "utf-8").Equals(sign.ToLower()))
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
            fail.Code = HMNotifyState.WaitAccountInit;
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
