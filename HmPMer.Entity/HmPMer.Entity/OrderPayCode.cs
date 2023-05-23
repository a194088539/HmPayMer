using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class OrderPayCode
	{
		[DataMember]
		[Ignore]
		public decimal OrderAmt
		{
			get;
			set;
		}

		[DataMember]
		[Ignore]
		public DateTime OrderTime
		{
			get;
			set;
		}

		[DataMember]
		[Ignore]
		public int PayState
		{
			get;
			set;
		}

		[DataMember]
		[Ignore]
		public DateTime ExpiredTime
		{
			get;
			set;
		}

		[DataMember]
		[Key]
		[InsertKey]
		public string OrderId
		{
			get;
			set;
		}

		[DataMember]
		public int PayMode
		{
			get;
			set;
		}

		[DataMember]
		public string PayCode
		{
			get;
			set;
		}

		[DataMember]
		public string ChannelCode
		{
			get;
			set;
		}

		[DataMember]
		public string Codes
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? AddTime
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? UpdateTime
		{
			get;
			set;
		}
	}
}
