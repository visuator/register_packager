namespace register_packager;

public class ChunkPreparerOptions
{
    private int _maxLimit;

    public ChunkPreparerOptions()
    {
        ChunkOptions = new(this);
    }

    public int MaxLimit
    {
        get => _maxLimit;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            _maxLimit = value;
        }
    }

    public bool Legacy_CoilsCompatibility { get; set; }
    public bool ReadOnlyMode { get; set; }
    public ChunkOptions ChunkOptions { get; }
}
