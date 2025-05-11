namespace register_packager;

public class ChunkPreparerOptions
{
    private int _maxLimit;
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
}
