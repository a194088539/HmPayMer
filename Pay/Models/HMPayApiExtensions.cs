using HM.Framework.PayApi;
using HmPMer.Entity;

namespace HmPMer.Pay.Models
{
	public static class HMPayApiExtensions
	{
        public static HMInterface ToHMInterface(this string str)
        {
            str = str.ToLower();
            switch (str.ToLower())
            {
                case "hmqrcode":
                    return HMInterface.MyBase;
                case "swiftpass":
                    return HMInterface.Swiftpass;
                case "pinganye":
                    return HMInterface.PingAnYe;
                case "pinganlm":
                    return HMInterface.PingAnLm;
                case "xftx":
                    return HMInterface.XingFuTianXia;
                case "shande":
                    return HMInterface.ShangDe;
                case "sdupay":
                    return HMInterface.Sdupay;
                case "pay591":
                    return HMInterface.Pay591V2;
                case "defengshande":
                    return HMInterface.ShangDeDeFeng;
                case "zhihuipayshangde":
                    return HMInterface.ShangDeZhiHuiPay;
                case "leniuniu":
                    return HMInterface.Leniuniu;
                case "careypay":
                    return HMInterface.CareyPay;
                case "shoufupay":
                    return HMInterface.ShouFuPay;
                case "alcode":
                    return HMInterface.AlCode;
                case "newcarepay":
                    return HMInterface.NewCareyPay;
                case "custompinganye":
                    return HMInterface.PingAnYe_CustomLm;
                case "citicbankpay":
                    return HMInterface.CiticBankPay;
                case "supay":
                    return HMInterface.SuPay;
                case "epay":
                    return HMInterface.Epay;
                case "yizhifu":
                    return HMInterface.YiZhiFu;
                case "wangfa":
                    return HMInterface.WangFa;
                case "newcarytwo":
                    return HMInterface.NewCaryTwo;
                case "krdpay":
                    return HMInterface.KrdPay;
                case "zonghedianshang":
                    return HMInterface.ZongHeDianShang;
                case "xinfa":
                    return HMInterface.XinFa;
                case "longfa":
                    return HMInterface.LongFa;
                case "baifu":
                    return HMInterface.BaiFu;
                case "gaosheng":
                    return HMInterface.GaoSheng;
                case "aoyou":
                    return HMInterface.AoYou;
                case "yidianfu":
                    return HMInterface.YiDianFu;
                case "jinzuan":
                    return HMInterface.JinZuan;
                case "xinfutong":
                    return HMInterface.XinFuTong;
                case "xiyangyang":
                    return HMInterface.XiYangYang;
                default:
                    if (str.StartsWith("_hmqrcode"))
                    {
                        return HMInterface.MyBase;
                    }

                    if (str.StartsWith("_swiftpass"))
                    {
                        return HMInterface.Swiftpass;
                    }

                    if (str.StartsWith("_pinganye"))
                    {
                        return HMInterface.PingAnYe;
                    }

                    if (str.StartsWith("_pinganlm"))
                    {
                        return HMInterface.PingAnLm;
                    }

                    if (str.StartsWith("_xftx"))
                    {
                        return HMInterface.XingFuTianXia;
                    }

                    if (str.StartsWith("_shande"))
                    {
                        return HMInterface.ShangDe;
                    }

                    if (str.StartsWith("_sdupay"))
                    {
                        return HMInterface.Sdupay;
                    }

                    if (str.StartsWith("_pay591"))
                    {
                        return HMInterface.Pay591V2;
                    }

                    if (str.StartsWith("_defengshande"))
                    {
                        return HMInterface.ShangDeDeFeng;
                    }

                    if (str.StartsWith("_zhihuipayshangde"))
                    {
                        return HMInterface.ShangDeZhiHuiPay;
                    }

                    if (str.StartsWith("_leniuniu"))
                    {
                        return HMInterface.Leniuniu;
                    }

                    if (str.StartsWith("_careypay"))
                    {
                        return HMInterface.CareyPay;
                    }

                    if (str.StartsWith("_shoufupay"))
                    {
                        return HMInterface.ShouFuPay;
                    }

                    if (str.StartsWith("_custompinganye"))
                    {
                        return HMInterface.PingAnYe_CustomLm;
                    }

                    if (str.StartsWith("_citicbankpay"))
                    {
                        return HMInterface.CiticBankPay;
                    }

                    if (str.StartsWith("_epay"))
                    {
                        return HMInterface.Epay;
                    }

                    if (str.StartsWith("_aoyou"))
                    {
                        return HMInterface.AoYou;
                    }

                    if (str.StartsWith("_yizhifu"))
                    {
                        return HMInterface.YiZhiFu;
                    }

                    if (str.StartsWith("_zonghedianshang"))
                    {
                        return HMInterface.ZongHeDianShang;
                    }

                    if (str.StartsWith("_alcode"))
                    {
                        return HMInterface.AlCode;
                    }

                    if (str.StartsWith("_alf2f"))
                    {
                        return HMInterface.AlF2F;
                    }

                    if (str.StartsWith("_daifu"))
                    {
                        return HMInterface.DAIFU;
                    }

                    if (str.StartsWith("_paysapi"))
                    {
                        return HMInterface.PAYSAPI;
                    }

                    if (str.StartsWith("_699u"))
                    {
                        return HMInterface.PAY699U;
                    }

                    if (str.StartsWith("_yunshanfu"))
                    {
                        return HMInterface.YunShanFu;
                    }

                    if (str.StartsWith("_bankpayv1"))
                    {
                        return HMInterface.BankPayV1;
                    }

                    if (str.StartsWith("_wxmdv1"))
                    {
                        return HMInterface.WXMaiDanV1;
                    }

                    return HMInterface.Unknown;
            }
        }

