using HmPMer.AgentUI.Models;
using HmPMer.Entity;
using System;
using System.Web;
using System.Web.Mvc;

namespace HmPMer.AgentUI.Fillters
{
	public class AuthAttribute : ActionFilterAttribute
	{
		private Action<ActionExecutingContext, int, string, string> initResponse = delegate(ActionExecutingContext filterContext, int code, string message, string data)
		{
			HttpResponseBase response = filterContext.RequestContext.HttpContext.Response;
			response.Buffer = true;
			response.ExpiresAbsolute = DateTime.Now.AddDays(-1.0);
			response.Cache.SetExpires(DateTime.Now.AddDays(-1.0));
			response.Expires = 0;
			response.CacheControl = "no-cache";
			response.Cache.SetNoStore();
			if (filterContext.HttpContext.Request.IsAjaxRequest())
			{
				filterContext.Result = new JsonResult
				{
					Data = new
					{
						code = code,
						message = message,
						data = data,
						IsSuccess = false
					},
					JsonRequestBehavior = JsonRequestBehavior.AllowGet
				};
			}
			else
			{
				filterContext.Result = new RedirectResult(data);
			}
		};

		public bool NoLogin
		{
			get;
			set;
		}

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (NoLogin)
			{
				filterContext.HttpContext.Request.IsAjaxRequest();
				base.OnActionExecuting(filterContext);
			}
			else
			{
				UserBase userModel = ModelCommon.GetUserModel();
				if (userModel == null)
				{
					initResponse(filterContext, 301, "对不起，您的登录状态已丢失请重新登录！", "/account/login");
				}
				else if (userModel.IsEnabled == 2)
				{
					initResponse(filterContext, 301, "对不起，您的账户已被禁用！", "/account/login");
				}
				else
				{
					filterContext.HttpContext.Request.IsAjaxRequest();
					filterContext.Controller.ViewBag.AgentUser = userModel;
					base.OnActionExecuting(filterContext);
				}
			}
		}
	}
}
