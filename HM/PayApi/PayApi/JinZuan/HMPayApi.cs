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

namespace PayApi.JinZuan
{
    public class HMPayApi : HMPayApiBase
    {
        public override string NOTIFY_SUCCESS => "{\"code\":0,\"message\":\"ok\", \"data\":{}}";
        public override string NOTIFY_FAIL => "fail";
        public override bool IsWithdraw => false;

        private string GetChannelCode(HMChannel channel)
        {
            switch (channel)
            {
                case HMChannel.ALIPAY_NATIVE:
                case HMChannel.ALIPAY_MICROPAY:
                    return "AliPay";
                case HMChannel.WEIXIN_NATIVE:
                case HMChannel.WEIXIN_JSAPI:
                    return "WechatPay";
                default:
                    return string.Empty;
            }
        }

        public override HMMode GetPayMode(HMChannel code)
        {
            return HMMode.跳转链接;
        }

        private long GetTimestamp()
        {
            var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            var t = (DateTime.Now.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
            return t;
        }

        protected override HMPayResult PayGatewayBody(HMOrder order)
        {
            var result = HMPayResult.Fail;
            result.Mode = GetPayMode(order.ChannelCode);

            var userId = new Random().Next(10000, 99999).ToString();
            var timestamp = GetTimestamp();

            var dict = new SortedDictionary<string, string>
            {
                {"jUserId", userId},
                {"jUserIp", order.ClientIp},
                {"jOrderId", order.OrderNo},
                {"orderType", "1"},
                {"payWay", GetChannelCode(order.ChannelCode)},
                {"amount", (order.OrderAmt / 100).ToString("0.00")},
                {"currency", "CNY"},
                {"jExtra", "pay"},
                {"notifyUrl", Supplier.NotifyUri},
                {"merchantId", Account.AccountUser},
                {"timestamp", timestamp.ToString()},
                {"signatureMethod", "HmacSHA256"},
                {"signatureVersion", "1"}
            };

            var signText = "";
            foreach (var item in dict)
            {
                signText += item.Key + "=" + item.Value + "&";
            }

            signText = signText.TrimEnd('&');

            var keyBytes = Encoding.UTF8.GetBytes(Account.Md5Pwd);
            var msgBytes = Encoding.UTF8.GetBytes(signText);

            var signature = "";
            using (var encrypt = new HMACSHA256(keyBytes))
            {
                var hashish = encrypt.ComputeHash(msgBytes);
                signature = BitConverter.ToString(hashish).Replace("-", "").ToUpper();
            }

            var url = Supplier.PostUri + "?signatureMethod=HmacSHA256&signatureVersion=1&merchantId=" +
                      Account.AccountUser + "&timestamp=" + timestamp + "&signature=" + HttpContext.Current.Server.UrlEncode(signature);
            var postData = new SortedDictionary<string, string>
            {
                {"jUserId", userId},
                {"jUserIp", order.ClientIp},
                {"jOrderId", order.OrderNo},
                {"orderType", "1"},
                {"payWay", GetChannelCode(order.ChannelCode)},
                {"amount", (order.OrderAmt / 100).ToString("0.00")},
                {"currency", "CNY"},
                {"jExtra", "pay"},
                {"notifyUrl", Supplier.NotifyUri}
            };

            var parameters = postData.Aggregate("",
                (current, item) => current + (item.Key + "=" + item.Value + "&"));
            parameters = parameters.TrimEnd('&');

            var callback = HttpPost.SendPost(url, parameters, 10000);
            HttpContext.Current.Response.Write(callback);
            try
            {
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

                if (Convert.ToString(jd["code"]) != "0")
                {
                    result.Message = "上游响应：" + Convert.ToString(jd["message"]);
                    return result;
                }

                result.Code = HMPayState.Success;
                result.Message = "success";
                result.Data = Convert.ToString(jd["data"]["paymentUrl"]);

                return result;
            }
            catch (Exception e)
            {
                result.Message = "未获得接口数据! <br />" + e.Message.Trim();
                return result;
            }
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

                allKeys = HttpContext.Current.Request.QueryString.AllKeys;
                foreach (var text in allKeys)
                {
                    var request = Utils.GetRequest(text);
                    dictionary.Add(text, request);
                }

                LogUtil.Debug("金钻GetNotifyParam=" + dictionary.ToJson());
                return dictionary;
            }
            catch (Exception exception)
            {
                LogUtil.Error("金钻获取失败.GetNotifyParam", exception);
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
                    var request = Utils.GetRequest(text);
                    dictionary.Add(text, request);
                }

                allKeys = HttpContext.Current.Request.QueryString.AllKeys;
                foreach (var text in allKeys)
                {
                    var request = Utils.GetRequest(text);
                    dictionary.Add(text, request);
                }

                LogUtil.Debug("金钻GetNotifyParam=" + dictionary.ToJson());
                return dictionary;
            }
            catch (Exception exception)
            {
                LogUtil.Error("金钻获取失败.GetNotifyParam", exception);
                return dictionary;
            }
        }

        protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
        {
            var result = HMNotifyResult<HMOrder>.Fail;
            if (!dic.ContainsKey("status") || (dic["status"] != "2" && dic["status"] != "3"))
            {
                result.Code = HMNotifyState.Fail;
                result.Data = new HMOrder();
                result.Message = "交易失败GetNotifyParam=" + dic.ToJson();

                return result;
            }

            result.Code = HMNotifyState.Success;
            result.Data = new HMOrder
            {
                OrderNo = dic["jOrderId"],
                SupplierOrderNo = dic["orderId"],
                OrderAmt = Convert.ToDecimal(dic["amount"]) * 100,
                OrderTime = DateTime.Now
            };

            return result;
        }

        protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
        {
            var result = HMNotifyResult<HMOrder>.Fail;
            if (!dic.ContainsKey("status") || (dic["status"] != "2" && dic["status"] != "3"))
            {
                result.Code = HMNotifyState.Fail;
                result.Data = new HMOrder();
                result.Message = "交易失败GetNotifyParam=" + dic.ToJson();

                return result;
            }

            result.Code = HMNotifyState.Success;
            result.Data = new HMOrder
            {
                OrderNo = dic["jOrderId"],
                SupplierOrderNo = dic["orderId"],
                OrderAmt = Convert.ToDecimal(dic["amount"]) * 100,
                OrderTime = DateTime.Now
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