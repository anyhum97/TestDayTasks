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

		public int? FirstOrDefaultByGeoPoint(GeoPoint point, double radius = 0.001);

		public IList<int>? GetAllObjectsInRadius(GeoPoint point, double radius);
	}
}
