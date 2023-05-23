using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class RiskLimit
	{
		[DataMember]
		[Key]
		[InsertKey]
		public string ID
		{
			get;
			set;
		}

		[DataMember]
		public int RiskType
		{
			get;
			set;
		}

		[DataMember]
		public int State
		{
			get;
			set;
		}

		[DataMember]
		public string TargetId
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? BeginTime
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? EndTime
		{
			get;
			set;
		}
	}
}
