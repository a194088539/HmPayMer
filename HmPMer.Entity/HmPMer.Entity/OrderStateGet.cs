using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class OrderStateGet
	{
		public string OrderId
		{
			get;
			set;
		}

		public DateTime OrderTime
		{
			get;
			set;
		}

		public DateTime ExpiredTime
		{
			get;
			set;
		}

		public DateTime PayTime
		{
			get;
			set;
		}

		public int PayState
		{
			get;
			set;
		}

		public string ReturnUrl
		{
			get;
			set;
		}
	}
}
