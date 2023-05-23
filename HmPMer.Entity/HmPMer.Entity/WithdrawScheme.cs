using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class WithdrawScheme
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
		public string SchemeName
		{
			get;
			set;
		}

		[DataMember]
		public int SchemeType
		{
			get;
			set;
		}

		[DataMember]
		public decimal MinAmtSingle
		{
			get;
			set;
		}

		[DataMember]
		public decimal MaxAmtSingle
		{
			get;
			set;
		}

		[DataMember]
		public int MaxtDay
		{
			get;
			set;
		}

		[DataMember]
		public decimal LimitAmtDay
		{
			get;
			set;
		}

		[DataMember]
		public decimal HandingRateSingle
		{
			get;
			set;
		}

		[DataMember]
		public int IsMinHandingSingle
		{
			get;
			set;
		}

		[DataMember]
		public decimal MinHandingSingle
		{
			get;
			set;
		}

		[DataMember]
		public int IsMaxHandingSingle
		{
			get;
			set;
		}

		[DataMember]
		public decimal MaxHandingSingle
		{
			get;
			set;
		}

		[DataMember]
		public int IsInterface
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
		public int Sort
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
