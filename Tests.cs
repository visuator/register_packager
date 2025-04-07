using Xunit;
using Xunit.Abstractions;

namespace register_packager;

public class Tests(ITestOutputHelper testOutputHelper)
{
    [Theory]
    [InlineData(8,  8, 9, 12)]
    [InlineData(5,  2, 3, 4, 5, 6)]
    [InlineData(17,  1, 10, 25)]
    [InlineData(12,  1, 10, 25)]
    [InlineData(26,  1, 25, 45)]
    [InlineData(5,  1, 4, 5, 8, 9, 44)]
    [InlineData(4,  1, 4, 5, 8, 9, 44)]
    [InlineData(10,  1, 10)]
    public void Sample1(int maxLimit, params int[] registers)
    {
        var o = Algorithm.Solve(maxLimit, registers);
        testOutputHelper.WriteLine($"[{string.Join(", ", o.Select(x => $"[{string.Join(", ", x)}]"))}]");
    }
    [Fact]
    public void Sample2()
    {
        int[][] reg = [[2, 3, 6, 8, 15], [19, 20, 26, 27, 30], [41, 52, 53], [59, 60, 69, 75], [76, 77, 85, 90], [97, 99]];
        int[][] expected = [[2, 3, 6, 8], [15, 19, 20, 26, 27, 30], [41], [52, 53, 59, 60], [69, 75, 76, 77], [85, 90, 97, 99]];
        var chunks = Algorithm.Chunk(16, reg.SelectMany(x => x).ToArray()).ToArray();
        testOutputHelper.WriteLine($"{string.Join(", ", chunks.Select(x => $"[{string.Join(", ", x)}]"))} -> [Chunks = {chunks.Length} Garbage = {chunks.Sum(Algorithm.CalculateGarbage)}]");
        var o = Algorithm.Solve(16, reg.SelectMany(x => x).ToArray()).ToArray();
        testOutputHelper.WriteLine($"{string.Join(", ", expected.Select(x => $"[{string.Join(", ", x)}]"))} -> [Chunks = {expected.Length} Garbage = {expected.Sum(Algorithm.CalculateGarbage)}]");
        testOutputHelper.WriteLine($"{string.Join(", ", o.Select(x => $"[{string.Join(", ", x)}]"))} -> [Chunks = {o.Length} Garbage = {o.Sum(Algorithm.CalculateGarbage)}]");
        Assert.Equal(reg.SelectMany(x => x), o.SelectMany(x => x));
        Assert.True(o.Length <= expected.Length);
        Assert.All(o, x => Assert.False(Algorithm.ExcessLimit(16, x, out _, out _)));
        Assert.True(o.Sum(Algorithm.CalculateGarbage) <= expected.Sum(Algorithm.CalculateGarbage));
    }
    [Theory]
    [InlineData(4, 100_00)]
    [InlineData(256, 10_000)]
    [InlineData(256, 100_000)]
    public void SampleMax(int max, int count)
    {
        var reg = Enumerable.Range(0, count)
            .Select(x => Random.Shared.Next(0, count))
            .Distinct()
            .OrderBy(x => x)
            .ToArray();
        var chunks = Algorithm.Chunk(max, reg).ToArray();
        testOutputHelper.WriteLine($"{string.Join(", ", chunks.Select(x => $"[{string.Join(", ", x)}]"))} -> [Chunks = {chunks.Length} Garbage = {chunks.Sum(Algorithm.CalculateGarbage)}]");
        var o = Algorithm.Solve(max, reg).ToArray();
        testOutputHelper.WriteLine($"{string.Join(", ", o.Select(x => $"[{string.Join(", ", x)}]"))} -> [Chunks = {o.Length} Garbage = {o.Sum(Algorithm.CalculateGarbage)}]");
        Assert.Equal(reg, o.SelectMany(x => x));
        Assert.All(o, x => Assert.False(Algorithm.ExcessLimit(max, x, out _, out _)));
        Assert.True(o.Length <= chunks.Length);
        Assert.True(o.Sum(Algorithm.CalculateGarbage) < chunks.Sum(Algorithm.CalculateGarbage));
    }
    [Theory]
    [InlineData(13, 1, 3, 5, 9, 10, 11, 13)]
    [InlineData(4, 1, 3, 5, 9, 10, 11, 13)]
    [InlineData(1, 1, 3, 5, 9, 10, 11, 13)]
    [InlineData(8,  8, 9, 12)]
    [InlineData(5,  2, 3, 4, 5, 6)]
    [InlineData(17,  1, 10, 25)]
    [InlineData(12,  1, 10, 25)]
    [InlineData(26,  1, 25, 45)]
    [InlineData(5,  1, 4, 5, 8, 9, 44)]
    [InlineData(4,  1, 4, 5, 8, 9, 44)]
    [InlineData(10,  1, 10)]
    public void Chunk(int maxLimit, params int[] registers)
    {
        var variation = Algorithm.Chunk(maxLimit, registers).ToArray();
        testOutputHelper.WriteLine(string.Join(", ", variation.Select(x => $"[{string.Join(", ", x)}]")));
    }
    [Fact]
    public void Combine()
    {
        int[] ch1 = [1, 2, 3, 4, 5];
        int[] ch2 = [6, 7, 8, 9, 10];
        foreach (var (trimLeft, joinRight) in Algorithm.Combine(ch1, ch2))
        {
            testOutputHelper.WriteLine($"[{string.Join(", ", trimLeft)}], [{string.Join(", ", joinRight)}]");
        }
    }
    [Fact]
    public void Combine2()
    {
        int[] ch1 = [8];
        int[] ch2 = [9];
        foreach (var (trimLeft, joinRight) in Algorithm.Combine(ch1, ch2))
        {
            testOutputHelper.WriteLine($"[{string.Join(", ", trimLeft)}], [{string.Join(", ", joinRight)}]");
        }
    }
}