using HM.Framework;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HmPMer.AdminUI.Models
{
	public class ResultPage<T> : ActionResult
	{
		public int code
		{
			get;
			set;
		}

		public string msg
		{
			get;
			set;
		}

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

		public override void ExecuteResult(ControllerContext context)
		{
			context.HttpContext.Response.ContentType = "application/json";
			context.HttpContext.Response.Write(this.ToJson());
		}
	}
}
