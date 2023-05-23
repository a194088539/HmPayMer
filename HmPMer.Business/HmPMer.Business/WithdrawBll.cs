using HM.Framework;
using HM.Framework.PayApi;
using HmPMer.Dal;
using HmPMer.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PayApi;

namespace HmPMer.Business
{
	public class WithdrawBll
	{
		private WithdrawDal _dal = new WithdrawDal();

		public List<WithdrawChannelInfo> GetWithdrawChannelPageList(WithdrawChannelInfo parm, ref Paging paging)
		{
			return _dal.GetWithdrawChannelPageList(parm, ref paging);
		}

		public List<WithdrawChannel> GetWithdrawChannelList(int isEnabled = -1)
		{
			return _dal.GetWithdrawChannelList(isEnabled);
		}

		public WithdrawChannelInfo WithdrawChannelGetModel(string Id)
		{
			return _dal.WithdrawChannelGetModel(Id);
		}

		public int UpWCIsEnabled(string Id, int IsEnabled)
		{
			return _dal.UpWCIsEnabled(Id, IsEnabled);
		}

		public long WithdrawChannelAdd(WithdrawChannel Model)
		{
			return _dal.WithdrawChannelAdd(Model);
		}

		public bool WithdrawChannelUpdate(WithdrawChannel Model)
		{
			return _dal.WithdrawChannelUpdate(Model);
		}

		public List<WithdrawSchemeinfo> GetWithdrawSchemePageList(WithdrawSchemeinfo parm, ref Paging paging)
		{
			return _dal.GetWithdrawSchemePageList(parm, ref paging);
		}

		public List<WithdrawScheme> GetAllWithdrawSchemeList(int UserType)
		{
			return _dal.GetAllWithdrawSchemeList(UserType);
		}

		public WithdrawScheme WithdrawSchemeGetModel(string Id)
		{
			return _dal.WithdrawSchemeGetModel(Id);
		}

		public long WithdrawSchemeAdd(WithdrawScheme Model)
		{
			return _dal.WithdrawSchemeAdd(Model);
		}

		public bool WithdrawSchemeUpdate(WithdrawScheme Model)
		{
			return _dal.WithdrawSchemeUpdate(Model);
		}

		public int DelWithdrawScheme(string Id)
		{
			return _dal.DelWithdrawScheme(Id);
		}

		public List<WithdrawOrderInfo> GetWithdrawOrderPageList(WithdrawOrderQueryParam parm, ref Paging paging)
		{
			return _dal.GetWithdrawOrderPageList(parm, ref paging);
		}

		public DataTable GetWithdrawOrderTable(WithdrawOrderQueryParam parm)
		{
			return _dal.GetWithdrawOrderTable(parm);
		}

		public DataTable GetWithdrawOrderUiTable(WithdrawOrderQueryParam parm)
		{
			return _dal.GetWithdrawOrderUiTable(parm);
		}

		public List<WithdrawOrderInfo> GetWithdrawOrderPageListUi(WithdrawOrderQueryParam parm, ref Paging paging)
		{
			return _dal.GetWithdrawOrderPageListUi(parm, ref paging);
		}

		public long WithdrawOrderAdd(WithdrawOrder Model)
		{
			Model.OrderId = Utils.CreateOrderNo("w", Model.UserId);
			return _dal.WithdrawOrderAdd(Model);
		}

		public WithdrawOrder GetWithdrawOrderModel(string OrderId)
		{
			return _dal.GetWithdrawOrderModel(OrderId);
		}

		public int UpdateInterfaceCode(string InterfaceCode, string OrderId)
		{
			return _dal.UpdateInterfaceCode(InterfaceCode, OrderId);
		}

