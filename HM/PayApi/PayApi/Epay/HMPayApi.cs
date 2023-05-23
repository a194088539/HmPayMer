using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using HM.Framework.PayApi;
using HM.Framework.Logging;

namespace HM.Framework.PayApi.Epay
{
    public class HMPayApi : HMPayApiBase
    {
        public override string NOTIFY_SUCCESS => "ok";

        public override string NOTIFY_FAIL => "FAIL";

        public override bool IsWithdraw => true;

        private string GetChannelCode(HMChannel channel)        //支付类型
        {
            switch (channel)
            {
                case HMChannel.ALIPAY_NATIVE:
                    return "ALIPAY";
                case HMChannel.ALIPAY_MICROPAY:
                case HMChannel.ALIPAY_H5:
                    return "ALIPAYWAP";
                case HMChannel.QQPAY_NATIVE:
                    return "QQPAY";
                case HMChannel.JD_NATIVE:
                    return "JDPAY";
                case HMChannel.WEIXIN_H5:
                    return "WEIXINWAP";
                case HMChannel.QQPAY_H5:
                    return "QQWAP";
                case HMChannel.WEIXIN_NATIVE:
                    return "WEIXIN";
                case HMChannel.EBank:
                    return "BANKPAY";
                case HMChannel.KUAIJIE:
                    return "EXPRESS";
                default:
                    return string.Empty;
            }
        }

        public override HMMode GetPayMode(HMChannel code)
        {
            switch (code)
            {
                //case HMChannel.ALIPAY_NATIVE:
                //case HMChannel.JD_NATIVE:
                //case HMChannel.WEIXIN_NATIVE:
                //case HMChannel.QQPAY_NATIVE:
                    //return HMMode.跳转扫码页面;
                default:
                    return HMMode.跳转链接;
            }
        }


//请求参数
        protected override HMPayResult PayGatewayBody(HMOrder order)
        {
            HMPayResult fail = HMPayResult.Fail;
            fail.Mode = GetPayMode(order.ChannelCode);
            GetChannelCode(order.ChannelCode);
            string partner = base.Account.AccountUser;
            string banktype = GetChannelCode(order.ChannelCode);
            string paymoney = (order.OrderAmt / 100m).ToString("0.00");
            string ordernumber = order.OrderNo;
            string callbackurl = base.Supplier.NotifyUri;
            string key = base.Account.Md5Pwd;
            string sign = EncryUtils.MD5($"partner={partner}&banktype={banktype}&paymoney={paymoney}&ordernumber={ordernumber}&callbackurl={callbackurl}{key}").ToLower();
            string data = base.Supplier.PostUri + "?" + $"partner={partner}&banktype={banktype}&paymoney={paymoney}&ordernumber={ordernumber}&callbackurl={callbackurl}&sign={sign}";
            fail.Code = HMPayState.Success;
            fail.Data = data;
            return fail;
        }

//异步返回参数
        protected override Dictionary<string, string> GetNotifyParam()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string request = Utils.GetRequest("partner");
            string request2 = Utils.GetRequest("ordernumber");
            string request3 = Utils.GetRequest("orderstatus");
            string request4 = Utils.GetRequest("paymoney");
            string request5 = Utils.GetRequest("sysnumber");
            string request6 = Utils.GetRequest("attach");
            string request7 = Utils.GetRequest("sign");
            dictionary.Add("partner", request);
            dictionary.Add("ordernumber", request2);
            dictionary.Add("orderstatus", request3);
            dictionary.Add("paymoney", request4);
            dictionary.Add("sysnumber", request5);
            dictionary.Add("attach", request6);
            dictionary.Add("sign", request7);
            return dictionary;
        }

        //同步返回参数
        protected override Dictionary<string, string> GetReturnParam()
        {
            throw new System.NotImplementedException();
        }


        //异步订单状态
		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
            string notifyRequest = GetNotifyRequest("partner");
            string notifyRequest2 = GetNotifyRequest("ordernumber");
            GetNotifyRequest("orderstatus");
            string notifyRequest3 = GetNotifyRequest("paymoney");
            string notifyRequest4 = GetNotifyRequest("sysnumber");
            string notifyRequest5 = GetNotifyRequest("attach");
            string notifyRequest6 = GetNotifyRequest("sign");
			fail.Code = HMNotifyState.Success;
			fail.Data = new HMOrder
			{
				OrderNo = notifyRequest2,
				SupplierOrderNo = notifyRequest4,
				OrderAmt = Utils.StringToDecimal(notifyRequest3, decimal.Zero) * 100m,
				OrderTime = Utils.StringToDateTime("", DateTime.Now).Value
			};
			return fail;
		}


        //同步订单状态
        protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
        {
            throw new System.NotImplementedException();
        }

        //异步验证签名
		public override HMNotifyResult<string> NotifySign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
            HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
            fail.Data = NOTIFY_FAIL;
            string notifyRequest = GetNotifyRequest("partner");
            string notifyRequest2 = GetNotifyRequest("ordernumber");
            string notifyRequest3 = GetNotifyRequest("orderstatus");
            string notifyRequest4 = GetNotifyRequest("paymoney");
            string notifyRequest5 = GetNotifyRequest("sysnumber");
            string notifyRequest6 = GetNotifyRequest("attach");
            string notifyRequest7 = GetNotifyRequest("sign");
            string avalue = EncryUtils.MD5($"partner={notifyRequest}&ordernumber={notifyRequest2}&orderstatus={notifyRequest3}&paymoney={notifyRequest4}{account.Md5Pwd}").ToLower();
/*
            LogUtil.Debug("接收报文1：" + "partner" + "=" + notifyRequest);
            LogUtil.Debug("接收报文2：" + "ordernumber" + "=" + notifyRequest2);
            LogUtil.Debug("接收报文3：" + "orderstatus" + "=" + notifyRequest3);
            LogUtil.Debug("接收报文4：" + "paymoney" + "=" + notifyRequest4);
            LogUtil.Debug("接收报文5：" + "sysnumber" + "=" + notifyRequest5);
            LogUtil.Debug("接收报文6：" + "attach" + "=" + notifyRequest6);
            LogUtil.Debug("接收报文7：" + "sign" + "=" + notifyRequest7);
            LogUtil.Debug("接收报文8：" + "avalue" + "=" + avalue);
*/
            if (avalue.Equals(notifyRequest7))
            {
                decimal d = Utils.StringToDecimal(notifyRequest4, decimal.Zero) * 100m;
                if (order.OrderAmt == d)
                {
                    fail.Code = HMNotifyState.Success;
                    fail.Data = NOTIFY_SUCCESS;
                }
                else
                {
                    fail.Message = "金额验证失败！";
                }
            }
            else
            {
                fail.Message = "签名失败!result_code=" + notifyRequest3;
            }
            fail.Code = HMNotifyState.Success;
            fail.Data = NOTIFY_SUCCESS;
            return fail;
        }

        //同步验证签名
        public override HMNotifyResult<string> ResultSign(HMOrder order, HMSupplier supplier, HMAccount account)
        {
            throw new System.NotImplementedException();
        }
    }
}