        public static HMChannel ToHMChannel(this string code)
		{
			switch (code.ToUpper())
			{
			    case "WEIXIN_NATIVE":
				    return HMChannel.WEIXIN_NATIVE;
			    case "WEIXIN_H5":
				    return HMChannel.WEIXIN_H5;
			    case "WEIXIN_JSAPI":
				    return HMChannel.WEIXIN_JSAPI;
			    case "ALIPAY_NATIVE":
				    return HMChannel.ALIPAY_NATIVE;
			    case "ALIPAY_HB_NATIVE":
				    return HMChannel.ALIPAY_HB_NATIVE;
			    case "ALIPAY_H5":
				    return HMChannel.ALIPAY_H5;
			    case "ALIPAY_HB_H5":
				    return HMChannel.ALIPAY_HB_H5;
                case "ALIPAY_F2F":
                    return HMChannel.ALIPAY_F2F;
			    case "ALIPAY_H5_URL":
				    return HMChannel.ALIPAY_H5_URL;
			    case "ALIPAY_APP":
				    return HMChannel.ALIPAY_JSAPI;
                case "ALIPAY_MICROPAY":
                    return HMChannel.ALIPAY_MICROPAY;
                case "QQPAY_NATIVE":
				    return HMChannel.QQPAY_NATIVE;
			    case "QQPAY_H5":
				    return HMChannel.QQPAY_H5;
			    case "JD_NATIVE":
				    return HMChannel.JD_NATIVE;
			    case "JD_H5":
				    return HMChannel.JD_H5;
			    case "SPDB":
				    return HMChannel.SPDB;
			    case "HXB":
				    return HMChannel.HXB;
			    case "SPABANK":
				    return HMChannel.SPABANK;
			    case "ECITIC":
				    return HMChannel.ECITIC;
			    case "CIB":
				    return HMChannel.CIB;
			    case "CEBB":
				    return HMChannel.CEBB;
			    case "CMBC":
				    return HMChannel.CMBC;
			    case "CMB":
				    return HMChannel.CMB;
			    case "BOC":
				    return HMChannel.BOC;
			    case "BCOM":
				    return HMChannel.BCOM;
			    case "CCB":
				    return HMChannel.CCB;
			    case "ICBC":
				    return HMChannel.ICBC;
			    case "ABC":
				    return HMChannel.ABC;
			    case "PSBC":
				    return HMChannel.PSBC;
			    case "GATEWAY_QUICK":
				    return HMChannel.GATEWAY_QUICK;
			    case "GATEWAY_NATIVE":
				    return HMChannel.GATEWAY_NATIVE;
                 case "PAYSAPI_ALIPAY":
				    return HMChannel.PAYSAPI_ALIPAY;
                case "PAYSAPI_WEIXIN":
				    return HMChannel.PAYSAPI_WEIXIN;
                case "U669_SALIPAY":
                    return HMChannel._669U_SALIPAY;
                case "U669_ALIPAY":
                    return HMChannel._669U_ALIPAY;
                case "UNION_WALLET":
                    return HMChannel.UNION_WALLET;
                case "UNION_PAY_H5":
                    return HMChannel.UNION_PAY_H5;
                case "KUAIJIE":
                    return HMChannel.KUAIJIE;
                case "EBANK":
                    return HMChannel.EBank;
                case "WXTOCARD":
                    return HMChannel.WXTOCARD;
                default:
				return HMChannel.Unknown;
			}
		}

