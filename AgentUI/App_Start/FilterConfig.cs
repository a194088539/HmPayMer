using HmPMer.AgentUI.Fillters;
using System.Web.Mvc;

namespace HmPMer.AgentUI
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
