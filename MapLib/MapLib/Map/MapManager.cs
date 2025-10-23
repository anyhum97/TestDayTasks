using MapLib.Map.Enums;
using MapLib.Map.Helpers;
using MapLib.Map.Objects;

using System.Runtime.CompilerServices;

namespace MapLib
{
	public static class MapManager
	{
		private const int _width = 1000;
		private const int _height = 1000;
		
		private const int _count = _width * _height;
		
		private static readonly long[] _tiles = new long[_count];
		
		private static readonly Dictionary<int, TerritoryInfo> _territories = [];
		
		/// <summary>
		/// Генерация карты-заглушки для PoC
		/// </summary>
		public static void GenerateMap(int territoryCount = 100)
		{
			const int _minTerritoryCount = 2;
			const int _maxTerritoryCount = 1000;
			
			territoryCount = Math.Max(territoryCount, _minTerritoryCount);
			territoryCount = Math.Min(territoryCount, _maxTerritoryCount);
			
			var random = new Random();
			
			// Инициализация тайлов случайными типами (TileType)
			
			Span<long> tilesSpan = _tiles;
			
			for(int i=0; i<tilesSpan.Length; ++i)
			{
				var type = (TileType) random.Next(0, 2); // Plain или Mountain
				tilesSpan[i] = TileEncoder.Encode(type, 0); // ID территории пока 0
			}
			
			// 2. Создание словаря территорий
			_territories.Clear();
			for (int t = 1; t <= territoryCount; t++)
			{
				_territories[t] = new TerritoryInfo
				{
					Id = t,
					Name = $"Territory {t}",
				};
			}
			
			// 3. Простейшее распределение территорий по карте
			int blockWidth = _width / territoryCount;
			int blockHeight = _height / territoryCount;
			
			for (int y = 0; y < _height; y++)
			{
				for (int x = 0; x < _width; x++)
				{
					int territoryId = ((x / blockWidth) % territoryCount) + 1;
					int index = GetIndex(x, y);
					var type = tilesSpan[index].GetTileType();
					tilesSpan[index] = TileEncoder.Encode(type, territoryId);
					_territories[territoryId].TileCount++;
				}
			}
		}
		
		public static long GetTile(int x, int y)
		{
			var index = GetIndex(x, y);
			
			return _tiles[index];
		}
		
		public static TileType GetTileType(int x, int y)
		{
			var index = GetIndex(x, y);
			
			return _tiles[index].GetTileType();
		}
		
		public static int GetTerritoryId(int x, int y)
		{
			var index = GetIndex(x, y);
			
			return _tiles[index].GetTerritoryId();
		}
		
		public static TerritoryInfo GetTerritoryInfo(int id)
		{
			return _territories[id];
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int GetIndex(int x, int y)
		{
			return y * _width + x;
		}
	}
}
