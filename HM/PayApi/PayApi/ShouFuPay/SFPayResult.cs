namespace HM.Framework.PayApi.ShouFuPay
{
	public class SFPayResult
	{
		public string respCode
		{
			get;
			set;
		}

		public string respMsg
		{
			get;
			set;
		}

		public string payInfo
		{
			get;
			set;
		}

		public string queryId
		{
			get;
			set;
		}

		public static SFPayResult FormStr(string str)
		{
			string[] array = str.Split('&');
			SFPayResult sFPayResult = new SFPayResult();
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split('=');
				string text = array2[0].Trim();
				string text2 = array2[1].Trim();
				for (int j = 2; j < array2.Length; j++)
				{
					text2 = text2 + "=" + array2[j].Trim();
				}
				if (text.Equals("respCode"))
				{
					sFPayResult.respCode = text2;
				}
				else if (text.Equals("respMsg"))
				{
					sFPayResult.respMsg = text2;
				}
				else if (text.Equals("payInfo"))
				{
					sFPayResult.payInfo = text2;
				}
				else if (text.Equals("queryId"))
				{
					sFPayResult.queryId = text2;
				}
			}
			return sFPayResult;
		}
	}
}
