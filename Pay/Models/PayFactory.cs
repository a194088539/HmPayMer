using HM.Framework.PayApi;
using HmPMer.Business;
using HmPMer.Entity;
using System.Collections.Generic;
using PayApi;

namespace HmPMer.Pay.Models
{
	public class PayFactory
	{
		private static SysConfigBll configBll;

		private static string apiUrl;

		static PayFactory()
		{
			configBll = new SysConfigBll();
			apiUrl = "";
			SysConfig forKey = configBll.GetForKey("ApiUrl");
			if (forKey.Value.EndsWith("/"))
			{
				apiUrl = forKey.Value;
			}
			else
			{
				apiUrl = forKey.Value + "/";
			}
		}

		private static string GetNotifyUri(string code, string orderId)
		{
			string text = apiUrl + "OrderNotity/" + code;
            InterfaceBll interfaceBll = new InterfaceBll();
            InterfaceBusiness interfaceBusiness = interfaceBll.GetInterfaceBusinessModel(code);
            if(interfaceBusiness != null 
                && !string.IsNullOrWhiteSpace(interfaceBusiness.CallbackDomain)
                && (interfaceBusiness.CallbackDomain.StartsWith("http://") || interfaceBusiness.CallbackDomain.StartsWith("https://")))
            {
                string baseUrl = "";
                if (interfaceBusiness.CallbackDomain.EndsWith("/"))
                {
                    baseUrl = interfaceBusiness.CallbackDomain;
                }
                else
                {
                    baseUrl = interfaceBusiness.CallbackDomain + "/";
                }
                text = baseUrl + "OrderNotity/" + code;
            }
			HashSet<string> hashSet = new HashSet<string>
			{
				"shande",
				"leniuniu",
				"shoufupay",
				"zhihuipayshangde",
				"zonghedianshang"
			};
			if (!string.IsNullOrEmpty(orderId) && !hashSet.Contains(code))
			{
				text = text + "/" + orderId;
			}
			return text;
		}

		private static string GetReturnUri(string code, string orderId)
		{
			string text = apiUrl + "OrderReturn/" + code;
			if (!string.IsNullOrEmpty(orderId))
			{
				text = text + "/" + orderId;
			}
			return text;
		}

		public static HMPayResult PayGateway(string channel, InterfaceBusiness interfaceBusiness, InterfaceAccount payAccount, OrderBase orderBase)
		{
			HMPayApiBase hMPayApiBase = HMPayFactory.CreatePayApi(interfaceBusiness.Code.ToHMInterface());
			HMSupplier hMSupplier = interfaceBusiness.ToHMSupplier();
			HMAccount hMAccount = payAccount.ToHMAccount();
			HMOrder hMOrder = orderBase.ToHMOrder();
			hMSupplier.NotifyUri = GetNotifyUri(hMSupplier.Code, hMOrder.OrderNo);
			hMSupplier.ReturnUri = GetReturnUri(hMSupplier.Code, hMOrder.OrderNo);
			if (!string.IsNullOrEmpty(hMAccount.BindDomain))
			{
				string text = hMAccount.BindDomain;
				if (!text.StartsWith("http:") || !text.StartsWith("https:"))
				{
					text = "http://" + text;
				}
				if (!text.EndsWith("/"))
				{
					text += "/";
				}
				hMSupplier.AuthUri = text + "PayAuth/" + hMSupplier.Code + "/" + hMOrder.OrderNo;
			}
			else
			{
				hMSupplier.AuthUri = apiUrl + "PayAuth/" + hMSupplier.Code + "/" + hMOrder.OrderNo;
			}
			hMPayApiBase.Supplier = hMSupplier;
			hMPayApiBase.Account = hMAccount;
			hMPayApiBase.Order = hMOrder;
			return hMPayApiBase.PayGateway(hMOrder);
		}

		public static HMPayResult SubmitAuthGateway(string channel, string code, InterfaceBusiness interfaceBusiness, InterfaceAccount payAccount, OrderBase orderBase)
		{
			HMPayApiBase hMPayApiBase = HMPayFactory.CreatePayApi(interfaceBusiness.Code.ToHMInterface());
			HMSupplier hMSupplier = interfaceBusiness.ToHMSupplier();
			HMAccount hMAccount = payAccount.ToHMAccount();
			HMOrder hMOrder = orderBase.ToHMOrder();
			hMSupplier.NotifyUri = GetNotifyUri(hMSupplier.Code, hMOrder.OrderNo);
			hMSupplier.ReturnUri = GetReturnUri(hMSupplier.Code, hMOrder.OrderNo);
			if (!string.IsNullOrEmpty(hMAccount.BindDomain))
			{
				string text = hMAccount.BindDomain;
				if (!text.StartsWith("http:") || !text.StartsWith("https:"))
				{
					text = "http://" + text;
				}
				if (!text.EndsWith("/"))
				{
					text += "/";
				}
				hMSupplier.AuthUri = text + "PayAuth/" + hMSupplier.Code + "/" + hMOrder.OrderNo;
			}
			else
			{
				hMSupplier.AuthUri = apiUrl + "PayAuth/" + hMSupplier.Code + "/" + hMOrder.OrderNo;
			}
			hMPayApiBase.Supplier = hMSupplier;
			hMPayApiBase.Account = hMAccount;
			hMPayApiBase.Order = hMOrder;
			return hMPayApiBase.AuthPayGateway(code, hMOrder);
		}

		public static HMPayResult QueryCallback(InterfaceBusiness interfaceBusiness, InterfaceAccount payAccount, OrderBase orderBase)
		{
			HMPayApiBase hMPayApiBase = HMPayFactory.CreatePayApi(interfaceBusiness.Code.ToHMInterface());
			HMSupplier hMSupplier = interfaceBusiness.ToHMSupplier();
			HMAccount hMAccount = payAccount.ToHMAccount();
			HMOrder hMOrder = orderBase.ToHMOrder();
			hMSupplier.NotifyUri = GetNotifyUri(hMSupplier.Code, hMOrder.OrderNo);
			hMSupplier.ReturnUri = GetReturnUri(hMSupplier.Code, hMOrder.OrderNo);
			if (!string.IsNullOrEmpty(hMAccount.BindDomain))
			{
				string text = hMAccount.BindDomain;
				if (!text.StartsWith("http:") || !text.StartsWith("https:"))
				{
					text = "http://" + text;
				}
				if (!text.EndsWith("/"))
				{
					text += "/";
				}
				hMSupplier.AuthUri = text + "PayAuth/" + hMSupplier.Code + "/" + hMOrder.OrderNo;
			}
			else
			{
				hMSupplier.AuthUri = apiUrl + "PayAuth/" + hMSupplier.Code + "/" + hMOrder.OrderNo;
			}
			hMPayApiBase.Supplier = hMSupplier;
			hMPayApiBase.Account = hMAccount;
			hMPayApiBase.Order = hMOrder;
			return hMPayApiBase.QueryCallback(hMOrder);
		}
	}
}