		public static HMSupplier ToHMSupplier(this InterfaceBusiness interfaceBusiness)
		{
			HMSupplier hMSupplier = new HMSupplier
			{
				Code = interfaceBusiness.Code,
				Name = interfaceBusiness.Name,
				PostUri = interfaceBusiness.SubMitUrl,
				QueryUri = interfaceBusiness.QueryUrl,
				AgentPayUrl = interfaceBusiness.AgentPayUrl
			};
			if (!string.IsNullOrEmpty(interfaceBusiness.Account))
			{
				hMSupplier.Account = new HMAccount
				{
					AccountUser = interfaceBusiness.Account,
					ChildAccountUser = interfaceBusiness.ChildAccount,
					AccountCode = interfaceBusiness.MD5Pwd,
					Md5Pwd = interfaceBusiness.MD5Pwd,
					RsaPublic = interfaceBusiness.RSAOpen,
					RsaPrivate = interfaceBusiness.RSAPrivate,
					AppId = interfaceBusiness.Appid,
					OpenId = interfaceBusiness.OpenId,
					OpenPwd = interfaceBusiness.OpenPwd,
					SubDomain = interfaceBusiness.SubDomain,
					BindDomain = interfaceBusiness.BindDomain
				};
			}
			return hMSupplier;
		}

		public static HMAccount ToHMAccount(this InterfaceAccount payAccount)
		{
			return new HMAccount
			{
				AccountUser = payAccount.Account,
				ChildAccountUser = payAccount.ChildAccount,
				AccountCode = payAccount.OpenPwd,
				Md5Pwd = payAccount.MD5Pwd,
				RsaPublic = payAccount.RSAOpen,
				RsaPrivate = payAccount.RSAPrivate,
				AppId = payAccount.Appid,
				OpenId = payAccount.OpenId,
				OpenPwd = payAccount.OpenPwd,
				SubDomain = payAccount.SubDomain,
				BindDomain = payAccount.BindDomain
			};
		}

		public static HMOrder ToHMOrder(this OrderBase orderBase)
		{
			return new HMOrder
			{
				OrderNo = orderBase.OrderId,
				MerOrderNo = orderBase.MerOrderNo,
				SupplierOrderNo = orderBase.ChannelOrderNo,
				OrderAmt = orderBase.OrderAmt,
				PayTypeCode = orderBase.PayCode,
				ChannelCode = orderBase.ChannelCode.ToHMChannel(),
				OrderTime = orderBase.OrderTime.Value,
				ClientIp = orderBase.ClientIp
			};
		}
	}
}
