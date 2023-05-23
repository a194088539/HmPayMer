using HmPMer.Business;
using HmPMer.Entity;
using HmPMer.WebAdminUI.Models;
using System;
using System.Web;
using System.Web.Mvc;

namespace HmPMer.AdminUI.Fillters
{
	public class AuthAttribute : ActionFilterAttribute
	{
		private Action<ActionExecutingContext, int, string, string, string> initResponse = delegate(ActionExecutingContext filterContext, int code, string message, string data, string ajaxMessage)
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
						message = ajaxMessage,
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

		public string FlagStr
		{
			get;
			set;
		}

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (NoLogin)
			{
				base.OnActionExecuting(filterContext);
			}
			else
			{
				HmAdmin userModel = ModelCommon.GetUserModel();
				if (userModel == null)
				{
					initResponse(filterContext, 301, "对不起，您的登录状态已丢失请重新登录！", "/home/login", "您的账户已在其他地方登录！");
				}
				else if (userModel.IsEnable == 0)
				{
					initResponse(filterContext, 301, "对不起，您的账户已被禁用！", "/home/login", "对不起，您的账户已被禁用！");
				}
				else if (!string.IsNullOrEmpty(FlagStr) && !new MenuRoleBll().CheckFlagStr(userModel.ID, FlagStr))
				{
					initResponse(filterContext, 303, "对不起，您的没有权限访问！", "/home/noflag", "对不起，您的没有权限访问！");
				}
				else
				{
					filterContext.Controller.ViewBag.Admin = userModel;
					base.OnActionExecuting(filterContext);
				}
			}
		}
	}
}
