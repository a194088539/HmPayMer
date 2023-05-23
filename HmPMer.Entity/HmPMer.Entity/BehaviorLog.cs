using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class BehaviorLog
	{
		[DataMember]
		[Ignore]
		public DateTime? BeginTime
		{
			get;
			set;
		}

		[DataMember]
		[Ignore]
		public DateTime? EndTime
		{
			get;
			set;
		}

		[DataMember]
		[Key]
		[InsertKey]
		public string Id
		{
			get;
			set;
		}

		[DataMember]
		public string BlName
		{
			get;
			set;
		}

		[DataMember]
		public int BlType
		{
			get;
			set;
		}

		[DataMember]
		public string ExSql
		{
			get;
			set;
		}

		[DataMember]
		public string parm
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
		public DateTime? addTime
		{
			get;
			set;
		}
	}
}
