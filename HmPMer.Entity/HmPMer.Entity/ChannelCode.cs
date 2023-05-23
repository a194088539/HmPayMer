using System.Collections.Generic;

namespace HmPMer.Entity
{
	public class ChannelCode
	{
		public const string AlipayQrCode = "al";

		public const string WeiXinQrCode = "wx";

		public const string WEIXIN_NATIVE = "WEIXIN_NATIVE";

		public const string WEIXIN_H5 = "WEIXIN_H5";

		public const string WEIXIN_APP = "WEIXIN_APP";

		public const string WEIXIN_JSAPI = "WEIXIN_JSAPI";

		public const string WEIXIN_MICROPAY = "WEIXIN_MICROPAY";

		public const string ALIPAY_NATIVE = "ALIPAY_NATIVE";

		public const string ALIPAY_HB_NATIVE = "ALIPAY_HB_NATIVE";

		public const string ALIPAY_HB_H5 = "ALIPAY_HB_H5";

		public const string ALIPAY_H5 = "ALIPAY_H5";

		public const string ALIPAY_APP = "ALIPAY_APP";

		public const string ALIPAY_MICROPAY = "ALIPAY_MICROPAY";

		public const string QQPAY_NATIVE = "QQPAY_NATIVE";

		public const string QQPAY_H5 = "QQPAY_H5";

		public const string QQPAY_APP = "QQPAY_APP";

		public const string JD_NATIVE = "JD_NATIVE";

		public const string JD_H5 = "JD_H5";

		public const string SPDB = "SPDB";

		public const string HXB = "HXB";

		public const string SPABANK = "SPABANK";

		public const string ECITIC = "ECITIC";

		public const string CIB = "CIB";

		public const string CEBB = "CEBB";

		public const string CMBC = "CMBC";

		public const string CMB = "CMB";

		public const string BOC = "BOC";

		public const string BCOM = "BCOM";

		public const string CCB = "CCB";

		public const string ICBC = "ICBC";

		public const string ABC = "ABC";

		public const string PSBC = "PSBC";

		public const string GATEWAY_QUICK = "GATEWAY_QUICK";

		public const string GATEWAY_NATIVE = "GATEWAY_NATIVE";

		public const string ALIPAY_H5_URL = "ALIPAY_H5_URL";

		private static HashSet<string> NativeToH5Set = new HashSet<string>(new string[] {
            "al",
            "ALIPAY_NATIVE",
            "ALIPAY_HB_NATIVE",
            "QQPAY_NATIVE",
            "JD_NATIVE"
        });

		public static string GetName(string code)
		{
			switch (code)
			{
			case "al":
				return "支付宝个人码";
			case "wx":
				return "微信个人码";
			case "ALIPAY_NATIVE":
				return "支付宝扫码";
			case "ALIPAY_HB_NATIVE":
				return "支付宝红包扫码";
			case "ALIPAY_HB_H5":
				return "支付宝红包H5";
			case "ALIPAY_H5":
				return "支付宝H5";
			case "WEIXIN_NATIVE":
				return "微信扫码";
			case "WEIXIN_H5":
				return "微信H5";
			case "QQPAY_NATIVE":
				return "QQ扫码";
			case "QQPAY_H5":
				return "QQH5";
			case "JD_NATIVE":
				return "京东扫码";
			case "JD_H5":
				return "京东H5";
			default:
				return code;
			}
		}

		public static bool IsGuMa(string type)
		{
			if (string.IsNullOrEmpty(type))
			{
				return false;
			}
			if ("al".Equals(type) || "wx".Equals(type) || type.Equals("mypay"))
			{
				return true;
			}
			return false;
		}

		public static bool IsNativeToH5(string type)
		{
			if (string.IsNullOrEmpty(type))
			{
				return false;
			}
			return NativeToH5Set.Contains(type.ToUpper());
		}
	}
}
