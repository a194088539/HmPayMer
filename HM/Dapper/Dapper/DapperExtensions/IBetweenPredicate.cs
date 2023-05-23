namespace HM.Framework.DapperExtensions
{
	public interface IBetweenPredicate : IPredicate
	{
		string PropertyName
		{
			get;
			set;
		}

		BetweenValues Value
		{
			get;
			set;
		}

		bool Not
		{
			get;
			set;
		}
	}
}
