using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class FeedbackReply
	{
		[DataMember]
		[Key]
		[InsertKey]
		public string Id
		{
			get;
			set;
		}

		[DataMember]
		public int FRType
		{
			get;
			set;
		}

		[DataMember]
		public string FeedbackId
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

		[DataMember]
		public string createUser
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? AddTime
		{
			get;
			set;
		}
	}
}
