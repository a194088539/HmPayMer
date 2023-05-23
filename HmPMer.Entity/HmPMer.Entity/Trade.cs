using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class Trade
	{
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

		[DataMember]
		[Key]
		[InsertKey]
		public string TradeId
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
		public int Type
		{
			get;
			set;
		}

		[DataMember]
		public int BillType
		{
			get;
			set;
		}

		[DataMember]
		public string BillNo
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? TradeTime
		{
			get;
			set;
		}

		[DataMember]
		public decimal BeforeAmount
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
		public decimal PayFlow
		{
			get;
			set;
		}

		[DataMember]
		public decimal Balance
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
		public string Remark
		{
			get;
			set;
		}
	}
}
