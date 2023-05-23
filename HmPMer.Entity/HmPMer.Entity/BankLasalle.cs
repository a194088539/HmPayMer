using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class BankLasalle
	{
		[DataMember]
		[Key]
		[InsertKey]
		public string BankLasalleCode
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
		public string BankLasalleName
		{
			get;
			set;
		}

		[DataMember]
		public int Proid
		{
			get;
			set;
		}

		[DataMember]
		public int Cityid
		{
			get;
			set;
		}
	}
}
