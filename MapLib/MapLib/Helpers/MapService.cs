using System.Threading.Channels;
using System.Runtime.CompilerServices;

using MagicOnion;
using MagicOnion.Server;

using MapLib.Map;
using MapLib.Interfaces;
using MapLib.Map.Objects;

public class MapService : ServiceBase<IMapService>, IMapService
{
	private readonly MapManager _mapManager;
	
	public MapService(MapManager mapManager)
	{
		_mapManager = mapManager;
	}
	
	public async UnaryResult<GetObjectsInAreaResponse> GetObjectsInArea(GetObjectsInAreaRequest request)
	{
		int width = request.X2 - request.X1 + 1;
		int height = request.Y2 - request.Y1 + 1;
		var objects = new List<MapObject>();
		
		for(int y = request.Y1; y <= request.Y2; y++)
		{
			for(int x = request.X1; x <= request.X2; x++)
			{
			    var tileObjects = _mapManager.GetAllObjectsInArea(x, y, 1);

			    objects.AddRange(tileObjects);
			}
		}
		
		return new GetObjectsInAreaResponse(objects.ToArray());
	}
	
	public async UnaryResult<GetRegionsInAreaResponse> GetRegionsInArea(GetRegionsInAreaRequest request)
	{
		int width = request.X2 - request.X1 + 1;
		int height = request.Y2 - request.Y1 + 1;
		
		var territories = _mapManager.GetAllTerritoriesInArea(request.X1, request.Y1, width, height);
		
		return new GetRegionsInAreaResponse(territories.ToArray());
	}
	
	public IAsyncEnumerable<MapObjectEvent> SubscribeObjectEvents([EnumeratorCancellation] CancellationToken ct = default)
	{
		return CreateEventStream(ct);
	}
	
	private async IAsyncEnumerable<MapObjectEvent> CreateEventStream([EnumeratorCancellation] CancellationToken ct)
	{
		var channel = Channel.CreateUnbounded<MapObjectEvent>();
		
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
