namespace HmPMer.Entity
{
	public class AccountSchemeDetailInfo : AccountSchemeDetail
	{
		public string StartimeStr
		{
			get
			{
				string result = "00:00";
				if (base.StarTime != 0)
				{
					result = ((double)base.StarTime / 100.0).ToString("0.00").Replace(".", ":");
				}
				return result;
			}
		}

		public string EndTimeStr
		{
			get
			{
				string result = "00:00";
				if (base.EndTime != 0)
				{
					result = ((double)base.EndTime / 100.0).ToString("0.00").Replace(".", ":");
				}
				return result;
			}
		}
	}
}
