using HM.Framework.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using WebSocketSharp;
using System.Threading;
using Newtonsoft.Json.Linq;
using HM.Framework.PayApi.Alipay;

namespace HM.Framework.PayApi.BankPay
{
	public class HMPayApiV1 : HMPayApiBase
	{
		public override string NOTIFY_SUCCESS => "SUCCESS";

		public override string NOTIFY_FAIL => "FAIL";

		public override bool IsWithdraw => false;

		private string GetChannelCode(HMChannel channel)
		{
			switch (channel)
			{
			case HMChannel.PAYSAPI_WEIXIN:
				return "WEIXIN_NATIVE";
			case HMChannel.PAYSAPI_ALIPAY:
				return "ALIPAY_NATIVE";
			default:
				return string.Empty;
			}
		}

		public override HMMode GetPayMode(HMChannel code)
		{
			return HMMode.跳转链接;
		}

		protected override HMPayResult PayGatewayBody(HMOrder order)
		{
			HMPayResult fail = HMPayResult.Fail;
			fail.Mode = GetPayMode(order.ChannelCode);
			string channelCode = GetChannelCode(order.ChannelCode);
			if (string.IsNullOrEmpty(channelCode))
			{
				fail.Message = "此接口通道暂不支持";
				return fail;
			}
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            SortedDictionary<string, string> obj = new SortedDictionary<string, string>
                {
                    
                    {
                        "total_fee",
                        ((int)order.OrderAmt).ToString()
                    },
                    {
                        "trade_type",
                        channelCode
                    },
                    {
                        "out_trade_no",
                        order.OrderNo
                    },
                    {
                        "nonce",
                        Guid.NewGuid().ToString("N")
                    },
                    {
                        "timestamp",
                        Convert.ToInt64(ts.TotalSeconds).ToString()
                    }
                };
            string signstr = Alipay.Core.CreateLinkString(obj) + "&key=" + base.Account.Md5Pwd;
            string sign = EncryUtils.MD5(signstr).ToUpper();
            obj.Add("sign", sign);
            bool notready = true;
            DateTime dateTime = DateTime.Now;
            try
            {
                using (var ws = new WebSocket(base.Supplier.PostUri))
                {
                    ws.OnMessage += (sender, e) =>
                    {
                        try
                        {
                            Command command = Command.Parse(e.Data);
                            if(command != null)
                            {
                                if (command.Code != 0)
                                {
                                    fail.Message = command.Msg;
                                }
                                else
                                {
                                    JObject data = command.GetData<JObject>();
                                    fail.Code = HMPayState.Success;
                                    fail.Data = data["qrCode"].ToString();
                                }
                            }
                            notready = false;
                        }
                        catch(Exception ex)
                        {
                            LogUtil.Error("每日付,WebSocket OnMessage ,订单号:" + order.OrderNo, ex);
                            notready = false;
                        }
                    };
                    ws.OnError += (sender, e) =>
                    {
                        LogUtil.Error("每日付,WebSocket OnError ,订单号:" + order.OrderNo , e.Exception);
                    };
                    ws.OnOpen += (sender, e) =>
                    {
                        ws.Send(Command.ToJson(0, "pay", obj));
                    };

                    ws.Connect();
                    while(notready)
                    {
                        Thread.Sleep(200);
                        if(DateTime.Now.Subtract(dateTime).TotalSeconds > 10)
                        {
                            ws.Close();
                            throw new Exception("请求超时");
                        }
                    }
                }
                if(fail.Code == HMPayState.Success)
                {
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
                            fail.Data
                        }
                    };
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendFormat("<form name='submit' action='{0}' _input_charset='utf-8' method='post'>", "/qrpay/");
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

                fail.Mode = HMMode.输出字符串;
                return fail;
            }
            catch (Exception exception)
            {
                fail.Message = "系统繁忙，请稍候再试！";
                LogUtil.Error("每日付,订单号:" + order.OrderNo, exception);
                return fail;
            }

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
                LogUtil.Debug("每日付GetNotifyParam=" + dictionary.ToJson());
                return dictionary;
            }
            catch (Exception exception)
			{
				LogUtil.Error("每日付获取失败.GetNotifyParam", exception);
				return dictionary;
			}
		}

		protected override Dictionary<string, string> GetReturnParam()
		{
			return new Dictionary<string, string>();
		}

		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
            LogUtil.Debug("每日付,NotifyParamToOrder=" + dic.ToJson());
            HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
            
            fail.Code = HMNotifyState.Success;
            fail.Data = new HMOrder
            {
                OrderNo = dic["out_trade_no"],
                SupplierOrderNo = dic["trade_no"],
                OrderAmt = Utils.StringToDecimal(dic["total_fee"], decimal.Zero), 
                OrderTime = Utils.StringToDateTime(dic["pay_time"], DateTime.Now).Value
            };

            return fail;
        }

		protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			fail.Code = HMNotifyState.WaitAccountInit;
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
            string signstr = "out_trade_no=" + notifyParams["out_trade_no"] +
                    "&pay_time=" + notifyParams["pay_time"] +
                    "&total_fee=" + notifyParams["total_fee"] +
                    "&trade_no=" + notifyParams["trade_no"];

            string sign = GetNotifyRequest("sign");
            if (AlipayMD5.Sign(signstr, "&key=" + supplier.Account.Md5Pwd, "utf-8").Equals(sign.ToLower()))
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

		public override HMNotifyResult<string> ResultSign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			fail.Code = HMNotifyState.Success;
			fail.Data = NOTIFY_SUCCESS;
			return fail;
		}
	}
}
