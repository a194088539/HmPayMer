using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class CountData
	{
		[DataMember]
		[Key]
		[InsertKey]
		public string Orderid
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
		public int UserType
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
		public DateTime? PayTime
		{
			get;
			set;
		}

		[DataMember]
		public string Paycode
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
		public decimal OrderAmt
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
		public decimal PromAmt
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

		[DataMember]
		public decimal Profits
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
		public string InterfaceName
		{
			get;
			set;
		}
	}
}
