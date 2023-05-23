using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class RiskScheme
	{
		[DataMember]
		[Key]
		[InsertKey]
		public string RiskSchemeId
		{
			get;
			set;
		}

		[DataMember]
		public string SchemeName
		{
			get;
			set;
		}

		[DataMember]
		public int RiskSchemeTaype
		{
			get;
			set;
		}

		[DataMember]
		public string UserId
		{
			get;
			set;
		}

		[DataMember]
		public decimal SingleMinAmt
		{
			get;
			set;
		}

		[DataMember]
		public decimal SingleMaxAmt
		{
			get;
			set;
		}

		[DataMember]
		public int IsDayCount
		{
			get;
			set;
		}

		[DataMember]
		public int DayCount
		{
			get;
			set;
		}

		[DataMember]
		public int IsDayAmt
		{
			get;
			set;
		}

		[DataMember]
		public decimal DayAmt
		{
			get;
			set;
		}

		[DataMember]
		public int IsMonthCount
		{
			get;
			set;
		}

		[DataMember]
		public int MonthCount
		{
			get;
			set;
		}

		[DataMember]
		public int IsMonthAmt
		{
			get;
			set;
		}

		[DataMember]
		public decimal MonthAmt
		{
			get;
			set;
		}

		[DataMember]
		public int Sort
		{
			get;
			set;
		}
	}
}
