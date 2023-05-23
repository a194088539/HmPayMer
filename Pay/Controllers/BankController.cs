using HM.Framework;
using HM.Framework.PayApi;
using HmPMer.Business;
using HmPMer.Entity;
using HmPMer.Pay.Models;
using System;
using System.Web.Mvc;

namespace HmPMer.Pay.Controllers
{
	public class BankController : Controller
	{
		private string GetPayType(string t)
		{
			switch (t)
			{
			case "992":
				return "ALIPAY_NATIVE";
			case "993":
				return "ALIPAY_H5_NATIVE";
			case "1003":
				return "ALIPAY_H5";
			case "1004":
				return "ALIPAY_HB_H5";
			case "1005":
				return "WEIXIN_NATIVE";
			case "1007":
				return "WEIXIN_H5";
			case "1008":
				return "QQPAY_NATIVE";
			case "1009":
				return "QQPAY_H5";
			case "2001":
				return "ALIPAY_MICROPAY";
			case "2002":
				return "WEIXIN_MICROPAY";
			case "3001":
				return "GATEWAY_QUICK";
			case "970":
				return "CMB";
			case "967":
				return "ICBC";
			case "965":
				return "CCB";
			case "964":
				return "ABC";
			case "980":
				return "CMBC";
			case "982":
				return "HXB";
			case "972":
				return "CIB";
			case "981":
				return "BCOM";
			case "986":
				return "CEBB";
			case "963":
			case "991":
				return "BOC";
			case "978":
				return "SPABANK";
			case "971":
				return "PSBC";
			case "977":
				return "SPDB";
			case "962":
				return "ECITIC";
			default:
				return string.Empty;
			}
		}

