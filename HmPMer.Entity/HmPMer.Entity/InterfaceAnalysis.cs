using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class InterfaceAnalysis
	{
		public DateTime? PayTime
		{
			get;
			set;
		}

		public string InterfaceName
		{
			get;
			set;
		}

		public string PayName
		{
			get;
			set;
		}

		public decimal CountOrderAmt
		{
			get;
			set;
		}

		public decimal CountPayOrderAmt
		{
			get;
			set;
		}

		public int OrderCount
		{
			get;
			set;
		}

		public int PayOrderCount
		{
			get;
			set;
		}
	}
}
