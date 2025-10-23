using MapLib.Map.Objects;

namespace MapLib.Interfaces
{
	/// <summary>
	/// Абстракция для работы с Redis
	/// </summary>
	public interface IRedisClient
	{
		Task GeoAddAsync(string key, double longitude, double latitude, string member);
		Task<IEnumerable<string>> GeoRadiusAsync(string key, double longitude, double latitude, double radius);
		Task RemoveAsync(string key, string member);
		Task<MapObject?> GetObjectAsync(string id);
		Task SetObjectAsync(MapObject obj);
	}
}
