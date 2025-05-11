namespace register_packager;

internal struct Min<T>(T initial) where T : IComparable<T>
{
    internal T Value { get; private set; } = initial;
    internal bool TryChange(T newValue)
    {
        if (Value.CompareTo(newValue) >= 1)
        {
            Value = newValue;
            return true;
        }
        return false;
    }
}
