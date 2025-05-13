namespace register_packager;

internal struct Min<T> where T : IComparable<T>
{
    private readonly IComparer<T>? _comparer;

    public Min(T initial)
    {
        Value = initial;
    }

    public Min(T initial, IComparer<T> comparer)
    {
        Value = initial;
        _comparer = comparer;
    }

    internal T Value { get; private set; }
    internal bool TryChange(T newValue)
    {
        if ((_comparer?.Compare(Value, newValue) ?? Value.CompareTo(newValue)) >= 1)
        {
            Value = newValue;
            return true;
        }
        return false;
    }
}
