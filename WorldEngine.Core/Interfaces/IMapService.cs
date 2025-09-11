using WorldEngine.Domain.Metadata;
using WorldEngine.Domain.Models;

namespace WorldEngine.Core.Interfaces;

public interface IMapService<TLayer> where TLayer : IMapLayer
{
    TileId GetTile(TLayer layer, int x, int y);
    void SetTile(TLayer layer, int x, int y, TileId id);
    bool CanPlaceObject(TLayer layer, int startX, int startY, int width, int height);
}