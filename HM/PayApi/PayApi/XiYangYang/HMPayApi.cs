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

namespace PayApi.XiYangYang
{
    public class HMPayApi : HMPayApiBase
    {
        public override string NOTIFY_SUCCESS => "success";
        public override string NOTIFY_FAIL => "fail";
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
                case HMChannel.WEIXIN_NATIVE:
                    return "9004";
                case HMChannel.ALIPAY_NATIVE:
                    return "9002";
                case HMChannel.UNION_WALLET:
                    return "9005";
                case HMChannel.WXTOCARD:
                    return "9008";
                case HMChannel.EBank:
                    return "9010";
                default:
                    return "";
            }
        }

        public override HMMode GetPayMode(HMChannel code)
        {
            return HMMode.跳转链接;
        }

        protected override HMPayResult PayGatewayBody(HMOrder order)
        {
            var result = HMPayResult.Fail;
            result.Mode = GetPayMode(order.ChannelCode);

            var domain = HttpContext.Current.Request.Url.Host;
            if (!HttpContext.Current.Request.Url.Port.Equals(80)) domain += ":" + HttpContext.Current.Request.Url.Port;

            var dict = new SortedDictionary<string, string>
            {
                {"mchId", Account.AccountUser},
                {"appId", Account.AppId},
                {"productId", GetChannelCode(order.ChannelCode)},
                {"mchOrderNo", order.OrderNo},
                {"currency", "cny"},
                {"amount", order.OrderAmt.ToString("0")},
                {"clientIp", order.ClientIp},
                {"device", ""},
                {"returnUrl", Supplier.ReturnUri},
                {"notifyUrl", Supplier.NotifyUri},
                {"subject", "pay"},
                {"body", "pay"},
                {"param1", ""},
                {"param2", ""},
                {"extra", ""}
            };

            var signText = dict.Where(item => !string.IsNullOrEmpty(item.Value)).Aggregate("", (current, item) => current + (item.Key + "=" + item.Value + "&"));
            signText += "key=" + Account.Md5Pwd;

            dict.Add("sign", MD5(signText, false).ToUpper());

            var parameters = dict.Aggregate("", (current, item) => current + (item.Key + "=" + item.Value + "&"));
            parameters = parameters.TrimEnd('&');

            var callback = HttpPost.SendPost(Supplier.PostUri, parameters);
            if (string.IsNullOrEmpty(callback))
            {
                result.Message = "上游无响应!";
                return result;
            }

            try
            {
                var jd = JsonMapper.ToObject(callback);
                if (jd == null)
                {
                    result.Message = "上游响应：" + callback;
                    return result;
                }

                if (!jd.ContainsKey("retCode") || jd["retCode"].ToString() != "SUCCESS")
                {
                    result.Message = "上游响应：" + callback;
                    return result;
                }

                result.Code = HMPayState.Success;
                result.Message = "success";
                result.Data = jd["payUrl"].ToString();

                return result;
            }
            catch (Exception e)
            {
                HttpContext.Current.Response.Write("解析上游数据格式失败，请重新尝试！");

                result.Message = "解析上游数据格式失败，请重新尝试！";
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

                LogUtil.Debug("喜羊羊GetNotifyParam=" + dictionary.ToJson());
                return dictionary;
            }
            catch (Exception exception)
            {
                LogUtil.Error("喜羊羊获取失败.GetNotifyParam", exception);
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

                LogUtil.Debug("喜羊羊GetNotifyParam=" + dictionary.ToJson());
                return dictionary;
            }
            catch (Exception exception)
            {
                LogUtil.Error("喜羊羊获取失败.GetNotifyParam", exception);
                return dictionary;
            }
        }

        protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
        {
            var result = HMNotifyResult<HMOrder>.Fail;
            if (!dic.ContainsKey("status"))
            {
                result.Code = HMNotifyState.Fail;
                result.Data = new HMOrder();
                result.Message = "交易失败GetNotifyParam=" + dic.ToJson();

                return result;
            }

            if (dic["status"] != "2" && dic["status"] != "3")
            {
                result.Code = HMNotifyState.Fail;
                result.Data = new HMOrder();
                result.Message = "交易失败GetNotifyParam=" + dic.ToJson();

                return result;
            }

            result.Code = HMNotifyState.Success;
            result.Data = new HMOrder
            {
                OrderNo = dic["mchOrderNo"],
                SupplierOrderNo = dic["payOrderId"],
                OrderAmt = Convert.ToDecimal(dic["amount"]),
                OrderTime = Utils.StringToDateTime(dic["paySuccTime"], DateTime.Now).Value
            };

            return result;
        }

        protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
        {
            var result = HMNotifyResult<HMOrder>.Fail;
            if (!dic.ContainsKey("status"))
            {
                result.Code = HMNotifyState.Fail;
                result.Data = new HMOrder();
                result.Message = "交易失败GetNotifyParam=" + dic.ToJson();

                return result;
            }

            if (dic["status"] != "2" && dic["status"] != "3")
            {
                result.Code = HMNotifyState.Fail;
                result.Data = new HMOrder();
                result.Message = "交易失败GetNotifyParam=" + dic.ToJson();

                return result;
            }

            result.Code = HMNotifyState.Success;
            result.Data = new HMOrder
            {
                OrderNo = dic["mchOrderNo"],
                SupplierOrderNo = dic["payOrderId"],
                OrderAmt = Convert.ToDecimal(dic["amount"]),
                OrderTime = Utils.StringToDateTime(dic["paySuccTime"], DateTime.Now).Value
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