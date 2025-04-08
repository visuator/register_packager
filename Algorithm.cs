namespace register_packager;

public class Algorithm
{
    public static int[][] Solve(int max, int[] regs)
    {
        return JoinRecursive(max, Chunk(max, regs).ToArray(), 0, true).Where(x => x.Length != 0).ToArray();
    }

    private static int[][] JoinRecursive(int max, int[][] chs, int i, bool canCreate)
    {
        ArgumentOutOfRangeException.ThrowIfZero(chs.Length);
        while (i < chs.Length)
        {
            var cur = chs[i];
            if (i + 1 < chs.Length)
            {
                var fl = chs[i + 1];
                ArgumentOutOfRangeException.ThrowIfZero(fl.Length);
                var g = CalculateGarbage(cur) + CalculateGarbage(fl);
                PriorityQueue<(int[] TrimLeft, int[] JoinRight), int> pq = new();
                foreach (var (tl, jr, g2) in Combine(g, cur, fl))
                {
                    pq.Enqueue((tl, jr), g2);
                }
                PriorityQueue<(int[][] Chunks, bool Inline), int> pq2 = new();
                while (pq.Count != 0)
                {
                    var (tl, jr) = pq.Dequeue();
                    if (tl.Length == 0 || CalculateGarbage(tl) + CalculateGarbage(jr) < g)
                    {
                        if (tl.Length != 0 && ExcessLimit(max, jr, out var taken, out var rest))
                        {
                            if (!canCreate)
                            {
                                continue;
                            }
                            var next = JoinRecursive(max, [..chs[..i], tl, taken, rest, ..chs[(i + 2)..]], i + 2, false);
                            if (next.Length <= chs.Length)
                            {
                                pq2.Enqueue((next, false), CalculateHeightWithGarbage(max, next));
                            }
                        }
                        if (!ExcessLimit(max, jr))
                        {
                            int[][] join = tl.Length == 0 ? [jr] : [tl, [..jr]];
                            int[][] k = [..chs[..i], ..join, ..chs[(i + 2)..]];
                            pq2.Enqueue((k, true), CalculateHeightWithGarbage(max, k));
                        }
                    }
                }
                if (pq2.Count != 0)
                {
                    (chs, var inline) = pq2.Dequeue();
                    if (inline)
                    {
                        i++;
                    }
                }
            }
            i++;
        }
        return chs;
    }
    
    public static int CalculateHeightWithGarbage(int max, int[][] chunks) => GetNumberWithZeros(max) + chunks.Sum(CalculateGarbage);
    
    private static int GetNumberWithZeros(int x) => (int)Math.Pow(10, (int)Math.Floor(Math.Log10(x)) + 1);
    
    public static bool ExcessLimit(int max, int[] ch, out int[] taken, out int[] rest)
    {
        ArgumentOutOfRangeException.ThrowIfZero(ch.Length);
        if (ExcessLimit(max, ch))
        {
            var i = ch.Length - 1;
            while (i >= 0)
            {
                if (!ExcessLimit(max, ch[..i]))
                {
                    break;
                }
                i--;
            }
            rest = ch[i..];
            taken = ch[..i];
            return true;
        }
        rest = [];
        taken = ch;
        return false;
    }
    
    public static bool ExcessLimit(int max, int[] chunk) => chunk[^1] - chunk[0] + 1 > max;
    
    public static int CalculateGarbage(int[] chunk)
    {
        if (chunk.Length == 0)
        {
            return 0;
        }
        var i = 0;
        var g = 0;
        var prev = chunk[0];
        while (i < chunk.Length)
        {
            var cur = chunk[i];
            g += Math.Max(0, cur - prev - 1);
            prev = cur;
            i++;
        }
        return g;
    }
    
    public static IEnumerable<(int[] TrimLeft, int[] JoinRight, int Garbage)> Combine(int g, int[] ch1, int[] ch2)
    {
        var arr = ch1.Concat(ch2).ToArray();
        for (var splitPoint = ch1.Length - 1; splitPoint >= 0; splitPoint--)
        {
            var trimLeft = arr.Take(splitPoint).ToArray();
            var joinRight = arr.Skip(splitPoint).ToArray();
            var garbage = CalculateGarbage(trimLeft) + CalculateGarbage(joinRight); 
            if (garbage < g || trimLeft.Length == 0)
            {
                yield return (trimLeft, joinRight, garbage);
            }
        }
    }
    
    public static IEnumerable<int[]> Chunk(int max, int[] regs)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(max);
        ArgumentOutOfRangeException.ThrowIfZero(regs.Length);
        var i = 0;
        var j = 0;
        var l = 1;
        var prev = regs[0];
        while (i < regs.Length)
        {
            var cur = regs[i];
            var d = cur - prev;
            l += d;
            if (l > max)
            {
                yield return regs[j..i];
                l = 1;
                j = i;
            }
            prev = cur;
            i++;
        }
        if (l != 0)
        {
            yield return regs[j..i];
        }
    }
}