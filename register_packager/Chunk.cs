namespace register_packager;

public readonly record struct ChunkPair(Chunk TrimLeft, Chunk JoinRight);
public readonly struct Chunk(int[] registers)
{
    public static Chunk Empty => new([]);
    public static implicit operator Chunk(int[] registers) => new(registers);
    
    public int[] Registers { get; } = registers;

    public int CalculateGarbage()
    {
        var garbage = 0;
        var index = 1;
        while (index < Registers.Length)
        {
            garbage += Registers[index] - Registers[index - 1] - 1;
            index++;
        }
        return garbage;
    }

    private static bool ExcessLimitInternal(int maxLimit, ReadOnlySpan<int> registers) => registers[^1] - registers[0] + 1 > maxLimit;
    public bool ExcessLimit(int maxLimit) => ExcessLimitInternal(maxLimit, Registers);
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
    
    public IEnumerable<ChunkPair> GetMinGarbageCandidates(Chunk second)
    {
        Min<int> min = new(CalculateGarbage() + second.CalculateGarbage());
        int[] concat = [..Registers, ..second.Registers];
        for (var splitPoint = Registers.Length - 1; splitPoint >= 0; splitPoint--)
        {
            Chunk trimLeft = concat[..splitPoint];
            Chunk joinRight = concat[splitPoint..];
            if (trimLeft.Registers.Length == 0 || min.TryChange(trimLeft.CalculateGarbage() + joinRight.CalculateGarbage()))
            {
                yield return new(trimLeft, joinRight);
            }
        }
    }
}