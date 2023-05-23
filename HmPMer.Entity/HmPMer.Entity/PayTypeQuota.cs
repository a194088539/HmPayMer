using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class PayTypeQuota
	{
		[DataMember]
		[Ignore]
		public string PayName
		{
			get;
			set;
		}

		[DataMember]
		[Key]
		[InsertKey]
		public string PayCode
		{
			get;
			set;
		}

		[DataMember]
		public decimal minMoney
		{
			get;
			set;
		}

		[DataMember]
		public decimal maxMoney
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? addtime
		{
			get;
			set;
		}
	}
}
