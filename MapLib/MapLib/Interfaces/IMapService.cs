using MagicOnion;
using MapLib.Map.Objects;
using MemoryPack;
using System.Runtime.CompilerServices;

namespace MapLib.Interfaces
{
	public interface IMapService : IService<IMapService>
	{
		// Запрос объектов в области
		UnaryResult<MapObjectsResponse> GetObjectsInArea(int x, int y, int radius);
		
		// Запрос регионов в области
		UnaryResult<TerritoriesResponse> GetTerritoriesInArea(int x, int y, int width, int height);
		
		// Подписка на события объектов
		IAsyncEnumerable<MapObjectEvent> SubscribeObjectEvents([EnumeratorCancellation] CancellationToken ct = default);
	}
	
	[MemoryPackable]
	public partial record MapObjectsResponse(MapObject[] Objects);
	
	[MemoryPackable]
	public partial record TerritoriesResponse(TerritoryInfo[] Territories);
	
	[MemoryPackable]
	public partial record MapObjectEvent(int? AddedId, int? RemovedId, MapObject? Updated);
}
