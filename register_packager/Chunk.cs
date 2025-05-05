using System.Collections;
using System.Text;

namespace register_packager;

public readonly record struct ChunkPair(Chunk TrimLeft, Chunk JoinRight, int Garbage);
public readonly struct Chunk(int[] registers) : IEnumerable<int>
{
    private readonly int[] _registers = registers;

    private static int CalculateDistanceInternal(ReadOnlySpan<int> registers) => registers.Length > 0 ? registers[^1] - registers[0] + 1 : 0;
    private static bool IsLegacy_CoilsCompatibleInternal(ReadOnlySpan<int> registers)
    {
        var distance = CalculateDistanceInternal(registers);
        return distance <= 256 || distance % 8 == 0;
    }

    public static implicit operator Chunk(int[] registers) => new(registers);
    public static implicit operator Chunk(ReadOnlySpan<int> registers) => new(registers.ToArray());
    public IEnumerator<int> GetEnumerator() => _registers.AsEnumerable().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    internal static Chunk Empty => new([]);
    internal int CalculateGarbage() => CalculateGarbageInternal(_registers);
    internal static int CalculateGarbageInternal(ReadOnlySpan<int> registers)
    {
        /*var garbage = 0;
        var index = 1;
        while (index < registers.Length)
        {
            garbage += registers[index] - registers[index - 1] - 1;
            index++;
        }
        return garbage;*/
        return CalculateDistanceInternal(registers);
    }
    internal bool ExcessLimit(int maxLimit) => CalculateDistanceInternal(_registers) > maxLimit;
    internal static bool IsLegacy_CoilsCompatible(ReadOnlySpan<int> registers) => IsLegacy_CoilsCompatibleInternal(registers);
    internal (int GarbageInitial, List<ChunkPair>) GetMinGarbageCandidates(ChunkPreparerOptions options, Chunk second, bool rearrange)
    {
        List<ChunkPair> buffer = new(_registers.Length);
        var garbageInitial = CalculateGarbageInternal(_registers) + CalculateGarbageInternal(second._registers);
        Min<int> min = new(garbageInitial);
        ReadOnlySpan<int> concat = [.._registers, ..second._registers];
        for (var splitPoint = _registers.Length - 1; splitPoint >= 0; splitPoint--)
        {
            //todo: if current = follow exclude this combination
            //todo: save chunk distance info
            var trimLeft = concat[..splitPoint];
            var joinRight = concat[splitPoint..];

            if (rearrange && CalculateDistanceInternal(joinRight) > options.MaxLimit)
            {
                break;
            }
            if (options.Legacy_CoilsCompatibility && !(IsLegacy_CoilsCompatible(trimLeft) && IsLegacy_CoilsCompatible(joinRight)))
            {
                continue;
            }
            var garbage = CalculateGarbageInternal(trimLeft) + CalculateGarbageInternal(joinRight);
            if (min.TryChange(garbage) || trimLeft.Length == 0)
            {
                buffer.Add(new(trimLeft, joinRight, garbage));
            }
        }
        return (garbageInitial, buffer);
    }
    internal int Length => _registers.Length;
    internal int[] AsArray() => _registers;
    public override string ToString() => $"[{string.Join(", ", _registers)}]";
}
