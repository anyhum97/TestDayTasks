using System.Runtime.CompilerServices;

using MapLib.Map.Enums;

namespace MapLib.Map.Helpers
{
    public static class TileEncoder
    {
		public const int TileTypeBits = 8; // Сколько младших бит выделяем под типы тайлов, до 256 типов

		public const long TileTypeMask = (1L << TileTypeBits) - 1;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Encode(TileType tileType, int territoryId)
		{
			return ((long)territoryId << TileTypeBits) | (long)tileType;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TileType GetTileType(this long value)
		{
			return (TileType)(value & TileTypeMask);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetTerritoryId(this long value)
		{
			return (int)(value >> TileTypeBits);
		}
    }
}
