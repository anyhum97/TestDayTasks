using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MapLib.Map.Objects
{
	[StructLayout(LayoutKind.Explicit)]
	public readonly struct GeoPoint(float latitude, float longitude)
	{
		[FieldOffset(0)]
		public readonly float Latitude = latitude;

		[FieldOffset(4)]
		public readonly float Longitude = longitude;

		#region [Служебные]

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(GeoPoint other)
		{
			if(Latitude != other.Latitude)
			{
				return false;
			}

			if(Longitude != other.Longitude)
			{
				return false;
			}

			return true;
		}

		public override bool Equals(object? obj)
		{
			if(obj is not GeoPoint other)
			{
				return false;
			}

			return Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Latitude, Longitude);
		}

		public static bool operator == (GeoPoint geo1, GeoPoint geo2)
		{
			return geo1.Equals(geo2);
		}

		public static bool operator != (GeoPoint geo1, GeoPoint geo2)
		{
			return !geo1.Equals(geo2);
		}

		#endregion
	}
}
