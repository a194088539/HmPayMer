using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class ConfigDate
	{
		[DataMember]
		[Key]
		[InsertKey]
		public DateTime RestDate
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
		public string Remark
		{
			get;
			set;
		}
	}
}