		public Tuple<int, string> WithdrawApply(string schemeId, WithdrawOrder model)
		{
			WithdrawScheme withdrawScheme = WithdrawSchemeGetModel(schemeId);
			if (withdrawScheme == null)
			{
				return new Tuple<int, string>(1, "没有结算方案");
			}
			model.InterfaceCode = withdrawScheme.DefaulInfaceCode;
			if (model.WithdrawAmt < withdrawScheme.MinAmtSingle)
			{
				return new Tuple<int, string>(1, $"您的提现金额小于单笔最低提现金额：{withdrawScheme.MinAmtSingle / 100m:0.00}元，无法进行提现！");
			}
			if (model.WithdrawAmt > withdrawScheme.MaxAmtSingle)
			{
				return new Tuple<int, string>(1, $"您的提现金额大于单笔最高提现金额：{withdrawScheme.MaxAmtSingle / 100m:0.00}元，无法进行提现！");
			}
			List<WithdrawOrder> withdrawOrderListByDay = _dal.GetWithdrawOrderListByDay(model.UserId, model.AddTime.Value.Date, model.AddTime.Value.Date.AddDays(1.0).AddSeconds(-1.0));
			if (withdrawOrderListByDay != null)
			{
				if (withdrawScheme.MaxtDay <= withdrawOrderListByDay.Count)
				{
					return new Tuple<int, string>(1, $"您的提现次数已经达到今日最大提现次数：{withdrawScheme.MaxtDay}次，无法进行提现！");
				}
				if (withdrawScheme.LimitAmtDay < withdrawOrderListByDay.Sum((WithdrawOrder p) => p.WithdrawAmt) + model.WithdrawAmt)
				{
					return new Tuple<int, string>(1, $"您已经超过今日提现上限：{withdrawScheme.LimitAmtDay / 100m:0.00}元，无法进行提现！");
				}
			}
			decimal num = model.WithdrawAmt * withdrawScheme.HandingRateSingle;
			if (withdrawScheme.IsMinHandingSingle > 0 && num < withdrawScheme.MinHandingSingle)
			{
				num = withdrawScheme.MinHandingSingle;
			}
			if (withdrawScheme.IsMaxHandingSingle > 0 && num > withdrawScheme.MaxHandingSingle)
			{
				num = withdrawScheme.MaxHandingSingle;
			}
			model.Handing = num;
			model.Amt = model.WithdrawAmt + model.Handing;
			UserAmt userAmt = new UserBaseBll().GetUserAmt(model.UserId);
			if (userAmt.Balance - model.Amt < decimal.Zero)
			{
				return new Tuple<int, string>(2, "余额不足");
			}
			if (WithdrawOrderAdd(model) <= 0)
			{
				return new Tuple<int, string>(3, "系统繁忙,生成结算订单失败");
			}
			Trade trade = new Trade();
			trade.BillNo = model.OrderId;
			trade.TradeId = new UserBaseBll().GetTradeId();
			trade.UserId = model.UserId;
			trade.Type = 0;
			trade.BillType = ((model.OrderType == 1) ? 3 : ((model.OrderType == 2) ? 4 : 0));
			trade.TradeTime = DateTime.Now;
			trade.BeforeAmount = userAmt.Balance;
			trade.Amount = model.Amt;
			trade.Balance = userAmt.Balance - model.Amt;
			trade.Remark = "提现金额：" + model.WithdrawAmt / 100m + " 手续费：" + model.Handing / 100m;
			new UserBaseBll().AddTrade(trade);
			if (withdrawScheme.IsInterface == 1 && !string.IsNullOrEmpty(withdrawScheme.DefaulInfaceCode))
			{
				WithdrawOrderPayment(model, model.UserId);
			}
			return new Tuple<int, string>(0, "申请成功！");
		}

