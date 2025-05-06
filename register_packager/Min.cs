namespace register_packager;

internal struct Min<TKey>(TKey initial) where TKey : IComparable<TKey>
{
    internal TKey Value { get; private set; } = initial;
    internal bool TryChange(TKey value)
    {
        if (Value.CompareTo(value) >= 1)
        {
            Value = value;
            return true;
        }
        return false;
    }
}
