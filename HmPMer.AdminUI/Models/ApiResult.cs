using HM.Framework;
using System.Web.Mvc;

namespace HmPMer.AdminUI.Models
{
	public class ApiResult<T> : ActionResult
	{
		public int code
		{
			get;
			set;
		}

		public bool IsSuccess
		{
			get;
			set;
		}

		public string message
		{
			get;
			set;
		}

		public T data
		{
			get;
			set;
		}

		public static ApiResult<T> Success
		{
			get
			{
				ApiResult<T> apiResult = new ApiResult<T>();
				apiResult.IsSuccess = true;
				return apiResult;
			}
		}

		public static ApiResult<T> Failing
		{
			get
			{
				ApiResult<T> apiResult = new ApiResult<T>();
				apiResult.IsSuccess = false;
				return apiResult;
			}
		}

		public override void ExecuteResult(ControllerContext context)
		{
			context.HttpContext.Response.ContentType = "application/json";
			context.HttpContext.Response.Write(this.ToJson());
		}
	}
}
