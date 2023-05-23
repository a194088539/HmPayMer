namespace HM.Framework.PayApi
{
	public class HMPayResult
	{
		public HMPayState Code
		{
			get;
			set;
		}

		public HMMode Mode
		{
			get;
			set;
		}

		public string Message
		{
			get;
			set;
		}

		public string Data
		{
			get;
			set;
		}

		public static HMPayResult Fail => new HMPayResult
		{
			Code = HMPayState.Fail
		};
	}
}
