using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class SmsSet
	{
		[DataMember]
		[Key]
		[InsertKey]
		public string SmsCode
		{
			get;
			set;
		}

		[DataMember]
		public string Account
		{
			get;
			set;
		}

		[DataMember]
		public string Pwd
		{
			get;
			set;
		}
	}
}
