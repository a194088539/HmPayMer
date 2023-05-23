namespace HM.Framework.DapperExtensions
{
	public interface IPropertyPredicate : IComparePredicate, IBasePredicate, IPredicate
	{
		string PropertyName2
		{
			get;
			set;
		}
	}
}
