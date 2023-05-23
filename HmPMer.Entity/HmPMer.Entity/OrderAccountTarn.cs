using System;

namespace HmPMer.Entity
{
	public class OrderAccountTarn
	{
		public string UserId
		{
			get;
			set;
		}

		public string MerName
		{
			get;
			set;
		}

		public decimal Balance
		{
			get;
			set;
		}

		public decimal UnBalance
		{
			get;
			set;
		}

		public decimal Amt
		{
			get;
			set;
		}

		public int AccountCount
		{
			get;
			set;
		}

		public DateTime? AccountDate
		{
			get;
			set;
		}
	}
}
