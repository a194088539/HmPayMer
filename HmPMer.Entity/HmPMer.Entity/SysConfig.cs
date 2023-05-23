using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class SysConfig
	{
		[DataMember]
		[Key]
		[InsertKey]
		public string Key
		{
			get;
			set;
		}

		[DataMember]
		public int Type
		{
			get;
			set;
		}

		[DataMember]
		public string Value
		{
			get;
			set;
		}

		[DataMember]
		public string Desc
		{
			get;
			set;
		}
	}
}
