namespace HM.Framework.DapperExtensions
{
	public interface IExistsPredicate : IPredicate
	{
		IPredicate Predicate
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
