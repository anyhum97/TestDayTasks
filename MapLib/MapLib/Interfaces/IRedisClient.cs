using MapLib.Map.Objects;

namespace MapLib.Interfaces
{
	/// <summary>
	/// Абстракция для работы с Redis
	/// </summary>
	public interface IRedisClient
	{
		public bool TryAddGeoPoint(int key, GeoPoint point);

		public bool TryRemoveGeoPoint(int key);

		public int? FirstOrDefaultByGeoPoint(GeoPoint point);

		public IList<MapObject> GetObjectsByGeoPoint(GeoPoint point);

		//Task<IEnumerable<string>> GeoRadiusAsync(string key, double longitude, double latitude, double radius);
		//
		//Task RemoveAsync(string key, string member);
		//
		//Task<MapObject?> GetObjectAsync(string id);
		//
		//Task SetObjectAsync(MapObject obj);
	}
}
