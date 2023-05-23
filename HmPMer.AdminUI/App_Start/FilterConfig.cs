using HmPMer.AdminUI.Fillters;
using System.Web.Mvc;

namespace HmPMer.AdminUI
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new AuthAttribute());
			filters.Add(new StaticAttribute());
			filters.Add(new HandleErrorAttribute());
		}
	}
}
