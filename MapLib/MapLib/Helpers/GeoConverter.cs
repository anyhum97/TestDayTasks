using MapLib.Map.Objects;

namespace MapLib.Helpers
{
	public static class GeoConverter
	{
		/// <summary>
		/// Преобразовывает позицию на карте в гео-координаты.
		/// </summary>
		public static GeoPoint ToGeo(Position position, int mapWidth, int mapHeight)
		{
			// Например, каждая карта 1000x1000 тайлов -> координаты 0..1000
			// Преобразуем в широту и долготу (допустим, 0..90 широта, 0..180 долгота)

			float latitude = (position.Y / mapHeight) * 90.0f;	// Y -> широта
			float longitude = (position.X / mapWidth) * 180.0f;	// X -> долгота
			
			return new GeoPoint(latitude, longitude);
		}

		/// <summary>
		/// Преобразовывает гео-координаты в позицию на карте.
		/// </summary>
		public static Position FromGeo(GeoPoint point, int mapWidth, int mapHeight)
		{
			int x = (int)((point.Longitude / 180.0) * mapWidth);
			int y = (int)((point.Latitude / 90.0) * mapHeight);

			return new Position(x, y);
		}
	}
}
