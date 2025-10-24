using System.Collections.Concurrent;

using MapLib.Interfaces;
using MapLib.Map.Objects;

namespace MapLib.Helpers
{
	/// <summary>
	/// Вместо реального подключения.
	/// </summary>
	public class InMemoryRedisClient : IRedisClient
	{
		private readonly ConcurrentDictionary<int, GeoPoint> _points = new();

		public bool TryAddGeoPoint(int key, GeoPoint point)
		{
			return _points.TryAdd(key, point);
		}

		public bool TryRemoveGeoPoint(int key)
		{
			return _points.TryRemove(key, out _);
		}

		public int? FirstOrDefaultByGeoPoint(GeoPoint point)
		{
			// Простая проверка точного совпадения (или можно заменить на поиск ближайшей точки)

			foreach(var kvp in _points)
			{
				if(kvp.Value.Equals(point))
				{
					return kvp.Key;
				}
			}

			return null;
		}

		public IList<int> GetAllObjectsInArea(GeoPoint point1, GeoPoint point2)
		{
			// Определяем границы области
			float minLat = Math.Min(point1.Latitude, point2.Latitude);
			float maxLat = Math.Max(point1.Latitude, point2.Latitude);
			float minLon = Math.Min(point1.Longitude, point2.Longitude);
			float maxLon = Math.Max(point1.Longitude, point2.Longitude);

			var result = new List<int>();

			foreach(var kvp in _points)
			{
				var p = kvp.Value;

				if(p.Latitude >= minLat && p.Latitude <= maxLat && p.Longitude >= minLon && p.Longitude <= maxLon)
				{
					result.Add(kvp.Key);
				}
			}

			return result;
		}
	}
}
