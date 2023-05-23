using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HM.Framework.DapperExtensions.Mapper
{
	public class PluralizedAutoClassMapper<T> : AutoClassMapper<T> where T : class
	{
		public static class Formatting
		{
			private static readonly IList<string> Unpluralizables = new List<string>
			{
				"equipment",
				"information",
				"rice",
				"money",
				"species",
				"series",
				"fish",
				"sheep",
				"deer"
			};

			private static readonly IDictionary<string, string> Pluralizations = new Dictionary<string, string>
			{
				{
					"person",
					"people"
				},
				{
					"ox",
					"oxen"
				},
				{
					"child",
					"children"
				},
				{
					"foot",
					"feet"
				},
				{
					"tooth",
					"teeth"
				},
				{
					"goose",
					"geese"
				},
				{
					"(.*)fe?$",
					"$1ves"
				},
				{
					"(.*)man$",
					"$1men"
				},
				{
					"(.+[aeiou]y)$",
					"$1s"
				},
				{
					"(.+[^aeiou])y$",
					"$1ies"
				},
				{
					"(.+z)$",
					"$1zes"
				},
				{
					"([m|l])ouse$",
					"$1ice"
				},
				{
					"(.+)(e|i)x$",
					"$1ices"
				},
				{
					"(octop|vir)us$",
					"$1i"
				},
				{
					"(.+(s|x|sh|ch))$",
					"$1es"
				},
				{
					"(.+)",
					"$1s"
				}
			};

			public static string Pluralize(string singular)
			{
				if (Unpluralizables.Contains(singular))
				{
					return singular;
				}
				string empty = string.Empty;
				foreach (KeyValuePair<string, string> pluralization in Pluralizations)
				{
					if (Regex.IsMatch(singular, pluralization.Key))
					{
						return Regex.Replace(singular, pluralization.Key, pluralization.Value);
					}
				}
				return empty;
			}
		}

		public override void Table(string tableName)
		{
			base.Table(Formatting.Pluralize(tableName));
		}
	}
}
