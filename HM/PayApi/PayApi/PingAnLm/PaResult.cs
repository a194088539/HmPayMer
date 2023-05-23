using System.Collections.Generic;

namespace HM.Framework.PayApi.PingAnLm
{
	public class PaResult
	{
		public string code_url
		{
			get;
			set;
		}

		public string qr_code
		{
			get;
			set;
		}

		public string pay_url
		{
			get;
			set;
		}

		public Dictionary<string, string> pay_info
		{
			get;
			set;
		}

		public string out_trade_no
		{
			get;
			set;
		}

		public string trade_no
		{
			get;
			set;
		}

		public string trade_status
		{
			get;
			set;
		}

		public decimal total_amount
		{
			get;
			set;
		}

		public string remark
		{
			get;
			set;
		}
	}
}
