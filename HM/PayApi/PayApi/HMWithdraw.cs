using System;

namespace HM.Framework.PayApi
{
	public class HMWithdraw
	{
		public HMWithdrawChannel WithdrawChannel
		{
			get;
			set;
		}

		public string OrderNo
		{
			get;
			set;
		}

		public string ChannelOrderNo
		{
			get;
			set;
		}

		public decimal Amount
		{
			get;
			set;
		}

		public int BankAccountType
		{
			get;
			set;
		}

		public string FactName
		{
			get;
			set;
		}

		public string BankName
		{
			get;
			set;
		}

		public string BankCode
		{
			get;
			set;
		}

		public string ProvinceName
		{
			get;
			set;
		}

		public string CityName
		{
			get;
			set;
		}

		public string BankAddress
		{
			get;
			set;
		}

		public string MobilePhone
		{
			get;
			set;
		}

		public string BankLasalleCode
		{
			get;
			set;
		}

		public string IdCard
		{
			get;
			set;
		}

		public string City
		{
			get;
			set;
		}

		public DateTime WithdrawTime
		{
			get;
			set;
		}
	}
}
