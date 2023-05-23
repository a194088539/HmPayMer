using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace HM.Framework.DapperExtensions
{
	public static class ReflectionHelper
	{
		private static List<Type> _simpleTypes = new List<Type>
		{
			typeof(byte),
			typeof(sbyte),
			typeof(short),
			typeof(ushort),
			typeof(int),
			typeof(uint),
			typeof(long),
			typeof(ulong),
			typeof(float),
			typeof(double),
			typeof(decimal),
			typeof(bool),
			typeof(string),
			typeof(char),
			typeof(Guid),
			typeof(DateTime),
			typeof(DateTimeOffset),
			typeof(byte[])
		};

		public static MemberInfo GetProperty(LambdaExpression lambda)
		{
			Expression expression = lambda;
			while (true)
			{
				switch (expression.NodeType)
				{
				case ExpressionType.Lambda:
					expression = ((LambdaExpression)expression).Body;
					break;
				case ExpressionType.Convert:
					expression = ((UnaryExpression)expression).Operand;
					break;
				case ExpressionType.MemberAccess:
					return ((MemberExpression)expression).Member;
				default:
					return null;
				}
			}
		}

		public static IDictionary<string, object> GetObjectValues(object obj)
		{
			IDictionary<string, object> dictionary = new Dictionary<string, object>();
			if (obj == null)
			{
				return dictionary;
			}
			PropertyInfo[] properties = obj.GetType().GetProperties();
			foreach (PropertyInfo obj2 in properties)
			{
				string name = obj2.Name;
				object obj3 = dictionary[name] = obj2.GetValue(obj, null);
			}
			return dictionary;
		}

		public static string AppendStrings(this IEnumerable<string> list, string seperator = ", ")
		{
			return list.Aggregate(new StringBuilder(), (StringBuilder sb, string s) => ((sb.Length == 0) ? sb : sb.Append(seperator)).Append(s), (StringBuilder sb) => sb.ToString());
		}

		public static bool IsSimpleType(Type type)
		{
			Type item = type;
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				item = type.GetGenericArguments()[0];
			}
			return _simpleTypes.Contains(item);
		}

		public static string GetParameterName(this IDictionary<string, object> parameters, string parameterName, char parameterPrefix)
		{
			return $"{parameterPrefix}{parameterName}_{parameters.Count}";
		}

		public static string SetParameterName(this IDictionary<string, object> parameters, string parameterName, object value, char parameterPrefix)
		{
			string parameterName2 = parameters.GetParameterName(parameterName, parameterPrefix);
			parameters.Add(parameterName2, value);
			return parameterName2;
		}
	}
}
