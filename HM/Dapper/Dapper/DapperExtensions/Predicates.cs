using System;
using System.Linq.Expressions;
using System.Reflection;

namespace HM.Framework.DapperExtensions
{
	public static class Predicates
	{
		public static IFieldPredicate Field<T>(Expression<Func<T, object>> expression, Operator op, object value, bool not = false) where T : class
		{
			PropertyInfo propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
			return new FieldPredicate<T>
			{
				PropertyName = propertyInfo.Name,
				Operator = op,
				Value = value,
				Not = not
			};
		}

		public static IPropertyPredicate Property<T, T2>(Expression<Func<T, object>> expression, Operator op, Expression<Func<T2, object>> expression2, bool not = false) where T : class where T2 : class
		{
			PropertyInfo propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
			PropertyInfo propertyInfo2 = ReflectionHelper.GetProperty(expression2) as PropertyInfo;
			return new PropertyPredicate<T, T2>
			{
				PropertyName = propertyInfo.Name,
				PropertyName2 = propertyInfo2.Name,
				Operator = op,
				Not = not
			};
		}

		public static IPredicateGroup Group(GroupOperator op, params IPredicate[] predicate)
		{
			return new PredicateGroup
			{
				Operator = op,
				Predicates = predicate
			};
		}

		public static IExistsPredicate Exists<TSub>(IPredicate predicate, bool not = false) where TSub : class
		{
			return new ExistsPredicate<TSub>
			{
				Not = not,
				Predicate = predicate
			};
		}

		public static IBetweenPredicate Between<T>(Expression<Func<T, object>> expression, BetweenValues values, bool not = false) where T : class
		{
			PropertyInfo propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
			return new BetweenPredicate<T>
			{
				Not = not,
				PropertyName = propertyInfo.Name,
				Value = values
			};
		}

		public static ISort Sort<T>(Expression<Func<T, object>> expression, bool ascending = true)
		{
			PropertyInfo propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
			return new Sort
			{
				PropertyName = propertyInfo.Name,
				Ascending = ascending
			};
		}
	}
}
