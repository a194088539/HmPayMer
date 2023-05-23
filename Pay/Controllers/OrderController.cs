using HM.Framework.Logging;
using HM.Framework.PayApi;
using HmPMer.Business;
using HmPMer.Business.Pay;
using HmPMer.Entity;
using HmPMer.Pay.Models;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using HM.Framework;
using PayApi;

namespace HmPMer.Pay.Controllers
{
	public class OrderController : Controller
	{
		public ActionResult Notity(string code, string id)
        {
            if (string.IsNullOrEmpty(code))
			{
				return Content("notity param null!");
			}
			HMInterface hMInterface = code.ToHMInterface();
			if (hMInterface == HMInterface.Unknown)
			{
				return Content("interface param null!");
			}
			PayBll payBll = new PayBll();
			OrderBll orderBll = new OrderBll();
			InterfaceBll interfaceBll = new InterfaceBll();
			OrderBase orderBase = null;
			HMOrder hMOrder = null;
			InterfaceBusiness interfaceBusiness = null;
			InterfaceAccount interfaceAccount = null;
			HMPayApiBase hMPayApiBase = HMPayFactory.CreatePayApi(hMInterface);
			HMNotifyResult<HMOrder> notifyOrder = hMPayApiBase.GetNotifyOrder();
			if (notifyOrder.Code == HMNotifyState.Fail)
			{
				return Content(notifyOrder.Message);
			}
			if (notifyOrder.Code == HMNotifyState.WaitAccountInit)
			{
				if (string.IsNullOrEmpty(id))
				{
					interfaceBusiness = interfaceBll.GetInterfaceBusinessModel(code);
					interfaceAccount = new InterfaceAccount
					{
						Code = interfaceBusiness.Code,
						Account = interfaceBusiness.Account,
						ChildAccount = interfaceBusiness.ChildAccount,
						MD5Pwd = interfaceBusiness.MD5Pwd,
						RSAOpen = interfaceBusiness.RSAOpen,
						RSAPrivate = interfaceBusiness.RSAPrivate,
						Appid = interfaceBusiness.Appid,
						OpenId = interfaceBusiness.OpenId,
						OpenPwd = interfaceBusiness.OpenPwd,
						SubDomain = interfaceBusiness.SubDomain,
						BindDomain = interfaceBusiness.BindDomain
					};
				}
				else
				{
					orderBase = orderBll.GetOrderDetail(id);
					if (orderBase == null)
					{
						LogUtil.DebugFormat("接口:{0},订单号：{1}，不存在", code, id);
						return Content("order not exist!");
					}
					interfaceBusiness = interfaceBll.GetInterfaceBusinessModel(orderBase.InterfaceCode);
					interfaceAccount = (string.IsNullOrEmpty(orderBase.AccountId) ? new InterfaceAccount
					{
						Code = interfaceBusiness.Code,
						Account = interfaceBusiness.Account,
						ChildAccount = interfaceBusiness.ChildAccount,
						MD5Pwd = interfaceBusiness.MD5Pwd,
						RSAOpen = interfaceBusiness.RSAOpen,
						RSAPrivate = interfaceBusiness.RSAPrivate,
						Appid = interfaceBusiness.Appid,
						OpenId = interfaceBusiness.OpenId,
						OpenPwd = interfaceBusiness.OpenPwd,
						SubDomain = interfaceBusiness.SubDomain,
						BindDomain = interfaceBusiness.BindDomain
					} : interfaceBll.GetInterfaceAccountModel(orderBase.AccountId));
				}
				hMPayApiBase.Account = interfaceAccount.ToHMAccount();
				notifyOrder = hMPayApiBase.GetNotifyOrder();
				if (notifyOrder.Code == HMNotifyState.Fail)
				{
					return Content(notifyOrder.Message);
				}
				hMOrder = notifyOrder.Data;
				if (orderBase == null)
				{
					orderBase = orderBll.GetOrderBase(hMOrder.OrderNo);
				}
			}
			else
			{
				hMOrder = notifyOrder.Data;
				orderBase = orderBll.GetOrderBase(hMOrder.OrderNo);
				if (orderBase == null)
				{
					LogUtil.DebugFormat("接口:{0},订单号：{1}，不存在", code, hMOrder.OrderNo);
					return Content("order not exist!");
				}
				interfaceBusiness = interfaceBll.GetInterfaceBusinessModel(orderBase.InterfaceCode);
				interfaceAccount = (string.IsNullOrEmpty(orderBase.AccountId) ? new InterfaceAccount
				{
					Code = interfaceBusiness.Code,
					Account = interfaceBusiness.Account,
					ChildAccount = interfaceBusiness.ChildAccount,
					MD5Pwd = interfaceBusiness.MD5Pwd,
					RSAOpen = interfaceBusiness.RSAOpen,
					RSAPrivate = interfaceBusiness.RSAPrivate,
					Appid = interfaceBusiness.Appid,
					OpenId = interfaceBusiness.OpenId,
					OpenPwd = interfaceBusiness.OpenPwd,
					SubDomain = interfaceBusiness.SubDomain,
					BindDomain = interfaceBusiness.BindDomain
				} : interfaceBll.GetInterfaceAccountModel(orderBase.AccountId));
				hMPayApiBase.Account = interfaceAccount.ToHMAccount();
			}
			if (!string.IsNullOrEmpty(id) && !hMOrder.OrderNo.Equals(id))
			{
				LogUtil.DebugFormat("接口:{0},订单号：{1}，不一致", code, id);
				return Content("order not exist!");
			}
			HMNotifyResult<string> hMNotifyResult = hMPayApiBase.NotifySign(orderBase.ToHMOrder(), interfaceBusiness.ToHMSupplier(), interfaceAccount.ToHMAccount());
			if (hMNotifyResult.Code == HMNotifyState.Success)
			{
				if (orderBase.OrderAmt != hMOrder.OrderAmt)
				{
					LogUtil.Info("订单:" + orderBase.OrderId + "，金额不一致!");
					return Content("金额不正确!");
				}
				orderBase.ChannelOrderNo = hMOrder.SupplierOrderNo;
				payBll.CompleteOrder(orderBase);
				return Content(hMNotifyResult.Data);
			}
			if (hMNotifyResult.Code == HMNotifyState.ReturnUrl)
			{
				UserBaseInfo modelForId = new UserBaseBll().GetModelForId(orderBase.UserId);
				return new RedirectResult(ApiNotity.CreateNotifyUrl(orderBase, false, modelForId.ApiKey));
			}
			LogUtil.DebugFormat("接口:{0},订单号：{1}，{2}", code, id, hMNotifyResult.Message);
			return Content(hMNotifyResult.Data);
		}

