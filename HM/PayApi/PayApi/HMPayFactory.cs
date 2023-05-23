using HM.Framework.PayApi;
using HM.Framework.PayApi.PingAnLm;
using HM.Framework.PayApi.PingAnYe;
using HM.Framework.PayApi.Swiftpass;
using PayApi.YiDianFu;

namespace PayApi
{
    public static class HMPayFactory
    {
        public static HMPayApiBase CreatePayApi(HMInterface code)
        {
            HMPayApiBase result = null;
            switch (code)
            {
                case HMInterface.Swiftpass:
                    return new HM.Framework.PayApi.Swiftpass.HMPayApi();
                case HMInterface.PingAnYe:
                    return new HM.Framework.PayApi.PingAnYe.HMPayApi();
                case HMInterface.PingAnLm:
                    return new HM.Framework.PayApi.PingAnLm.HMPayApi();
                case HMInterface.PingAnYuE:
                    return new HMPayYEApi();
                case HMInterface.XingFuTianXia:
                    return new HM.Framework.PayApi.XingFuTianXia.HMPayApi();
                case HMInterface.ShangDe:
                    return new HM.Framework.PayApi.ShangDe.HMPayApi();
                case HMInterface.ShangDeDeFeng:
                    return new HM.Framework.PayApi.ShangDeDeFeng.HMPayApi();
                case HMInterface.ShangDeZhiHuiPay:
                    return new HM.Framework.PayApi.ShangDeZhiHuiPay.HMPayApi();
                case HMInterface.Sdupay:
                case HMInterface.Pay591V2:
                    return new HM.Framework.PayApi.Sdupay.HMPayApi();
                case HMInterface.Leniuniu:
                    return new HM.Framework.PayApi.Leniuniu.HMPayApi();
                case HMInterface.CareyPay:
                    return new HM.Framework.PayApi.Careypay.HMPayApi();
                case HMInterface.AlCode:
                    return new HM.Framework.PayApi.Alipay.HMPayApi();
                case HMInterface.AlF2F:
                    return new HM.Framework.PayApi.Alipay.HMF2FPayApi();
                case HMInterface.ShouFuPay:
                    return new HM.Framework.PayApi.ShouFuPay.HMPayApi();
                case HMInterface.YunFuBao:
                    return new HM.Framework.PayApi.YunFuBao.HMPayApi();
                case HMInterface.NewCareyPay:
                    return new HM.Framework.PayApi.NewCarepay.HMPayApi();
                case HMInterface.HMTransfer:
                    return new HM.Framework.PayApi.HMTransfer.HMPayApi();
                case HMInterface.PingAnYe_CustomLm:
                    return new CustomPayApi();
                case HMInterface.CiticBankPay:
                    return new CiticBankPay();
                case HMInterface.SuPay:
                    return new HM.Framework.PayApi.SuPay.HMPayApi();
                case HMInterface.Epay:
                    return new HM.Framework.PayApi.Epay.HMPayApi();
                case HMInterface.YiZhiFu:
                    return new HM.Framework.PayApi.YiZhiFu.HMPayApi();
                case HMInterface.WangFa:
                    return new HM.Framework.PayApi.WangFa.HMPayApi();
                case HMInterface.NewCaryTwo:
                    return new HM.Framework.PayApi.NewCarepay.HMCrPayApi();
                case HMInterface.KrdPay:
                    return new HM.Framework.PayApi.KrdPay.HMCrPayApi();
                case HMInterface.ZongHeDianShang:
                    return new HM.Framework.PayApi.ZongHeDianShang.HMPayApi();
                case HMInterface.DAIFU:
                    return new HM.Framework.PayApi.DaiFu.HMPayApi();
                case HMInterface.PAYSAPI:
                    return new HM.Framework.PayApi.PaysApi.HMPayApi();
                case HMInterface.PAY699U:
                    return new HM.Framework.PayApi._669U.HMPayApi();
                case HMInterface.YunShanFu:
                    return new HM.Framework.PayApi.YunShanFu.HMPayApi();
                case HMInterface.BankPayV1:
                    return new HM.Framework.PayApi.BankPay.HMPayApiV1();
                case HMInterface.WXMaiDanV1:
                    return new HM.Framework.PayApi.WeiXin.HMMaidanPayApi();
                case HMInterface.XinFa:
                    return new XinFa.HMPayApi();
                case HMInterface.LongFa:
                    return new LongFa.HMPayApi();
                case HMInterface.BaiFu:
                    return new BaiFu.HMPayApi();
                case HMInterface.GaoSheng:
                    return new GaoSheng.HMPayApi();
                case HMInterface.AoYou:
                    return new AoYou.HMPayApi();
                case HMInterface.JinZuan:
                    return new JinZuan.HMPayApi();
                case HMInterface.XinFuTong:
                    return new XFT.HMPayApi();
                case HMInterface.XiYangYang:
                    return new XiYangYang.HMPayApi();
                default:
                    return result;
            }
        }
    }
}
