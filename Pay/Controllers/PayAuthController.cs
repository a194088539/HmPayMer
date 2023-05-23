using HM.Framework;
using HM.Framework.Logging;
using HM.Framework.PayApi;
using HmPMer.Business;
using HmPMer.Entity;
using HmPMer.Pay.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HmPMer.Pay.Controllers
{
	public class PayAuthController : Controller
	{
		public ActionResult Gateway(string supplierCode, string id)
		{
			if (string.IsNullOrEmpty(supplierCode))
			{
				return Content("参数错误！");
			}
			if (string.IsNullOrEmpty(id))
			{
				return Content("参数错误！");
			}
			string request = Utils.GetRequest("code");
			if (string.IsNullOrEmpty(request))
			{
				request = Utils.GetRequest("openid");
			}
			if (string.IsNullOrEmpty(request))
			{
				return Content("参数不正确！");
			}
			DateTime now = DateTime.Now;
			OrderBll orderBll = new OrderBll();
			OrderBase order = orderBll.GetOrderBase(id);
			if (order != null)
			{
				switch (order.PayState)
				{
				case 1:
					return Content("error:1051 订单已支付！");
				case 2:
					return Content("error:1052 订单支付失败！");
				case 3:
					return Content("error:1053 此订单已失效！");
				default:
				{
					if (order.ExpiredTime <= now)
					{
						return Content("error:1053 此订单已失效！");
					}
					InterfaceBll interfaceBll = new InterfaceBll();
					InterfaceBusiness interfaceBusiness = interfaceBll.GetInterfaceBusinessModel(order.InterfaceCode);
					if (interfaceBusiness == null)
					{
						return Content("error:1053 此接口已经下线，无法支付！");
					}
					InterfaceAccount interfaceAccount = null;
					if (interfaceBusiness.AccType == 1)
					{
						interfaceAccount = interfaceBll.GetInterfaceAccountModel(order.AccountId);
					}
					else
					{
						interfaceAccount = new InterfaceAccount
						{
							Account = interfaceBusiness.Account,
							ChildAccount = interfaceBusiness.ChildAccount,
							MD5Pwd = interfaceBusiness.MD5Pwd,
							RSAOpen = interfaceBusiness.RSAOpen,
							RSAPrivate = interfaceBusiness.RSAPrivate,
							Appid = interfaceBusiness.Appid,
							OpenId = interfaceBusiness.OpenId,
							OpenPwd = interfaceBusiness.OpenPwd,
							SubDomain = interfaceBusiness.OpenPwd,
							BindDomain = interfaceBusiness.BindDomain
						};
					}
					HMPayResult hMPayResult = PayFactory.SubmitAuthGateway(order.ChannelCode, request, interfaceBusiness, interfaceAccount, order);
					if (hMPayResult == null)
					{
						return Content("error:999 支付接口发生异常，无法支付！");
					}
					if (hMPayResult.Code == HMPayState.Success)
					{
						if (hMPayResult.Code == HMPayState.PaymentingQueryResult)
						{
							new Task(delegate
							{
								HMPayResult hMPayResult2 = null;
								for (int i = 1; i <= 10; i++)
								{
									try
									{
										hMPayResult2 = PayFactory.QueryCallback(interfaceBusiness, interfaceAccount, order);
										if (hMPayResult2.Code != HMPayState.Paymenting && hMPayResult2.Code != HMPayState.PaymentingQueryResult)
										{
											if (hMPayResult2.Code == HMPayState.Fail)
											{
												return;
											}
											if (hMPayResult2.Code == HMPayState.Success)
											{
												order.PayState = PayState.Success.ToInt();
												order.ChannelOrderNo = hMPayResult2.Data;
												if (new PayBll().CompleteOrder(order) > 0)
												{
													return;
												}
											}
											goto IL_00a0;
										}
									}
									catch (Exception exception)
									{
										LogUtil.Error("查询订单出错,orderId=" + order.OrderId, exception);
										goto IL_00a0;
									}
									continue;
									IL_00a0:
									Thread.Sleep(5000);
								}
							}).Start();
						}
						switch (hMPayResult.Mode)
						{
						case HMMode.输出字符串:
							return Content(hMPayResult.Data);
						case HMMode.输出Json:
							return Json(hMPayResult.Data);
						case HMMode.跳转链接:
							return new RedirectResult(hMPayResult.Data);
						default:
							return View();
						}
					}
					return Content("error:999 " + hMPayResult.Message);
				}
				}
			}
			return Content("error:1053 此订单不存在！");
		}

		public ActionResult Micropay(string supplierCode, string id)
		{
			if (string.IsNullOrEmpty(supplierCode))
			{
				return Content("参数错误！");
			}
			if (string.IsNullOrEmpty(id))
			{
				return Content("参数错误！");
			}
			return View();
		}
	}
}
