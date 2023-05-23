using HmPMer.WebAdminUI.Models;
using System.Web.Mvc;

namespace HmPMer.AdminUI.Fillters
{
	public class StaticAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (!filterContext.HttpContext.Request.IsAjaxRequest())
			{
				filterContext.Controller.ViewBag.StaticVersion = ModelCommon.GetVersion();
			}
			base.OnActionExecuting(filterContext);
		}
	}
}
