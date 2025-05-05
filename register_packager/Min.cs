namespace register_packager;

internal struct Min<TKey, TValue>(TKey initial, TValue candidate) where TKey : IComparable<TKey>
{
    private TKey _value = initial;

    internal TValue BestCandidate { get; private set; } = candidate;
    internal void TryChange(TKey value, TValue candidate)
    {
        if (_value.CompareTo(value) >= 1)
        {
            _value = value;
            BestCandidate = candidate;
        }
    }
}
internal struct Min<TKey>(TKey initial) where TKey : IComparable<TKey>
{
    private TKey _value = initial;

    internal bool TryChange(TKey value)
    {
        if (_value.CompareTo(value) >= 1)
        {
            _value = value;
            return true;
        }
        return false;
    }
}
