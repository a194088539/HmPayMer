using System.Collections.Generic;

namespace HM.Framework.PayApi
{
	public abstract class HMPayApiBase
	{
		public abstract string NOTIFY_SUCCESS
		{
			get;
		}

		public abstract string NOTIFY_FAIL
		{
			get;
		}

		public abstract bool IsWithdraw
		{
			get;
		}

		public HMSupplier Supplier
		{
			get;
			set;
		}

		public HMAccount Account
		{
			get;
			set;
		}

		public HMOrder Order
		{
			get;
			set;
		}

		protected Dictionary<string, string> NotifyParams
		{
			get;
			set;
		}

		protected Dictionary<string, string> ReturnParams
		{
			get;
			set;
		}

		protected string GetNotifyRequest(string key)
		{
			if (NotifyParams == null || !NotifyParams.ContainsKey(key))
			{
				return string.Empty;
			}
			return NotifyParams[key];
		}

		protected string GetReturnRequest(string key)
		{
			if (ReturnParams == null || !ReturnParams.ContainsKey(key))
			{
				return string.Empty;
			}
			return ReturnParams[key];
		}

		public HMPayApiBase(HMSupplier supplier, HMAccount account)
		{
			Supplier = supplier;
			Account = account;
		}

		public HMPayApiBase()
		{
		}

		public abstract HMMode GetPayMode(HMChannel code);

		protected abstract HMPayResult PayGatewayBody(HMOrder order);

		protected abstract Dictionary<string, string> GetNotifyParam();

		protected abstract Dictionary<string, string> GetReturnParam();

		protected abstract HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic);

		protected abstract HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic);

		public HMPayResult PayGateway(HMOrder order)
		{
			HMPayResult fail = HMPayResult.Fail;
			fail.Mode = HMMode.输出字符串;
			if (Supplier == null)
			{
				fail.Code = HMPayState.Error;
				fail.Message = "没有设置支付接口！";
				return fail;
			}
			if (Account == null)
			{
				fail.Code = HMPayState.Error;
				fail.Message = "没有设置支付账户！";
				return fail;
			}
			if (order == null)
			{
				fail.Code = HMPayState.Error;
				fail.Message = "支付订单为空！";
				return fail;
			}
			return PayGatewayBody(order);
		}

		public virtual HMNotifyResult<HMOrder> GetNotifyOrder()
		{
			NotifyParams = GetNotifyParam();
			return NotifyParamToOrder(NotifyParams);
		}

		public virtual HMNotifyResult<HMOrder> GetReturnOrder()
		{
			ReturnParams = GetReturnParam();
			return ReturnParamToOrder(ReturnParams);
		}

		public abstract HMNotifyResult<string> NotifySign(HMOrder order, HMSupplier supplier, HMAccount account);

		public abstract HMNotifyResult<string> ResultSign(HMOrder order, HMSupplier supplier, HMAccount account);

		public HMPayResult WithdrawGateway(HMWithdraw withdraw)
		{
			HMPayResult fail = HMPayResult.Fail;
			if (!IsWithdraw)
			{
				fail.Message = "此接口不支持代付！";
				return fail;
			}
			if (Supplier == null)
			{
				fail.Message = "接口数据未设置！";
				return fail;
			}
			if (Account == null)
			{
				fail.Message = "接口账户未设置！";
				return fail;
			}
			if (withdraw == null)
			{
				fail.Message = "下发账户未设置！";
				return fail;
			}
			return WithdrawGatewayBody(withdraw);
		}

		public HMPayResult WithdrawQuery(HMWithdraw withdraw)
		{
			HMPayResult fail = HMPayResult.Fail;
			if (!IsWithdraw)
			{
				fail.Message = "此接口不支持代付！";
				return fail;
			}
			if (Supplier == null)
			{
				fail.Message = "接口数据未设置！";
				return fail;
			}
			if (Account == null)
			{
				fail.Message = "接口账户未设置！";
				return fail;
			}
			if (withdraw == null)
			{
				fail.Message = "下发账户未设置！";
				return fail;
			}
			return WithdrawQueryBody(withdraw);
		}

		public string GetPayResultStr(HMPayState payState)
		{
			if (payState == HMPayState.Success)
			{
				return NOTIFY_SUCCESS;
			}
			return NOTIFY_FAIL;
		}

		public HMPayResult QueryCallback(HMOrder order)
		{
			HMPayResult fail = HMPayResult.Fail;
			fail.Mode = HMMode.输出字符串;
			if (Supplier == null)
			{
				fail.Code = HMPayState.Error;
				fail.Message = "没有设置支付接口！";
				return fail;
			}
			if (Account == null)
			{
				fail.Code = HMPayState.Error;
				fail.Message = "没有设置支付账户！";
				return fail;
			}
			if (order == null)
			{
				fail.Code = HMPayState.Error;
				fail.Message = "支付订单为空！";
				return fail;
			}
			return QueryCallbackBody(order);
		}

		protected virtual HMPayResult QueryCallbackBody(HMOrder order)
		{
			HMPayResult fail = HMPayResult.Fail;
			fail.Message = "此接口不支持！";
			return fail;
		}

		public virtual HMPayResult AuthPayGateway(string code, HMOrder order)
		{
			HMPayResult fail = HMPayResult.Fail;
			fail.Message = "授权网关已关闭！";
			return fail;
		}

		protected virtual HMPayResult WithdrawGatewayBody(HMWithdraw withdraw)
		{
			HMPayResult fail = HMPayResult.Fail;
			fail.Message = "代付接口未实现";
			return fail;
		}

		protected virtual HMPayResult WithdrawQueryBody(HMWithdraw withdraw)
		{
			HMPayResult fail = HMPayResult.Fail;
			fail.Message = "代付查询接口未实现";
			return fail;
		}
	}
}
