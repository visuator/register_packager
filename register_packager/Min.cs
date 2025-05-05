namespace register_packager;

internal struct Min<T>(T initial, IComparer<T> comparer) where T : IComparable<T>
{
    private T _value = initial;
    internal bool TryChange(T newValue)
    {
        if (comparer.Compare(_value, newValue) >= 1)
        {
            _value = newValue;
            return true;
        }
        return false;
    }
}
