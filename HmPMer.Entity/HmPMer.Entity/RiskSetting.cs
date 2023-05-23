using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class RiskSetting
	{
		[DataMember]
		[Key]
		[InsertKey]
		public string RiskSettingId
		{
			get;
			set;
		}

		[DataMember]
		public int RiskSettingType
		{
			get;
			set;
		}

		[DataMember]
		public string RiskSchemeId
		{
			get;
			set;
		}

		[DataMember]
		public string TargetId
		{
			get;
			set;
		}
	}
}
