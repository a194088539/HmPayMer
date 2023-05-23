using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class OrderPayScale
	{
		public string PayTime
		{
			get;
			set;
		}

		public string PayName
		{
			get;
			set;
		}

		public decimal SumAmt
		{
			get;
			set;
		}

		public decimal PayScale
		{
			get;
			set;
		}
	}
}
