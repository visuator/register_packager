namespace register_packager;

public readonly record struct RegisterPair(Register Start, Register End)
{
    public static RegisterPair CreateSingle(Register register) => new(register, default);
    public RegisterPair CompareByGarbage(RegisterPair p1) => this == default ? p1 : CalculateGarbage() > p1.CalculateGarbage() ? p1 : this;
    private int CalculateGarbage() => End.Address - Start.Address - 1;
    public override string ToString() => $"({Start.Address}, {End.Address})";
}