namespace HM.Framework.Caching
{
	public class CachingFactory
	{
		public static ICache GetCaching()
		{
			return new RedisCache();
		}
	}
}
