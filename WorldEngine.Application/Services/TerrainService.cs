using WorldEngine.Core.Interfaces;
using WorldEngine.Domain.Metadata;
using WorldEngine.Domain.Models;

namespace WorldEngine.Application.Services;

public class TerrainService : IMapService<TerrainLayer>
{
    public TileId GetTile(TerrainLayer layer, int x, int y)
    {
        ValidateCoords(layer, x, y);
        return layer.GetTile(x, y);
    }

    public void SetTile(TerrainLayer layer, int x, int y, TileId id)
    {
        ValidateCoords(layer, x, y);
        layer.SetTile(x, y, id);
    }
    
    public static TerrainLayer CreateFromArray(TileId[,] source)
    {
        var width = source.GetLength(0);
        var height = source.GetLength(1);
        var layer = new TerrainLayer(width, height);

        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
                layer.SetTile(x, y, source[x, y]);

        return layer;
    }
    
    public static void FillArea(TerrainLayer layer, int startX, int startY, int width, int height, TileId id)
    {
        for (var y = startY; y < startY + height; y++)
            for (var x = startX; x < startX + width; x++)
            {
                if (IsInside(layer, x, y)) layer.SetTile(x, y, id);
            }
    }
    
    public bool CanPlaceObject(TerrainLayer layer, int startX, int startY, int width, int height)
    {
        for (var y = startY; y < startY + height; y++)
            for (var x = startX; x < startX + width; x++)
            {
                if (!IsInside(layer, x, y)) return false;

                var tile = layer.GetTile(x, y);
                var props = TileRegistry.Get(tile);

                if (!props.CanPlaceObject) return false;
            }
        return true;
    }

    private static void ValidateCoords(IMapLayer layer, int x, int y)
    {
        if (x < 0 || y < 0 || x >= layer.Width || y >= layer.Height)
            throw new ArgumentOutOfRangeException($"Coordinates out of range: ({x},{y})");
    }

    private static bool IsInside(IMapLayer layer, int x, int y) =>
        x >= 0 && y >= 0 && x < layer.Width && y < layer.Height;
}