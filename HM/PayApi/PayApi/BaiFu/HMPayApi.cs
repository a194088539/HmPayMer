using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HM.Framework;
using HM.Framework.Logging;
using HM.Framework.PayApi;
using LitJson;
using Newtonsoft.Json;

namespace PayApi.BaiFu
{
    public class HMPayApi : HMPayApiBase
    {
        public override string NOTIFY_SUCCESS => "000000";
        public override string NOTIFY_FAIL => "FAIL";
        public override bool IsWithdraw => false;

        private string GetChannelCode(HMChannel channel)
        {
            switch (channel)
            {
                case HMChannel.ALIPAY_NATIVE:
                    return "ZFB";
                case HMChannel.ALIPAY_MICROPAY:
                    return "ZFB_WAP";
                case HMChannel.WEIXIN_NATIVE:
                    return "WX";
                case HMChannel.WEIXIN_JSAPI:
                    return "WX_WAP";
                case HMChannel.QQPAY_NATIVE:
                    return "QQ";
                case HMChannel.QQPAY_H5:
                    return "QQ_WAP";
                case HMChannel.JD_NATIVE:
                    return "JDQB";
                case HMChannel.JD_H5:
                    return "JD_WAP";
                case HMChannel.UNION_WALLET:
                    return "YL";
                default:
                    return string.Empty;
            }
        }

        public override HMMode GetPayMode(HMChannel code)
        {
            switch (code)
            {
                case HMChannel.ALIPAY_NATIVE:
                case HMChannel.JD_NATIVE:
                case HMChannel.UNION_WALLET:
                case HMChannel.QQPAY_NATIVE:
                    return HMMode.跳转扫码页面;
                default:
                    return HMMode.跳转链接;
            }
        }

        protected override HMPayResult PayGatewayBody(HMOrder order)
        {
            var result = HMPayResult.Fail;
            result.Mode = GetPayMode(order.ChannelCode);

            var dict = new Dictionary<string, string>
            {
                {"merchantNo", Account.AccountUser},
                {"netwayCode", GetChannelCode(order.ChannelCode)},
                {"randomNum", new Random().Next(1000, 9999).ToString()},
                {"orderNum", order.OrderNo},
                {"payAmount", order.OrderAmt.ToString("0")},
                {"goodsName", "iPhone配件"},
                {"callBackUrl", Supplier.NotifyUri},
                {"frontBackUrl", Supplier.ReturnUri},
                {"requestIP", order.ClientIp}
            };

            dict = dict.OrderBy(o => o.Key).ToDictionary(o => o.Key, pp => pp.Value);

            var sourceStr = dict.Aggregate("{", (current, v) => current + $"\"{v.Key}\":\"{v.Value}\",");
            sourceStr = sourceStr.Substring(0, sourceStr.Length - 1) + "}" + Account.Md5Pwd;
            dict.Add("sign", MD5Encrypt.MD5(sourceStr, false).ToUpper());

            dict = dict.OrderBy(o => o.Key).ToDictionary(o => o.Key, pp => pp.Value);
            sourceStr = dict.Aggregate("{", (current, v) => current + $"\"{v.Key}\":\"{v.Value}\",");
            sourceStr = sourceStr.Substring(0, sourceStr.Length - 1) + "}";

            var strResult = HttpPost.SendPost(Supplier.PostUri, "paramData=" + sourceStr, 10000);
            try
            {
                if (string.IsNullOrEmpty(strResult))
                {
                    result.Message = "上游无响应!";
                    return result;
                }

                var jd = JsonMapper.ToObject(strResult);
                if (jd == null)
                {
                    result.Message = "上游响应：" + strResult;
                    return result;
                }

                if ((string)jd["resultCode"] != "00")
                {
                    result.Message = "上游响应：" + (string)jd["resultMsg"];
                    return result;
                }

                result.Code = HMPayState.Success;
                result.Message = "success";
                result.Data = (string)jd["CodeUrl"];

                return result;
            }
            catch (Exception e)
            {
                result.Message = "未获得接口数据!";
                return result;
            }

        }

