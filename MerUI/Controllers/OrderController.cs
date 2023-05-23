using HM.Framework;
using HM.Framework.Execl;
using HmPMer.Business;
using HmPMer.Entity;
using HmPMer.MerUI.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HmPMer.MerUI.Controllers
{
	public class OrderController : Controller
	{
		private UserBase user = ModelCommon.GetUserModel();

		public ActionResult Index()
		{
			return View();
		}

		public ActionResult OrderList(int? PageIndex, int? PageSize, OrderSearchParam param)
		{
			param.UserId = user.UserId;
			Paging paging = new Paging
			{
				PageIndex = ((!PageIndex.HasValue) ? 1 : PageIndex.Value),
				PageSize = (PageSize.HasValue ? PageSize.Value : 15)
			};
			OrderBll orderBll = new OrderBll();
			OrderListCount countData = null;
			List<OrderInfo> orderList = orderBll.GetOrderList(param, ref paging, out countData);
			new ResultPage<OrderInfo>();
			base.ViewData["PageSize"] = paging.PageSize;
			base.ViewData["TotalCount"] = paging.TotalCount;
			base.ViewData["PageCount"] = paging.PageCount;
			base.ViewData["page"] = new PageInfo().createAjaxPageControl("page", paging.PageSize, paging.PageIndex, paging.TotalCount);
			return View(orderList);
		}

		public void OrderImprotExcel(OrderSearchParam param)
		{
			try
			{
				param.UserId = user.UserId;
				ExcelHelper.ExportDataTableToExcel(new OrderBll().GetOrderListTableUI(param), "订单记录(" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ").xls", "订单记录");
			}
			catch (Exception)
			{
			}
		}

		public ActionResult Reissue(string OrderId)
		{
			ApiResult<string> failing = ApiResult<string>.Failing;
			try
			{
				OrderBll orderBll = new OrderBll();
				OrderNotity orderNotity = orderBll.GetOrderNotity(OrderId);
				string text2 = orderNotity.NotityContext = HttpUtils.SendRequest(orderNotity.NotityUrl, "");
				if (text2.ToUpper().StartsWith("SUCCESS"))
				{
					orderNotity.NotityState = 1;
				}
				else if (orderNotity.NotityCount >= 5)
				{
					orderNotity.NotityState = 2;
				}
				else
				{
					orderNotity.NotityCount++;
					orderNotity.NotityTime = orderNotity.NotityTime.Value.AddMinutes(5.0);
				}
				orderBll.EditOrderNotity(orderNotity);
				failing.data = text2;
				if (orderNotity.NotityState != 1)
				{
					failing.message = "补发失败！补发结果：" + text2;
					return failing;
				}
				failing.IsSuccess = true;
				failing.message = "补发成功！";
				return failing;
			}
			catch (Exception)
			{
				failing.message = "程序异常！";
				return failing;
			}
		}
	}
}
