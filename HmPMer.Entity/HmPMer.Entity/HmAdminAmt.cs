using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class HmAdminAmt
	{
		[DataMember]
		public string AdminId
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
		public decimal TotalBalance
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
		public int OrderNum
		{
			get;
			set;
		}
	}
}
