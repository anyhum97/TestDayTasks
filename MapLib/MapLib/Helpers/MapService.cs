using MagicOnion.Server;
using MagicOnion;
using MapLib.Interfaces;
using MapLib.Map.Objects;
using MapLib.Map;
using System.Runtime.CompilerServices;

public class MapService : ServiceBase<IMapService>, IMapService
{
	private readonly MapManager _mapManager;
	
	public MapService(MapManager mapManager)
	{
		_mapManager = mapManager;
	}
	
	public async UnaryResult<MapObjectsResponse> GetObjectsInArea(int x, int y, int radius)
	{
		var objects = _mapManager.GetAllObjectsInArea(x, y, radius);

		return new MapObjectsResponse(objects.ToArray());
	}
	
	public async UnaryResult<TerritoriesResponse> GetTerritoriesInArea(int x, int y, int width, int height)
	{
		var territories = _mapManager.GetAllTerritoriesInArea(x, y, width, height);
		
		return new TerritoriesResponse(territories.ToArray());
	}
	
	public IAsyncEnumerable<MapObjectEvent> SubscribeObjectEvents([EnumeratorCancellation] CancellationToken ct = default)
	{
		return CreateEventStream(ct);
	}
	
	private async IAsyncEnumerable<MapObjectEvent> CreateEventStream([EnumeratorCancellation] CancellationToken ct)
	{
		var channel = System.Threading.Channels.Channel.CreateUnbounded<MapObjectEvent>();
		
		void OnAdded(MapObject obj) => channel.Writer.TryWrite(new MapObjectEvent(obj.Id, null, null));
		void OnRemoved(int id) => channel.Writer.TryWrite(new MapObjectEvent(null, id, null));
		void OnUpdated(MapObject obj) => channel.Writer.TryWrite(new MapObjectEvent(null, null, obj));
		
		_mapManager.ObjectAdded += OnAdded;
		_mapManager.ObjectRemoved += OnRemoved;
		_mapManager.ObjectUpdated += OnUpdated;
		
		try
		{
			while(!ct.IsCancellationRequested)
			{
				if(await channel.Reader.WaitToReadAsync(ct))
				{
					while(channel.Reader.TryRead(out var evt))
					{
						yield return evt;
					}
				}
			}
		}
		finally
		{
			_mapManager.ObjectAdded -= OnAdded;
			_mapManager.ObjectRemoved -= OnRemoved;
			_mapManager.ObjectUpdated -= OnUpdated;
		}
	}
}
