using ServiceStack.Text;

namespace HM.Framework
{
	public static class TextExtensions
	{
		public static string ToJson<T>(this T obj)
		{
			return JsonSerializer.SerializeToString(obj);
		}

		public static T FormJson<T>(this string json)
		{
			return JsonSerializer.DeserializeFromString<T>(json);
		}
	}
}
