using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class InterfaceBusiness
	{
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

		[DataMember]
		[Key]
		[InsertKey]
		public string Code
		{
			get;
			set;
		}

		[DataMember]
		public string Name
		{
			get;
			set;
		}

		[DataMember]
		public int AccType
		{
			get;
			set;
		}

		[DataMember]
		public string SubMitUrl
		{
			get;
			set;
		}

		[DataMember]
		public string AgentPayUrl
		{
			get;
			set;
		}

		[DataMember]
		public string QueryUrl
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
		public int OrderNo
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
        public string CallbackDomain
        {
            get;
            set;
        }

    }
}
