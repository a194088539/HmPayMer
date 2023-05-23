using HM.Framework;
using HmPMer.Dal;
using HmPMer.Entity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace HmPMer.Business
{
	public class InterfaceBll
	{
		private readonly InterfaceDal _dal = new InterfaceDal();

		private static ConcurrentDictionary<string, int> InterfaceBusinessRandomDic = new ConcurrentDictionary<string, int>();

		private static ConcurrentDictionary<string, int> accountRandomDic = new ConcurrentDictionary<string, int>();

		private static ConcurrentDictionary<string, int> interfaceRandomDic = new ConcurrentDictionary<string, int>();

        private static Random _random = new Random();
        private static List<string> _scheduleCodes = new List<string>();
        private static Dictionary<string, InterfaceBusiness> _interfaceBusiness = new Dictionary<string, InterfaceBusiness>();
        private static DateTime _lastRefreshTime = DateTime.Now;

        public List<InterfaceBusiness> LoadInterfaceBusiness(InterfaceBusiness param, ref Paging paging)
		{
			return _dal.LoadInterfaceBusiness(param, ref paging);
		}

		public List<InterfaceBusiness> GetInterfaceBusinessList(int isEnabled)
		{
			return _dal.GetInterfaceBusinessList(isEnabled);
		}

		public List<InterfaceBusiness> GetIBForAgentPay(int AgentPay)
		{
			return _dal.GetIBForAgentPay(AgentPay);
		}
        
        public InterfaceBusiness GetInterfaceBusinessRandomModel(string Code)
        {
            if (Code.StartsWith("_random"))
            {//开启轮询
                InterfaceBusiness interfaceBusiness = _dal.GetInterfaceBusinessModel(Code);
                if(interfaceBusiness == null || interfaceBusiness.IsEnabled != 1)
                {
                    return interfaceBusiness;
                }
                string c = Code.Substring("_random".Length);
                lock(_interfaceBusiness)
                {
                    if (_interfaceBusiness.Count == 0 || DateTime.Now.Subtract(_lastRefreshTime).TotalMinutes > 6)
                    {
                        List<InterfaceBusiness> interfaces = this.GetInterfaceBusinessList(-1);
                        string[] tcods = new string[_interfaceBusiness.Count];
                        _interfaceBusiness.Keys.CopyTo(tcods, 0);
                        foreach (InterfaceBusiness i in interfaces)
                        {
                            _interfaceBusiness[i.Code] = i;
                            for(int j = 0; j < tcods.Length; j++)
                            {
                                if(tcods[j] == i.Code)
                                {
                                    tcods[j] = null;
                                }
                            }
                        }
                        for (int j = 0; j < tcods.Length; j++)
                        {
                            if (tcods[j] != null)
                            {
                                _interfaceBusiness[tcods[j]].IsEnabled = 0;
                            }
                        }
                        foreach (var i in _interfaceBusiness)
                        {
                            if (i.Key.StartsWith(c) && i.Value.IsEnabled == 1 && !_scheduleCodes.Contains(i.Key))
                            {
                                _scheduleCodes.Add(i.Key);
                            }
                        }

                        _lastRefreshTime = DateTime.Now;
                    }


                    string last = "";
                    
                    InterfaceBusiness tb = null;
                    while (_scheduleCodes.Count > 0)
                    {
                        last = _scheduleCodes[0];
                        _scheduleCodes.RemoveAt(0);
                        if (_interfaceBusiness.TryGetValue(last, out tb))
                        {
                            if (tb.IsEnabled == 1)
                            {
                                _scheduleCodes.Add(last);
                                return tb;
                            }
                        }
                    }
                }
                
                return null;
            }
            return _dal.GetInterfaceBusinessModel(Code);
        }
        public InterfaceBusiness GetInterfaceBusinessModel(string Code)
		{
			return _dal.GetInterfaceBusinessModel(Code);
		}

		public InterfaceAccount GetInterfaceAccountFroAccount(string Account)
		{
			return _dal.GetInterfaceAccountFroAccount(Account);
		}

		public long AddInterfaceBusiness(InterfaceBusiness Model)
		{
			return _dal.AddInterfaceBusiness(Model);
		}

		public int UpInterfaceBusinessEnabled(string Code, int IsEnabled)
		{
			return _dal.UpInterfaceBusinessEnabled(Code, IsEnabled);
		}

		public int UpInterfaceBusinessAgentPay(string Code, int AgentPay)
		{
			return _dal.UpInterfaceBusinessAgentPay(Code, AgentPay);
		}

		public bool UpdateInterfaceBusiness(InterfaceBusiness Model)
		{
			return _dal.UpdateInterfaceBusiness(Model);
		}

		public int DelInterface(string Code)
		{
			return _dal.DelInterface(Code);
		}

		public List<PayType> GetInterfaceType(string Code, int Type)
		{
			return _dal.GetInterfaceType(Code, Type);
		}

		public List<PayType> GetInterfaceChannelType(string Code, int Type)
		{
			return _dal.GetInterfaceChannelType(Code, Type);
		}

        public List<InterfaceType> GetInterfaceTypeOnly(string Code, int Type)
        {
            return _dal.GetInterfaceTypeOnly(Code, Type);
        }

        public long SetInterfaceType(List<InterfaceType> listModel, string InterfaceCode)
		{
			return _dal.SetInterfaceType(listModel, InterfaceCode);
		}

		public List<InterfaceAccount> LoadInterfaceAccount(InterfaceAccount param, ref Paging paging)
		{
			return _dal.LoadInterfaceAccount(param, ref paging);
		}

		public List<InterfaceAccount> LoadInterfaceAccount(string code, int isEnabled = -1)
		{
			return _dal.LoadInterfaceAccount(code, isEnabled);
		}

		public InterfaceAccount GetInterfaceAccountModel(string Id)
		{
			return _dal.GetInterfaceAccountModel(Id);
		}

		public long AddInterfaceAccount(InterfaceAccount Model)
		{
			return _dal.AddInterfaceAccount(Model);
		}

		public int UpInterfaceAccountEnabled(string Id, int IsEnabled)
		{
			return _dal.UpInterfaceAccountEnabled(Id, IsEnabled);
		}

		public bool UpdateInterfaceAccount(InterfaceAccount Model)
		{
			return _dal.UpdateInterfaceAccount(Model);
		}

		public bool DelInterfaceAccountBat(List<string> idlist)
		{
			return _dal.DelInterfaceAccountBat(idlist);
		}

		public long AddInterfaceAccountBat(List<InterfaceAccount> list)
		{
			return _dal.AddInterfaceAccountBat(list);
		}

		public InterfaceBusiness RandomInterface(bool isadd)
		{
			List<InterfaceBusiness> interfaceBusinessList = _dal.GetInterfaceBusinessList(1);
			if (interfaceBusinessList == null || interfaceBusinessList.Count == 0)
			{
				return null;
			}
			if (interfaceBusinessList.Count == 1)
			{
				return interfaceBusinessList[0];
			}
			string key = string.Format("{0}.{1}", "interfacelist", EncryUtils.MD5("interfacelist" + interfaceBusinessList.Count));
			int value = 0;
			if (InterfaceBusinessRandomDic.ContainsKey(key) && InterfaceBusinessRandomDic.TryGetValue(key, out value))
			{
				value += (isadd ? 1 : 0);
			}
			if (value < 0 || value >= interfaceBusinessList.Count)
			{
				value = 0;
			}
			InterfaceBusinessRandomDic[key] = value;
			return interfaceBusinessList[value];
		}

		public InterfaceAccount RandomInterfaceAccount(string interfaceCode, List<InterfaceAccount> list, bool isadd)
		{
			string key = $"{interfaceCode}.{EncryUtils.MD5(interfaceCode + list.Count)}";
			int value = 0;
			if (interfaceRandomDic.ContainsKey(key) && interfaceRandomDic.TryGetValue(key, out value))
			{
				value += (isadd ? 1 : 0);
			}
			if (value < 0 || value >= list.Count)
			{
				value = 0;
			}
			interfaceRandomDic[key] = value;
			return list[value];
		}
	}
}
