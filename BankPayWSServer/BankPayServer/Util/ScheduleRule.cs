using System;
using System.Collections.Generic;

namespace BankPayWSServer.BankPayServer
{

    public class ScheduleRule
    {
        
        public const string dispatch_range = "dispatch_range";
        public const string limit_alipay = "limit_alipay";
        public const string limit_weixin = "limit_weixin";
        public string Name { get; set; }
        public decimal Start { get; set; }
        public decimal End { get; set; }
        public string[] InterfaceCodes { get; set; }
        public bool AllInterface { get; set; }

        public static ScheduleRule[] ParseRules(string content)
        {
            string[] rules = content.Split('\n');
            List<ScheduleRule> rulesRs = new List<ScheduleRule>();
            foreach (string s in rules)
            {
                string row = s.Trim();
                if(row.StartsWith("#"))
                {
                    continue;
                }
                string[] items = row.Split('=');
                if(items.Length < 2)
                {
                    continue;
                }
                for(int m = 0; m < items.Length; m++)
                {
                    items[m] = items[m].Trim();
                }
                string name = items[0];
                ScheduleRule scheduleRule = null;
                if (name == dispatch_range && items.Length == 2)
                {
                    string[] sp = items[1].Split('|');
                    if (sp.Length != 2)
                    {
                        continue;
                    }
                    string range = sp[0].Trim();
                    decimal range1 = 0;
                    decimal range2 = Decimal.MaxValue;

                    int i = range.IndexOf('-');
                    if (i == 0)
                    {
                        range1 = 0;
                    }
                    else
                    {
                        if (!Decimal.TryParse(range.Substring(0, i).Trim(), out range1))
                        {
                            range1 = 0;
                        }
                    }
                    if (i == range.Length - 1)
                    {
                        range2 = Decimal.MaxValue;
                    }
                    else
                    {
                        if (!Decimal.TryParse(range.Substring(i + 1).Trim(), out range2))
                        {
                            range2 = Decimal.MaxValue;
                        }
                    }
                    scheduleRule = new ScheduleRule() {
                        Name = name,
                        Start = range1 * 100,
                        End = range2 * 100,
                        InterfaceCodes = sp[1].Trim().Split(','),
                        AllInterface = sp[1].Trim() == "*"
                    };
                    for(int m = 0; m < scheduleRule.InterfaceCodes.Length; m++)
                    {
                        scheduleRule.InterfaceCodes[m] = scheduleRule.InterfaceCodes[m].Trim();
                    }
                } else if((name == limit_alipay || name == limit_weixin) && items.Length == 2)
                {
                    string[] sp = items[1].Split('|');
                    if (sp.Length != 2)
                    {
                        continue;
                    }
                    string range = sp[0].Trim();
                    decimal range1 = 0;
                    if (!Decimal.TryParse(range, out range1))
                    {
                        continue;
                    }
                    scheduleRule = new ScheduleRule()
                    {
                        Name = name,
                        Start = range1 * 100,
                        InterfaceCodes = sp[1].Trim().Split(','),
                        AllInterface = sp[1].Trim() == "*"
                    };
                    for (int m = 0; m < scheduleRule.InterfaceCodes.Length; m++)
                    {
                        scheduleRule.InterfaceCodes[m] = scheduleRule.InterfaceCodes[m].Trim();
                    }
                }
                if(scheduleRule != null)
                {
                    rulesRs.Add(scheduleRule);
                }
            }
            return rulesRs.ToArray();
        }
    }
}
