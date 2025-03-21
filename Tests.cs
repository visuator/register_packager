using Xunit;
using Xunit.Abstractions;

namespace register_packager;

public class Tests(ITestOutputHelper testOutputHelper)
{
    [Theory]
    [InlineData(1, 10, 25)]
    [InlineData(1, 25, 45)]
    public void Sample1(params int[] addresses)
    {
        var registers = addresses.Select(x => new Register(x)).ToArray();
        var packages = Algorithm.Solve(registers, 16, out var debugPrintedTree);
        testOutputHelper.WriteLine(debugPrintedTree);
    }
}