using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class OrderAccount
	{
		[DataMember]
		[Key]
		[InsertKey]
		public string OId
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
		public decimal OrderAmt
		{
			get;
			set;
		}

		[DataMember]
		public string AccountrRnge
		{
			get;
			set;
		}

		[DataMember]
		public int SchemeType
		{
			get;
			set;
		}

		[DataMember]
		public int TDay
		{
			get;
			set;
		}

		[DataMember]
		public int AmtSingle
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
		public DateTime AddTime
		{
			get;
			set;
		}

		[DataMember]
		public DateTime AccountTime
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? EndTime
		{
			get;
			set;
		}

		[DataMember]
		public int AccountState
		{
			get;
			set;
		}

		[DataMember]
		public string LockId
		{
			get;
			set;
		}

		[DataMember]
		public string AdminId
		{
			get;
			set;
		}
	}
}
