namespace HM.Framework.DapperExtensions
{
	public interface IBasePredicate : IPredicate
	{
		string PropertyName
		{
			get;
			set;
		}
	}
}
