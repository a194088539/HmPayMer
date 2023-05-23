using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class OrderListCount
	{
		[DataMember]
		public decimal TotalOrderAmt
		{
			get;
			set;
		}

		[DataMember]
		public decimal TotalFlow
		{
			get;
			set;
		}

		[DataMember]
		public decimal TotalCostAmt
		{
			get;
			set;
		}

		[DataMember]
		public decimal TotalPromAmt
		{
			get;
			set;
		}

		[DataMember]
		public decimal TotalAgentAmt
		{
			get;
			set;
		}

		[DataMember]
		public decimal TotalProfits
		{
			get;
			set;
		}

		[DataMember]
		public decimal OutOrderCount
		{
			get;
			set;
		}

		[DataMember]
		public decimal PageOrderAmt
		{
			get;
			set;
		}

		[DataMember]
		public decimal PageFlow
		{
			get;
			set;
		}

		[DataMember]
		public decimal PageCostAmt
		{
			get;
			set;
		}

		[DataMember]
		public decimal PagePromAmt
		{
			get;
			set;
		}

		[DataMember]
		public decimal PageProfits
		{
			get;
			set;
		}
	}
}
