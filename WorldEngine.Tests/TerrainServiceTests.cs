using System.Diagnostics;
using WorldEngine.Application.Services;
using WorldEngine.Core.Interfaces;
using WorldEngine.Domain.Metadata;
using WorldEngine.Domain.Models;
using Xunit;
using Assert = Xunit.Assert;

namespace WorldEngine.Tests;

public class TerrainServiceTests
{
    private readonly IMapService<TerrainLayer> _service;
    private readonly TileId _plains = TileId.FromTypeWithFlags(1, 0);
    private readonly TileId _mountain = TileId.FromTypeWithFlags(2, 0);

    public TerrainServiceTests()
    {
        _service = new TerrainService();
        TileRegistry.Register(_plains, new TileProperties { CanPlaceObject = true, MovementCost = 1, TextureKey = "plains" });
        TileRegistry.Register(_mountain, new TileProperties { CanPlaceObject = false, MovementCost = 5, TextureKey = "mountain" });
    }

    [Fact]
    public void CanCreateLayerAndGetTile()
    {
        var layer = new TerrainLayer(10, 10);
        layer.SetTile(0, 0, _plains);
        Assert.Equal(_plains.Value, layer.GetTile(0, 0).Value);
    }

    [Fact]
    public void SetAndGetTileUsingService()
    {
        var layer = new TerrainLayer(5, 5);
        _service.SetTile(layer, 2, 3, _mountain);
        var tile = _service.GetTile(layer, 2, 3);
        Assert.Equal(_mountain.Value, tile.Value);
    }

    [Fact]
    public void FillAreaCorrectly()
    {
        var layer = new TerrainLayer(5, 5);
        TerrainService.FillArea(layer, 1, 1, 3, 3, _mountain);

        for (var y = 1; y < 4; y++)
            for (var x = 1; x < 4; x++)
                Assert.Equal(_mountain.Value, layer.GetTile(x, y).Value);
    }

    [Fact]
    public void CanPlaceObjectShouldBeTrueOrFalse()
    {
        var layer = new TerrainLayer(3, 3);
        TerrainService.FillArea(layer, 0, 0, 2, 2, _plains);
        _service.SetTile(layer, 2, 2, _mountain);

        Assert.True(_service.CanPlaceObject(layer, 0, 0, 2, 2));
        Assert.False(_service.CanPlaceObject(layer, 1, 1, 2, 2));
    }

    [Fact]
    public void AccessOutsideBoundsThrows()
    {
        var layer = new TerrainLayer(3, 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetTile(layer, 3, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.SetTile(layer, -1, 0, _plains));
    }

    [Fact]
    public void CanCreateLayerFromArray()
    {
        var source = new TileId[2, 2]
        {
            { _plains, _mountain },
            { _mountain, _plains }
        };

        var layer = TerrainService.CreateFromArray(source);

        Assert.Equal(2, layer.Width);
        Assert.Equal(2, layer.Height);
        Assert.Equal(_plains.Value, layer.GetTile(0, 0).Value);
        Assert.Equal(_mountain.Value, layer.GetTile(1, 0).Value);
        Assert.Equal(_mountain.Value, layer.GetTile(0, 1).Value);
        Assert.Equal(_plains.Value, layer.GetTile(1, 1).Value);
    }

    [Fact]
    public void CanPlaceObjectWithFlagsShouldBeFalse()
    {
        var layer = new TerrainLayer(3, 3);
        TerrainService.FillArea(layer, 2, 2, 2, 2, _mountain);

        Assert.Equal(_mountain.Value, layer.GetTile(2, 2).Value);
        Assert.Equal(0, layer.GetTile(0, 0).Value);
        Assert.Equal(0, layer.GetTile(1, 1).Value);
    }

    [Fact]
    public void CanPlaceObject_NegativeCase_OutsideLayer()
    {
        var layer = new TerrainLayer(2, 2);
        TerrainService.FillArea(layer, 0, 0, 2, 2, _plains);

        var canPlace = _service.CanPlaceObject(layer, 1, 1, 2, 2);
        Assert.False(canPlace);
    }

    [Fact]
    public void FromTypeWithFlagsShouldBeTrue()
    {
        var typeId = 5;
        var flags = 10;
        var tileId = TileId.FromTypeWithFlags(typeId, flags);

        Assert.Equal(typeId, tileId.GetTypeId());
        Assert.Equal(flags, tileId.GetFlags());
    }

    [Fact]
    public void TileIdFromTypeShouldReturnCorrectValue()
    {
        const int typeId = 7;
        var tileId = TileId.FromType(typeId);

        Assert.Equal(typeId, tileId.GetTypeId()); 
        Assert.Equal(0, tileId.GetFlags()); 
    }

    [Fact]
    public void RegisterAndGetTileShouldReturnCorrectProperties()
    {
        var tileId = TileId.FromTypeWithFlags(1, 1);
        var props = new TileProperties
        {
            CanPlaceObject = true,
            MovementCost = 2,
            TextureKey = "grass.png"
        };

        TileRegistry.Register(tileId, props);

        var retrieved = TileRegistry.Get(tileId);
        Assert.Equal(props.CanPlaceObject, retrieved.CanPlaceObject);
        Assert.Equal(props.MovementCost, retrieved.MovementCost);
        Assert.Equal(props.TextureKey, retrieved.TextureKey);
    }

    [Fact]
    public void UnregisteredTileShouldReturnDefaultProperties()
    {
        var unregisteredTile = TileId.FromTypeWithFlags(999, 0);
        var props = TileRegistry.Get(unregisteredTile);

        Assert.False(props.CanPlaceObject);
        Assert.Equal(int.MaxValue, props.MovementCost);
        Assert.Equal("unknown", props.TextureKey);
    }

    [Fact]
    public void TileRegistryUpdatesPropertiesShouldBeTrue()
    {
        var tileId = TileId.FromTypeWithFlags(2, 0);
        TileRegistry.Register(tileId, new TileProperties { CanPlaceObject = false });
        TileRegistry.Register(tileId, new TileProperties { CanPlaceObject = true });

        var retrieved = TileRegistry.Get(tileId);
        Assert.True(retrieved.CanPlaceObject);
    }

    [Fact]
    public void GetTileShouldAccessAnyPositionInstantlyForLargeLayer()
    {
        var size = 1000;
        var layer = new TerrainLayer(size, size);

        for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
                layer.SetTile(x, y, _plains);

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        _ = layer.GetTile(0, 0);
        _ = layer.GetTile(size - 1, 0);
        _ = layer.GetTile(0, size - 1);
        _ = layer.GetTile(size - 1, size - 1);
        stopwatch.Stop();

        Assert.True(stopwatch.ElapsedMilliseconds < 1);
    }

    [Fact]
    public void FillAreaShouldCompleteFastWhenFillingLargeLayer()
    {
        var size = 1000;
        var layer = new TerrainLayer(size, size);
        var stopwatch = new Stopwatch();
        
        stopwatch.Start();
        TerrainService.FillArea(layer, 0, 0, size, size, _plains);
        stopwatch.Stop();
        
        Assert.True(stopwatch.ElapsedMilliseconds < 50);
    }
}