		public Tuple<int, string> WithdrawOrderPayment(WithdrawOrder model, string payUser, string TarnRemark = "")
		{
			if (_dal.GetWithdrawOrderTarnCount(model.OrderId, 0, 2, 3) > 0 && model.PayState != 2)
			{
				return new Tuple<int, string>(1, "有待交易记录！");
			}
			UserBaseInfo modelForId = new UserBaseBll().GetModelForId(model.UserId);
			if (modelForId.IdCardStatus != 1)
			{
				return new Tuple<int, string>(2, "商户未通过资质认证！");
			}
			if (modelForId.Freeze < model.Amt)
			{
				return new Tuple<int, string>(3, "数据错误：冻结金额小于提现金额!");
			}
			if (string.IsNullOrEmpty(TarnRemark))
			{
				TarnRemark = ((!string.IsNullOrEmpty(model.InterfaceCode)) ? "走接口付款！" : "人工付款！");
			}
			WithdrawOrderTarn withdrawOrderTarn = new WithdrawOrderTarn
			{
				TarnId = Utils.CreateOrderNo("t", model.UserId),
				OrderId = model.OrderId,
				WithdrawChannelCode = model.WithdrawChannelCode,
				WithdrawChanneName = model.WithdrawChanneName,
				InterfaceCode = model.InterfaceCode,
				FactName = model.FactName,
				BankCode = model.BankCode,
				BankAddress = model.BankAddress,
				BankLasalleCode = model.BankLasalleCode,
				ReservedPhone = model.ReservedPhone,
				AccountType = model.AccountType,
				Amount = model.WithdrawAmt,
				TarnRemark = TarnRemark,
				TarnState = 0,
				AddTime = DateTime.Now,
				AddUser = payUser,
				ModifyTime = DateTime.Now
			};
			if (!_dal.WithdrawOrderTarnAdd(withdrawOrderTarn))
			{
				return new Tuple<int, string>(4, "生成交易记录失败");
			}
			Tuple<int, string> result = new Tuple<int, string>(0, "付款成功！");
			if (string.IsNullOrEmpty(model.InterfaceCode))
			{
				withdrawOrderTarn.ChannelOrderNo = Utils.CreateOrderNo("R", model.UserId);
				withdrawOrderTarn.ModifyDesc = "系统付款成功";
				withdrawOrderTarn.TarnState = 1;
				CompleteWithdrawTarn(withdrawOrderTarn, model);
				return result;
			}
			InterfaceBusiness interfaceBusinessModel = new InterfaceBll().GetInterfaceBusinessModel(withdrawOrderTarn.InterfaceCode);
			if (interfaceBusinessModel == null)
			{
				return new Tuple<int, string>(5, "付款接口已损坏");
			}
			HMPayApiBase hMPayApiBase = HMPayFactory.CreatePayApi(ToHMInterface(interfaceBusinessModel.Code));
			hMPayApiBase.Supplier = new HMSupplier
			{
				Code = interfaceBusinessModel.Code,
				Name = interfaceBusinessModel.Name,
				PostUri = interfaceBusinessModel.SubMitUrl,
				AgentPayUrl = interfaceBusinessModel.AgentPayUrl,
				QueryUri = interfaceBusinessModel.QueryUrl
			};
			hMPayApiBase.Account = new HMAccount
			{
				AccountUser = interfaceBusinessModel.Account,
				ChildAccountUser = interfaceBusinessModel.ChildAccount,
				AccountCode = interfaceBusinessModel.MD5Pwd,
				Md5Pwd = interfaceBusinessModel.MD5Pwd,
				RsaPublic = interfaceBusinessModel.RSAOpen,
				RsaPrivate = interfaceBusinessModel.RSAPrivate,
				AppId = interfaceBusinessModel.Appid,
				OpenId = interfaceBusinessModel.OpenId,
				OpenPwd = interfaceBusinessModel.OpenPwd,
				SubDomain = interfaceBusinessModel.SubDomain,
				BindDomain = interfaceBusinessModel.BindDomain
			};
			HMWithdraw hMWithdraw = GetHMWithdraw(model, modelForId);
			HMPayResult hMPayResult = hMPayApiBase.WithdrawGateway(hMWithdraw);
			if (hMPayResult.Code == HMPayState.Success)
			{
				model.ChannelOrderNo = hMPayResult.Data;
				withdrawOrderTarn.ChannelOrderNo = hMPayResult.Data;
				withdrawOrderTarn.ModifyDesc = "走接口付款成功";
				withdrawOrderTarn.TarnState = 1;
				CompleteWithdrawTarn(withdrawOrderTarn, model);
				return new Tuple<int, string>(0, "付款成功！");
			}
			if (hMPayResult.Code == HMPayState.Fail)
			{
				_dal.UpdateOrderPayState(2, hMPayResult.Message, model.OrderId);
				return new Tuple<int, string>(6, "付款失败，待确认！ ");
			}
			if (hMPayResult.Code == HMPayState.Paymenting)
			{
				withdrawOrderTarn.ModifyDesc = "走接口付款ing";
				withdrawOrderTarn.TarnState = 3;
				model.ChannelOrderNo = hMPayResult.Data;
				withdrawOrderTarn.ChannelOrderNo = hMPayResult.Data;
				CompleteWithdrawTarn(withdrawOrderTarn, model);
				return new Tuple<int, string>(0, "已提交到银行，等待处理！");
			}
			return result;
		}

