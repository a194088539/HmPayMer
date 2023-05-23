using HM.Framework;
using System.Text;

namespace HmPMer.MerUI.Models
{
	public class PageInfo
	{
		protected int total;

		private int _pageSize = 15;

		private int _pageIndex = 1;

		private string _sort;

		private string _order;

		public int pageSize
		{
			get
			{
				return _pageSize;
			}
			set
			{
				_pageSize = value;
			}
		}

		public int pageIndex
		{
			get
			{
				int requestQueryToInt = Utils.GetRequestQueryToInt("pageIndex", 1);
				if (requestQueryToInt <= 0)
				{
					return 1;
				}
				return requestQueryToInt;
			}
			set
			{
				_pageIndex = value;
			}
		}

		public string sort
		{
			get
			{
				return Utils.GetRequest("sort");
			}
			set
			{
				_sort = value;
			}
		}

		public string order
		{
			get
			{
				return Utils.GetRequest("order");
			}
			set
			{
				_order = value;
			}
		}

		public string createPageControl(string pageControlID)
		{
			return createPageControl(pageControlID, pageSize, pageIndex, total);
		}

		public string createPageControl(string _pageControlID, int _pageSize, int _pageIndex, int _totalIndex)
		{
			if (_totalIndex > 0)
			{
				string function = $"page(\"{_pageControlID}\", {_totalIndex}, {_pageSize}, {_pageIndex - 1});";
				return FunctionInit(function);
			}
			return string.Empty;
		}

		public string createAjaxPageControl(string _pageControlID, int _pageSize, int _pageIndex, int _totalIndex)
		{
			if (_totalIndex > 0)
			{
				string function = $"pageSarch(\"{_pageControlID}\", {_totalIndex}, {_pageSize}, {_pageIndex - 1});";
				return FunctionInit(function);
			}
			return string.Empty;
		}

		public string FunctionInit(string function)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<script type=\"text/javascript\" language=\"javascript\">");
			stringBuilder.Append(" $(function () {");
			stringBuilder.Append(function);
			stringBuilder.Append(" });");
			stringBuilder.Append("</script>");
			return stringBuilder.ToString();
		}
	}
}
