using WorldEngine.Domain.Metadata;

namespace WorldEngine.Domain.Models;

public interface IMapLayer
{
    int Width { get; }
    int Height { get; }

    TileId GetTile(int x, int y);
    void SetTile(int x, int y, TileId tile);
}