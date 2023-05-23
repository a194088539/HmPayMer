using HM.Framework;
using System.Web.Mvc;

namespace HmPMer.MerUI.Models
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

		public static ApiResult<T> Success => new ApiResult<T>
		{
			IsSuccess = true
		};

		public static ApiResult<T> Failing => new ApiResult<T>
		{
			IsSuccess = false
		};

		public override void ExecuteResult(ControllerContext context)
		{
			context.HttpContext.Response.ContentType = "application/json";
			context.HttpContext.Response.Write(this.ToJson());
		}
	}
}
