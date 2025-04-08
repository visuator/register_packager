namespace register_packager;

public class Algorithm
{
    public static int[][] Solve(int max, int[] regs)
    {
        return JoinRecursive(max, Chunk(max, regs).ToArray(), 0, false).ToArray();
    }

    private static int[][] JoinRecursive(int maxLimit, int[][] chunks, int index, bool rearrange)
    {
        ArgumentOutOfRangeException.ThrowIfZero(chunks.Length);
        
        while (index < chunks.Length)
        {
            var current = chunks[index];
            if (index + 1 < chunks.Length)
            {
                var follow = chunks[index + 1];
                ArgumentOutOfRangeException.ThrowIfZero(follow.Length);

                var min = -1;
                var prefer = chunks;
                foreach (var (trimLeft, joinRight) in CombineWithLowerGarbageThanSource(current, follow))
                {
                    if (trimLeft.Length != 0 && ExcessLimit(maxLimit, joinRight, out var taken, out var rest))
                    {
                        if (rearrange)
                        {
                            continue;
                        }
                        var next = JoinRecursive(maxLimit, InjectChunks(chunks, index, out var startIndex, trimLeft, taken, rest), startIndex, true);
                        if (next.Length <= chunks.Length)
                        {
                            var g = next.Sum(x => CalculateGarbage(x));
                            if (g < min || min == -1)
                            {
                                min = g;
                                prefer = next;
                            }
                        }
                    }
                    if (!ExcessLimit(maxLimit, joinRight))
                    {
                        var newChunks = InjectChunks(chunks, index, out _, trimLeft, joinRight);
                        var g = newChunks.Sum(x => CalculateGarbage(x));
                        if (g < min || min == -1)
                        {
                            min = g;
                            prefer = newChunks;
                        }
                    }
                }
                chunks = prefer;
            }
            else
            {
                return chunks;
            }
            index++;
        }
        return chunks;
    }

    private static int[][] InjectChunks(int[][] source, int index, out int startIndex, params int[][] chunks)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(chunks.Length, 2);
        
        startIndex = index + Math.Max(2, chunks.Length - 1);
        return [..source[..index], ..chunks.Where(x => x.Length != 0), ..source[startIndex..]];
    }
    
    private static bool ExcessLimit(int maxLimit, ReadOnlySpan<int> chunk, out int[] taken, out int[] rest)
    {
        ArgumentOutOfRangeException.ThrowIfZero(chunk.Length);
        
        if (ExcessLimit(maxLimit, chunk))
        {
            var index = chunk.Length - 1;
            while (index >= 0)
            {
                if (!ExcessLimit(maxLimit, chunk[..index]))
                {
                    break;
                }
                index--;
            }
            rest = chunk[index..].ToArray();
            taken = chunk[..index].ToArray();
            return true;
        }
        rest = [];
        taken = chunk.ToArray();
        return false;
    }
    
    private static bool ExcessLimit(int maxLimit, ReadOnlySpan<int> chunk) => chunk[^1] - chunk[0] + 1 > maxLimit;
    
    private static int CalculateGarbage(ReadOnlySpan<int> chunk1, ReadOnlySpan<int> chunk2) => chunk1.Length == 0 ? CalculateGarbage(chunk2) : CalculateGarbage(chunk1) + CalculateGarbage(chunk2);
    private static int CalculateGarbage(ReadOnlySpan<int> chunk)
    {
        ArgumentOutOfRangeException.ThrowIfZero(chunk.Length);
        
        var index = 0;
        var garbage = 0;
        var previous = chunk[0];
        while (index < chunk.Length)
        {
            var current = chunk[index];
            garbage += Math.Max(0, current - previous - 1);
            previous = current;
            index++;
        }
        return garbage;
    }
    
    private static (int[] TrimLeft, int[] JoinRight)[] CombineWithLowerGarbageThanSource(ReadOnlySpan<int> chunk1, ReadOnlySpan<int> chunk2)
    {
        List<(int[] TrimLeft, int[] JoinRight)> res = [];
        var min = -1;
        var maxGarbage = CalculateGarbage(chunk1, chunk2);
        ReadOnlySpan<int> concat = [..chunk1, ..chunk2];
        for (var splitPoint = chunk1.Length - 1; splitPoint >= 0; splitPoint--)
        {
            var trimLeft = concat[..splitPoint];
            var joinRight = concat[splitPoint..];
            var garbage = CalculateGarbage(trimLeft, joinRight);
            if (garbage < maxGarbage || trimLeft.Length == 0)
            {
                if (min == -1)
                {
                    min = garbage;
                    res.Add((trimLeft.ToArray(), joinRight.ToArray()));
                }
                if (garbage < min)
                {
                    min = garbage;
                    res.Add((trimLeft.ToArray(), joinRight.ToArray()));
                }
            }
        }
        return res.ToArray();
    }
    
    private static IEnumerable<int[]> Chunk(int maxLimit, int[] registers)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxLimit);
        ArgumentOutOfRangeException.ThrowIfZero(registers.Length);
        
        var index = 0;
        var previous = registers[0];
        var chunkStart = 0;
        var currentLimit = 1;
        while (index < registers.Length)
        {
            var current = registers[index];
            var distance = current - previous;
            currentLimit += distance;
            if (currentLimit > maxLimit)
            {
                yield return registers[chunkStart..index];
                currentLimit = 1;
                chunkStart = index;
            }
            previous = current;
            index++;
        }
        if (currentLimit != 0)
        {
            yield return registers[chunkStart..index];
        }
    }
}