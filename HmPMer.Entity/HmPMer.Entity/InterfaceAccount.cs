using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class InterfaceAccount
	{
		[DataMember]
		[Ignore]
		public decimal OrderAmt
		{
			get;
			set;
		}

		[DataMember]
		[Ignore]
		public string RiskScheme
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

		[DataMember]
		[Ignore]
		public DateTime? TarnDate
		{
			get;
			set;
		}

		[DataMember]
		[Ignore]
		public int TarnCount
		{
			get;
			set;
		}

		[DataMember]
		[Ignore]
		public decimal TarnAmt
		{
			get;
			set;
		}

		[DataMember]
		[Key]
		[InsertKey]
		public string Id
		{
			get;
			set;
		}

		[DataMember]
		public string Code
		{
			get;
			set;
		}

		[DataMember]
		public string Account
		{
			get;
			set;
		}

		[DataMember]
		public string ChildAccount
		{
			get;
			set;
		}

		[DataMember]
		public string MD5Pwd
		{
			get;
			set;
		}

		[DataMember]
		public string RSAOpen
		{
			get;
			set;
		}

		[DataMember]
		public string RSAPrivate
		{
			get;
			set;
		}

		[DataMember]
		public string Appid
		{
			get;
			set;
		}

		[DataMember]
		public string OpenId
		{
			get;
			set;
		}

		[DataMember]
		public string OpenPwd
		{
			get;
			set;
		}

		[DataMember]
		public string SubDomain
		{
			get;
			set;
		}

		[DataMember]
		public string BindDomain
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
		public int OrderNo
		{
			get;
			set;
		}
	}
}
