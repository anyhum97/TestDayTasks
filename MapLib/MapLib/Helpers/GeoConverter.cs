using MapLib.Map.Objects;

namespace MapLib.Helpers
{
	public static class GeoConverter
	{
		// Например, каждая карта 1000x1000 тайлов -> координаты 0..1000
		// Преобразуем в широту и долготу (допустим, 0..90 широта, 0..180 долгота)
		public static GeoPoint ToGeo(int x, int y, int mapWidth, int mapHeight)
		{
			double lat = ((double)y / mapHeight) * 90.0;       // Y -> широта
			double lon = ((double)x / mapWidth) * 180.0;       // X -> долгота
			
			return new GeoPoint(lat, lon);
		}

		public static (int X, int Y) FromGeo(double lat, double lon, int mapWidth, int mapHeight)
		{
			int x = (int)((lon / 180.0) * mapWidth);
			int y = (int)((lat / 90.0) * mapHeight);
			return (x, y);
		}
	}
}