		public ActionResult Return(string code, string id)
		{
			if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(id))
			{
				return Content("notity param null!");
			}
			HMInterface hMInterface = code.ToHMInterface();
			if (hMInterface == HMInterface.Unknown)
			{
				return Content("interface param null!");
			}
			new PayBll();
			OrderBll orderBll = new OrderBll();
			InterfaceBll interfaceBll = new InterfaceBll();
			OrderBase orderBase = null;
			HMOrder hMOrder = null;
			InterfaceBusiness interfaceBusiness = null;
			InterfaceAccount interfaceAccount = null;
			HMPayApiBase hMPayApiBase = HMPayFactory.CreatePayApi(hMInterface);
			HMNotifyResult<HMOrder> returnOrder = hMPayApiBase.GetReturnOrder();
			if (returnOrder.Code == HMNotifyState.Fail)
			{
				return Content(returnOrder.Message);
			}
			if (returnOrder.Code == HMNotifyState.WaitAccountInit)
			{
				orderBase = orderBll.GetOrderDetail(id);
				if (orderBase == null)
				{
					LogUtil.DebugFormat("接口:{0},订单号：{1}，不存在", code, id);
					return Content("order not exist!");
				}
				interfaceBusiness = interfaceBll.GetInterfaceBusinessModel(orderBase.InterfaceCode);
				interfaceAccount = (string.IsNullOrEmpty(orderBase.AccountId) ? new InterfaceAccount
				{
					Code = interfaceBusiness.Code,
					Account = interfaceBusiness.Account,
					ChildAccount = interfaceBusiness.ChildAccount,
					MD5Pwd = interfaceBusiness.MD5Pwd,
					RSAOpen = interfaceBusiness.RSAOpen,
					RSAPrivate = interfaceBusiness.RSAPrivate,
					Appid = interfaceBusiness.Appid,
					OpenId = interfaceBusiness.OpenId,
					OpenPwd = interfaceBusiness.OpenPwd,
					SubDomain = interfaceBusiness.SubDomain,
					BindDomain = interfaceBusiness.BindDomain
				} : interfaceBll.GetInterfaceAccountModel(orderBase.AccountId));
				hMPayApiBase.Account = interfaceAccount.ToHMAccount();
				returnOrder = hMPayApiBase.GetReturnOrder();
				if (returnOrder.Code == HMNotifyState.Fail)
				{
					return Content(returnOrder.Message);
				}
				hMOrder = returnOrder.Data;
			}
			else
			{
				hMOrder = returnOrder.Data;
				orderBase = orderBll.GetOrderBase(hMOrder.OrderNo);
				if (orderBase == null)
				{
					LogUtil.DebugFormat("接口:{0},订单号：{1}，不存在", code, id);
					return Content("order not exist!");
				}
				interfaceBusiness = interfaceBll.GetInterfaceBusinessModel(orderBase.InterfaceCode);
				interfaceAccount = (string.IsNullOrEmpty(orderBase.AccountId) ? new InterfaceAccount
				{
					Code = interfaceBusiness.Code,
					Account = interfaceBusiness.Account,
					ChildAccount = interfaceBusiness.ChildAccount,
					MD5Pwd = interfaceBusiness.MD5Pwd,
					RSAOpen = interfaceBusiness.RSAOpen,
					RSAPrivate = interfaceBusiness.RSAPrivate,
					Appid = interfaceBusiness.Appid,
					OpenId = interfaceBusiness.OpenId,
					OpenPwd = interfaceBusiness.OpenPwd,
					SubDomain = interfaceBusiness.SubDomain,
					BindDomain = interfaceBusiness.BindDomain
				} : interfaceBll.GetInterfaceAccountModel(orderBase.AccountId));
				hMPayApiBase.Account = interfaceAccount.ToHMAccount();
			}
			HMNotifyResult<string> hMNotifyResult = hMPayApiBase.ResultSign(orderBase.ToHMOrder(), null, interfaceAccount.ToHMAccount());
			if (hMNotifyResult.Code == HMNotifyState.Success)
			{
				try
				{
					UserBaseInfo modelForId = new UserBaseBll().GetModelForId(orderBase.UserId);
					return new RedirectResult(ApiNotity.CreateNotifyUrl(orderBase, false, modelForId.ApiKey));
				}
				catch (Exception exception)
				{
					LogUtil.Error("订单回调报错", exception);
				}
			}
			return Content(hMNotifyResult.Data);
		}
	}
}
