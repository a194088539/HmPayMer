using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class OrderBase
	{
		[DataMember]
		[Key]
		[InsertKey]
		public string OrderId
		{
			get;
			set;
		}

		[DataMember]
		public string UserId
		{
			get;
			set;
		}

		[DataMember]
		public string MerOrderNo
		{
			get;
			set;
		}

		[DataMember]
		public string ChannelOrderNo
		{
			get;
			set;
		}

		[DataMember]
		public string AccountId
		{
			get;
			set;
		}

		[DataMember]
		public decimal OrderAmt
		{
			get;
			set;
		}

		[DataMember]
		public decimal MerOrderAmt
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? OrderTime
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? ExpiredTime
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? PayTime
		{
			get;
			set;
		}

		[DataMember]
		public int PayState
		{
			get;
			set;
		}

		[DataMember]
		public string ChannelId
		{
			get;
			set;
		}

		[DataMember]
		public decimal PayFlow
		{
			get;
			set;
		}

		[DataMember]
		public decimal PayRate
		{
			get;
			set;
		}

		[DataMember]
		public string Attach
		{
			get;
			set;
		}

		[DataMember]
		public string ClientIp
		{
			get;
			set;
		}

		[DataMember]
		public string NotifyUrl
		{
			get;
			set;
		}

		[DataMember]
		public string ReturnUrl
		{
			get;
			set;
		}

		[DataMember]
		public decimal PromRate
		{
			get;
			set;
		}

		[DataMember]
		public decimal PromAmt
		{
			get;
			set;
		}

		[DataMember]
		public decimal Profits
		{
			get;
			set;
		}

		[DataMember]
		public string PromId
		{
			get;
			set;
		}

		[DataMember]
		public decimal CostRate
		{
			get;
			set;
		}

		[DataMember]
		public decimal CostAmt
		{
			get;
			set;
		}

		[DataMember]
		public string Version
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
		public string InterfaceCode
		{
			get;
			set;
		}

		[DataMember]
		public string AgentId
		{
			get;
			set;
		}

		[DataMember]
		public decimal AgentRate
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
		public decimal MerAmt
		{
			get;
			set;
		}

		[DataMember]
		public decimal AgentAmt
		{
			get;
			set;
		}
	}
}
