namespace HM.Framework.DapperExtensions
{
	public interface IComparePredicate : IBasePredicate, IPredicate
	{
		Operator Operator
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
