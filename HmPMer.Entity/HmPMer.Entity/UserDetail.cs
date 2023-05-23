using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class UserDetail
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
	}
}
