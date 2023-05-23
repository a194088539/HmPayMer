using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class OrderSettlement
	{
		[DataMember]
		[Key]
		[InsertKey]
		public string Id
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
		public string UserId
		{
			get;
			set;
		}

		[DataMember]
		public decimal AccountAmt
		{
			get;
			set;
		}

		[DataMember]
		public decimal SettlementAmt
		{
			get;
			set;
		}

		[DataMember]
		public int ToAccountType
		{
			get;
			set;
		}

		[DataMember]
		public int ToAccountDay
		{
			get;
			set;
		}

		[DataMember]
		public decimal ToAccountRate
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
		public DateTime? PaymentTime
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? AccountingTime
		{
			get;
			set;
		}

		[DataMember]
		public int SettlementState
		{
			get;
			set;
		}
	}
}
