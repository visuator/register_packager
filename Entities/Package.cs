namespace register_packager;

public readonly record struct Package(List<RegisterPair[]> Joins)
{
    public override string ToString() => $"{string.Join(", ", Joins.Select(x => $"[{string.Join("; ", x.Select(x => $"{x.Start.Address}-{x.End.Address}"))}]"))}";
}