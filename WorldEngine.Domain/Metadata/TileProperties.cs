namespace WorldEngine.Domain.Metadata;

public class TileProperties
{
    public bool CanPlaceObject { get; init; }
    public int MovementCost { get; init; }
    public string TextureKey { get; init; } = string.Empty;
}