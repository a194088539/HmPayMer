using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class PayType
	{
		[DataMember]
		[Ignore]
		public string DefaulInfaceCode
		{
			get;
			set;
		}

		[DataMember]
		[Ignore]
		public string AccountScheme
		{
			get;
			set;
		}

		[DataMember]
		[Key]
		[InsertKey]
		public string PayCode
		{
			get;
			set;
		}

		[DataMember]
		public string PayName
		{
			get;
			set;
		}

		[DataMember]
		public int IsEnable
		{
			get;
			set;
		}

		[DataMember]
		public int PaySort
		{
			get;
			set;
		}
	}
}
