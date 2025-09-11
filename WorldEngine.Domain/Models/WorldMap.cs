using WorldEngine.Domain.Enums;
using WorldEngine.Domain.Metadata;

namespace WorldEngine.Domain.Models;

public class WorldMap
{
    public int Width { get; init; }
    public int Height { get; init; }

    public Dictionary<LayerKind, IMapLayer> Layers { get; init; } = new();
}