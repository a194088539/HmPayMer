using System;

namespace HmPMer.AdminUI.Models
{
	[Serializable]
	public class ResultBase
	{
		public bool Success
		{
			get;
			set;
		}

		public string Code
		{
			get;
			set;
		}

		public string Message
		{
			get;
			set;
		}

		public object Data
		{
			get;
			set;
		}

		public ResultBase()
		{
			Success = true;
			Code = "00";
			Message = "操作成功";
		}
	}
}
