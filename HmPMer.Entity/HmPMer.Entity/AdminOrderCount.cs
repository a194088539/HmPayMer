using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class AdminOrderCount
	{
		public int CountBusiness
		{
			get;
			set;
		}

		public decimal SumBusinessAmt
		{
			get;
			set;
		}

		public int CountAgent
		{
			get;
			set;
		}

		public decimal SumAgentamt
		{
			get;
			set;
		}

		public int CountProm
		{
			get;
			set;
		}

		public decimal SumPromAmt
		{
			get;
			set;
		}

		public int CountOrder
		{
			get;
			set;
		}

		public int CountOrderPay
		{
			get;
			set;
		}

		public decimal SumOrderAmt
		{
			get;
			set;
		}

		public decimal SumOrderPayAmt
		{
			get;
			set;
		}

		public decimal SumProfits
		{
			get;
			set;
		}
	}
}
