using HM.Framework.Logging;
using HM.Framework.PayApi.YunFuBao.Lib;
using System;
using System.Collections.Generic;

namespace HM.Framework.PayApi.YunFuBao
{
	public class HMPayApi : HMPayApiBase
	{
		public override string NOTIFY_SUCCESS => "success";

		public override string NOTIFY_FAIL => "fail";

		public override bool IsWithdraw => true;

		public override HMMode GetPayMode(HMChannel code)
		{
			throw new NotImplementedException();
		}

		public override HMNotifyResult<string> NotifySign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			throw new NotImplementedException();
		}

		public override HMNotifyResult<string> ResultSign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			throw new NotImplementedException();
		}

		protected override Dictionary<string, string> GetNotifyParam()
		{
			throw new NotImplementedException();
		}

		protected override Dictionary<string, string> GetReturnParam()
		{
			throw new NotImplementedException();
		}

		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
			throw new NotImplementedException();
		}

		protected override HMPayResult PayGatewayBody(HMOrder order)
		{
			throw new NotImplementedException();
		}

		protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
		{
			throw new NotImplementedException();
		}

		private string GetWithdrawChannelCode(HMWithdrawChannel code)
		{
			switch (code)
			{
			case HMWithdrawChannel.ICBC:
				return "工商银行";
			case HMWithdrawChannel.BOC:
				return "中国银行";
			case HMWithdrawChannel.CCB:
				return "建设银行";
			case HMWithdrawChannel.ABC:
				return "农业银行";
			case HMWithdrawChannel.CMB:
				return "招商银行";
			case HMWithdrawChannel.PSBC:
				return "中国邮政储蓄银行";
			case HMWithdrawChannel.BCOM:
				return "交通银行";
			case HMWithdrawChannel.CEBB:
				return "光大银行";
			case HMWithdrawChannel.CMBC:
				return "民生银行";
			case HMWithdrawChannel.SPDB:
				return "浦发银行";
			case HMWithdrawChannel.HXB:
				return "华夏银行";
			default:
				return "";
			}
		}

		protected override HMPayResult WithdrawGatewayBody(HMWithdraw withdraw)
		{
			HMPayResult fail = HMPayResult.Fail;
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("login_token", "");
			dictionary.Add("req_no", withdraw.OrderNo);
			dictionary.Add("app_code", "apc_02000001977");
			dictionary.Add("service_code", "sne_00000000002");
			dictionary.Add("app_version", "1.0.0");
			dictionary.Add("plat_form", "03");
			dictionary.Add("merchant_number", base.Account.AccountUser);
			dictionary.Add("order_number", withdraw.OrderNo);
			dictionary.Add("wallet_id", base.Account.AppId);
			dictionary.Add("asset_id", base.Account.OpenId);
			dictionary.Add("password_type", "02");
			dictionary.Add("encrypt_type", "02");
			dictionary.Add("pay_password", EncryUtils.MD5(base.Account.Md5Pwd));
			dictionary.Add("customer_type", (withdraw.BankAccountType == 1) ? "02" : "01");
			dictionary.Add("customer_name", withdraw.FactName);
			dictionary.Add("currency", "CNY");
			dictionary.Add("amount", (withdraw.Amount / 100m).ToString("0.00"));
			dictionary.Add("async_notification_addr", "http://localhost:57924/");
			dictionary.Add("asset_type_code", "000002");
			dictionary.Add("account_type_code", (withdraw.BankAccountType == 1) ? "04" : "01");
			dictionary.Add("effective_time", "");
			dictionary.Add("account_number", withdraw.BankCode);
			dictionary.Add("issue_bank_name", withdraw.BankName);
			dictionary.Add("headquarters_bank_name", withdraw.BankName);
			dictionary.Add("province_name", withdraw.ProvinceName);
			dictionary.Add("city_name", withdraw.CityName);
			try
			{
				M2Sdk m2Sdk = new M2Sdk();
				LogUtil.Info("美付宝代付接口请求地址：" + base.Supplier.AgentPayUrl + "   Aid:" + base.Account.RsaPublic + "   key:" + base.Account.RsaPrivate + "  Data:" + dictionary.ToJson());
				string text = m2Sdk.InvokeM2(base.Supplier.AgentPayUrl, base.Account.RsaPublic, "epay_api_deal@agent_for_paying", base.Account.RsaPrivate, dictionary.ToJson());
				LogUtil.Info("美付宝代付接口httpResult：" + text);
				YFBResult yFBResult = text.FormJson<YFBResult>();
				if (!yFBResult.op_ret_code.Equals("000"))
				{
					fail.Code = HMPayState.Fail;
					fail.Message = yFBResult.op_err_msg;
					return fail;
				}
				fail.Code = HMPayState.Paymenting;
				return fail;
			}
			catch (Exception exception)
			{
				fail.Message = "系统繁忙，请稍候再试！";
				LogUtil.Error("云付宝代付接口出错：", exception);
				return fail;
			}
		}

		protected override HMPayResult WithdrawQueryBody(HMWithdraw withdraw)
		{
			HMPayResult fail = HMPayResult.Fail;
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("login_token", "");
			dictionary.Add("req_no", withdraw.OrderNo);
			dictionary.Add("app_code", "apc_02000001977");
			dictionary.Add("service_code", "sne_00000000002");
			dictionary.Add("app_version", "1.0.0");
			dictionary.Add("plat_form", "03");
			dictionary.Add("merchant_number", base.Account.AccountUser);
			dictionary.Add("order_number", withdraw.OrderNo);
			dictionary.Add("deal_type", "07");
			try
			{
				string text = new M2Sdk().InvokeM2(base.Supplier.AgentPayUrl, base.Account.RsaPublic, "epay_api_deal@get_order_deal_result", base.Account.RsaPrivate, dictionary.ToJson());
				LogUtil.Info("云付宝代付查询接口httpResult：" + text);
				YFBResult yFBResult = text.FormJson<YFBResult>();
				withdraw.ChannelOrderNo = yFBResult.order_id;
				if (!yFBResult.op_ret_code.Equals("000"))
				{
					if (!yFBResult.op_ret_code.Equals("600"))
					{
						if (!yFBResult.op_ret_code.Equals("701"))
						{
							fail.Code = HMPayState.Fail;
							fail.Message = yFBResult.op_err_msg;
							return fail;
						}
						fail.Code = HMPayState.Paymenting;
						return fail;
					}
					fail.Code = HMPayState.Fail;
					fail.Message = yFBResult.op_err_msg;
					return fail;
				}
				fail.Code = HMPayState.Success;
				return fail;
			}
			catch (Exception exception)
			{
				fail.Message = "系统繁忙，请稍候再试！";
				LogUtil.Error("云付宝代付查询接口出错：", exception);
				return fail;
			}
		}
	}
}
