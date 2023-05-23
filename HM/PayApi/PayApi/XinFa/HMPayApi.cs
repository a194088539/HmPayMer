using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using HM.Framework;
using HM.Framework.Logging;
using HM.Framework.PayApi;
using LitJson;

namespace PayApi.XinFa
{
    public class HMPayApi : HMPayApiBase
    {
        public override string NOTIFY_SUCCESS => "SUCCESS";
        public override string NOTIFY_FAIL => "FAIL";
        public override bool IsWithdraw => false;

        private string GetChannelCode(HMChannel channel)
        {
            switch (channel)
            {
                case HMChannel.WEIXIN_NATIVE:
                    return "WX";
                case HMChannel.WEIXIN_JSAPI:
                    return "WX_WAP";
                case HMChannel.WEIXIN_H5:
                    return "WX_H5";
                case HMChannel.ALIPAY_NATIVE:
                    return "ZFB";
                case HMChannel.ALIPAY_MICROPAY:
                    return "ZFB_WAP";
                case HMChannel.QQPAY_NATIVE:
                    return "QQ";
                case HMChannel.QQPAY_H5:
                    return "QQ_WAP";
                case HMChannel.JD_NATIVE:
                    return "JD";
                case HMChannel.JD_H5:
                    return "JD_WAP";
                case HMChannel.UNION_PAY_H5:
                    return "UNION_PAY_H5";
                case HMChannel.UNION_WALLET:
                    return "UNION_WALLET";
                case HMChannel.ALIPAY_TO_CARD:
                    return "ZFB_HB_H5";
                case HMChannel.ALIPAY_TO_CARD_H5:
                    return "ZFB_HB";
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

            var domain = HttpContext.Current.Request.Url.Host;
            if (!HttpContext.Current.Request.Url.Port.Equals(80)) domain += ":" + HttpContext.Current.Request.Url.Port;

            var paramdic = new Dictionary<String, String>();
            paramdic.Add("version", "V3.3.0.0");
            paramdic.Add("merchNo", Account.AccountUser);
            paramdic.Add("payType", GetChannelCode(order.ChannelCode));
            paramdic.Add("randomNum", new Random().Next(1000, 9999).ToString());
            paramdic.Add("orderNo", order.OrderNo);
            paramdic.Add("amount", order.OrderAmt.ToString("0"));
            paramdic.Add("goodsName", "iPhone配件");
            paramdic.Add("notifyUrl", Supplier.NotifyUri);
            paramdic.Add("notifyViewUrl", Supplier.ReturnUri);
            paramdic.Add("charsetCode", "UTF-8");

            paramdic = paramdic.OrderBy(o => o.Key).ToDictionary(o => o.Key, pp => pp.Value);

            var sourceStr = paramdic.Aggregate("{", (current, v) => current + $"\"{v.Key}\":\"{v.Value}\",");
            sourceStr = sourceStr.Substring(0, sourceStr.Length - 1) + "}" + Account.Md5Pwd;
            paramdic.Add("sign", MD5Encrypt.MD5(sourceStr, false).ToUpper());

            paramdic = paramdic.OrderBy(o => o.Key).ToDictionary(o => o.Key, pp => pp.Value);
            sourceStr = paramdic.Aggregate("{", (current, v) => current + $"\"{v.Key}\":\"{v.Value}\",");
            sourceStr = sourceStr.Substring(0, sourceStr.Length - 1) + "}";

            var publicKey = Account.RsaPublic;
            var cipher_data = "";
            publicKey = RSAEncodHelper.RSAPublicKeyJava2DotNet(publicKey);

            var cdatabyte = RSAEncodHelper.RSAPublicKeySignByte(sourceStr, publicKey);
            cipher_data = Convert.ToBase64String(cdatabyte);

            var paramstr = "data=" + System.Web.HttpUtility.UrlEncode(cipher_data) + "&merchNo=" + Account.AccountUser;
            var strResult = HttpPost.SendPost(Supplier.PostUri, paramstr, 20000);

            try
            {
                if (string.IsNullOrEmpty(strResult))
                {
                    result.Message = "上游无响应!";
                    return result;
                }

                var paramsDic = new Dictionary<string, string>();
                var jd = JsonMapper.ToObject(strResult);
                if (jd == null)
                {
                    result.Message = "上游响应：" + strResult;
                    return result;
                }

                if ((string)jd["stateCode"] != "00")
                {
                    result.Message = "上游响应：" + (string)jd["msg"];
                    return result;
                }

                paramsDic.Add("merchNo", (string)jd["merchNo"]);
                paramsDic.Add("stateCode", (string)jd["stateCode"]);
                paramsDic.Add("msg", (string)jd["msg"]);
                paramsDic.Add("orderNo", (string)jd["orderNo"]);
                paramsDic.Add("qrcodeUrl", (string)jd["qrcodeUrl"]);

                paramsDic = paramsDic.OrderBy(o => o.Key).ToDictionary(o => o.Key, pp => pp.Value);
                sourceStr = paramsDic.Aggregate("{", (current, v) => current + $"\"{v.Key}\":\"{v.Value}\",");
                sourceStr = sourceStr.Substring(0, sourceStr.Length - 1) + "}" + Account.Md5Pwd;

                if (MD5Encrypt.MD5(sourceStr, false).ToUpper().Equals((string) jd["sign"]) == false)
                {
                    result.Message = "验签失败：" + strResult;
                    return result;
                }

                result.Code = HMPayState.Success;
                result.Message = "success";
                result.Data = (string) jd["qrcodeUrl"];

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
                var allKeys = HttpContext.Current.Request.Form.AllKeys;
                foreach (var text in allKeys)
                {
                    var request = Utils.GetRequest(text);
                    dictionary.Add(text, request);
                }

                LogUtil.Debug("鑫发GetNotifyParam=" + dictionary.ToJson());
                return dictionary;
            }
            catch (Exception exception)
            {
                LogUtil.Error("鑫发获取失败.GetNotifyParam", exception);
                return dictionary;
            }
        }

        protected override Dictionary<string, string> GetReturnParam()
        {
            return new Dictionary<string, string>();
        }

        protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
        {
            const string priKey = "MIICdgIBADANBgkqhkiG9w0BAQEFAASCAmAwggJcAgEAAoGBALTE01FRg+EJYQM5MDRFViE4M7rwaHYClpaVKgHQ/ki9iRal+Y2vqWES6VPfFh/+RqYxer/3S7LkikxC/ENRpEo9x1SY0javEDm88yNPutz7rAaUXjywA9/8KzxPYs56GXgXNH1MvltUl8cHO6NiLcWIkht2RBckLifOm/wKyDAlAgMBAAECgYBy7cy4n83sDm16giqOoSKwfi4NhXm0xdJ+LJoHi5uQHckB+EZgebWDvWVRaL2FW8IJOd4kHJMpSrpO4CvIlSiQ5Aho38PDrFziRPdXyJChLDLS07Hie0ziCi9dGfGPiUgoTb6YVONaChKkahP99+pNwxc1PsuRA/AB7Wb/fM7zBQJBAO7lcoR9y3xQENTPIJ5g8imWdqhZMT5kBv33a9FpY1qyvhpUsQylcHabnQZEwOWOJazNn3+jri6G0Bw2byzFgnMCQQDBtgDrxQW66NELB0VJofZ9Mw2RlrLBOSMZ1Errs4DkQvDHt6WL8kQsSRVWVQQR+aQ2TH84O50OpBfe+LRnnSUHAkA0HKkWbbptq6mAQo6C/fLVT5UitBIh4gAEdmx2id66cjM12zOj5pLZzKg8LdNGP/PH9jLMzQ+55n6PKPCne44xAkAxkiTMMOAVg6yzGMo3XJYaVe07+4eFAN0lp/kANjBeuW6qfSmFFZsyhgVK0Xi4LN1D8/F+b2ryqzVcnuJzvXgTAkEA7jk4Q4aRrfziW/ZzHV/HQUCT83LlSoCxwOE5lGoqDdAoqZUBgCJj3du5oL81YLxeZ4DHvIAocwDAWy1bVWO5Sw==";
            try
            {
                if (!dic.ContainsKey("data")) return new HMNotifyResult<HMOrder>();

                var str = RSAHelper.decryptData(dic["data"], priKey, "utf-8");
                var jd = JsonMapper.ToObject(str);

                LogUtil.Debug("鑫发,NotifyParamToOrder=" + str);

                var result = HMNotifyResult<HMOrder>.Fail;
                result.Code = HMNotifyState.Success;
                result.Data = new HMOrder
                {
                    OrderNo = jd["orderNo"].ToString(),
                    SupplierOrderNo = jd["orderNo"].ToString(),
                    OrderAmt = Convert.ToDecimal(jd["amount"].ToString()),
                    OrderTime = Utils.StringToDateTime(jd["payDate"].ToString(), DateTime.Now).Value
                };

                return result;
            }
            catch (Exception e)
            {
                LogUtil.Error("鑫发解密失败.NotifyParamToOrder", e);
                return new HMNotifyResult<HMOrder>();
            }
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