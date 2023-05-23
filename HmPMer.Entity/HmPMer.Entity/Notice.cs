using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class Notice
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
		public string NoticeType
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

		[DataMember]
		public int IsRelease
		{
			get;
			set;
		}

		[DataMember]
		public int IsEnabled
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? Addtime
		{
			get;
			set;
		}

		[DataMember]
		public string AddUser
		{
			get;
			set;
		}
	}
}
