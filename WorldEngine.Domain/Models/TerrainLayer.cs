using WorldEngine.Domain.Metadata;

namespace WorldEngine.Domain.Models;

public class TerrainLayer : IMapLayer
{
    public int Width { get; init; }
    public int Height { get; init; }
    public TileId[] Tiles { get; init; }

    public TerrainLayer(int width, int height)
    {
        Width = width;
        Height = height;
        Tiles = new TileId[width * height];
    }

    public TileId GetTile(int x, int y) => Tiles[y * Width + x];
    public void SetTile(int x, int y, TileId tile) => Tiles[y * Width + x] = tile;
}