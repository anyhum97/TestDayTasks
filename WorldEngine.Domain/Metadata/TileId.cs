namespace WorldEngine.Domain.Metadata;
public readonly struct TileId
{
    public long Value { get; }
    public TileId (long value) { Value = value; }
    public override string ToString() => Value.ToString();
    public static TileId FromType(int typeId) =>
        FromTypeWithFlags(typeId, 0);
    public static TileId FromTypeWithFlags(int typeId, int flags) =>
        new TileId(((long)typeId << 32) | (uint)flags);
    
    public int GetTypeId() => (int)(Value >> 32);
    public int GetFlags() => (int)(Value & 0xFFFFFFFF);
}