using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class ProfitAnalysis
	{
		public DateTime? PayTime
		{
			get;
			set;
		}

		public string PayName
		{
			get;
			set;
		}

		public int CountOrder
		{
			get;
			set;
		}

		public decimal OrderAmt
		{
			get;
			set;
		}

		public decimal MerAmt
		{
			get;
			set;
		}

		public decimal PromAmt
		{
			get;
			set;
		}

		public decimal AgentAmt
		{
			get;
			set;
		}

		public decimal Profits
		{
			get;
			set;
		}
	}
}
