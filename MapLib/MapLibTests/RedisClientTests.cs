using MapLib.Helpers;
using MapLib.Map.Objects;

namespace MapLibTests
{
	[TestFixture]
    public class RedisClientTests
    {
		private RedisGeoClient _redis;

		[SetUp]
		public void Setup()
		{
			_redis = new RedisGeoClient("localhost:6379");
		}

		[Test]
		public void PingRedis()
		{
			var state = _redis.IsConnected();

			Assert.IsTrue(state);
		}

		[Test]
		public void AddTest()
		{
			var position = new Position(1, 1);

			var geo = GeoConverter.ToGeo(position, 1000, 1000);

			_redis.TryRemoveGeoPoint(1);

			var state = _redis.TryAddGeoPoint(1, geo);

			_redis.TryRemoveGeoPoint(1);

			Assert.IsTrue(state);
		}

		[Test]
		public void RemoveTest()
		{
			var position = new Position(2, 2);

			var geo = GeoConverter.ToGeo(position, 1000, 1000);

			_redis.TryRemoveGeoPoint(2);

			var state1 = _redis.TryAddGeoPoint(2, geo);

			var state2 = _redis.TryRemoveGeoPoint(2);

			Assert.IsTrue(state1);
			Assert.IsTrue(state2);
		}

		[Test]
		public void FindOneObject()
		{
			var position = new Position(3, 3);

			var geo = GeoConverter.ToGeo(position, 1000, 1000);

			_redis.TryRemoveGeoPoint(3);

			var state1 = _redis.TryAddGeoPoint(3, geo);

			var result = _redis.FirstOrDefaultByGeoPoint(geo);

			var state3 = _redis.TryRemoveGeoPoint(3);

			Assert.IsTrue(state1);
			Assert.AreEqual(3, result);
			Assert.IsTrue(state3);
		}

		[Test]
		public void FindAllObjects()
		{
			var position1 = new Position(3, 3);
			var position2 = new Position(3.00001f, 3.000015f);
			var position3 = new Position(3.000004f, 3.00000078f);
			var position4 = new Position(100, 100);

			var geo1 = GeoConverter.ToGeo(position1, 1000, 1000);
			var geo2 = GeoConverter.ToGeo(position2, 1000, 1000);
			var geo3 = GeoConverter.ToGeo(position3, 1000, 1000);
			var geo4 = GeoConverter.ToGeo(position4, 1000, 1000);

			_redis.TryRemoveGeoPoint(4);
			_redis.TryRemoveGeoPoint(5);
			_redis.TryRemoveGeoPoint(6);
			_redis.TryRemoveGeoPoint(7);

			var state1 = _redis.TryAddGeoPoint(4, geo1);
			var state2 = _redis.TryAddGeoPoint(5, geo2);
			var state3 = _redis.TryAddGeoPoint(6, geo3);
			var state4 = _redis.TryAddGeoPoint(7, geo4);

			Assert.IsTrue(state1);
			Assert.IsTrue(state2);
			Assert.IsTrue(state3);
			Assert.IsTrue(state4);

			var result = _redis.GetAllObjectsInRadius(geo1, 1000);

			Assert.IsNotNull(result);

			// Объекты должны быть
			Assert.IsTrue(result.Contains(4));
			Assert.IsTrue(result.Contains(5));
			Assert.IsTrue(result.Contains(5));

			// Объект слишком далеко
			Assert.IsFalse(result.Contains(7));
		}
    }
}
