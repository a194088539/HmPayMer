using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class AccountSchemeDetail
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
		public string AccountSchemeId
		{
			get;
			set;
		}

		[DataMember]
		public int StarTime
		{
			get;
			set;
		}

		[DataMember]
		public int EndTime
		{
			get;
			set;
		}

		[DataMember]
		public int SchemeType1
		{
			get;
			set;
		}

		[DataMember]
		public int TDay1
		{
			get;
			set;
		}

		[DataMember]
		public int AmtSingle1
		{
			get;
			set;
		}

		[DataMember]
		public int SchemeType2
		{
			get;
			set;
		}

		[DataMember]
		public int TDay2
		{
			get;
			set;
		}

		[DataMember]
		public int AmtSingle2
		{
			get;
			set;
		}
	}
}
