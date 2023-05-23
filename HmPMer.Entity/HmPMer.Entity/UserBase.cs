using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class UserBase
	{
		[DataMember]
		[Key]
		[InsertKey]
		public string UserId
		{
			get;
			set;
		}

		[DataMember]
		public string Pass
		{
			get;
			set;
		}

		[DataMember]
		public string Pass2
		{
			get;
			set;
		}

		[DataMember]
		public int AccountType
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

		[DataMember]
		public int AloneRate
		{
			get;
			set;
		}

		[DataMember]
		public string ApiKey
		{
			get;
			set;
		}

		[DataMember]
		public string MerName
		{
			get;
			set;
		}

		[DataMember]
		public string MobilePhone
		{
			get;
			set;
		}

		[DataMember]
		public string Email
		{
			get;
			set;
		}

		[DataMember]
		public string QQ
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? RegTime
		{
			get;
			set;
		}

		[DataMember]
		public string RegIp
		{
			get;
			set;
		}

		[DataMember]
		public int IsEnabled
		{
			get;
			set;
		}

		[DataMember]
		public int IsMobilePhone
		{
			get;
			set;
		}

		[DataMember]
		public int IsEmail
		{
			get;
			set;
		}

		[DataMember]
		public int IsPass
		{
			get;
			set;
		}

		[DataMember]
		public int AgentPay
		{
			get;
			set;
		}

		[DataMember]
		public int WithdrawStatus
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? WithdrawTime
		{
			get;
			set;
		}

		[DataMember]
		public string WithdrawAuditDes
		{
			get;
			set;
		}

		[DataMember]
		public int IdCardStatus
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? IdCardTime
		{
			get;
			set;
		}

		[DataMember]
		public string IdCardAuditDes
		{
			get;
			set;
		}

		[DataMember]
		public string GradeId
		{
			get;
			set;
		}

		[DataMember]
		public string PromId
		{
			get;
			set;
		}

		[DataMember]
		public string AgentId
		{
			get;
			set;
		}

		[DataMember]
		public string AgentCode
		{
			get;
			set;
		}

		[DataMember]
		public string WithdrawSchemeId
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? LastLoginTime
		{
			get;
			set;
		}

		[DataMember]
		public string LastLoginIp
		{
			get;
			set;
		}
	}
}
