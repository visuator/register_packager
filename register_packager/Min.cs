namespace register_packager;

internal struct Min<T>(T initial, IComparer<T> comparer) where T : IComparable<T>
{
    internal T Value { get; private set; } = initial;
    internal bool TryChange(T newValue)
    {
        if (comparer.Compare(Value, newValue) >= 1)
        {
            Value = newValue;
            return true;
        }
        return false;
    }
}
