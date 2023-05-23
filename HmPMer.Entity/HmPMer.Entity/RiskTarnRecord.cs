using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class RiskTarnRecord
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
		public int AccountType
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
		public DateTime? TarnDate
		{
			get;
			set;
		}

		[DataMember]
		public int TarnCount
		{
			get;
			set;
		}

		[DataMember]
		public decimal TarnAmt
		{
			get;
			set;
		}
	}
}