		public Tuple<int, string> WithdrawOrderTarnQuery(WithdrawOrder model, string payUser)
		{
			if (model.PayState != 4)
			{
				return new Tuple<int, string>(1, "订单不是处理中，不需要查询！");
			}
			WithdrawOrderTarn withdrawOrderTarn = new WithdrawOrderTarn
			{
				OrderId = model.OrderId,
				ModifyDesc = "后台查询",
				ModifyTime = DateTime.Now
			};
			InterfaceBusiness interfaceBusinessModel = new InterfaceBll().GetInterfaceBusinessModel(model.InterfaceCode);
			if (interfaceBusinessModel == null)
			{
				return new Tuple<int, string>(5, "付款接口已损坏");
			}
			HMPayApiBase hMPayApiBase = HMPayFactory.CreatePayApi(ToHMInterface(interfaceBusinessModel.Code));
			hMPayApiBase.Supplier = new HMSupplier
			{
				Code = interfaceBusinessModel.Code,
				Name = interfaceBusinessModel.Name,
				PostUri = interfaceBusinessModel.SubMitUrl,
				AgentPayUrl = interfaceBusinessModel.AgentPayUrl,
				QueryUri = interfaceBusinessModel.QueryUrl
			};
			hMPayApiBase.Account = new HMAccount
			{
				AccountUser = interfaceBusinessModel.Account,
				ChildAccountUser = interfaceBusinessModel.ChildAccount,
				AccountCode = interfaceBusinessModel.MD5Pwd,
				Md5Pwd = interfaceBusinessModel.MD5Pwd,
				RsaPublic = interfaceBusinessModel.RSAOpen,
				RsaPrivate = interfaceBusinessModel.RSAPrivate,
				AppId = interfaceBusinessModel.Appid,
				OpenId = interfaceBusinessModel.OpenId,
				OpenPwd = interfaceBusinessModel.OpenPwd,
				SubDomain = interfaceBusinessModel.SubDomain,
				BindDomain = interfaceBusinessModel.BindDomain
			};
			UserBaseInfo modelForId = new UserBaseBll().GetModelForId(model.UserId);
			HMWithdraw hMWithdraw = GetHMWithdraw(model, modelForId);
			Tuple<int, string> result = new Tuple<int, string>(0, "查询成功！");
			HMPayResult hMPayResult = hMPayApiBase.WithdrawQuery(hMWithdraw);
			if (hMPayResult.Code == HMPayState.Success)
			{
				withdrawOrderTarn.ModifyDesc = "走接口付款成功";
				model.ChannelOrderNo = hMPayResult.Data;
				withdrawOrderTarn.ChannelOrderNo = hMPayResult.Data;
				withdrawOrderTarn.TarnState = 1;
				CompleteWithdrawTarn(withdrawOrderTarn, model);
				return new Tuple<int, string>(0, "付款成功！");
			}
			if (hMPayResult.Code == HMPayState.Fail)
			{
				_dal.UpdateOrderPayState(2, hMPayResult.Message, model.OrderId);
				return new Tuple<int, string>(6, "付款失败，待确认！ ");
			}
			return result;
		}

		public HMInterface ToHMInterface(string str)
		{
			str = str.ToLower();
			switch (str.ToLower())
			{
			case "hmqrcode":
				return HMInterface.MyBase;
			case "swiftpass":
				return HMInterface.Swiftpass;
			case "pinganye":
				return HMInterface.PingAnYe;
			case "pinganlm":
				return HMInterface.PingAnLm;
			case "xftx":
				return HMInterface.XingFuTianXia;
			case "shande":
				return HMInterface.ShangDe;
			case "sdupay":
				return HMInterface.Sdupay;
			case "pay591":
				return HMInterface.Pay591V2;
			case "defengshande":
				return HMInterface.ShangDeDeFeng;
			case "leniuniu":
				return HMInterface.Leniuniu;
			case "careypay":
				return HMInterface.CareyPay;
			case "yunfubao":
				return HMInterface.YunFuBao;
			case "pinganyue":
				return HMInterface.PingAnYuE;
			default:
				if (str.StartsWith("_pinganyue"))
				{
					return HMInterface.PingAnYuE;
				}
				if (str.StartsWith("_hmqrcode"))
				{
					return HMInterface.MyBase;
				}
				if (str.StartsWith("_swiftpass"))
				{
					return HMInterface.Swiftpass;
				}
				if (str.StartsWith("_pinganye"))
				{
					return HMInterface.PingAnYe;
				}
				if (str.StartsWith("_pinganlm"))
				{
					return HMInterface.PingAnLm;
				}
				if (str.StartsWith("_xftx"))
				{
					return HMInterface.XingFuTianXia;
				}
				if (str.StartsWith("_shande"))
				{
					return HMInterface.ShangDe;
				}
				if (str.StartsWith("_sdupay"))
				{
					return HMInterface.Sdupay;
				}
				if (str.StartsWith("_pay591"))
				{
					return HMInterface.Pay591V2;
				}
				if (str.StartsWith("_defengshande"))
				{
					return HMInterface.ShangDeDeFeng;
				}
				if (str.StartsWith("_leniuniu"))
				{
					return HMInterface.Leniuniu;
				}
				if (str.StartsWith("_careypay"))
				{
					return HMInterface.CareyPay;
				}
                if (str.StartsWith("_daifu"))
                {
                    return HMInterface.DAIFU;
                }
                return HMInterface.Unknown;
			}
		}

