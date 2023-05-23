using System;

namespace HmPMer.Entity
{
	public class OrderSearchParam
	{
		public string UserId
		{
			get;
			set;
		}

		public DateTime? OrderBeginTime
		{
			get;
			set;
		}

		public DateTime? OrderEndTime
		{
			get;
			set;
		}

		public DateTime? PayBeginTime
		{
			get;
			set;
		}

		public DateTime? PayEndTime
		{
			get;
			set;
		}

		public string PayCode
		{
			get;
			set;
		}

		public string Channel
		{
			get;
			set;
		}

		public int PayState
		{
			get;
			set;
		}

		public int OrderState
		{
			get;
			set;
		}

		public string OrderId
		{
			get;
			set;
		}

		public string MerOrderNo
		{
			get;
			set;
		}

		public string PromId
		{
			get;
			set;
		}

		public string AgentId
		{
			get;
			set;
		}

		public string ChannelOrderNo
		{
			get;
			set;
		}

		public decimal OrderAmt
		{
			get;
			set;
		}

		public int UserType
		{
			get;
			set;
		}

		public string InterfaceCode
		{
			get;
			set;
		}
	}
}
