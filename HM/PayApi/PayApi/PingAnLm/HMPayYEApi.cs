using HM.Framework.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace HM.Framework.PayApi.PingAnLm
{
	public class HMPayYEApi : HMPayApiBase
	{
		public override string NOTIFY_SUCCESS => "SUCCESS";

		public override string NOTIFY_FAIL => "FAIL";

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

		protected override HMPayResult WithdrawGatewayBody(HMWithdraw withdraw)
		{
			HMPayResult fail = HMPayResult.Fail;
			string accountUser = base.Account.AccountUser;
			string arg = "merchant.withdraw";
			string arg2 = "MD5";
			string arg3 = "1.0";
			string text = "";
			string text2 = "";
			text = new SortedDictionary<string, string>
			{
				{
					"merchant_no",
					base.Account.ChildAccountUser
				},
				{
					"out_trade_no",
					withdraw.OrderNo
				},
				{
					"amount",
					(withdraw.Amount / 100m).ToString("0.00")
				},
				{
					"bank_account_no",
					withdraw.BankCode
				},
				{
					"bank_account_name",
					withdraw.FactName
				},
				{
					"bank_mobile",
					withdraw.MobilePhone
				},
				{
					"is_company",
					withdraw.BankAccountType.ToString()
				},
				{
					"bank_code",
					withdraw.BankLasalleCode
				},
				{
					"bank_name",
					withdraw.BankName
				},
				{
					"remark",
					"余额转账"
				}
			}.ToJson();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("app_id={0}", accountUser).AppendFormat("&content={0}", text).AppendFormat("&method={0}", arg)
				.AppendFormat("&version={0}", arg3);
			text2 = EncryUtils.MD5(stringBuilder.ToString() + $"&key={base.Account.Md5Pwd}", "UTF-8").ToLower();
			stringBuilder.AppendFormat("&sign_type={0}", arg2);
			stringBuilder.AppendFormat("&sign={0}", text2);
			try
			{
				string text3 = stringBuilder.ToString();
				string text4 = HttpUtils.SendRequest(base.Supplier.AgentPayUrl, text3, "POST", "UTF-8");
				LogUtil.DebugFormat(" 平安余额代付 提交地址：{0}，提交内容：{1}, 提交结果：{2}", base.Supplier.AgentPayUrl, text3, text4);
				if (string.IsNullOrEmpty(text4))
				{
					fail.Message = "接口商返回结果为空！";
					return fail;
				}
				DfResult dfResult = text4.FormJson<DfResult>();
				if (!dfResult.error_code.Equals("0"))
				{
					fail.Code = HMPayState.Fail;
					fail.Message = dfResult.error_msg;
					return fail;
				}
				fail.Code = HMPayState.Paymenting;
				return fail;
			}
			catch (Exception exception)
			{
				fail.Message = "系统繁忙，请稍候再试！";
				LogUtil.Error("平安余额代付接口出错:", exception);
				return fail;
			}
		}

		protected override HMPayResult WithdrawQueryBody(HMWithdraw withdraw)
		{
			HMPayResult fail = HMPayResult.Fail;
			string accountUser = base.Account.AccountUser;
			string arg = "merchant.withdraw_query";
			string arg2 = "MD5";
			string arg3 = "1.0";
			string text = "";
			string text2 = "";
			text = new SortedDictionary<string, string>
			{
				{
					"out_trade_no",
					withdraw.OrderNo
				}
			}.ToJson();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("app_id={0}", accountUser).AppendFormat("&content={0}", text).AppendFormat("&method={0}", arg)
				.AppendFormat("&version={0}", arg3);
			text2 = EncryUtils.MD5(stringBuilder.ToString() + $"&key={base.Account.Md5Pwd}", "UTF-8").ToLower();
			stringBuilder.AppendFormat("&sign_type={0}", arg2);
			stringBuilder.AppendFormat("&sign={0}", text2);
			try
			{
				string text3 = stringBuilder.ToString();
				string text4 = HttpUtils.SendRequest(base.Supplier.QueryUri, text3, "POST", "UTF-8");
				LogUtil.DebugFormat(" 平安余额代付查询 提交地址：{0}，提交内容：{1}, 提交结果：{2}", base.Supplier.QueryUri, text3, text4);
				if (string.IsNullOrEmpty(text4))
				{
					fail.Message = "接口商返回结果为空！";
					return fail;
				}
				DfSResult dfSResult = text4.FormJson<DfSResult>();
				withdraw.ChannelOrderNo = dfSResult.trade_no;
				if (dfSResult.trade_status != 1)
				{
					if (dfSResult.trade_status != 2)
					{
						fail.Code = HMPayState.Paymenting;
						fail.Message = dfSResult.remark;
						return fail;
					}
					fail.Code = HMPayState.Fail;
					fail.Message = dfSResult.remark;
					return fail;
				}
				fail.Code = HMPayState.Success;
				return fail;
			}
			catch (Exception exception)
			{
				fail.Message = "系统繁忙，请稍候再试！";
				LogUtil.Error("平安余额代付查询接口出错：", exception);
				return fail;
			}
		}
	}
}
