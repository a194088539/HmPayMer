using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class ReadNotice
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
		[Key]
		[InsertKey]
		public string NoticeId
		{
			get;
			set;
		}
	}
}
