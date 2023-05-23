using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class Paging
	{
		[DataMember]
		public int PageIndex
		{
			get;
			set;
		}

		[DataMember]
		public int PageSize
		{
			get;
			set;
		}

		[DataMember]
		public int TotalCount
		{
			get;
			set;
		}

		[DataMember]
		public int PageCount
		{
			get
			{
				return (TotalCount + PageSize - 1) / PageSize;
			}
		}
	}
}
