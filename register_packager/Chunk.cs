using System.Collections;

namespace register_packager;

internal readonly record struct ChunkPair(Chunk TrimLeft, Chunk JoinRight);
internal readonly struct Chunk(int[] registers) : IEnumerable<int>
{
    internal static Chunk Empty => new([]);
    public static implicit operator Chunk(int[] registers) => new(registers);

    private static int CalculateGarbageInternal(ReadOnlySpan<int> registers)
    {
        var garbage = 0;
        var index = 1;
        while (index < registers.Length)
        {
            garbage += registers[index] - registers[index - 1] - 1;
            index++;
        }
        return garbage;
    }

    private readonly int[] _registers = registers;
    public int this[int i] => _registers[i];
    internal static int CalculateGarbage(ReadOnlySpan<int> registers) => CalculateGarbageInternal(registers);
    internal int CalculateGarbage() => CalculateGarbageInternal(_registers);

    private static int CalculateDistanceInternal(ReadOnlySpan<int> registers) => registers.Length > 0 ? registers[^1] - registers[0] + 1 : 0;
    internal static int CalculateDistance(ReadOnlySpan<int> registers) => CalculateDistanceInternal(registers);

    private static bool ExcessLimitInternal(int maxLimit, ReadOnlySpan<int> registers) => CalculateDistanceInternal(registers) > maxLimit;
    internal static bool ExcessLimit(int maxLimit, ReadOnlySpan<int> registers) => ExcessLimitInternal(maxLimit, registers);
    internal bool ExcessLimit(int maxLimit, out int[] taken, out int[] rest)
    {
        ArgumentOutOfRangeException.ThrowIfZero(_registers.Length);

        if (ExcessLimitInternal(maxLimit, _registers))
        {
            var index = _registers.Length - 1;
            while (index >= 0)
            {
                if (!ExcessLimitInternal(maxLimit, _registers.AsSpan()[..index]))
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

    private static bool IsLegacy_CoilsCompatibleInternal(ReadOnlySpan<int> registers)
    {
        var distance = CalculateDistance(registers);
        return distance <= 256 || distance % 8 == 0;
    }

    private static bool IsLegacy_CoilsCompatible(ReadOnlySpan<int> registers1, ReadOnlySpan<int> registers2) => IsLegacy_CoilsCompatibleInternal(registers1) && IsLegacy_CoilsCompatibleInternal(registers2);
    internal static bool IsLegacy_CoilsCompatible(ReadOnlySpan<int> registers) => IsLegacy_CoilsCompatibleInternal(registers);
    internal List<ChunkPair> GetMinGarbageCandidates(ChunkPreparerOptions options, Chunk second)
    {
        List<ChunkPair> buffer = new(_registers.Length);
        Min<int> min = new(CalculateGarbage() + second.CalculateGarbage());
        ReadOnlySpan<int> concat = [.._registers, ..second._registers];
        for (var splitPoint = _registers.Length - 1; splitPoint >= 0; splitPoint--)
        {
            var trimLeft = concat[..splitPoint];
            var joinRight = concat[splitPoint..];

            if (options.Legacy_CoilsCompatibility && !IsLegacy_CoilsCompatible(trimLeft, joinRight))
            {
                continue;
            }
            if (trimLeft.Length == 0 || min.TryChange(CalculateGarbageInternal(trimLeft) + CalculateGarbageInternal(joinRight)))
            {
                buffer.Add(new(trimLeft.ToArray(), joinRight.ToArray()));
            }
        }
        return buffer;
    }
    public int Length => _registers.Length;
    public int[] ToArray() => _registers;
    public IEnumerator<int> GetEnumerator() => _registers.AsEnumerable().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
