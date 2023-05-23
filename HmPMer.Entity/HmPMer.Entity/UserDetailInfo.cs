using System;

namespace HmPMer.Entity
{
	public class UserDetailInfo : UserDetail
	{
		public int UserType
		{
			get;
			set;
		}

		public int WithdrawStatus
		{
			get;
			set;
		}

		public DateTime? WithdrawTime
		{
			get;
			set;
		}

		public string WithdrawAuditDes
		{
			get;
			set;
		}

		public int IdCardStatus
		{
			get;
			set;
		}

		public DateTime? IdCardTime
		{
			get;
			set;
		}

		public string IdCardAuditDes
		{
			get;
			set;
		}
	}
}
