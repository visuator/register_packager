using System.Collections;

namespace register_packager;

public readonly record struct ChunkPair(Chunk TrimLeft, Chunk JoinRight, int Garbage);
public readonly struct Chunk(int[] registers) : IEnumerable<int>
{
    private readonly int[] _registers = registers;

    public static implicit operator Chunk(int[] registers) => new(registers);
    public static implicit operator Chunk(ReadOnlySpan<int> registers) => new(registers.ToArray());
    internal static Chunk Empty => new([]);
    internal static int CalculateDistance(ReadOnlySpan<int> registers) => registers.Length > 0 ? registers[^1] - registers[0] + 1 : 0;
    internal static int CalculateGarbage(ReadOnlySpan<int> registers)
    {
        /*var garbage = 0;
        var index = 1;
        while (index < registers.Length)
        {
            garbage += registers[index] - registers[index - 1] - 1;
            index++;
        }
        return garbage;*/
        return CalculateDistance(registers);
    }
    internal static bool IsLegacy_CoilsCompatible(int distance) => distance <= 256 || distance % 8 == 0;
    internal static bool IsLegacy_CoilsCompatible(ReadOnlySpan<int> registers) => IsLegacy_CoilsCompatible(CalculateDistance(registers));

    public int this[int index] => _registers[index];
    public int[] this[Range range] => _registers[range];
    public override string ToString() => $"[{string.Join(", ", _registers)}]";
    public IEnumerator<int> GetEnumerator() => _registers.AsEnumerable().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal (int GarbageInitial, List<ChunkPair>) GetMinGarbageCandidates(ChunkPreparerOptions options, Chunk second, bool rearrange)
    {
        List<ChunkPair> buffer = new(_registers.Length);
        var garbageInitial = CalculateGarbage(_registers) + CalculateGarbage(second._registers);
        Min<int> min = new(garbageInitial);
        ReadOnlySpan<int> concat = [.._registers, ..second._registers];
        for (var splitPoint = _registers.Length - 1; splitPoint >= 0; splitPoint--)
        {
            //todo: if current = follow exclude this combination
            //todo: save chunk distance info
            var trimLeft = concat[..splitPoint];
            var joinRight = concat[splitPoint..];

            var distance = CalculateDistance(joinRight);
            if (rearrange && distance > options.MaxLimit)
            {
                break;
            }
            if (options.Legacy_CoilsCompatibility && !(IsLegacy_CoilsCompatible(trimLeft) && IsLegacy_CoilsCompatible(distance)))
            {
                continue;
            }
            var garbage = CalculateGarbage(trimLeft) + CalculateGarbage(joinRight);
            if (min.TryChange(garbage) || trimLeft.Length == 0)
            {
                buffer.Add(new(trimLeft, joinRight, garbage));
            }
        }
        return (garbageInitial, buffer);
    }
    internal int Length { get; } = registers.Length;
    internal int Distance { get; } = CalculateDistance(registers);
}
