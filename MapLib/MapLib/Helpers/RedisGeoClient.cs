using StackExchange.Redis;

using MapLib.Interfaces;
using MapLib.Map.Objects;

public class RedisGeoClient : IRedisClient
{
	private const string _geoKey = "map_objects";

	private readonly IDatabase _db;

	public RedisGeoClient(string connectionString)
	{
		var options = ConfigurationOptions.Parse(connectionString);
        
		options.AllowAdmin = true;

		var redis = ConnectionMultiplexer.Connect(options);

		_db = redis.GetDatabase();
	}

	public bool TryAddGeoPoint(int key, GeoPoint point)
	{
		try
		{
			return _db.GeoAdd(_geoKey, point.Longitude, point.Latitude, key.ToString());
		}
		catch(Exception ex)
		{
			// ToDo - Log
			return false;
		}
	}

	public bool TryRemoveGeoPoint(int key)
	{
		try
		{
			return _db.SetRemove(_geoKey, key.ToString());
		}
		catch(Exception ex)
		{
			// ToDo - Log
			return false;
		}
	}

	public int? FirstOrDefaultByGeoPoint(GeoPoint point, double radius = 1)
	{
		// Ищем объекты в указанном радиусе

		try
		{
			var result = _db.GeoRadius(_geoKey, point.Longitude, point.Latitude, radius, GeoUnit.Meters, 1);

			if(result == null || result.Length == 0)
			{
				return null;
			}

			if(int.TryParse(result[0].Member, out int id))
			{
				return id;
			}
		}
		catch(Exception ex)
		{
			// ToDo - Log
		}

		return null;
	}

	public IList<int>? GetAllObjectsInArea(GeoPoint point, double radius)
	{
		try
		{
			var result = _db.GeoRadius(_geoKey, point.Longitude, point.Latitude, radius, GeoUnit.Meters);

			var list = new List<int>(result.Length);

			foreach(var record in result)
			{
				if(int.TryParse(record.Member, out var id))
				{
					list.Add(id);
				}
			}

			return list;
		}
		catch(Exception ex)
		{
			// ToDo - Log
		}

		return null;
	}
}
