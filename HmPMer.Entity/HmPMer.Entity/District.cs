using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class District
	{
		[DataMember]
		[Key]
		[InsertKey]
		public int Id
		{
			get;
			set;
		}

		[DataMember]
		public string Name
		{
			get;
			set;
		}

		[DataMember]
		public int ParentId
		{
			get;
			set;
		}

		[DataMember]
		public int Level
		{
			get;
			set;
		}
	}
}
