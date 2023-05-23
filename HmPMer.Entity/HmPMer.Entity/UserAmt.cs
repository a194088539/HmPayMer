using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class UserAmt
	{
		[DataMember]
		[Key]
		[InsertKey]
		public string UserId
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
		public decimal UnBalance
		{
			get;
			set;
		}

		[DataMember]
		public decimal SettleBalance
		{
			get;
			set;
		}

		[DataMember]
		public decimal Freeze
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
	}
}
