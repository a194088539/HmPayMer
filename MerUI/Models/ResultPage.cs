using System.Collections.Generic;

namespace HmPMer.MerUI.Models
{
	public class ResultPage<T>
	{
		public int pageIndex
		{
			get;
			set;
		}

		public int pageSize
		{
			get;
			set;
		}

		public int totalCount
		{
			get;
			set;
		}

		public int pageCount
		{
			get;
			set;
		}

		public List<T> Item
		{
			get;
			set;
		}

		public dynamic Data
		{
			get;
			set;
		}
	}
}
