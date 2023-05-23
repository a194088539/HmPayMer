using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BankPayWSServer.BankPayServer
{
    public class OrderInfo
    {
        public enum PayType
        {
            WeixinPay, Alipay
        }


        public string InterfaceCode { get; set; }
        /// <summary>
        /// 分
        /// </summary>
        public decimal Amount { get; set; }
        public DateTime OrderTime { get; set; }

        /// <summary>
        /// 0 未付款 1已付款
        /// </summary>
        public int OrderState { get; set; }

        public string OrderId { get; set; }
        /// <summary>
        /// 支付平台订单号
        /// </summary>
        public string TradeOrderNo { get; set; }
        /// <summary>
        /// 通道，WEIXIN_NATIVE ALIPAY_NATIVE
        /// </summary>
        public string ChannelCode { get; set; }

        public PayType GetPayType()
        {
            if(ChannelCode == "WEIXIN_NATIVE")
            {
                return PayType.WeixinPay;
            }else
            {
                return PayType.Alipay;
            }
        }

        public int InterfaceIndex { get; set; }
        public DateTime CheckTime { get; set; }
        public DateTime PayTime { get; set; }
        /// <summary>
        /// 回调时间
        /// </summary>
        public DateTime CallBackTime { get; set; }
        /// <summary>
        /// 回调状态
        /// </summary>
        public int CallBackState { get; set; }

        public Task<HttpResponseMessage> CallbackTask { get; set; }

        public string SignMd5 { get; set; }

    }

    public class InterfaceInfo
    {
        Dictionary<OrderInfo.PayType, decimal> _successOrderAmount = new Dictionary<OrderInfo.PayType, decimal>();
        public InterfaceInfo()
        {
            WeixinEnable = true;
            AlipayEnable = true;
            Enable = true;
        }
        //平台相关信息
        public string InterfaceCode { get; set; }
        public string InterfaceAccount { get; set; }
        public string InterfaceMd5 { get; set; }

        public string InterfaceRsaPublic { get; set; }
        public string InterfaceRsaPrivate { get; set; }

        public bool Enable { get; set; }//是否启用

        public bool WeixinEnable { get; set; }//微信是否启用
        public bool AlipayEnable { get; set; }//支付宝是否启用
        //支付相关信息
        public string key { get; set; }
        public int cashid { get; set; }
        public string cashName { get; set; }

        public string password { get; set; }
        public string account { get; set; }//登录名，跟cashid是一样的

        public DateTime logintime { get; set; }//登录时间

        public DateTime loginchecktime { get; set; }//登录检测时间

        public int loginErrorNum { get; set; }//登录失败

        public string LocalIp { get; set; }

        public Dictionary<OrderInfo.PayType, decimal> SuccessOrderAmount { get { return _successOrderAmount; } }
    }
}