		public HMWithdraw GetHMWithdraw(WithdrawOrder model, UserBaseInfo userInfo)
		{
			HMWithdraw hMWithdraw = new HMWithdraw();
			hMWithdraw.OrderNo = model.OrderId;
			hMWithdraw.ChannelOrderNo = model.ChannelOrderNo;
			hMWithdraw.Amount = model.WithdrawAmt;
			hMWithdraw.BankAccountType = model.AccountType;
			hMWithdraw.FactName = model.FactName;
			hMWithdraw.BankName = model.BankName;
			hMWithdraw.BankCode = model.BankCode;
			hMWithdraw.BankAddress = model.BankAddress;
			hMWithdraw.MobilePhone = model.ReservedPhone;
			hMWithdraw.BankLasalleCode = model.BankLasalleCode;
			if (userInfo.AccountType == 1)
			{
				UserDetail userDetail = new UserBaseBll().GetUserDetail(userInfo.UserId);
				hMWithdraw.IdCard = userDetail.LicenseId;
			}
			hMWithdraw.City = model.CityId.ToString();
			hMWithdraw.WithdrawTime = DateTime.Now;
			try
			{
				hMWithdraw.WithdrawChannel = (HMWithdrawChannel)Convert.ToInt32(model.WithdrawChannelCode);
				return hMWithdraw;
			}
			catch (Exception)
			{
				hMWithdraw.WithdrawChannel = HMWithdrawChannel.Unknown;
				return hMWithdraw;
			}
		}

		public bool PointFailTrue(WithdrawOrder model, string payUser, int type = 1)
		{
			WithdrawOrderTarn tarn = new WithdrawOrderTarn
			{
				OrderId = model.OrderId,
				TarnRemark = "系统确认失败",
				TarnState = 2,
				AddTime = DateTime.Now,
				AddUser = payUser,
				ModifyTime = DateTime.Now
			};
			bool num = CompleteWithdrawTarn(tarn, model);
			if (num)
			{
				UserAmt userAmt = new UserBaseBll().GetUserAmt(model.UserId);
				Trade model2 = new Trade
				{
					BillNo = model.OrderId,
					TradeId = new UserBaseBll().GetTradeId(),
					UserId = model.UserId,
					Type = 1,
					BillType = ((model.OrderType == 1) ? 3 : ((model.OrderType == 2) ? 4 : ((model.OrderType == 3) ? 5 : 0))),
					TradeTime = DateTime.Now,
					BeforeAmount = userAmt.Balance,
					Amount = model.Amt,
					Balance = userAmt.Balance + model.Amt,
					Remark = "结算确认失败"
				};
				new UserBaseBll().AddTrade(model2);
			}
			return num;
		}

		public bool PointFailReturn(WithdrawOrder model, string payUser, int type = 1)
		{
			WithdrawOrderTarn tarn = new WithdrawOrderTarn
			{
				OrderId = model.OrderId,
				TarnRemark = "系统退回",
				TarnState = 4,
				AddTime = DateTime.Now,
				AddUser = payUser,
				ModifyTime = DateTime.Now
			};
			bool num = CompleteWithdrawTarn(tarn, model);
			if (num)
			{
				UserAmt userAmt = new UserBaseBll().GetUserAmt(model.UserId);
				Trade model2 = new Trade
				{
					BillNo = model.OrderId,
					TradeId = new UserBaseBll().GetTradeId(),
					UserId = model.UserId,
					Type = 1,
					BillType = ((model.OrderType == 1) ? 3 : ((model.OrderType == 2) ? 4 : ((model.OrderType == 3) ? 5 : 0))),
					TradeTime = DateTime.Now,
					BeforeAmount = userAmt.Balance,
					Amount = model.Amt,
					Balance = userAmt.Balance + model.Amt,
					Remark = "结算退回"
				};
				new UserBaseBll().AddTrade(model2);
			}
			return num;
		}

