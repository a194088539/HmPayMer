using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class OrderCountInfo
	{
		[DataMember]
		public string CountDate
		{
			get;
			set;
		}

		[DataMember]
		public decimal CountOrderAmt
		{
			get;
			set;
		}

		[DataMember]
		public decimal CountPayOrderAmt
		{
			get;
			set;
		}

		[DataMember]
		public decimal TodayOrderCount
		{
			get;
			set;
		}

		[DataMember]
		public decimal TodayPayOrderCount
		{
			get;
			set;
		}

		[DataMember]
		public decimal TodayOrderAmt
		{
			get;
			set;
		}

		[DataMember]
		public string PayName
		{
			get;
			set;
		}
	}
}
