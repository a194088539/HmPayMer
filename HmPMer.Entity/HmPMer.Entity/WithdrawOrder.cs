using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class WithdrawOrder
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
		public int OrderType
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
		public string UserId
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
		public int ProvinceId
		{
			get;
			set;
		}

		[DataMember]
		public string ProvinceName
		{
			get;
			set;
		}

		[DataMember]
		public int CityId
		{
			get;
			set;
		}

		[DataMember]
		public string CityName
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
		public string BankName
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
		public string FactName
		{
			get;
			set;
		}

		[DataMember]
		public decimal WithdrawAmt
		{
			get;
			set;
		}

		[DataMember]
		public decimal Handing
		{
			get;
			set;
		}

		[DataMember]
		public decimal Amt
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
		public int PayState
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

		[DataMember]
		public string AuditDesc
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
	}
}
