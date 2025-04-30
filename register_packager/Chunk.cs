using System.Collections;

namespace register_packager;

internal readonly record struct ChunkPair(Chunk TrimLeft, Chunk JoinRight);
internal readonly struct Chunk(int[] registers) : IEnumerable<int>
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
    internal int CalculateDistance() => CalculateDistanceInternal(_registers);
    internal bool ExcessLimit(int maxLimit, out Chunk taken, out Chunk rest)
    {
        ArgumentOutOfRangeException.ThrowIfZero(_registers.Length);

        var span = _registers.AsSpan();
        if (CalculateDistanceInternal(span) > maxLimit)
        {
            var index = _registers.Length - 1;
            while (index >= 0)
            {
                if (CalculateDistanceInternal(span[..index]) <= maxLimit)
                {
                    break;
                }
                index--;
            }
            rest = _registers[index..].ToArray();
            taken = _registers[..index].ToArray();
            return true;
        }
        rest = [];
        taken = _registers.ToArray();
        return false;
    }
    internal static bool IsLegacy_CoilsCompatible(ReadOnlySpan<int> registers) => IsLegacy_CoilsCompatibleInternal(registers);
    internal List<ChunkPair> GetMinGarbageCandidates(ChunkPreparerOptions options, Chunk second, bool rearrange)
    {
        List<ChunkPair> buffer = new(_registers.Length);
        Min<int> min = new(CalculateDistanceInternal(_registers) + CalculateDistanceInternal(second._registers));
        ReadOnlySpan<int> concat = [.._registers, ..second._registers];
        for (var splitPoint = _registers.Length - 1; splitPoint >= 0; splitPoint--)
        {
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
            var garbage = CalculateDistanceInternal(trimLeft) + CalculateDistanceInternal(joinRight);
            if (min.TryChange(garbage) || trimLeft.Length == 0)
            {
                buffer.Add(new(trimLeft, joinRight));
            }
        }
        return buffer;
    }
    internal int Length => _registers.Length;
    internal int[] AsArray() => _registers;
}
