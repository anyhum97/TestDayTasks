using MapLib.Helpers;
using MapLib.Interfaces;
using MapLib.Map.Objects;
using System.Runtime.CompilerServices;

namespace MapLib.Map
{
	/// <summary>
	/// Класс для работы с тайловой картой в однопоточной среде.
	/// </summary>
	public class MapManager
	{
		private const int _defaultWidth = 1000;
		private const int _defaultHeight = 1000;

		private int _width = _defaultWidth;
		private int _height = _defaultHeight;

		private readonly Dictionary<int, MapObject> _objects = [];

		private IRedisClient _redis;

		private Tile[] _tiles;

		public int Height => _height;

		public int Width => _width;

		#region [События]

		public event Action<MapObject>? ObjectAdded;

		public event Action<int>? ObjectRemoved;

		public event Action<MapObject>? ObjectUpdated;

		#endregion

		public MapManager(IRedisClient redis, int width = _defaultWidth, int height = _defaultHeight)
		{
			ValidateAndSetBounds(width, height);
			ValidateAndSetRedis(redis);

			_tiles = new Tile[_width * _height];
		}

		public MapManager(IRedisClient redis, Tile[] tiles, int width, int height)
		{
			ValidateAndSetBounds(width, height);
			ValidateAndSetTilesArray(tiles, width, height);
			ValidateAndSetRedis(redis);
		}

		public MapManager(IRedisClient redis, IEnumerable<Tile> tiles, int width, int height)
		{
			ValidateAndSetBounds(width, height);
			ValidateAndSetTilesCollection(tiles, width, height);
			ValidateAndSetRedis(redis);
		}

		/// <summary>
		/// Получает тайл по координатам.
		/// </summary>
		public Tile GetTile(int x, int y)
		{
			var index = GetIndex(x, y);

			return _tiles[index];
		}

		/// <summary>
		/// Устанавливает тайл по координатам.
		/// </summary>
		public void SetTile(int x, int y, Tile tile)
		{
			var index = GetIndex(x, y);

			_tiles[index] = tile;
		}

		/// <summary>
		/// Заполняет прямоугольную область тайлами.
		/// </summary>
		public void FillArea(int startX, int startY, int width, int height, Tile tile)
		{
			// Оптимизация: считаем линейный индекс сразу

			int rowEnd = startY + height;
			int colEnd = startX + width;

			for(int y=startY; y<rowEnd; ++y)
			{
				int index = y * _width + startX;

				for(int x=startX; x<colEnd; ++x, ++index)
				{
					_tiles[index] = tile;
				}
			}
		}

		/// <summary>
		/// Проверяет возможность размещения объекта в прямоугольной области.
		/// </summary>
		public bool CanPlaceInArea(int startX, int startY, int width, int height)
		{
			// Оптимизация: считаем линейный индекс сразу

			int rowEnd = startY + height;
			int colEnd = startX + width;

			for (int y=startY; y<rowEnd; ++y)
			{
				int index = y * _width + startX;

				for(int x=startX; x<colEnd; ++x, ++index)
				{
					if(!_tiles[index].CanPlaceHere())
					{
						return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Добавление объекта.
		/// </summary>
		public bool TryAddObject(MapObject mapObject)
		{
			var id = mapObject.Id;

			var point = GeoConverter.ToGeo(mapObject.Position, _width, _height);
			
			// ToDo - продумать, что делать, если не удалось добавить в Redis
			_redis.TryAddGeoPoint(id, point); 

			if(_objects.TryAdd(id, mapObject))
			{
				Task.Run(() => ObjectAdded?.Invoke(mapObject));

				return true;
			}

			return false;
		}

		/// <summary>
		/// Добавление объекта.
		/// </summary>
		public bool TryRemoveObject(int id)
		{
			// ToDo - продумать, что делать, если не удалось удалить из Redis
			_redis.TryRemoveGeoPoint(id);

			if(_objects.Remove(id))
			{
				Task.Run(() => ObjectRemoved?.Invoke(id));

				return true;
			}

			return false;
		}

		public MapObject? FirstOrDefaultByPosition(Position position)
		{
			var point = GeoConverter.ToGeo(position, _width, _height);

			var id = _redis.FirstOrDefaultByGeoPoint(point);

			if(id.HasValue)
			{
				if(_objects.TryGetValue(id.Value, out var mapObject))
				{
					return mapObject;
				}
			}

			return null;
		}

		/// <summary>
		/// Проверяет вхождение объекта в указанную область.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IntersectsArea(int id, int x, int y, int width, int height)
		{
			var mapObject = _objects[id];

			return mapObject.Intersects(x, y, width, height);
		}

		private void ValidateAndSetBounds(int width, int height)
		{
			ArgumentOutOfRangeException.ThrowIfLessThan(width, 1, nameof(width));
			ArgumentOutOfRangeException.ThrowIfLessThan(height, 1, nameof(height));

			_width = width;
			_height = height;
		}

		private void ValidateAndSetTilesArray(Tile[] tiles, int width, int height)
		{
			ArgumentNullException.ThrowIfNull(tiles);

			var length = tiles.Length;

			var expected = width * height;

			if(length != expected)
			{
				throw new ArgumentOutOfRangeException("Invalid array lenght");
			}

			_tiles = tiles;
		}

		private void ValidateAndSetTilesCollection(IEnumerable<Tile> tiles, int width, int height)
		{
			ArgumentNullException.ThrowIfNull(tiles);

			var count = tiles.Count();

			var expected = width * height;

			if(count != expected)
			{
				throw new ArgumentOutOfRangeException("Invalid collection size");
			}

			_tiles = tiles.ToArray();
		}

		private void ValidateAndSetRedis(IRedisClient redis)
		{
			ArgumentNullException.ThrowIfNull(redis);

			_redis = redis;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int GetIndex(int x, int y)
		{
			#if DEBUG
			
			// Проверяем диапазон значений только в DEBUG сборке.

			if(x < 0 || x >= _width)
			{
				throw new IndexOutOfRangeException();
			}

			if(y < 0 || y >= _height)
			{
				throw new IndexOutOfRangeException();
			}

			#endif

			return y * _width + x;
		}
	}
}
