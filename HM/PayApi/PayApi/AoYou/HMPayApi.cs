using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using HM.Framework;
using HM.Framework.Logging;
using HM.Framework.PayApi;
using LitJson;

namespace PayApi.AoYou
{
    public class HMPayApi : HMPayApiBase
    {
        public override string NOTIFY_SUCCESS => "ok";
        public override string NOTIFY_FAIL => "Fail";
        public override bool IsWithdraw => false;

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
        private string GetChannelCode(HMChannel channel)
        {
            switch (channel)
            {
                case HMChannel.ALIPAY_NATIVE:
                    return "903";
                case HMChannel.ALIPAY_MICROPAY:
                    return "945";
                case HMChannel.ALIPAY_H5:
                    return "933";
                case HMChannel.WEIXIN_H5:
                    return "952";
                default:
                    return string.Empty;
            }
        }

        public override HMMode GetPayMode(HMChannel code)
        {
            return code.Equals(HMChannel.ALIPAY_NATIVE) ? HMMode.跳转链接 : HMMode.输出字符串;
        }

        protected override HMPayResult PayGatewayBody(HMOrder order)
        {
            var result = HMPayResult.Fail;
            result.Mode = GetPayMode(order.ChannelCode);
            
            var dict = new Dictionary<string, string>();
            dict.Add("pay_orderid", order.OrderNo);
            dict.Add("pay_amount", order.OrderAmt.ToString("0"));
            dict.Add("pay_memberid", Account.AccountUser);
            dict.Add("pay_applydate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("pay_bankcode", GetChannelCode(order.ChannelCode));
            dict.Add("pay_notifyurl", Supplier.NotifyUri);
            dict.Add("pay_callbackurl", Supplier.ReturnUri);

            dict = dict.OrderBy(o => o.Key).ToDictionary(o => o.Key, pp => pp.Value);

            var signText = dict.Aggregate("", (current, item) => current + (item.Key + "=" + item.Value + "&"));

            signText += "key=" + Account.Md5Pwd;

            dict.Add("pay_attach", "");
            dict.Add("pay_productname", "");
            dict.Add("pay_md5sign", MD5(signText, false).ToUpper());

            if (order.ChannelCode.Equals(HMChannel.ALIPAY_NATIVE))
            {
                var parameters = dict.Aggregate("", (current, item) => current + (item.Key + "=" + item.Value + "&"));
                parameters = parameters.TrimEnd('&');

                var callback = HttpPost.SendPost(Supplier.PostUri, parameters);
                if (string.IsNullOrEmpty(callback))
                {
                    result.Message = "上游无响应!";
                    return result;
                }

                var jd = JsonMapper.ToObject(callback);
                if (jd == null)
                {
                    result.Message = "上游响应：" + callback;
                    return result;
                }

                if (!jd.ContainsKey("returncode") || jd["returncode"].ToString() != "0")
                {
                    result.Message = "上游响应：" + callback;
                    return result;
                }

                result.Code = HMPayState.Success;
                result.Message = "success";
                result.Data = jd["url"].ToString();

                return result;
            }

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
                builder.Append("<input type=\"hidden\" name=\"" + item.Key + "\" value=\"" + item.Value + "\">");
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
            var dictionary = new Dictionary<string, string>();
            try
            {
                var allKeys = HttpContext.Current.Request.Form.AllKeys;
                foreach (var text in allKeys)
                {
                    var request = Utils.GetRequest(text);
                    dictionary.Add(text, request);
                }

                LogUtil.Debug("遨游GetNotifyParam=" + dictionary.ToJson());
                return dictionary;
            }
            catch (Exception exception)
            {
                LogUtil.Error("遨游获取失败.GetNotifyParam", exception);
                return dictionary;
            }
        }

        protected override Dictionary<string, string> GetReturnParam()
        {
            var dictionary = new Dictionary<string, string>();
            try
            {
                var allKeys = HttpContext.Current.Request.QueryString.AllKeys;
                foreach (var text in allKeys)
                {
                    var request = Utils.GetRequest(text);
                    dictionary.Add(text, request);
                }

                LogUtil.Debug("遨游GetReturnParam=" + dictionary.ToJson());
                return dictionary;
            }
            catch (Exception exception)
            {
                LogUtil.Error("遨游获取失败.GetReturnParam", exception);
                return dictionary;
            }
        }

        protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
        {
            var result = HMNotifyResult<HMOrder>.Fail;
            if (!dic.ContainsKey("returncode") || dic["returncode"] != "00")
            {
                result.Code = HMNotifyState.Fail;
                result.Data = new HMOrder();
                result.Message = "交易失败GetNotifyParam=" + dic.ToJson();

                return result;
            }

            result.Code = HMNotifyState.Success;
            result.Data = new HMOrder
            {
                OrderNo = dic["orderid"],
                SupplierOrderNo = dic["transaction_id"],
                OrderAmt = Convert.ToDecimal(dic["amount"])*100,
                OrderTime = Utils.StringToDateTime(dic["datetime"], DateTime.Now).Value
            };

            return result;
        }

        protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
        {
            var result = HMNotifyResult<HMOrder>.Fail;
            if (!dic.ContainsKey("returncode") || dic["returncode"] != "00")
            {
                result.Code = HMNotifyState.Fail;
                result.Data = new HMOrder();
                result.Message = "交易失败GetNotifyParam=" + dic.ToJson();

                return result;
            }

            result.Code = HMNotifyState.Success;
            result.Data = new HMOrder
            {
                OrderNo = dic["orderid"],
                SupplierOrderNo = dic["transaction_id"],
                OrderAmt = Convert.ToDecimal(dic["amount"]) / 100,
                OrderTime = Utils.StringToDateTime(dic["datetime"], DateTime.Now).Value
            };

            return result;
        }

        public override HMNotifyResult<string> NotifySign(HMOrder order, HMSupplier supplier, HMAccount account)
        {
            var fail = HMNotifyResult<string>.Fail;
            if (true)
            {
                fail.Code = HMNotifyState.Success;
                fail.Data = NOTIFY_SUCCESS;
            }

            return fail;
        }

        public override HMNotifyResult<string> ResultSign(HMOrder order, HMSupplier supplier, HMAccount account)
        {
            var fail = HMNotifyResult<string>.Fail;
            if (true)
            {
                fail.Code = HMNotifyState.Success;
                fail.Data = NOTIFY_SUCCESS;
            }

            return fail;
        }
    }
}