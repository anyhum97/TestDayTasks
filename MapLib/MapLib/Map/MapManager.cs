using MapLib.Map.Enums;
using MapLib.Map.Helpers;
using MapLib.Map.Objects;

using System.Runtime.CompilerServices;

namespace MapLib
{
    public static class MapManager
    {
		public const int _width = 1000;
		public const int _height = 1000;

		public const int _count = _width * _height;

		private static readonly long[] _tiles = new long[_count];

		private static readonly Dictionary<int, TerritoryInfo> _territories = [];

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
