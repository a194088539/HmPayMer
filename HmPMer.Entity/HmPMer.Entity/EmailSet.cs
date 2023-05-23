using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class EmailSet
	{
		[DataMember]
		[Key]
		[InsertKey]
		public string EmialCode
		{
			get;
			set;
		}

		[DataMember]
		public string Sendserver
		{
			get;
			set;
		}

		[DataMember]
		public string port
		{
			get;
			set;
		}

		[DataMember]
		public string Account
		{
			get;
			set;
		}

		[DataMember]
		public string Pwd
		{
			get;
			set;
		}

		[DataMember]
		public string displayName
		{
			get;
			set;
		}
	}
}
