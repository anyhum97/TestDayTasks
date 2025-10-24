using System.Runtime.CompilerServices;

using MapLib.Helpers;
using MapLib.Interfaces;
using MapLib.Map.Enums;
using MapLib.Map.Objects;

namespace MapLib.Map
{
	/// <summary>
	/// Класс для работы с тайловой картой в однопоточной среде.
	/// </summary>
	public class MapManager
	{
		private const int _defaultWidth = 1000;
		private const int _defaultHeight = 1000;

		private const int _defaultTerritoriesCount = 100;

		private int _width = _defaultWidth;
		private int _height = _defaultHeight;

		private readonly Dictionary<int, MapObject> _objects = [];
		private readonly Dictionary<ushort, TerritoryInfo> _territories = [];

		private IRedisClient _redis;

		private Tile[] _tiles;

		public int Height => _height;

		public int Width => _width;

		#region [События]

		public event Action<MapObject>? ObjectAdded;

		public event Action<int>? ObjectRemoved;

		public event Action<MapObject>? ObjectUpdated;

		#endregion

		public MapManager(IRedisClient redis, 
			Dictionary<ushort, TerritoryInfo> territories = null, 
			int width = _defaultWidth, 
			int height = _defaultHeight, 
			int territoriesCount = _defaultTerritoriesCount,
			int? seed = null)
		{
			ValidateAndSetBounds(width, height);
			ValidateAndSetRedis(redis);

			_tiles = new Tile[_width * _height];

			if(territories == null)
			{
				GenerateTerritories(territoriesCount);
			}
			else
			{
				_territories = territories;
			}

			GenerateMap(seed);
		}

		public MapManager(IRedisClient redis, 
			Tile[] tiles, 
			int width, 
			int height, 
			Dictionary<ushort, TerritoryInfo> territories = null, 
			int territoriesCount = _defaultTerritoriesCount,
			int? seed = null)
		{
			ValidateAndSetBounds(width, height);
			ValidateAndSetTilesArray(tiles, width, height);
			ValidateAndSetRedis(redis);

			if(territories == null)
			{
				GenerateTerritories(territoriesCount);
			}
			else
			{
				_territories = territories;
			}
		}

		public MapManager(IRedisClient redis, 
			IEnumerable<Tile> tiles, 
			int width, 
			int height, 
			Dictionary<ushort, TerritoryInfo> territories = null, 
			int territoriesCount = _defaultTerritoriesCount,
			int? seed = null)
		{
			ValidateAndSetBounds(width, height);
			ValidateAndSetTilesCollection(tiles, width, height);
			ValidateAndSetRedis(redis);

			if(territories == null)
			{
				GenerateTerritories(territoriesCount);
			}
			else
			{
				_territories = territories;
			}
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
			#if DEBUG

			if(width <= 0 && height <= 0)
			{
				throw new ArgumentOutOfRangeException();
			}

			#endif

			var mapObject = _objects[id];

			return mapObject.Intersects(x, y, width, height);
		}

		/// <summary>
		/// Получить все объекты в указанной области.
		/// </summary>
		public List<MapObject> GetAllObjectsInArea(int x, int y, int radius)
		{
			var point = GeoConverter.ToGeo(new Position(x, y), _width, _height);

			var ids = _redis.GetAllObjectsInArea(point, radius);

			var result = new List<MapObject>(ids.Count);

			foreach(var id in ids)
			{
				if(_objects.TryGetValue(id, out var obj))
				{
					result.Add(obj);
				}
			}

			return result;
		}

		/// <summary>
		/// Получение метаданных региона по ID.
		/// </summary>
		public bool TryGetTerritoryInfoById(ushort id, out TerritoryInfo? territoryInfo)
		{
			return _territories.TryGetValue(id, out territoryInfo);
		}

		/// <summary>
		/// Проверяет, принадлежит ли тайл региону.
		/// </summary>
		public bool IsTileInTerritory(int tileX, int tileY, TerritoryInfo territoryInfo)
		{
			var tile = GetTile(tileX, tileY);

			if(tile.TerritoryId == territoryInfo.Id)
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Получение всех регионов, пересекающих указанную прямоугольную область.
		/// </summary>
		public List<TerritoryInfo> GetAllTerritoriesInArea(int startX, int startY, int width, int height)
		{
			#if DEBUG

			if(width <= 0 || height <= 0)
			{
				throw new ArgumentOutOfRangeException();
			}

			#endif
			
			var result = new HashSet<ushort>();
			
			startX = Math.Max(0, startX);
			startY = Math.Max(0, startY);

			int rowEnd = Math.Min(startY + height, _height);
			int colEnd = Math.Min(startX + width, _width);
			
			for(int y=startY; y<rowEnd; ++y)
			{
				int index= y * _width + startX;
				
				for(int x=startX; x<colEnd; ++x, ++index)
				{
					var territoryId = _tiles[index].TerritoryId;

					if(territoryId != 0) // 0 = отсутствие территории
					{
						result.Add(territoryId);
					}
				}
			}
			
			var territories = new List<TerritoryInfo>(result.Count);
			
			foreach(var id in result)
			{
				if(_territories.TryGetValue(id, out var info))
				{
					territories.Add(info);
				}
			}
			
			return territories;
		}

		/// <summary>
		/// Генерация регионов
		/// </summary>
		private void GenerateTerritories(int territories)
		{
			if(territories >= ushort.MaxValue)
			{
				throw new ArgumentOutOfRangeException();
			}

			for(int i=1; i<=territories; ++i)
			{
				var id = (ushort)i;

				var TerritoryInfo = new	TerritoryInfo()
				{
					Id = id,
					Name = $"Territory {i}",
				};

				_territories.Add(id, TerritoryInfo);
			}
		}

		/// <summary>
		/// Генерация карты - заполнение тайлов.
		/// </summary>
		/// <param name="seed"></param>
		private void GenerateMap(int? seed)
		{
			if(seed == null)
			{
				var gen = new Random();

				seed = gen.Next();
			}

			var random = new Random(seed.Value);

			var territoryIds = _territories.Keys.ToArray();

			int territoryCount = territoryIds.Length;

			for(int y=0; y<_height; ++y)
			{
				int index = y * _width;

				for(int x=0; x<_width; ++x, ++index)
				{
					// Случайный TileType
					var tileType = (TileType)random.Next(0, 2);

					// TerritoryId в зависимости от положения, чтобы все сгенерированные регионы были одинаковыми по площади
					ushort territoryId = (ushort)((index / territoryCount) % territoryCount + 1);

					_tiles[index] = new Tile(tileType, territoryId);
				}
			}
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
