using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class PayChannel
	{
		[DataMember]
		[Key]
		[InsertKey]
		public string Code
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
		public string ChannelName
		{
			get;
			set;
		}

		[DataMember]
		public int ChannelSort
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
		public string InterfaceCode
		{
			get;
			set;
		}
	}
}
