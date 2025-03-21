using Xunit;
using Xunit.Abstractions;

namespace register_packager;

public class Tests(ITestOutputHelper testOutputHelper)
{
    //1 4-5 8-9 44
    [Theory]
    [InlineData(8, new int[] { }, 8, 9, 12)]
    [InlineData(20, new int[] { })]
    [InlineData(5, new int[] { }, 2, 3, 4, 5, 6)]
    [InlineData(17, new int[] { }, 1, 10, 25)]
    [InlineData(12, new int[] { }, 1, 10, 25)]
    [InlineData(26, new int[] { }, 1, 25, 45)]
    [InlineData(4,  new [] { 3 }, 1, 4, 5, 8, 9, 44)]
    public void Sample1(int limit, int[] holes, params int[] addresses)
    {
        var chunks = Algorithm.Solve(addresses, holes.Select(x => (x, x)).ToArray(), limit);
        testOutputHelper.WriteLine($"{string.Join(", ", chunks.Select(x => $"[{string.Join(", ", x)}]"))}");
    }
}