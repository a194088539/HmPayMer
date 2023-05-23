namespace HmPMer.Entity
{
	public class WithrawUserBaseInfo : UserBase
	{
		public decimal OrderAmt
		{
			get;
			set;
		}

		public decimal Freeze
		{
			get;
			set;
		}

		public decimal Balance
		{
			get;
			set;
		}

		public decimal UnBalance
		{
			get;
			set;
		}

		public int WithdrawAccountType
		{
			get;
			set;
		}

		public string WithdrawBank
		{
			get;
			set;
		}

		public string WithdrawFactName
		{
			get;
			set;
		}

		public string WithdrawBankCode
		{
			get;
			set;
		}

		public string WithdrawBankBranch
		{
			get;
			set;
		}
	}
}
