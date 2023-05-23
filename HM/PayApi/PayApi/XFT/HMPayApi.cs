using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using HM.Framework;
using HM.Framework.Logging;
using HM.Framework.PayApi;
using LitJson;

namespace PayApi.XFT
{
    public class HMPayApi : HMPayApiBase
    {
        public override string NOTIFY_SUCCESS => "success";
        public override string NOTIFY_FAIL => "fail";
        public override bool IsWithdraw => false;


        private string GetChannelCode(HMChannel channel)
        {
            switch (channel)
            {
                case HMChannel.ALIPAY_NATIVE:
                    return "ALIPAY";
                case HMChannel.ALIPAY_MICROPAY:
                    return "ALIPAY";
                case HMChannel.WEIXIN_NATIVE:
                    return "WXPAY";
                case HMChannel.WEIXIN_JSAPI:
                    return "WXPAY";
                case HMChannel.UNION_WALLET:
                    return "UNIONQRPAY";
                case HMChannel.KUAIJIE:
                    return "EASYQUICK";
                default:
                    return "UNIONPAY";
            }
        }

        public override HMMode GetPayMode(HMChannel code)
        {
            return HMMode.输出字符串;
        }

        protected override HMPayResult PayGatewayBody(HMOrder order)
        {
            var result = HMPayResult.Fail;
            result.Mode = GetPayMode(order.ChannelCode);

            var d = new Dictionary<string, string>
            {
                {"orderNo", order.OrderNo},
                {"totalFee", (order.OrderAmt / 100).ToString("0.00")},
                {"defaultbank", GetChannelCode(order.ChannelCode)},
                {"title", "test"},
                {"paymethod", "directPay"},
                {"service", "online_pay"},
                {"paymentType", "1"},
                {"merchantId", Account.AccountUser},
                {"returnUrl", Supplier.ReturnUri},
                {"notifyUrl", Supplier.NotifyUri},
                {"charset", "UTF-8"},
                {"body", "bodys"},
                {"isApp", "web"}
            };
            d.Add("sign", Utils.GetSHAstr(d, Account.Md5Pwd));
            d.Add("signType", "SHA");

            var builder = new StringBuilder();
            builder.Append("<html>");
            builder.Append("<head>");
            builder.Append("<meta charset=\"utf-8\">");
            builder.Append("<title>loading...</title>");
            builder.Append("</head>");

            builder.Append("<body onload=\"document.form1.submit();\">");
            builder.Append("<form name=\"form1\" method=\"post\" action=\"" + Supplier.PostUri + "\">");

            foreach (var item in d)
            {
                builder.Append("<input type=\"hidden\" name=\"" + item.Key + "\" value=\"" + item.Value + "\" />");
            }

            builder.Append("</form>");
            builder.Append("</body>");

            builder.Append("</html>");

            result.Code = HMPayState.Success;
            result.Message = "ok!";
            result.Data = Convert.ToString(builder);

            return result;

            //var response = Utils.Post(Supplier.PostUri + order.OrderNo, d);

            //try
            //{
            //    if (string.IsNullOrEmpty(response))
            //    {
            //        result.Message = "上游无响应!";
            //        return result;
            //    }

            //    var paramsDic = new Dictionary<string, string>();
            //    var jd = JsonMapper.ToObject(response);
            //    if (jd == null)
            //    {
            //        result.Message = "上游响应：" + response;
            //        return result;
            //    }

            //    if ((string)jd["respCode"] != "00")
            //    {
            //        result.Message = "上游响应：" + (string)jd["respMessage"];
            //        return result;
            //    }

            //    result.Code = HMPayState.Success;
            //    result.Message = "success";
            //    result.Data = (string)jd["codeUrl"];

            //    return result;
            //}
            //catch (Exception e)
            //{
            //    result.Message = "未获得接口数据!";
            //    return result;
            //}
        }

        protected override Dictionary<string, string> GetNotifyParam()
        {
            var dictionary = new Dictionary<string, string>();
            try
            {
                var allKeys = HttpContext.Current.Request.Form.AllKeys;
                foreach (var text in allKeys)
                {
                    var request = HM.Framework.Utils.GetRequest(text);
                    dictionary.Add(text, request);
                }

                LogUtil.Debug("信付通GetNotifyParam=" + dictionary.ToJson());
                return dictionary;
            }
            catch (Exception exception)
            {
                LogUtil.Error("信付通获取失败.GetNotifyParam", exception);
                return dictionary;
            }
        }

        protected override Dictionary<string, string> GetReturnParam()
        {
            var dictionary = new Dictionary<string, string>();
            try
            {
                var allKeys = HttpContext.Current.Request.Form.AllKeys;
                foreach (var text in allKeys)
                {
                    var request = HM.Framework.Utils.GetRequest(text);
                    dictionary.Add(text, request);
                }

                LogUtil.Debug("信付通GetNotifyParam=" + dictionary.ToJson());
                return dictionary;
            }
            catch (Exception exception)
            {
                LogUtil.Error("信付通获取失败.GetNotifyParam", exception);
                return dictionary;
            }
        }

        protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
        {
            var result = HMNotifyResult<HMOrder>.Fail;
            if (!dic.ContainsKey("trade_status") || dic["trade_status"] != "TRADE_FINISHED")
            {
                result.Code = HMNotifyState.Fail;
                result.Data = new HMOrder();
                result.Message = "交易失败GetNotifyParam=" + dic.ToJson();

                return result;
            }

            result.Code = HMNotifyState.Success;
            result.Data = new HMOrder
            {
                OrderNo = dic["order_no"],
                SupplierOrderNo = dic["trade_no"],
                OrderAmt = Convert.ToDecimal(dic["total_fee"]) * 100,
                OrderTime = HM.Framework.Utils.StringToDateTime(dic["gmt_payment"], DateTime.Now).Value
            };

            return result;
        }

        protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
        {
            var result = HMNotifyResult<HMOrder>.Fail;
            if (!dic.ContainsKey("trade_status") || dic["trade_status"] != "TRADE_FINISHED")
            {
                result.Code = HMNotifyState.Fail;
                result.Data = new HMOrder();
                result.Message = "交易失败GetNotifyParam=" + dic.ToJson();

                return result;
            }

            result.Code = HMNotifyState.Success;
            result.Data = new HMOrder
            {
                OrderNo = dic["order_no"],
                SupplierOrderNo = dic["trade_no"],
                OrderAmt = Convert.ToDecimal(dic["total_fee"]) * 100,
                OrderTime = HM.Framework.Utils.StringToDateTime(dic["gmt_payment"], DateTime.Now).Value
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