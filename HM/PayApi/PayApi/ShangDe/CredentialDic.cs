using System.Collections.Generic;

namespace HM.Framework.PayApi.ShangDe
{
	public class CredentialDic
	{
		public string submitUrl
		{
			get;
			set;
		}

		public string payMode
		{
			get;
			set;
		}

		public Dictionary<string, string> paramDic
		{
			get;
			set;
		}
	}
}
