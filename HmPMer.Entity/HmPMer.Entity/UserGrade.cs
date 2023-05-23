using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class UserGrade
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
		public string GradeName
		{
			get;
			set;
		}

		[DataMember]
		public int UserType
		{
			get;
			set;
		}
	}
}
