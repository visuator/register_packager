namespace register_packager;

public readonly record struct Package(List<RegisterPair> Registers)
{
    public override string ToString() => $"{string.Join(", ", Registers.Select(x => x.ToString()))}";
}