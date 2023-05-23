using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class OrderInfo : OrderBase
	{
		[DataMember]
		public int OrderState
		{
			get;
			set;
		}

		[DataMember]
		public string ChannelName
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

		[DataMember]
		public string InterfaceName
		{
			get;
			set;
		}
	}
}
