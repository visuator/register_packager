namespace register_packager;

public readonly record struct ChunkPair(Chunk TrimLeft, Chunk JoinRight);
public readonly struct Chunk(int[] registers)
{
    public static Chunk Empty => new([]);
    public static implicit operator Chunk(int[] registers) => new(registers);
    
    public int[] Registers { get; } = registers;

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
    public static int CalculateGarbage(ReadOnlySpan<int> registers) => CalculateGarbageInternal(registers);
    public int CalculateGarbage() => CalculateGarbageInternal(Registers);
    
    private static int CalculateDistanceInternal(ReadOnlySpan<int> registers) => registers.Length > 0 ? registers[^1] - registers[0] + 1 : 0;
    public static int CalculateDistance(ReadOnlySpan<int> registers) => CalculateDistanceInternal(registers);
    
    private static bool ExcessLimitInternal(int maxLimit, ReadOnlySpan<int> registers) => CalculateDistanceInternal(registers) > maxLimit;
    public static bool ExcessLimit(int maxLimit, ReadOnlySpan<int> registers) => ExcessLimitInternal(maxLimit, registers);
    public bool ExcessLimit(int maxLimit, out int[] taken, out int[] rest)
    {
        ArgumentOutOfRangeException.ThrowIfZero(Registers.Length);
        
        if (ExcessLimitInternal(maxLimit, Registers))
        {
            var index = Registers.Length - 1;
            while (index >= 0)
            {
                if (!ExcessLimitInternal(maxLimit, Registers.AsSpan()[..index]))
                {
                    break;
                }
                index--;
            }
            rest = Registers[index..].ToArray();
            taken = Registers[..index].ToArray();
            return true;
        }
        rest = [];
        taken = Registers.ToArray();
        return false;
    }

    private static bool IsLegacy_CoilsCompatibleInternal(ReadOnlySpan<int> registers)
    {
        var distance = CalculateDistance(registers); 
        return distance <= 256 || distance % 8 == 0;
    }

    private static bool IsLegacy_CoilsCompatible(ReadOnlySpan<int> registers1, ReadOnlySpan<int> registers2) => IsLegacy_CoilsCompatibleInternal(registers1) && IsLegacy_CoilsCompatibleInternal(registers2);
    public static bool IsLegacy_CoilsCompatible(ReadOnlySpan<int> registers) => IsLegacy_CoilsCompatibleInternal(registers);
    public List<ChunkPair> GetMinGarbageCandidates(ChunkPreparerOptions options, Chunk second)
    {
        List<ChunkPair> buffer = new(Registers.Length);
        Min<int> min = new(CalculateGarbage() + second.CalculateGarbage());
        ReadOnlySpan<int> concat = [..Registers, ..second.Registers];
        for (var splitPoint = Registers.Length - 1; splitPoint >= 0; splitPoint--)
        {
            var trimLeft = concat[..splitPoint];
            var joinRight = concat[splitPoint..];
            
            if (options.Legacy_CoilsCompatibility && !IsLegacy_CoilsCompatible(trimLeft, joinRight))
            {
                continue;
            }
            if (trimLeft.Length == 0 || min.TryChange(CalculateGarbageInternal(trimLeft) + CalculateGarbageInternal(joinRight)))
            {
                if (trimLeft.Contains(14999) || joinRight.Contains(14999))
                {
                    
                }
                buffer.Add(new(trimLeft.ToArray(), joinRight.ToArray()));
            }
        }
        return buffer;
    }
}