namespace HM.Framework.DapperExtensions
{
	public interface IFieldPredicate : IComparePredicate, IBasePredicate, IPredicate
	{
		object Value
		{
			get;
			set;
		}
	}
}
