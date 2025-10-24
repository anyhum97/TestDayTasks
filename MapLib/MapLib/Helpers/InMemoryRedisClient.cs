using System.Collections.Concurrent;

using MapLib.Interfaces;
using MapLib.Map.Objects;

/*
namespace MapLib.Helpers
{
	/// <summary>
	/// Вместо реального подключения.
	/// </summary>
	public class InMemoryRedisClient : IRedisClient
	{
		private readonly ConcurrentDictionary<string, MapObject> _objects = new();

		// GEO индекс: ключ -> List<GeoEntry>
		private readonly ConcurrentDictionary<string, List<GeoEntry>> _geoIndexes = new();

		public struct GeoEntry
		{
			public string Member;
			public double Longitude;
			public double Latitude;

			public GeoEntry(string member, double longitude, double latitude)
			{
				Member = member;
				Longitude = longitude;
				Latitude = latitude;
			}
		}

		public Task GeoAddAsync(string key, double longitude, double latitude, string member)
		{
			var list = _geoIndexes.GetOrAdd(key, _ => new List<GeoEntry>());

			lock(list)
			{
				int index = -1;
				for(int i = 0; i < list.Count; i++)
				{
					if(list[i].Member == member)
					{
						index = i;
						break;
					}
				}

				if(index >= 0)
				{
					list[index] = new GeoEntry(member, longitude, latitude);
				}
				else
				{
					list.Add(new GeoEntry(member, longitude, latitude));
				}
			}

			return Task.CompletedTask;
		}

		public Task<IEnumerable<string>> GeoRadiusAsync(string key, double longitude, double latitude, double radius)
		{
			if(!_geoIndexes.TryGetValue(key, out var list))
			{
				return Task.FromResult((IEnumerable<string>)new List<string>());
			}

			List<string> result = new List<string>();

			lock(list)
			{
				for(int i = 0; i < list.Count; i++)
				{
					var entry = list[i];
					double dx = entry.Longitude - longitude;
					double dy = entry.Latitude - latitude;
					if(Math.Sqrt(dx * dx + dy * dy) <= radius)
					{
						result.Add(entry.Member);
					}
				}
			}

			return Task.FromResult((IEnumerable<string>)result);
		}

		public Task RemoveAsync(string key, string member)
		{
			if(_geoIndexes.TryGetValue(key, out var list))
			{
				lock(list)
				{
					for(int i = list.Count - 1; i >= 0; i--)
					{
						if(list[i].Member == member)
						{
							list.RemoveAt(i);
						}
					}
				}
			}

			_objects.TryRemove(member, out _);

			return Task.CompletedTask;
		}

		public Task<MapObject?> GetObjectAsync(string id)
		{
			_objects.TryGetValue(id, out var obj);

			return Task.FromResult<MapObject?>(obj);
		}

		public Task SetObjectAsync(MapObject obj)
		{
			_objects[obj.Id] = obj;

			return Task.CompletedTask;
		}
	}
}
*/
