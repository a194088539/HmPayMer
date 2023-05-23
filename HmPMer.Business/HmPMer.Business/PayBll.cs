using HM.Framework;
using HM.Framework.Caching;
using HM.Framework.Logging;
using HmPMer.Business.Pay;
using HmPMer.Dal;
using HmPMer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HmPMer.Business
{
	public class PayBll
	{
		private readonly PayDal _dal = new PayDal();

		public List<PayType> LoadRechargePayType(PayType param, ref Paging paging)
		{
			return _dal.LoadRechargePayType(param, ref paging);
		}

		public PayType GetPayTypeModel(string PayCode)
		{
			return _dal.GetPayTypeModel(PayCode);
		}

		public long AddPayType(PayType Model)
		{
			return _dal.AddPayType(Model);
		}

		public int UpdatePayType(PayType Model)
		{
			return _dal.UpdatePayType(Model);
		}

		public int UpPayTypeEnabled(string PayCode, int IsEnabled)
		{
			int num = _dal.UpPayTypeEnabled(PayCode, IsEnabled);
			if (num > 0)
			{
				ICache caching = CachingFactory.GetCaching();
				string key = CachingKey.CreatePayChannelSettingKey();
				caching.Remove(key);
			}
			return num;
		}

		public int DelPayType(string PayCode)
		{
			return _dal.DelPayType(PayCode);
		}

		public List<PayType> GetPayTypeList()
		{
			return _dal.GetPayTypeList();
		}

		public List<PayType> GetInterfaceType(string Code, int Type)
		{
			return _dal.GetInterfaceType(Code, Type);
		}

		public long SetInterfaceType(List<InterfaceType> listModel, string InterfaceCode)
		{
			return _dal.SetInterfaceType(listModel, InterfaceCode);
		}

		public List<PayChannelInfo> LoadRechargePayChannel(PayChannelInfo param, ref Paging paging)
		{
			return _dal.LoadRechargePayChannel(param, ref paging);
		}

		public PayChannel GetPayChannelModel(string Code)
		{
			return _dal.GetPayChannelModel(Code);
		}

		public long AddPayChannel(PayChannel Model)
		{
			return _dal.AddPayChannel(Model);
		}

		public int UpdatePayChannel(PayChannel Model)
		{
			return _dal.UpdatePayChannel(Model);
		}

		public int UpPayChannelEnabled(string Code, int IsEnabled)
		{
			int num = _dal.UpPayChannelEnabled(Code, IsEnabled);
			if (num > 0)
			{
				ICache caching = CachingFactory.GetCaching();
				string key = CachingKey.CreatePayChannelSettingKey();
				caching.Remove(key);
			}
			return num;
		}

		public int DelChannel(string Code)
		{
			return _dal.DelChannel(Code);
		}

		public List<PayChannel> GetPayChannelList()
		{
			return _dal.GetPayChannelList();
		}

		public List<PayChannel> GetUserPayChannelList(string UserId)
		{
			return _dal.GetUserPayChannelList(UserId);
		}

		public List<PayChannelSetting> LoadChannelSettingEnable()
		{
			return _dal.LoadChannelSettingEnable();
		}

		public string GetPayType(string channel)
		{
			string result = "";
			ICache caching = CachingFactory.GetCaching();
			string hashId = CachingKey.CreatePayChannelSettingKey();
			List<PayChannelSetting> list = LoadChannelSettingEnable();
			Dictionary<string, string> dic = new Dictionary<string, string>();
			list?.ForEach(delegate(PayChannelSetting item)
			{
				dic.Add(item.ChannelCode, item.PayCode);
			});
			caching.Add(hashId, (IDictionary<string, string>)dic);
			if (dic.ContainsKey(channel))
			{
				result = dic[channel];
			}
			return result;
		}

		public InterfaceType GetPayTypeUser(string userId, string payCode)
		{
			return _dal.GetPayTypeUser(userId, payCode);
		}

		public InterfaceAccount GetInterfaceAccount(string payCode, string channelCode, InterfaceBusiness interfaceBusiness, decimal amt, DateTime time, DateTime expiredTime)
		{
			InterfaceAccount interfaceAccount = null;
			if (interfaceBusiness.AccType == 1)
			{
				List<InterfaceAccount> list = new InterfaceBll().LoadInterfaceAccount(interfaceBusiness.Code, 1);
				if (list.Count > 0)
				{
					interfaceAccount = new InterfaceBll().RandomInterfaceAccount(interfaceBusiness.Code, list, isadd: true);
				}
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
					SubDomain = interfaceBusiness.SubDomain,
					BindDomain = interfaceBusiness.BindDomain
				};
			}
			if (interfaceAccount != null && interfaceAccount.OrderAmt <= decimal.Zero)
			{
                //随机增加
                if (interfaceBusiness.Code.StartsWith("_bankpayv1"))
                {
                    Random random = new Random();
                    if(amt < 1000)
                    {
                        amt -= random.Next(5, 10);
                    }
                    else
                    {
                        amt -= random.Next(5, 20);
                    }
                }
                interfaceAccount.OrderAmt = amt;
			}
			return interfaceAccount;
		}

		public bool InsertOrderInfo(OrderBase model)
		{
			bool num = _dal.InsertOrderInfo(model);
			if (num)
			{
				ICache caching = CachingFactory.GetCaching();
				caching.Add(CachingKey.CreateMerOrderPending(model.MerOrderNo), value: true, model.ExpiredTime.Value.AddSeconds(15.0));
				caching.Add(CachingKey.CreateMerOrderKey(model.MerOrderNo), model, model.ExpiredTime.Value.AddSeconds(30.0));
			}
			return num;
		}

		public bool InsertOrderPayCode(OrderPayCode model)
		{
			return _dal.InsertOrderPayCode(model);
		}

		public OrderPayCode GetOrderPayCodeByMerOrderNo(string merOrderNo, string userId)
		{
			return _dal.GetOrderPayCodeByMerOrderNo(merOrderNo, userId);
		}

		public int CompleteOrder(OrderBase order)
		{
			if (order.PayState == PayState.Success.ToInt())
			{
				return 1;
			}
			order.PayState = PayState.Success.ToInt();
			RateBll rateBll = new RateBll();
			order.PayTime = DateTime.Now;
			order.PayFlow = order.MerOrderAmt * order.PayRate;
			order.MerAmt = order.MerOrderAmt - order.PayFlow;
			PayRate payRate = rateBll.GetPayRate(RateType.InterfaceBusiness.ToInt(), order.PayCode, order.InterfaceCode);
			order.CostRate = (payRate?.Rate ?? decimal.Zero);
			order.CostAmt = order.MerOrderAmt * order.CostRate;
			if (!string.IsNullOrEmpty(order.AgentId))
			{
				UserBaseInfo modelForId = new UserBaseBll().GetModelForId(order.AgentId);
				if (modelForId != null)
				{
					PayRateInfo payRate2 = rateBll.GetPayRate(RateType.Agent.ToInt(), order.PayCode, order.AgentId, modelForId.GradeId);
					if (payRate2 != null)
					{
						order.AgentRate = payRate2.Rate;
						order.AgentAmt = order.PayFlow - order.MerOrderAmt * payRate2.Rate;
					}
				}
			}
			if (!string.IsNullOrEmpty(order.PromId))
			{
				HmAdmin hmAdmin = new AccountBll().GetHmAdmin(order.PromId);
				if (hmAdmin != null)
				{
					order.PromRate = hmAdmin.Rate;
					order.PromAmt = (order.PayFlow - order.CostAmt - order.AgentAmt) * order.PromRate;
				}
			}
			order.Profits = order.PayFlow - order.CostAmt - order.AgentAmt - order.PromAmt;
			InterfaceType payTypeUser = GetPayTypeUser(order.UserId, order.PayCode);
			List<OrderAccount> orderAccountList = new List<OrderAccount>();
			if (payTypeUser != null && !string.IsNullOrEmpty(payTypeUser.AccountScheme))
			{
				orderAccountList = CompleteAccountList(payTypeUser.AccountScheme, order);
			}
			if (_dal.CompleteOrder(order, orderAccountList))
			{
				ApiNotity.NotifyOrder(order);
				new Task(delegate(object _obj)
				{
					OrderBase orderBase = (OrderBase)_obj;
					RiskScheme riskScheme = null;
					DateTime now = DateTime.Now;
					RiskBll riskBll = new RiskBll();
					new RiskTarnRecord().TarnCount = 1;
					RiskTarnRecord riskTarnRecord = new RiskTarnRecord
					{
						TarnCount = 1
					};
					string empty = string.Empty;
					riskTarnRecord.AccountType = RiskType.InterfaceAccount.ToInt();
					riskTarnRecord.AccountId = orderBase.AccountId;
					riskScheme = riskBll.GetRiskSchemeModel(RiskType.InterfaceBusiness.ToInt(), orderBase.UserId);
					riskTarnRecord.TarnDate = now.Date;
					riskTarnRecord.TarnAmt = order.OrderAmt;
					riskBll.AddRiskTarnRecord(riskTarnRecord);
					LogUtil.InfoFormat("riskScheme={0}", riskScheme == null);
					if (riskScheme != null && riskScheme.IsDayAmt > 0)
					{
						LogUtil.InfoFormat("riskScheme:{0}", riskScheme.ToJson());
						LogUtil.InfoFormat("riskTarnRecord_Account:{0}", riskTarnRecord.ToJson());
						riskTarnRecord = riskBll.GetRiskTarn(riskTarnRecord);
						if (riskTarnRecord.TarnAmt >= riskScheme.DayAmt)
						{
							RiskLimit model = new RiskLimit
							{
								ID = Guid.NewGuid().ToString(),
								RiskType = riskTarnRecord.AccountType,
								State = 1,
								TargetId = riskTarnRecord.AccountId,
								BeginTime = DateTime.Now,
								EndTime = DateTime.Now.Date.AddDays(1.0).AddMinutes(1.0)
							};
							riskBll.AddRiskLimt(model);
						}
					}
				}, order).Start();
				return 1;
			}
			return 0;
		}

		public List<OrderAccount> CompleteAccountList(string accountScheme, OrderBase order)
		{
			List<OrderAccount> list = new List<OrderAccount>();
			List<AccountSchemeDetail> accountSchemeDetail = new WithdrawBll().GetAccountSchemeDetail(accountScheme);
			if (accountSchemeDetail == null || accountSchemeDetail.Count == 0)
			{
				return list;
			}
			decimal merAmt = order.MerAmt;
			DateTime value = order.PayTime.Value;
			int timeInt = value.Hour * 100 + value.Minute;
			AccountSchemeDetail accountSchemeDetail2 = (from p in accountSchemeDetail
			orderby p.StarTime
			select p).FirstOrDefault(delegate(AccountSchemeDetail p)
			{
				if (p.StarTime <= timeInt)
				{
					return timeInt <= p.EndTime;
				}
				return false;
			});
			if (accountSchemeDetail2 == null)
			{
				return list;
			}
			SystemBll systemBll = new SystemBll();
			decimal d = Convert.ToDecimal(accountSchemeDetail2.AmtSingle1) / 100m;
			OrderAccount orderAccount = new OrderAccount
			{
				OId = order.OrderId + "1",
				OrderId = order.OrderId,
				UserId = order.UserId,
				OrderAmt = order.OrderAmt,
				AccountrRnge = accountSchemeDetail2.StarTime + "-" + accountSchemeDetail2.EndTime,
				SchemeType = accountSchemeDetail2.SchemeType1,
				TDay = accountSchemeDetail2.TDay1,
				AmtSingle = accountSchemeDetail2.AmtSingle1,
				Amt = merAmt * d,
				AddTime = value,
				AccountTime = systemBll.GetTDDate(value, accountSchemeDetail2.TDay1, accountSchemeDetail2.SchemeType1)
			};
			if (orderAccount.AccountTime.Date == value.Date)
			{
				orderAccount.AccountState = 1;
				orderAccount.EndTime = value;
			}
			list.Add(orderAccount);
			if (merAmt - orderAccount.Amt > decimal.Zero)
			{
				OrderAccount orderAccount2 = new OrderAccount
				{
					OId = order.OrderId + "2",
					OrderId = order.OrderId,
					UserId = order.UserId,
					OrderAmt = order.OrderAmt,
					AccountrRnge = accountSchemeDetail2.StarTime + "-" + accountSchemeDetail2.EndTime,
					SchemeType = accountSchemeDetail2.SchemeType2,
					TDay = accountSchemeDetail2.TDay2,
					AmtSingle = accountSchemeDetail2.AmtSingle2,
					Amt = merAmt - orderAccount.Amt,
					AddTime = value,
					AccountTime = systemBll.GetTDDate(value, accountSchemeDetail2.TDay2, accountSchemeDetail2.SchemeType2)
				};
				if (orderAccount2.AccountTime.Date == value.Date)
				{
					orderAccount2.AccountState = 1;
					orderAccount2.EndTime = value;
				}
				list.Add(orderAccount2);
			}
			return list;
		}

		public List<OrderSettlement> GetOrderSettlementStateList()
		{
			return _dal.GetOrderSettlementStateList();
		}

		public int SettlementUserAmt(OrderSettlement Model)
		{
			return _dal.SettlementUserAmt(Model);
		}
	}
}
