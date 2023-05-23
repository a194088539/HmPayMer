using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class OrderDetail : OrderBase
	{
		[DataMember]
		public string ChannelName
		{
			get;
			set;
		}

		[DataMember]
		public int NotityState
		{
			get;
			set;
		}

		[DataMember]
		public int NotityCount
		{
			get;
			set;
		}

		[DataMember]
		public string NotityAddress
		{
			get;
			set;
		}

		[DataMember]
		public string NotityContext
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? NotityTime
		{
			get;
			set;
		}

		[DataMember]
		public string NotityStateNmae
		{
			get;
			set;
		}

		[DataMember]
		public string PayStateName
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

		[DataMember]
		public string PayName
		{
			get;
			set;
		}

		[DataMember]
		public int OrderState
		{
			get;
			set;
		}
	}
}