        protected override Dictionary<string, string> GetNotifyParam()
        {
            var dictionary = new Dictionary<string, string>();
            try
            {
                var paramData = HttpContext.Current.Request.Form["paramData"] == null
                    ? ""
                    : HttpContext.Current.Request.Form["paramData"].Trim();
                if (string.IsNullOrEmpty(paramData))
                {
                    LogUtil.Debug("佰富GetNotifyParam=" + paramData);

                    return dictionary;
                }

                var jd = JsonMapper.ToObject(paramData);
                if (jd == null)
                {
                    LogUtil.Debug("佰富GetNotifyParam=" + paramData);

                    return dictionary;
                }

                dictionary.Add("merchantNo", (string)jd["merchantNo"]);
                dictionary.Add("netwayCode", (string)jd["netwayCode"]);
                dictionary.Add("orderNum", (string)jd["orderNum"]);
                dictionary.Add("payAmount", (string)jd["payAmount"]);
                dictionary.Add("goodsName", (string)jd["goodsName"]);
                dictionary.Add("resultCode", (string)jd["resultCode"]);
                dictionary.Add("payDate", (string)jd["payDate"]);
                dictionary.Add("sign", (string)jd["sign"]);

                LogUtil.Debug("佰富GetNotifyParam=" + dictionary.ToJson());
                return dictionary;
            }
            catch (Exception exception)
            {
                LogUtil.Error("佰富获取失败.GetNotifyParam", exception);
                return dictionary;
            }
        }

        protected override Dictionary<string, string> GetReturnParam()
        {
            var dictionary = new Dictionary<string, string>();
            try
            {
                var paramData = HttpContext.Current.Request.Form["paramData"] == null
                    ? ""
                    : HttpContext.Current.Request.Form["paramData"].Trim();
                if (string.IsNullOrEmpty(paramData))
                {
                    LogUtil.Debug("佰富GetNotifyParam=" + paramData);

                    return dictionary;
                }

                var jd = JsonMapper.ToObject(paramData);
                if (jd == null)
                {
                    LogUtil.Debug("佰富GetNotifyParam=" + paramData);

                    return dictionary;
                }

                dictionary.Add("merchantNo", (string)jd["merchantNo"]);
                dictionary.Add("netwayCode", (string)jd["netwayCode"]);
                dictionary.Add("orderNum", (string)jd["orderNum"]);
                dictionary.Add("payAmount", (string)jd["payAmount"]);
                dictionary.Add("goodsName", (string)jd["goodsName"]);
                dictionary.Add("resultCode", (string)jd["resultCode"]);
                dictionary.Add("payDate", (string)jd["payDate"]);
                dictionary.Add("sign", (string)jd["sign"]);

                LogUtil.Debug("佰富GetNotifyParam=" + dictionary.ToJson());
                return dictionary;
            }
            catch (Exception exception)
            {
                LogUtil.Error("佰富获取失败.GetNotifyParam", exception);
                return dictionary;
            }
        }

        protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
        {
            var result = HMNotifyResult<HMOrder>.Fail;
            if (!dic.ContainsKey("resultCode") || dic["resultCode"] != "00")
            {
                result.Code = HMNotifyState.Fail;
                result.Data = new HMOrder();
                result.Message = "交易失败GetNotifyParam=" + dic.ToJson();

                return result;
            }

            result.Code = HMNotifyState.Success;
            result.Data = new HMOrder
            {
                OrderNo = dic["orderNum"],
                SupplierOrderNo = dic["orderNum"],
                OrderAmt = Convert.ToDecimal(dic["payAmount"]),
                OrderTime = Utils.StringToDateTime(dic["payDate"], DateTime.Now).Value
            };

            return result;
        }

        protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
        {
            var fail = HMNotifyResult<HMOrder>.Fail;
            fail.Code = HMNotifyState.WaitAccountInit;
            return fail;
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
            fail.Code = HMNotifyState.Success;
            fail.Data = NOTIFY_SUCCESS;
            return fail;
        }
    }
}