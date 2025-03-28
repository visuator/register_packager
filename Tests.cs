using Xunit;
using Xunit.Abstractions;

namespace register_packager;

public class Tests(ITestOutputHelper testOutputHelper)
{
    [Theory]
    [InlineData(8, new int[] { }, 8, 9, 12)]
    [InlineData(5, new int[] { }, 2, 3, 4, 5, 6)]
    [InlineData(17, new int[] { }, 1, 10, 25)]
    [InlineData(12, new int[] { }, 1, 10, 25)]
    [InlineData(26, new int[] { }, 1, 25, 45)]
    [InlineData(5,  new [] { 3 }, 1, 4, 5, 8, 9, 44)]
    [InlineData(4,  new [] { 3 }, 1, 4, 5, 8, 9, 44)]
    public void Sample1(int limit, int[] holes, params int[] addresses)
    {
        var chunks = Algorithm.Solve(addresses, holes.Select(x => (x, x)).ToArray(), limit);
        testOutputHelper.WriteLine($"{string.Join(", ", chunks.Select(x => $"[{string.Join(", ", x)}]"))}");
    }
    [Fact]
    public void Sample2()
    {
        var limit = 16;
        var addresses = Enumerable.Range(0, 1000).Select(x => Random.Shared.Next(0, 5000)).OrderBy(x => x).ToArray();
        var holes = Enumerable.Range(0, 30).Select(x => (Random.Shared.Next(0, 1000), Random.Shared.Next(1000, 5000))).OrderBy(x => x).ToArray();
        var chunks = Algorithm.Solve(addresses, holes, limit);
        testOutputHelper.WriteLine($"{string.Join(", ", chunks.Select(x => $"[{string.Join(", ", x)}]"))}");
    }
}