using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class HmAdmin
	{
		[DataMember]
		[Ignore]
		public string RoleStr
		{
			get;
			set;
		}

		[DataMember]
		[Key]
		[InsertKey]
		public string ID
		{
			get;
			set;
		}

		[DataMember]
		public string AdmUser
		{
			get;
			set;
		}

		[DataMember]
		public string AdmPass
		{
			get;
			set;
		}

		[DataMember]
		public string AdmPass2
		{
			get;
			set;
		}

		[DataMember]
		public string NickName
		{
			get;
			set;
		}

		[DataMember]
		public int Flag
		{
			get;
			set;
		}

		[DataMember]
		public int IsEnable
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? AddTime
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
		public decimal Rate
		{
			get;
			set;
		}

		[DataMember]
		public string RegCode
		{
			get;
			set;
		}
	}
}
