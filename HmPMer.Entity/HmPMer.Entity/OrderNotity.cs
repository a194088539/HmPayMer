using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class OrderNotity
	{
		[DataMember]
		[Key]
		[InsertKey]
		public string NotityId
		{
			get;
			set;
		}

		[DataMember]
		public string OrderId
		{
			get;
			set;
		}

		[DataMember]
		public int NotityState
		{
			get;
			set;
		}

		[DataMember]
		public int NotityCount
		{
			get;
			set;
		}

		[DataMember]
		public string NotityUrl
		{
			get;
			set;
		}

		[DataMember]
		public string NotityContext
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? AddTime
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? NotityTime
		{
			get;
			set;
		}

		[DataMember]
		public string ReturnUrl
		{
			get;
			set;
		}
	}
}
