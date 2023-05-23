using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class UserBaseBack
	{
		[DataMember]
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

		[DataMember]
		public decimal Balance
		{
			get;
			set;
		}

		[DataMember]
		public decimal UnBalance
		{
			get;
			set;
		}

		[DataMember]
		public decimal SettleBalance
		{
			get;
			set;
		}

		[DataMember]
		public decimal Freeze
		{
			get;
			set;
		}

		[DataMember]
		public decimal OrderAmt
		{
			get;
			set;
		}

		[DataMember]
		public string CompanyName
		{
			get;
			set;
		}

		[DataMember]
		public string LicenseId
		{
			get;
			set;
		}

		[DataMember]
		public string FactName
		{
			get;
			set;
		}

		[DataMember]
		public string IdCard
		{
			get;
			set;
		}

		[DataMember]
		public int CompanyProId
		{
			get;
			set;
		}

		[DataMember]
		public string CompanyProName
		{
			get;
			set;
		}

		[DataMember]
		public int CompanyCityId
		{
			get;
			set;
		}

		[DataMember]
		public string CompanyCityName
		{
			get;
			set;
		}

		[DataMember]
		public int CompanyDicId
		{
			get;
			set;
		}

		[DataMember]
		public string CompanyDicName
		{
			get;
			set;
		}

		[DataMember]
		public string Address
		{
			get;
			set;
		}

		[DataMember]
		public string CustTel
		{
			get;
			set;
		}

		[DataMember]
		public string IdCardImg1
		{
			get;
			set;
		}

		[DataMember]
		public string IdCardImg2
		{
			get;
			set;
		}

		[DataMember]
		public string LicenseImg
		{
			get;
			set;
		}

		[DataMember]
		public int WithdrawAccountType
		{
			get;
			set;
		}

		[DataMember]
		public string WithdrawChannelCode
		{
			get;
			set;
		}

		[DataMember]
		public string WithdrawBank
		{
			get;
			set;
		}

		[DataMember]
		public string WithdrawFactName
		{
			get;
			set;
		}

		[DataMember]
		public string WithdrawBankCode
		{
			get;
			set;
		}

		[DataMember]
		public int WithdrawProvinceId
		{
			get;
			set;
		}

		[DataMember]
		public string WithdrawProvince
		{
			get;
			set;
		}

		[DataMember]
		public int WithdrawCityId
		{
			get;
			set;
		}

		[DataMember]
		public string WithdrawCity
		{
			get;
			set;
		}

		[DataMember]
		public string WithdrawBankBranch
		{
			get;
			set;
		}

		[DataMember]
		public string WithdrawBankLasalleCode
		{
			get;
			set;
		}

		[DataMember]
		public string WithdrawReservedPhone
		{
			get;
			set;
		}

		[DataMember]
		public string WithdrawData1
		{
			get;
			set;
		}

		[DataMember]
		public string WithdrawData2
		{
			get;
			set;
		}

		[DataMember]
		public DateTime DeleteTime
		{
			get;
			set;
		}

		[DataMember]
		public string DeleteUser
		{
			get;
			set;
		}
	}
}
