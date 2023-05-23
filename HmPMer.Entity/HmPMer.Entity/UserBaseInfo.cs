using HM.Framework.Entity;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	public class UserBaseInfo : UserBase
	{
		public decimal Balance
		{
			get;
			set;
		}

		public decimal Freeze
		{
			get;
			set;
		}

		public decimal UnBalance
		{
			get;
			set;
		}

		public decimal OrderAmt
		{
			get;
			set;
		}

		public int Type
		{
			get;
			set;
		}

		public string SchemeName
		{
			get;
			set;
		}

		public string GradeName
		{
			get;
			set;
		}

		[DataMember]
		[Ignore]
		public string RiskSchemeId
		{
			get;
			set;
		}

		[DataMember]
		[Ignore]
		public string RiskSchemeName
		{
			get;
			set;
		}
	}
}
