using HM.Framework;
using HM.Framework.Execl;
using HM.Framework.Logging;
using HmPMer.AdminUI.Fillters;
using HmPMer.AdminUI.Models;
using HmPMer.Business;
using HmPMer.Entity;
using HmPMer.WebAdminUI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;

namespace HmPMer.AdminUI.Controllers
{
	public class OrderController : Controller
	{
		[Auth(FlagStr = "Order")]
		public ActionResult Index()
		{
			return View();
		}

		[Auth(FlagStr = "PromOrderIndex")]
		public ActionResult PromOrderIndex()
		{
			return View();
		}

		[HttpGet]
		public ActionResult QueryOrderList(int? page, int? limit)
		{
			string request = Utils.GetRequest("UserId");
			string request2 = Utils.GetRequest("OrderId");
			string request3 = Utils.GetRequest("MerOrderNo");
			string request4 = Utils.GetRequest("InterfaceCode");
			string request5 = Utils.GetRequest("ChannelOrderNo");
			string request6 = Utils.GetRequest("OrderTime");
			string request7 = Utils.GetRequest("endTime");
			int requestToInt = Utils.GetRequestToInt("OrderState", -1);
			int requestToInt2 = Utils.GetRequestToInt("PayState", -1);
			string request8 = Utils.GetRequest("PayCode");
			HmAdmin userModel = ModelCommon.GetUserModel();
			OrderSearchParam orderSearchParam = new OrderSearchParam();
			orderSearchParam.PromId = Utils.GetRequest("PromId");
			orderSearchParam.AgentId = Utils.GetRequest("AgentId");
			orderSearchParam.OrderBeginTime = Utils.StringToDateTime(Utils.GetRequest("BeginTime"), null);
			orderSearchParam.OrderEndTime = Utils.StringToDateTime(Utils.GetRequest("EndTime"), null);
			Paging paging = new Paging
			{
				PageIndex = ((!page.HasValue) ? 1 : page.Value),
				PageSize = (limit.HasValue ? limit.Value : 30)
			};
			orderSearchParam.UserId = request;
			orderSearchParam.OrderId = request2;
			orderSearchParam.MerOrderNo = request3;
			orderSearchParam.ChannelOrderNo = request5;
			orderSearchParam.OrderState = requestToInt;
			orderSearchParam.PayState = requestToInt2;
			orderSearchParam.PayCode = request8;
			orderSearchParam.InterfaceCode = request4;
			OrderBll orderBll = new OrderBll();
			OrderListCount countData = null;
			List<OrderInfo> orderList = orderBll.GetOrderList(orderSearchParam, ref paging, out countData);
			ResultPage<OrderInfo> resultPage = new ResultPage<OrderInfo>();
			resultPage.msg = "查询成功";
			resultPage.Item = orderList;
			resultPage.Data = countData;
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		public void OrderImprotExcel()
		{
			try
			{
				string request = Utils.GetRequest("UserId");
				string request2 = Utils.GetRequest("OrderId");
				string request3 = Utils.GetRequest("MerOrderNo");
				string request4 = Utils.GetRequest("InterfaceCode");
				string request5 = Utils.GetRequest("ChannelOrderNo");
				string request6 = Utils.GetRequest("OrderTime");
				string request7 = Utils.GetRequest("endTime");
				int requestToInt = Utils.GetRequestToInt("OrderState", -1);
				int requestToInt2 = Utils.GetRequestToInt("PayState", -1);
				string request8 = Utils.GetRequest("PayCode");
				HmAdmin userModel = ModelCommon.GetUserModel();
				OrderSearchParam orderSearchParam = new OrderSearchParam();
				orderSearchParam.PromId = Utils.GetRequest("PromId");
				orderSearchParam.AgentId = Utils.GetRequest("AgentId");
				orderSearchParam.OrderBeginTime = Utils.StringToDateTime(Utils.GetRequest("BeginTime"), null);
				orderSearchParam.OrderEndTime = Utils.StringToDateTime(Utils.GetRequest("EndTime"), null);
				orderSearchParam.UserId = request;
				orderSearchParam.OrderId = request2;
				orderSearchParam.MerOrderNo = request3;
				orderSearchParam.ChannelOrderNo = request5;
				orderSearchParam.OrderState = requestToInt;
				orderSearchParam.PayState = requestToInt2;
				orderSearchParam.PayCode = request8;
				orderSearchParam.InterfaceCode = request4;
				DataTable orderListTable = new OrderBll().GetOrderListTable(orderSearchParam);
				ExcelHelper.ExportDataTableToExcel(orderListTable, "订单记录(" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ").xls", "订单记录");
			}
			catch (Exception)
			{
			}
		}

		public ActionResult OrderInfo(string OrderId)
		{
			OrderDetail orderDetail = new OrderBll().GetOrderDetail(OrderId);
			if (orderDetail != null)
			{
				switch (orderDetail.NotityState)
				{
				case 0:
					orderDetail.NotityStateNmae = "处理中";
					break;
				case 1:
					orderDetail.NotityStateNmae = "成功";
					break;
				case 2:
					orderDetail.NotityStateNmae = "失败";
					break;
				}
				switch (orderDetail.PayState)
				{
				case 0:
					orderDetail.PayStateName = "待支付";
					break;
				case 1:
					orderDetail.PayStateName = "支付成功";
					break;
				case 2:
					orderDetail.PayStateName = "支付失败";
					break;
				case 3:
					orderDetail.PayStateName = "订单过期";
					break;
				}
			}
			return View(orderDetail);
		}

		public ActionResult OrderDetailInfo(string OrderId)
		{
			ResultBase resultBase = new ResultBase();
			OrderDetail orderDetail = new OrderBll().GetOrderDetail(OrderId);
			if (orderDetail == null)
			{
				resultBase.Success = false;
			}
			resultBase.Data = orderDetail;
			return Json(resultBase);
		}

		[HttpPost]
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
				bool flag = orderBll.EditOrderNotity(orderNotity);
				failing.data = text2;
				if (orderNotity.NotityState != 1)
				{
					failing.message = "补发失败！补发结果：" + text2;
					return failing;
				}
				failing.IsSuccess = true;
				failing.message = "补发成功！";
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "订单补发";
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"OrderId={OrderId}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
				return failing;
			}
			catch (Exception exception)
			{
				LogUtil.Error("补发", exception);
				failing.message = "程序异常！";
				return failing;
			}
		}

