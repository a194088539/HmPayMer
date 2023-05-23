namespace HM.Framework.PayApi
{
	public class HMNotifyResult<T>
	{
		public HMNotifyState Code
		{
			get;
			set;
		}

		public string Message
		{
			get;
			set;
		}

		public T Data
		{
			get;
			set;
		}

		public static HMNotifyResult<T> Fail => new HMNotifyResult<T>
		{
			Code = HMNotifyState.Fail
		};
	}
}
