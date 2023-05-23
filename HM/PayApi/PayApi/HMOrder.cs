using System;

namespace HM.Framework.PayApi
{
	public class HMOrder
	{
		public string OrderNo
		{
			get;
			set;
		}

		public string MerOrderNo
		{
			get;
			set;
		}

		public string SupplierOrderNo
		{
			get;
			set;
		}

		public string TarnNo
		{
			get;
			set;
		}

		public decimal OrderAmt
		{
			get;
			set;
		}

		public string PayTypeCode
		{
			get;
			set;
		}

		public HMChannel ChannelCode
		{
			get;
			set;
		}

		public DateTime OrderTime
		{
			get;
			set;
		}

		public string ClientIp
		{
			get;
			set;
		}

		public string Attach
		{
			get;
			set;
		}
	}
}
