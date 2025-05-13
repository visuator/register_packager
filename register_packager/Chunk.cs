using System.Collections;

namespace register_packager;

public readonly record struct ChunkPair(Chunk TrimLeft, Chunk JoinRight);
public readonly struct Chunk(int[] registers) : IEnumerable<int>
{
    private readonly int[] _registers = registers;

    public static implicit operator Chunk(int[] registers) => new(registers);
    public static implicit operator Chunk(ReadOnlySpan<int> registers) => new(registers.ToArray());
    internal static Chunk Empty => new([]);
    internal static int CalculateDistance(ReadOnlySpan<int> registers) => registers.Length > 0 ? registers[^1] - registers[0] + 1 : 0;
    internal static int CalculateGarbage(ReadOnlySpan<int> registers) => registers.Length > 0 ? registers[^1] - registers[0] - 1 - (registers.Length - 2) : 0;
    internal static bool IsLegacy_CoilsCompatible(int distance) => distance <= 256 || distance % 8 == 0;
    internal static bool IsLegacy_CoilsCompatible(ReadOnlySpan<int> registers) => IsLegacy_CoilsCompatible(CalculateDistance(registers));

    public int this[int index] => _registers[index];
    public int[] this[Range range] => _registers[range];
    public override string ToString() => $"[{string.Join(", ", _registers)}]";
    public IEnumerator<int> GetEnumerator() => _registers.AsEnumerable().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    internal int Length { get; } = registers.Length;
    internal int Distance { get; } = CalculateDistance(registers);
    internal int Garbage { get; } = CalculateGarbage(registers);
    internal List<ChunkPair> GetMinGarbageCandidates(ChunkPreparerOptions options, Chunk second, bool rearrange)
    {
        List<ChunkPair> buffer = new(_registers.Length);
        Min<int> min = new(Garbage + second.Garbage);
        ReadOnlySpan<int> concat = [.._registers, ..second._registers];
        for (var splitPoint = _registers.Length - 1; splitPoint >= 0; splitPoint--)
        {
            var trimLeft = concat[..splitPoint];
            var joinRight = concat[splitPoint..];

            var distance = CalculateDistance(joinRight);

            if (rearrange && distance > options.MaxLimit)
            {
                break;
            }
            if (!options.ChunkOptions.IsLegacy_CoilsCompatible(trimLeft) || !options.ChunkOptions.IsLegacy_CoilsCompatible(distance))
            {
                continue;
            }

            var garbage = CalculateGarbage(trimLeft) + CalculateGarbage(joinRight);
            if (min.TryChange(garbage) || trimLeft.Length == 0)
            {
                buffer.Add(new(trimLeft, joinRight));
            }
        }
        return buffer;
    }

    internal List<ChunkPair> GetMinGarbageCandidates(ChunkPreparerOptions options, bool rearrange)
    {
        var left = 0;
        var right = _registers.Length;
        var result = 0;

        while (left <= right)
        {
            var mid = (left + right) / 2;
            if (CalculateDistance(_registers.AsSpan()[..mid]) <= options.MaxLimit)
            {
                result = mid;
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }
        return new Chunk(_registers[..result]).GetMinGarbageCandidates(options, _registers[result..], rearrange);
    }
}
