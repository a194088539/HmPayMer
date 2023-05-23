using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class SeedBase
	{
		[DataMember]
		[Key]
		[InsertKey]
		public string SeedKey
		{
			get;
			set;
		}

		[DataMember]
		public int SeedVal
		{
			get;
			set;
		}
	}
}
