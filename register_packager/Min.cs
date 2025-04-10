namespace register_packager;

public struct Min<T>(T initial) where T : IComparable<T>
{
    public T Value { get; private set; } = initial;
    public bool TryChange(T newValue)
    {
        if (Value.CompareTo(newValue) >= 1)
        {
            Value = newValue;
            return true;
        }
        return false;
    }
}