		public ActionResult Index()
		{
			PayParam payParam = PayParam.GetPayParam("HM.591");
			if (string.IsNullOrEmpty(payParam.sign))
			{
				return Content("error:1000 签名不能空！");
			}
			if (string.IsNullOrEmpty(payParam.app_id))
			{
				return Content("error:1001 商户号不能空！");
			}
			if (string.IsNullOrEmpty(payParam.trade_type))
			{
				return Content("error:1002 交易类型不能空！");
			}
			if (string.IsNullOrEmpty(payParam.total_amount))
			{
				return Content("error:1003 订单金额不能空！");
			}
			if (payParam.amount == decimal.Zero)
			{
				return Content("error:1003 订单金额不能空！");
			}
			if (string.IsNullOrEmpty(payParam.out_trade_no))
			{
				return Content("error:1004 订单号不能空！");
			}
			if (string.IsNullOrEmpty(payParam.notify_url))
			{
				return Content("error:1005 异步通知地址不能空！");
			}
			if (payParam.app_id.Length != 10)
			{
				return Content("error:1020 商户号长度不正确！");
			}
			if (payParam.trade_type.Length > 30)
			{
				return Content("error:1021 交易类型长度不正确！");
			}
			if (payParam.out_trade_no.Length > 36)
			{
				return Content("error:1022 订单号长度不能超过36位！");
			}
			if (payParam.notify_url.Length > 255)
			{
				return Content("error:1023 异步通知地址长度超过255位！");
			}
			if (!string.IsNullOrEmpty(payParam.return_url) && payParam.return_url.Length > 255)
			{
				return Content("error:1024 同步通知地址长度超过255位！");
			}
			if (payParam.client_ip.Length > 100)
			{
				return Content("error:1025 客户IP长度不正确！");
			}
			if (!string.IsNullOrEmpty(payParam.extra_return_param) && payParam.extra_return_param.Length > 250)
			{
				return Content("error:1026 订单备注长度超出250位！");
			}
			if (payParam.sign.Length > 64)
			{
				return Content("error:1027 签名长度不正确！");
			}
			if (!Utils.IsNumeric(payParam.app_id))
			{
				return Content("error:1030 商户号不符合规范！");
			}
			if (payParam.amount < decimal.Zero)
			{
				return Content("error:1031 订单金额不能是负数！");
			}
			if (!Utils.IsNotifyUrlOk(payParam.notify_url))
			{
				return Content("error:1033 异步通知格式不正确！");
			}
			if (!string.IsNullOrEmpty(payParam.return_url) && !Utils.IsNotifyUrlOk(payParam.return_url))
			{
				return Content("error:1034 同步通知格式不正确！");
			}
			if (!string.IsNullOrEmpty(payParam.client_ip) && !Utils.IsIPSect(payParam.client_ip))
			{
				return Content("error:1035 同步通知客户IP格式不正确！");
			}
			string text = "";
			UserBase userBase = null;
			OrderBase orderBase = null;
			OrderPayCode orderPayCode = null;
			DateTime now = DateTime.Now;
			UserBaseBll userBaseBll = new UserBaseBll();
			PayBll payBll = new PayBll();
			RateBll rateBll = new RateBll();
			new OrderBll();
			text = payBll.GetPayType(GetPayType(payParam.trade_type));
			if (string.IsNullOrEmpty(text))
			{
				return Content("error:1036 通道类型未开启！");
			}
			userBase = userBaseBll.GetModelForId(payParam.app_id);
			if (userBase == null)
			{
				return Content("error:1041 商户号不存在！");
			}
			if (userBase.IsEnabled == 0)
			{
				return Content("error:1042 商户被禁用，无法进行支付！");
			}
			if (userBase.IsEnabled == 2)
			{
				return Content("error:1042 商户已被冻结，无法进行支付！");
			}
			payParam.apiKey = userBase.ApiKey;
			if (!payParam.ValidSign())
			{
				return Content("error:1043 签名验证失败！");
			}
			orderPayCode = payBll.GetOrderPayCodeByMerOrderNo(payParam.out_trade_no, payParam.app_id);
			if (orderPayCode != null)
			{
				switch (orderPayCode.PayState)
				{
				case 1:
					return Content("error:1051 订单已支付！");
				case 2:
					return Content("error:1052 订单支付失败！");
				case 3:
					return Content("error:1053 此订单已失效！");
				default:
					if (orderPayCode.ExpiredTime <= now)
					{
						return Content("error:1053 此订单已失效！");
					}
					if (orderPayCode.AddTime.HasValue)
					{
						switch (orderPayCode.PayMode)
						{
						case 1:
							return Content(orderPayCode.Codes);
						case 2:
							return Json(orderPayCode.Codes);
						case 0:
							return new RedirectResult(orderPayCode.Codes);
						default:
							base.ViewBag.Order = orderPayCode;
							return View();
						}
					}
					return Content("error:1054 刷新频率过高,请间隔五秒钟再次刷新页面！");
				}
			}
			InterfaceType payTypeUser = payBll.GetPayTypeUser(payParam.app_id, text);
			if (payTypeUser == null)
			{
				return Content("error:1061 您还未签约此通道类型，无法使用！");
			}
			PayRateInfo payRate = rateBll.GetPayRate(text, userBase.UserId, userBase.GradeId);
			if (payRate == null)
			{
				return Content("error:1062 您还未得到签约费率，无法使用！");
			}
			PayTypeQuota payTypeQuotaForPayCode = new SystemBll().GetPayTypeQuotaForPayCode(text);
			if (payTypeQuotaForPayCode != null)
			{
				if (payTypeQuotaForPayCode.minMoney > decimal.Zero && payParam.amount < payTypeQuotaForPayCode.minMoney)
				{
					return Content("error:1071 交易金额小于最低限额" + (payTypeQuotaForPayCode.minMoney / 100m).ToString("0.00") + "元！");
				}
				if (payTypeQuotaForPayCode.maxMoney > decimal.Zero && payParam.amount > payTypeQuotaForPayCode.maxMoney)
				{
					return Content("error:1071 交易金额大于最高限额" + (payTypeQuotaForPayCode.maxMoney / 100m).ToString("0.00") + "元！");
				}
			}
			payParam.trade_type = GetPayType(payParam.trade_type);
			DateTime dateTime = now.AddMinutes(5.0);
			InterfaceBusiness interfaceBusiness = null;
			if (string.IsNullOrEmpty(payTypeUser.DefaulInfaceCode))
			{
				PayChannel payChannelModel = payBll.GetPayChannelModel(payParam.trade_type);
				if (string.IsNullOrEmpty(payChannelModel.InterfaceCode))
				{
					return Content("error:1063 通道未配置接口！");
				}
				interfaceBusiness = new InterfaceBll().GetInterfaceBusinessModel(payChannelModel.InterfaceCode);
			}
			else
			{
				interfaceBusiness = new InterfaceBll().GetInterfaceBusinessModel(payTypeUser.DefaulInfaceCode);
			}
			if (interfaceBusiness == null)
			{
				return Content("error:1063 支付接口不存在！");
			}
			InterfaceAccount interfaceAccount = payBll.GetInterfaceAccount(text, payParam.trade_type, interfaceBusiness, payParam.amount, now, dateTime);
			if (interfaceAccount == null)
			{
				return Content("error:1064 支付账户未设置！");
			}
			orderBase = new OrderBase
			{
				UserId = payParam.app_id,
				MerOrderNo = payParam.out_trade_no,
				OrderTime = now,
				PayTime = now,
				ExpiredTime = dateTime,
				MerOrderAmt = payParam.amount,
				OrderAmt = interfaceAccount.OrderAmt,
				Attach = payParam.extra_return_param,
				NotifyUrl = payParam.notify_url,
				ReturnUrl = payParam.return_url,
				ClientIp = payParam.client_ip,
				Version = payParam.interface_version,
				PayRate = payRate.Rate,
				InterfaceCode = interfaceBusiness.Code,
				AccountId = interfaceAccount.Id,
				PromId = userBase.PromId,
				AgentId = userBase.AgentId,
				ChannelCode = payParam.trade_type,
				PayCode = text
			};
			if (!payBll.InsertOrderInfo(orderBase))
			{
				return Content("error:1060 生成订单数据失败！");
			}
			HMPayResult hMPayResult = PayFactory.PayGateway(payParam.trade_type, interfaceBusiness, interfaceAccount, orderBase);
			if (hMPayResult == null)
			{
				return Content("error:999 支付接口发生异常，无法支付！");
			}
			if (hMPayResult.Code == HMPayState.Success)
			{
				orderPayCode = new OrderPayCode
				{
					OrderId = orderBase.OrderId,
					PayMode = (int)hMPayResult.Mode,
					PayCode = orderBase.PayCode,
					ChannelCode = orderBase.ChannelCode,
					Codes = hMPayResult.Data,
					AddTime = DateTime.Now,
					UpdateTime = DateTime.Now,
					OrderAmt = orderBase.OrderAmt,
					OrderTime = orderBase.OrderTime.Value
				};
				payBll.InsertOrderPayCode(orderPayCode);
				switch (hMPayResult.Mode)
				{
				case HMMode.输出字符串:
					return Content(hMPayResult.Data);
				case HMMode.输出Json:
					return Json(hMPayResult.Data);
				case HMMode.跳转链接:
					return new RedirectResult(hMPayResult.Data);
				default:
					base.ViewBag.Order = orderPayCode;
					return View();
				}
			}
			return Content("error:999 " + hMPayResult.Message);
		}
	}
}
