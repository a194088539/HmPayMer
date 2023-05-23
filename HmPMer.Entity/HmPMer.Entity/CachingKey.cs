namespace HmPMer.Entity
{
	public class CachingKey
	{
		public const string ORDER = "o.";

		public const string ORDER_USER = "o.u.";

		private const string ORDER_USER_PAYPENDING = "o.u.pp.";

		public const string USER = "u.";

		private const string USER_PAYAMT = "u.m.";

		private const string USER_REGCODE = "u.hm.";

		private const string USER_ADMIN = "u.admin.";

		private const string USER_PAYHELP = "u.phelp.";

		private const string PAY_CHANNEL = "p.c.";

		private const string PAY_QRAMT = "p.qra.";

		private const string PAY_RISK = "p.risk.";

		public static string CreateOrderKey(string orderId)
		{
			return "o." + orderId;
		}

		public static string CreateMerOrderKey(string orderId)
		{
			return "o.u." + orderId;
		}

		public static string CreateMerOrderPending(string orderId)
		{
			return "o.u.pp." + orderId;
		}

		public static string CreatePayAmt(string payAmtId)
		{
			return "u.m." + payAmtId;
		}

		public static string CreateRegCode(string regCode)
		{
			return "u.hm." + regCode;
		}

		public static string CreateHmAdmin(string id)
		{
			return "u.admin." + id;
		}

		public static string CreateUserPayHelp(string userId)
		{
			return "u.phelp." + userId;
		}

		public static string CreatePayChannelSettingKey()
		{
			return "p.c..setting";
		}

		public static string CreatePayQrAmtKey(string id)
		{
			return "p.qra." + id;
		}

		public static string CreateSystemKey()
		{
			return "sys.setting";
		}
	}
}
