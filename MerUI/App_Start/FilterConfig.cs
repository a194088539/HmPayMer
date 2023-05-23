using HmPMer.MerUI.Fillters;
using System.Web.Mvc;

namespace HmPMer.MerUI
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new AuthAttribute());
			filters.Add(new HandleErrorAttribute());
		}
	}
}
