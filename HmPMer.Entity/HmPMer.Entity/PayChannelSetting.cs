using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class PayChannelSetting
	{
		[DataMember]
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
		public string ChannelCode
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
	}
}
