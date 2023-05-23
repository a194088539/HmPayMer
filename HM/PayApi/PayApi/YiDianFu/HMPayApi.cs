using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using HM.Framework.PayApi;

namespace PayApi.YiDianFu
{
    public class HMPayApi : HMPayApiBase
    {
        public override string NOTIFY_SUCCESS => "SUCCESS";
        public override string NOTIFY_FAIL => "Fail";
        public override bool IsWithdraw => false;

        private string GetChannelCode(HMChannel channel)
        {
            switch (channel)
            {
                case HMChannel.ALIPAY_NATIVE:
                    return "ALIPAY_NATIVE";
                case HMChannel.ALIPAY_MICROPAY:
                    return "ALIPAY_MICROPAY";
                case HMChannel.WEIXIN_NATIVE:
                    return "WEIXIN_NATIVE";
                case HMChannel.WEIXIN_JSAPI:
                    return "WEIXIN_JSAPI";
                case HMChannel.QQPAY_NATIVE:
                    return "QQPAY_NATIVE";
                case HMChannel.QQPAY_H5:
                    return "QQPAY_H5";
                case HMChannel.JD_NATIVE:
                    return "JD_NATIVE";
                case HMChannel.JD_H5:
                    return "JD_H5";
                case HMChannel.UNION_WALLET:
                    return "UNION_WALLET";
                default:
                    return string.Empty;
            }
        }

        private string MD5(string str, bool half)
        {
            byte[] b = Encoding.UTF8.GetBytes(str);
            b = new MD5CryptoServiceProvider().ComputeHash(b);
            string ret = "";
            for (int i = 0; i < b.Length; i++)
            {
                ret += b[i].ToString("x").PadLeft(2, '0');
            }

            if (half)
            {
                ret = ret.Substring(8, 16);
            }

            return ret;
        }

        public override HMMode GetPayMode(HMChannel code)
        {
            return HMMode.输出字符串;
        }

        protected override HMPayResult PayGatewayBody(HMOrder order)
        {
            var result = HMPayResult.Fail;
            result.Mode = GetPayMode(order.ChannelCode);

            var dict = new Dictionary<string, string>
            {
                {"app_id", Account.AccountUser},
                {"trade_type", GetChannelCode(order.ChannelCode)},
                {"total_amount", order.OrderAmt.ToString("0")},
                {"out_trade_no", order.OrderNo},
                {"notify_url", Supplier.NotifyUri},
                {"interface_version", "V2.0"},
                {"return_url", Supplier.ReturnUri},
                {"extra_return_param", ""},
                {"client_ip", order.ClientIp}
            };

            var signText = "app_id=" + dict["app_id"] + "&trade_type=" + dict["trade_type"] + "&total_amount=" +
                           dict["total_amount"] + "&out_trade_no=" + dict["out_trade_no"] + "&notify_url=" + dict["notify_url"] +
                           Account.Md5Pwd;
            dict.Add("sign", MD5(signText, false).ToLower());

            var builder = new StringBuilder();
            builder.Append("<html>");
            builder.Append("<head>");
            builder.Append("<meta charset=\"utf-8\">");
            builder.Append("<title>loading...</title>");
            builder.Append("</head>");
            builder.Append("<body onload=\"javascript:document.form1.submit();\">");
            builder.Append("<form name=\"form1\" method=\"post\" action=\"" + Supplier.PostUri + "\">");

            foreach (var item in dict)
            {
                builder.Append("<input type=\"hidden\" name=\"" + item.Key + "\" value=\"" + item.Value + "\" />");
            }

            builder.Append("</form>");
            builder.Append("</body>");
            builder.Append("</html>");

            result.Code = HMPayState.Success;
            result.Message = "success";
            result.Data = Convert.ToString(builder);

            return result;
        }

        protected override Dictionary<string, string> GetNotifyParam()
        {
            throw new System.NotImplementedException();
        }

        protected override Dictionary<string, string> GetReturnParam()
        {
            throw new System.NotImplementedException();
        }

        protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
        {
            throw new System.NotImplementedException();
        }

        protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
        {
            throw new System.NotImplementedException();
        }

        public override HMNotifyResult<string> NotifySign(HMOrder order, HMSupplier supplier, HMAccount account)
        {
            throw new System.NotImplementedException();
        }

        public override HMNotifyResult<string> ResultSign(HMOrder order, HMSupplier supplier, HMAccount account)
        {
            throw new System.NotImplementedException();
        }
    }
}