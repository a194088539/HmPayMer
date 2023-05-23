using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class UserRole
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
		public string userID
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
