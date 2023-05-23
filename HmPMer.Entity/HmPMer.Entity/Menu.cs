using HM.Framework.Entity;
using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class Menu
	{
		[DataMember]
		[Ignore]
		public string NewId
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
		public string menuName
		{
			get;
			set;
		}

		[DataMember]
		public string menuUrl
		{
			get;
			set;
		}

		[DataMember]
		public int menuLeval
		{
			get;
			set;
		}

		[DataMember]
		public string parentID
		{
			get;
			set;
		}

		[DataMember]
		public int orderNo
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
		public DateTime? createTime
		{
			get;
			set;
		}

		[DataMember]
		public string icon
		{
			get;
			set;
		}

		[DataMember]
		public string FlagStr
		{
			get;
			set;
		}
	}
}
