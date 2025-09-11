namespace WorldEngine.Domain.Metadata;

public static class TileRegistry
{
    private static readonly Dictionary<long, TileProperties> Map = new();
    public static void Register(TileId id, TileProperties props) { Map[id.Value] = props; }
    public static TileProperties Get(TileId id)
    {
        if (Map.TryGetValue(id.Value, out var props)) return props;
        
        return new TileProperties
        {
            CanPlaceObject = false,
            MovementCost = int.MaxValue,
            TextureKey = "unknown"
        };
    }
}