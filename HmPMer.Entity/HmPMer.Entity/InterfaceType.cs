using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class InterfaceType
	{
		[DataMember]
		public string InterfaceCode
		{
			get;
			set;
		}

		[DataMember]
		public string PayCode
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
		public string DefaulInfaceCode
		{
			get;
			set;
		}

		[DataMember]
		public string AccountScheme
		{
			get;
			set;
		}
	}
}
