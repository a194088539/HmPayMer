using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class RoleMenu
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
		public string roleID
		{
			get;
			set;
		}

		[DataMember]
		public string menuID
		{
			get;
			set;
		}

		[DataMember]
		public DateTime createTime
		{
			get;
			set;
		}

		[DataMember]
		public string createUser
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? modifyTime
		{
			get;
			set;
		}

		[DataMember]
		public string modifyUser
		{
			get;
			set;
		}
	}
}
