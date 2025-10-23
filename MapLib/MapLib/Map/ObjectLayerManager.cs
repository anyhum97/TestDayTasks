using MapLib.Helpers;
using MapLib.Interfaces;
using MapLib.Map.Objects;

namespace MapLib.Map
{
	public class ObjectLayerManager(IRedisClient redis, int mapWidth, int mapHeight)
	{
		private readonly IRedisClient _redis = redis;
		
		private readonly int _mapWidth = mapWidth;
		private readonly int _mapHeight = mapHeight;
		
		public event Action<MapObject>? ObjectAdded;
		public event Action<MapObject>? ObjectRemoved;
		public event Action<MapObject>? ObjectUpdated;

		/// <summary>
		/// Добавление объекта.
		/// </summary>
		public async Task AddObjectAsync(MapObject obj)
		{
			GeoPoint point = GeoConverter.ToGeo(obj.X, obj.Y, _mapWidth, _mapHeight);
			
			await _redis.GeoAddAsync("mapObjects", point.Longitude, point.Latitude, obj.Id);
			
			ObjectAdded?.Invoke(obj);
		}
		
		/// <summary>
		/// Удаление объекта
		/// </summary>
		public async Task RemoveObjectAsync(MapObject obj)
		{
			await _redis.RemoveAsync("mapObjects", obj.Id);
			
			ObjectRemoved?.Invoke(obj);
		}
		
		/// <summary>
		/// Получение по ID
		/// </summary>
		public async Task<MapObject?> GetByIdAsync(string id)
		{
			var obj = await _redis.GetObjectAsync(id); // Нужно хранить сериализованный MapObject в Redis
			
			return obj;
		}
		
		/// <summary>
		/// Получение всех объектов в области
		/// </summary>
		public async Task<IEnumerable<MapObject>> GetObjectsInAreaAsync(int x, int y, int width, int height)
		{
			GeoPoint topLeft = GeoConverter.ToGeo(x, y, _mapWidth, _mapHeight);
			GeoPoint bottomRight = GeoConverter.ToGeo(x + width, y + height, _mapWidth, _mapHeight);
			
			double centerLon = (topLeft.Longitude + bottomRight.Longitude) / 2;
			double centerLat = (topLeft.Latitude + bottomRight.Latitude) / 2;
			double radius = Math.Max(width, height);
			
			var ids = await _redis.GeoRadiusAsync("mapObjects", centerLon, centerLat, radius);
			
			var tasks = new List<Task<MapObject?>>();
			
			foreach(var id in ids)
			{
				tasks.Add(_redis.GetObjectAsync(id));
			}
		
			var objects = await Task.WhenAll(tasks);
		
			var result = new List<MapObject>();

			foreach(var obj in objects)
			{
				if(obj != null && obj.Intersects(x, y, width, height))
				{
					result.Add(obj);
				}
			}
			
			return result;
		}

		public async Task UpdateObjectAsync(MapObject obj)
		{
			await _redis.SetObjectAsync(obj);

			ObjectUpdated?.Invoke(obj);
		}
	}
}
