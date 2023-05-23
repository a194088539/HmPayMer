using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class SmsModel
	{
		[DataMember]
		[Key]
		[InsertKey]
		public string Code
		{
			get;
			set;
		}

		[DataMember]
		public string Title
		{
			get;
			set;
		}

		[DataMember]
		public string Content
		{
			get;
			set;
		}
	}
}
