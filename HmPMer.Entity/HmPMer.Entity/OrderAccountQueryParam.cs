using System;

namespace HmPMer.Entity
{
	public class OrderAccountQueryParam : OrderAccount
	{
		public DateTime? AddTimeBgin
		{
			get;
			set;
		}

		public DateTime? AddTimeEnd
		{
			get;
			set;
		}

		public DateTime? AccountTimeBegin
		{
			get;
			set;
		}

		public DateTime? AccountTimeEnd
		{
			get;
			set;
		}
	}
}
