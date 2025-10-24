using System.Runtime.CompilerServices;

using MagicOnion;
using MemoryPack;

using MapLib.Map.Objects;

namespace MapLib.Interfaces
{
    // Запросы
    [MemoryPackable]
    public partial record GetObjectsInAreaRequest(int X1, int Y1, int X2, int Y2);

    [MemoryPackable]
    public partial record GetRegionsInAreaRequest(int X1, int Y1, int X2, int Y2);

    // Ответы
    [MemoryPackable]
    public partial record GetObjectsInAreaResponse(MapObject[] Objects);

    [MemoryPackable]
    public partial record GetRegionsInAreaResponse(TerritoryInfo[] Territories);

    // События
    [MemoryPackable]
    public partial record MapObjectEvent(int? AddedId, int? RemovedId, MapObject? Updated);

    public interface IMapService : IService<IMapService>
    {
        UnaryResult<GetObjectsInAreaResponse> GetObjectsInArea(GetObjectsInAreaRequest request);

        UnaryResult<GetRegionsInAreaResponse> GetRegionsInArea(GetRegionsInAreaRequest request);

        IAsyncEnumerable<MapObjectEvent> SubscribeObjectEvents([EnumeratorCancellation] CancellationToken ct = default);
    }
}
