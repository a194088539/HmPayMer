using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class UsersPayBank
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
		public string UserId
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
		public string BankCode
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
		public string WithdrawChannelCode
		{
			get;
			set;
		}

		[DataMember]
		public string BankName
		{
			get;
			set;
		}

		[DataMember]
		public int ProvinceId
		{
			get;
			set;
		}

		[DataMember]
		public string ProvinceName
		{
			get;
			set;
		}

		[DataMember]
		public int CityId
		{
			get;
			set;
		}

		[DataMember]
		public string CityName
		{
			get;
			set;
		}

		[DataMember]
		public string BankLasalleCode
		{
			get;
			set;
		}

		[DataMember]
		public string ReservedPhone
		{
			get;
			set;
		}

		[DataMember]
		public string BankAddress
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? Addtime
		{
			get;
			set;
		}
	}
}
