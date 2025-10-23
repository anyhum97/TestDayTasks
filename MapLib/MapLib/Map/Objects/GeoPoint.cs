namespace MapLib.Map.Objects
{
	public readonly struct GeoPoint(double latitude, double longitude)
	{
		public readonly double Latitude = latitude;
		public readonly double Longitude = longitude;
	}
}
