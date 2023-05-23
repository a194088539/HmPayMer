namespace HM.Framework.DapperExtensions
{
	public interface ISort
	{
		string PropertyName
		{
			get;
			set;
		}

		bool Ascending
		{
			get;
			set;
		}
	}
}