		public bool CompleteWithdrawTarn(WithdrawOrderTarn tarn)
		{
			WithdrawOrder withdrawOrderModel = GetWithdrawOrderModel(tarn.OrderId);
			if (withdrawOrderModel == null)
			{
				return false;
			}
			return CompleteWithdrawTarn(tarn, withdrawOrderModel);
		}

		public bool CompleteWithdrawTarn(WithdrawOrderTarn tarn, WithdrawOrder order)
		{
			return _dal.CompleteWithdrawTarn(tarn, order);
		}

		public List<WithdrawOrderTarn> GetWithdrawOrderTarnPageList(WithdrawOrderTarn parm, ref Paging paging)
		{
			return _dal.GetWithdrawOrderTarnPageList(parm, ref paging);
		}

		public DataTable GetWithdrawOrderTarnTable(WithdrawOrderTarn parm)
		{
			return _dal.GetWithdrawOrderTarnTable(parm);
		}

		public long AddAccountScheme(AccountScheme Model)
		{
			return _dal.AddAccountScheme(Model);
		}

		public long GetMaxAccountSchemeId()
		{
			return _dal.GetMaxAccountSchemeId();
		}

		public List<AccountScheme> GetListAccountScheme()
		{
			return _dal.GetListAccountScheme();
		}

		public AccountScheme GetAccountScheme(string Id)
		{
			return _dal.GetAccountScheme(Id);
		}

		public long UpdateAccountScheme(AccountScheme Model)
		{
			return _dal.UpdateAccountScheme(Model);
		}

		public int DelAccountScheme(string Id)
		{
			return _dal.DelAccountScheme(Id);
		}

		public List<AccountScheme> GetAccountSchemeList(AccountScheme parm, ref Paging paging)
		{
			return _dal.GetAccountSchemeList(parm, ref paging);
		}

		public List<AccountSchemeDetail> GetAccountSchemeDetail(string AccountSchemeId)
		{
			return _dal.GetAccountSchemeDetail(AccountSchemeId);
		}

		public long GetMaxSchemeDetailEndtime(string AccountSchemeId)
		{
			return _dal.GetMaxSchemeDetailEndtime(AccountSchemeId);
		}

		public List<AccountSchemeDetailInfo> GetAccountSchemeDetailList(AccountSchemeDetail parm, ref Paging paging)
		{
			return _dal.GetAccountSchemeDetailList(parm, ref paging);
		}

		public AccountSchemeDetailInfo GetAccountSchemeDetailInfo(string Id)
		{
			return _dal.GetAccountSchemeDetailInfo(Id);
		}

		public long AddAccountSchemeDetail(AccountSchemeDetail Model)
		{
			return _dal.AddAccountSchemeDetail(Model);
		}

		public int UpdateAccountSchemeDetail(AccountSchemeDetail Model)
		{
			return _dal.UpdateAccountSchemeDetail(Model);
		}

		public int DelAccountSchemeDetail(string Id)
		{
			return _dal.DelAccountSchemeDetail(Id);
		}

		public long SetAccountSchemeDetail(string AccountSchemeId, List<AccountSchemeDetail> listModel)
		{
			return _dal.SetAccountSchemeDetail(AccountSchemeId, listModel);
		}

		public List<WithrawUserBaseInfo> LoadWithdrawUser(UserBaseInfo param, ref Paging paging)
		{
			return _dal.LoadWithdrawUser(param, ref paging);
		}

		public List<OrderAccount> GetOrderAccountList(OrderAccountQueryParam param, ref Paging paging)
		{
			return _dal.GetOrderAccountList(param, ref paging);
		}

		public bool ConfirmOrderAccount(OrderAccount model)
		{
			return _dal.ConfirmOrderAccount(model);
		}

		public List<OrderAccountTarn> GetOrderAccountTarnList(OrderAccountTarn param, ref Paging paging)
		{
			return _dal.GetOrderAccountTarnList(param, ref paging);
		}

		public bool ConfirmOrderAccountTarnBat(List<OrderAccountTarn> list, OrderAccount model)
		{
			model.LockId = EncryUtils.MD5(model.AdminId + model.AccountTime.ToString("yyyyMMddHHmmss"));
			return _dal.ConfirmOrderAccountTarnBat(list, model);
		}

		public WithdrawOrder GetTxOrder()
		{
			return _dal.GetTxOrder();
		}
	}
}
