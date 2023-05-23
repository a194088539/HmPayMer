using System;

namespace HmPMer.Entity
{
	public class WithdrawOrderQueryParam : WithdrawOrder
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
	}
}
