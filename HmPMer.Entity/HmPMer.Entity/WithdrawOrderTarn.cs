using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class WithdrawOrderTarn
	{
		[DataMember]
		[Key]
		[InsertKey]
		public string TarnId
		{
			get;
			set;
		}

		[DataMember]
		public string OrderId
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
		public string WithdrawChannelCode
		{
			get;
			set;
		}

		[DataMember]
		public string WithdrawChanneName
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
		public string AccountId
		{
			get;
			set;
		}

		[DataMember]
		public string FactName
		{
			get;
			set;
		}

		[DataMember]
		public string BankCode
		{
			get;
			set;
		}

		[DataMember]
		public string BankAddress
		{
			get;
			set;
		}

		[DataMember]
		public decimal Amount
		{
			get;
			set;
		}

		[DataMember]
		public string TarnRemark
		{
			get;
			set;
		}

		[DataMember]
		public int TarnState
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
		public string AddUser
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? ModifyTime
		{
			get;
			set;
		}

		[DataMember]
		public string ModifyDesc
		{
			get;
			set;
		}

		[DataMember]
		public string BankLasalleCode
		{
			get;
			set;
		}

		[DataMember]
		public string ReservedPhone
		{
			get;
			set;
		}

		[DataMember]
		public int AccountType
		{
			get;
			set;
		}

		[DataMember]
		[Ignore]
		public string UserId
		{
			get;
			set;
		}

		[DataMember]
		[Ignore]
		public string InterfaceName
		{
			get;
			set;
		}

		[DataMember]
		[Ignore]
		public decimal Handing
		{
			get;
			set;
		}

		[DataMember]
		[Ignore]
		public DateTime? BeginTime
		{
			get;
			set;
		}

		[DataMember]
		[Ignore]
		public DateTime? EndTime
		{
			get;
			set;
		}
	}
}
