using HM.Framework;
using System.Collections.Generic;
using System.Text;

namespace HmPMer.Pay.Models
{
	public class PayParam
	{
		public string interface_version
		{
			get;
			set;
		}

		public string app_id
		{
			get;
			set;
		}

		public string trade_type
		{
			get;
			set;
		}

		public string total_amount
		{
			get;
			set;
		}

		public decimal amount
		{
			get;
			set;
		}

		public string out_trade_no
		{
			get;
			set;
		}

		public string notify_url
		{
			get;
			set;
		}

		public string return_url
		{
			get;
			set;
		}

		public string extra_return_param
		{
			get;
			set;
		}

		public string client_ip
		{
			get;
			set;
		}

		public string sign
		{
			get;
			set;
		}

		public string apiKey
		{
			get;
			set;
		}

		public static PayParam GetPayParam(string version)
		{
			PayParam payParam = null;
			switch (version)
			{
			case "HM.591":
				payParam = PayParam_591();
				payParam.amount = Utils.StringToDecimal(payParam.total_amount, decimal.Zero) * 100m;
				break;
			case "1.0":
				payParam = PayParam_V10();
				payParam.amount = Utils.StringToDecimal(payParam.total_amount, decimal.Zero);
				break;
			case "V2.0":
				payParam = PayParam_V20();
				payParam.amount = Utils.StringToDecimal(payParam.total_amount, decimal.Zero);
				break;
			}
			payParam.interface_version = version;
			if (string.IsNullOrEmpty(payParam.client_ip))
			{
				payParam.client_ip = Utils.GetClientIp();
			}
			return payParam;
		}

		public string CreateSign()
		{
			string result = string.Empty;
			switch (interface_version)
			{
			case "HM.591":
				result = CreateSign_591();
				break;
			case "1.0":
				result = CreateSign_V10();
				break;
			case "V2.0":
				result = CreateSign_V20();
				break;
			}
			return result;
		}

		public bool ValidSign()
		{
			return CreateSign().Equals(sign);
		}

		private static PayParam PayParam_591()
		{
			return new PayParam
			{
				app_id = Utils.GetRequest("parter"),
				trade_type = Utils.GetRequest("type"),
				total_amount = Utils.GetRequest("value"),
				out_trade_no = Utils.GetRequest("orderid"),
				notify_url = Utils.GetRequest("callbackurl"),
				return_url = Utils.GetRequest("hrefbackurl"),
				client_ip = Utils.GetRequest("clientip"),
				extra_return_param = Utils.GetRequest("attach"),
				sign = Utils.GetRequest("sign"),
				interface_version = "HM.591"
			};
		}

		private static PayParam PayParam_V10()
		{
			return new PayParam
			{
				app_id = Utils.GetRequest("parter"),
				trade_type = Utils.GetRequest("type"),
				total_amount = Utils.GetRequest("value"),
				out_trade_no = Utils.GetRequest("orderid"),
				notify_url = Utils.GetRequest("notifyurl"),
				return_url = Utils.GetRequest("returnurl"),
				client_ip = Utils.GetRequest("clientip"),
				extra_return_param = Utils.GetRequest("attach"),
				sign = Utils.GetRequest("sign"),
				interface_version = "1.0"
			};
		}

		private static PayParam PayParam_V20()
		{
			return new PayParam
			{
				app_id = Utils.GetRequest("app_id"),
				trade_type = Utils.GetRequest("trade_type"),
				total_amount = Utils.GetRequest("total_amount"),
				out_trade_no = Utils.GetRequest("out_trade_no"),
				notify_url = Utils.GetRequest("notify_url"),
				return_url = Utils.GetRequest("return_url"),
				client_ip = Utils.GetRequest("client_ip"),
				extra_return_param = Utils.GetRequest("extra_return_param"),
				sign = Utils.GetRequest("sign"),
				interface_version = "V2.0"
			};
		}

		private string CreateSign_591()
		{
			return EncryUtils.MD5($"parter={app_id}&type={trade_type}&value={total_amount}&orderid={out_trade_no}&callbackurl={notify_url}{apiKey}").ToLower();
		}

		private string CreateSign_V10()
		{
			return EncryUtils.MD5($"parter={app_id}&type={trade_type}&value={total_amount}&orderid={out_trade_no}&notifyurl={notify_url}{apiKey}").ToLower();
		}

		private string CreateSign_V20()
		{
			SortedDictionary<string, string> obj = new SortedDictionary<string, string>
			{
				{
					"app_id",
					app_id
				},
				{
					"trade_type",
					trade_type
				},
				{
					"total_amount",
					total_amount
				},
				{
					"out_trade_no",
					out_trade_no
				},
				{
					"notify_url",
					notify_url
				}
			};
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, string> item in obj)
			{
				stringBuilder.AppendFormat("&{0}={1}", item.Key, item.Value);
			}
			stringBuilder.Append(apiKey);
			stringBuilder.Remove(0, 1);
			return EncryUtils.MD5(stringBuilder.ToString()).ToLower();
		}
	}
}