		[HttpPost]
		public ActionResult MakeUpOrder(string orderId)
		{
			ApiResult<string> failing = ApiResult<string>.Failing;
			if (string.IsNullOrEmpty(orderId))
			{
				failing.message = "请指定补单的订单号";
				return failing;
			}
			PayBll payBll = new PayBll();
			OrderBll orderBll = new OrderBll();
			DateTime now = DateTime.Now;
			OrderBase orderBase = orderBll.GetOrderBase(orderId);
			if (orderBase == null)
			{
				failing.message = "此订单不存在";
				return failing;
			}
			if (orderBase.PayState == PayState.Success.ToInt())
			{
				failing.message = "此订单不需要补单";
				return failing;
			}
			orderBase.ChannelOrderNo = "补:" + now.ToString("yyyyMMddHHmmss");
			switch (payBll.CompleteOrder(orderBase))
			{
			case 1:
				failing.IsSuccess = true;
				failing.message = "补单成功";
				break;
			case 0:
				failing.message = "补单失败";
				break;
			case 2:
				failing.message = "此订单无法补单";
				break;
			}
			if (failing.IsSuccess)
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "订单补单";
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"OrderId={orderId}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return failing;
		}

		[Auth(FlagStr = "OrderNotityIndex")]
		public ActionResult OrderNotityIndex()
		{
			return View();
		}

		public ActionResult GetOrderNotityPageList(int? page, int? limit)
		{
			string request = Utils.GetRequest("UserId");
			string request2 = Utils.GetRequest("OrderId");
			int requestQueryToInt = Utils.GetRequestQueryToInt("NotityState", -1);
			OrderNotityInfo orderNotityInfo = new OrderNotityInfo();
			orderNotityInfo.UserId = request;
			orderNotityInfo.OrderId = request2;
			orderNotityInfo.NotityState = requestQueryToInt;
			orderNotityInfo.BeginTime = Utils.StringToDateTime(Utils.GetRequest("BeginTime"), null);
			orderNotityInfo.EndTime = Utils.StringToDateTime(Utils.GetRequest("EndTime"), null);
			Paging paging = new Paging
			{
				PageIndex = ((!page.HasValue) ? 1 : page.Value),
				PageSize = (limit.HasValue ? limit.Value : 30)
			};
			List<OrderNotityInfo> orderNotityPageList = new OrderBll().GetOrderNotityPageList(orderNotityInfo, ref paging);
			ResultPage<OrderNotityInfo> resultPage = new ResultPage<OrderNotityInfo>();
			resultPage.msg = "查询成功";
			resultPage.Item = orderNotityPageList;
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		[Auth(FlagStr = "MakeUpOrderIndex")]
		public ActionResult MakeUpOrderIndex()
		{
			return View();
		}

		[Auth(FlagStr = "ClearOrder")]
		public ActionResult ClearOrder()
		{
			return View();
		}

		[HttpPost]
		public ActionResult DelOrder(OrderSearchParam param)
		{
			ApiResult<string> failing = ApiResult<string>.Failing;
			OrderBll orderBll = new OrderBll();
			int num = orderBll.DelOrder(param);
			if (num > 0)
			{
				failing.IsSuccess = true;
				failing.message = "删除成功！";
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "订单清理";
				behaviorLog.BlType = 3;
				behaviorLog.parm = param.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			else
			{
				failing.message = "清除失败！";
			}
			return failing;
		}
	}
}
