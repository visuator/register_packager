// r: required, p: package, g: garbage, ml: max length
// priority = count(p) > sum(g)
// distance = r_end - r_start + 1
// garbage = required_end - required_start - 1

// 1 8 9 12 ml: 8
// ok(1, 8-12)
// 1:r 10:r 21:r ml:12
// ok(1-10:p 21:p -> 9:g), bad(1-1:p 10-21:p -> 10:g)
// 1:r 10:r 25:r ml:12
// ok(1-10:p 25:p -> 9:g), bad(<empty>)
// 1:r 10:r 25:r ml:16
// ok(1-10:p 25:p -> 9:g), bad(1:p 10-25:p -> 14:g)
// 1:r 10:r 25:r ml:26
// ok(1-25:p -> 14:g), bad(<empty>)
// 1:r 25:r 45:r ml:26
// ok(1:p 25-45:p -> 19:g), bad(1-25:p 45:p -> 23:g)

namespace register_packager;

public class Algorithm
{
    public static int[][] Solve(int[] vertices, (int Start, int End)[] holes, int limit)
    {
        var slices = Slice(limit, holes, vertices, false);
        return slices.Combination.Select(x => x.Chunk).ToArray();
    }
    private static bool BinarySearch((int Start, int End)[] array, int searchedValue, int first, int last)
    {
        if (first > last)
        {
            return false;
        }
        var middle = (first + last) / 2;
        var middleValue = array[middle];
        if (searchedValue >= middleValue.Start && searchedValue <= middleValue.End)
        {
            return true;
        }
        if (middleValue.End > searchedValue)
        {
            return BinarySearch(array, searchedValue, first, middle - 1);
        }
        return BinarySearch(array, searchedValue, middle + 1, last);
    }
    private static IEnumerable<int[]> BreakByHoles((int Start, int End)[] holes, int[] registers)
    {
        var previous = registers[0];
        var current = previous;
        var i = 0;
        List<int> taken = [];
        while (i < registers.Length)
        {
            for (var j = previous; j <= current; j++)
            {
                if (BinarySearch(holes, j, 0, holes.Length - 1))
                {
                    yield return taken.ToArray();
                    taken = [];
                    break;
                }
                if (j == current)
                {
                    taken.Add(registers[i]);
                }
            }
            i++;
            previous = current;
            current = registers[Math.Min(i, registers.Length - 1)];
        }
        if (taken.Count != 0)
        {
            yield return taken.ToArray();
        }
    }
    private record struct ChunkInfo(int[] Chunk, int Distance);
    private record struct CombinationInfo(ChunkInfo[] Combination, int Distance);
    private static CombinationInfo Slice(int limit, (int Start, int End)[] holes, int[] registers, bool split)
    {
        // memoize!!!
        List<int[][]> res = [];
        var m = BreakByHoles(holes, registers).ToArray();
        if (m.Length > 0)
        {
            var c1 = m[0];
            if (1 < m.Length)
            {
                for (var j = 1; j < m.Length; j++)
                {
                    var c2 = m[j];
                    for (var k = 0; k < c2.Length; k++)
                    {
                        res.Add([c1, c2[..k], c2[k..]]);
                    }
                }
            }
            else
            {
                for (var k = 0; k < c1.Length; k++)
                {
                    res.Add([c1[..k], c1[k..]]);
                }
            }
        }
        List<CombinationInfo> _t = [];
        var combinationInfos = res.Select(x => new CombinationInfo(x.Select(x => new ChunkInfo(x, CalculateDistance(x))).ToArray(), x.Sum(x => CalculateDistance(x)))).ToArray().ToArray();
        var min1 = combinationInfos.Min(x => x.Distance);
        foreach (var ci in combinationInfos.Where(x => x.Distance == min1))
        {
            List<ChunkInfo> newChunks = [];
            var s = 0;
            foreach (var c in ci.Combination)
            {
                if (c.Distance > limit)
                {
                    var newCombination = Slice(limit, holes, c.Chunk, true);
                    s += newCombination.Distance;
                    newChunks.AddRange(newCombination.Combination);
                }
                else
                {
                    s += c.Distance;
                    newChunks.Add(c);
                }
            }
            _t.Add(new CombinationInfo(newChunks.ToArray(), s));
        }
        return _t.MinBy(x => x.Distance);
    }
    private static int CalculateDistance(int[] chunk) => chunk.Length == 0 ? 0 : chunk[^1] - chunk[0] + 1;
}