using System;

namespace HmPMer.Entity
{
	public class OrderNotityInfo : OrderNotity
	{
		public string UserId
		{
			get;
			set;
		}

		public DateTime? BeginTime
		{
			get;
			set;
		}

		public DateTime? EndTime
		{
			get;
			set;
		}
	}
}
