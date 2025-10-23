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
		
		private const int _minTerritoryCount = 2;
		private const int _maxTerritoryCount = 1000;

		private static readonly long[] _tiles = new long[_count];
		
		private static readonly Dictionary<int, TerritoryInfo> _territories = new(_maxTerritoryCount);
		
		/// <summary>
		/// Генерация карты-заглушки для PoC
		/// </summary>
		public static void GenerateMap(int territoryCount = 100)
		{
			territoryCount = Math.Max(territoryCount, _minTerritoryCount);
			territoryCount = Math.Min(territoryCount, _maxTerritoryCount);
			
			var random = new Random();
			
			// Инициализация тайлов случайными значенифми
			
			Span<long> tilesSpan = _tiles;
			
			for(int i=0; i<_count; ++i)
			{
				var tileType = (TileType)random.Next(0, 2);

				var territoryId = random.Next(territoryCount); 

				tilesSpan[i] = TileEncoder.Encode(tileType, territoryId);
			}
			
			// Создание словаря территорий

			_territories.Clear();

			for(int t=1; t<=territoryCount; ++t)
			{
				_territories[t] = new TerritoryInfo
				{
					Id = t,
					Name = $"Territory {t}",
				};
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
