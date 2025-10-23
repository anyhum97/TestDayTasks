using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using MapLib.Map.Enums;

namespace MapLib.Map.Objects
{
	[StructLayout(LayoutKind.Explicit)]
	public readonly struct Tile(TileType tileType, ushort territoryId) : IEquatable<Tile>
	{
		[FieldOffset(0)]
		public readonly TileType TileType = tileType;
		
		[FieldOffset(2)]
		public readonly ushort TerritoryId = territoryId;
		
		/// <summary>
		/// Проверяет возможность размещения объекта.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool CanPlaceHere()
		{
			if(TileType == TileType.Mountain)
			{
				return false;
			}
			
			return true;
		}

		#region [Служебные]

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Tile other)
		{
			if(TileType != other.TileType)
			{
				return false;
			}

			if(TerritoryId != other.TerritoryId)
			{
				return false;
			}

			return true;
		}

		public override bool Equals(object? obj)
		{
			if(obj is not Tile other)
			{
				return false;
			}

			return Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(TileType, TerritoryId);
		}

		public static bool operator == (Tile tile1, Tile tile2)
		{
			return tile1.Equals(tile2);
		}

		public static bool operator != (Tile tile1, Tile tile2)
		{
			return !tile1.Equals(tile2);
		}

		#endregion
	}
}
