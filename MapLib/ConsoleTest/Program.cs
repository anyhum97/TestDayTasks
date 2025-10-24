using MapLib.Helpers;
using MapLib.Interfaces;
using MapLib.Map.Objects;

namespace ConsoleTest
{
	public static class Program
	{
		public static void Main()
		{
			var position1 = new Position(1, 1);
			var position2 = new Position(20, 20);

			var geo1 = GeoConverter.ToGeo(position1, 1000, 1000);
			var geo2 = GeoConverter.ToGeo(position2, 1000, 1000);

			IRedisClient redis = new RedisGeoClient("localhost:6379");

			redis.TryRemoveGeoPoint(1);
			redis.TryRemoveGeoPoint(2);

			var state1 = redis.TryAddGeoPoint(1, geo1);
			var state2 = redis.TryAddGeoPoint(2, geo2);

			var id1 = redis.FirstOrDefaultByGeoPoint(geo1);
			var id2 = redis.FirstOrDefaultByGeoPoint(geo2);

			var objects1 = redis.GetAllObjectsInArea(geo1, 1);
			var objects2 = redis.GetAllObjectsInArea(geo2, 1000000); // Потому что реальные расстояния


		}
	}
}
