namespace HM.Framework.DapperExtensions
{
	public abstract class ComparePredicate : BasePredicate
	{
		public Operator Operator
		{
			get;
			set;
		}

		public bool Not
		{
			get;
			set;
		}

		public virtual string GetOperatorString()
		{
			switch (Operator)
			{
			case Operator.Gt:
				if (!Not)
				{
					return ">";
				}
				return "<=";
			case Operator.Ge:
				if (!Not)
				{
					return ">=";
				}
				return "<";
			case Operator.Lt:
				if (!Not)
				{
					return "<";
				}
				return ">=";
			case Operator.Le:
				if (!Not)
				{
					return "<=";
				}
				return ">";
			case Operator.Like:
				if (!Not)
				{
					return "LIKE";
				}
				return "NOT LIKE";
			default:
				if (!Not)
				{
					return "=";
				}
				return "<>";
			}
		}
	}
}
