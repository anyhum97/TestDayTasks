using MapLib.Map;
using MapLib.Map.Objects;
using MapLib.Interfaces;

namespace MapLib.Tests
{
	[TestFixture]
	public class MapManagerTests
	{
		private IRedisClient _redis;

		[SetUp]
		public void Setup()
		{
			_redis = new RedisGeoClient("localhost:6379");
		}

		
	}
}
