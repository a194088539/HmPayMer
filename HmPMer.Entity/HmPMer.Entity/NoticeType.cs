using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class NoticeType
	{
		[DataMember]
		[Key]
		[InsertKey]
		public string id
		{
			get;
			set;
		}

		[DataMember]
		public string name
		{
			get;
			set;
		}

		[DataMember]
		public string parentID
		{
			get;
			set;
		}
	}
}
