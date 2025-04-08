using System.Security.Cryptography;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace register_packager;

public class Fixture
{
    private ITestOutputHelper _testOutputHelper = null!;
    public void Inject(ITestOutputHelper testOutputHelper) => _testOutputHelper = testOutputHelper;
    public int[][] Run(int maxLimit, int[] registers)
    {
        var result = Algorithm.Solve(maxLimit, registers);
        var greedy = Chunk(maxLimit, registers).ToArray();
        _testOutputHelper.WriteLine($"[{string.Join(", ", registers)}]");
        _testOutputHelper.WriteLine(string.Empty);
        _testOutputHelper.WriteLine($"[{string.Join(", ", greedy.Select(x => $"[{string.Join(", ", x)}]"))}] -> [Chunks: {greedy.Length}, Garbage: {greedy.Sum(CalculateGarbage)}]");
        _testOutputHelper.WriteLine(string.Empty);
        _testOutputHelper.WriteLine($"[{string.Join(", ", result.Select(x => $"[{string.Join(", ", x)}]"))}] -> [Chunks: {result.Length}, Garbage: {result.Sum(CalculateGarbage)}]");
        DefaultAsserts(maxLimit, registers, greedy, result);
        return result;
    }
    private static void DefaultAsserts(int maxLimit, int[] registers, int[][] greedyChunks, int[][] chunks)
    {
        chunks.Should().NotBeEmpty();
        
        var flattenChunks = chunks.SelectMany(x => x).ToArray();
        flattenChunks.Should().BeEquivalentTo(registers);
        
        chunks.Should().AllSatisfy(x => ExcessLimit(maxLimit, x).Should().BeFalse());
        chunks.Should().HaveCountLessThanOrEqualTo(greedyChunks.Length);
        chunks.Sum(CalculateGarbage).Should().BeLessThanOrEqualTo(greedyChunks.Sum(CalculateGarbage));
    }
    private static bool ExcessLimit(int maxLimit, int[] chunk) => chunk[^1] - chunk[0] + 1 > maxLimit;
    private static int CalculateGarbage(int[] chunk)
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
    private static IEnumerable<int[]> Chunk(int maxLimit, int[] registers)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxLimit);
        ArgumentOutOfRangeException.ThrowIfZero(registers.Length);
        var i = 0;
        var j = 0;
        var l = 1;
        var prev = registers[0];
        while (i < registers.Length)
        {
            var cur = registers[i];
            var d = cur - prev;
            l += d;
            if (l > maxLimit)
            {
                yield return registers[j..i];
                l = 1;
                j = i;
            }
            prev = cur;
            i++;
        }
        if (l != 0)
        {
            yield return registers[j..i];
        }
    }
}
public class Tests : IClassFixture<Fixture>
{
    private readonly Fixture _fixture;
    public Tests(ITestOutputHelper testOutputHelper, Fixture fixture)
    {
        _fixture = fixture;
        _fixture.Inject(testOutputHelper);
    }
    
    [Fact]
    public void Should_Pack_Two_Registers_Into_One_Package()
    {
        const int maxLimit = 10;
        int[] registers = [1, 10];

        var result = _fixture.Run(maxLimit, registers);
        result.Should().BeEquivalentTo((int[][]) [[1, 10]], "limit not exceeded");
    }
    
    [Fact]
    public void Should_Pack_Two_Registers_Into_Two_Package()
    {
        const int maxLimit = 5;
        int[] registers = [1, 10];

        var result = _fixture.Run(maxLimit, registers);
        result.Should().BeEquivalentTo((int[][]) [[1], [10]], "limit exceeded");
    }
    
    [Fact]
    public void Should_Choose_The_Best_Combination_With_Less_Garbage_1()
    {
        const int maxLimit = 15;
        int[] registers = [1, 15, 20];

        var result = _fixture.Run(maxLimit, registers);
        result.Should().BeEquivalentTo((int[][]) [[1], [15, 20]], "20 - 15 < 15 - 1");
    }
    
    [Fact]
    public void Should_Choose_The_Best_Combination_With_Less_Garbage_2()
    {
        const int maxLimit = 25;
        int[] registers = [1, 25, 45];

        var result = _fixture.Run(maxLimit, registers);
        result.Should().BeEquivalentTo((int[][]) [[1], [25, 45]], "45 - 25 < 25 - 1");
    }
    
    [Fact]
    public void Should_Choose_The_Best_Combination_With_Less_Garbage_3()
    {
        const int maxLimit = 5;
        int[] registers = [1, 4, 5, 8, 9, 40];

        var result = _fixture.Run(maxLimit, registers);
        result.Should().BeEquivalentTo((int[][]) [[1, 4, 5], [8, 9], [40]], "[[1, 4, 5], [8, 9] [40]] is optimal solution");
    }
    
    [Fact]
    public void Should_Join_Chunks_If_Possible_1()
    {
        const int maxLimit = 5;
        int[] registers = [1, 4, 5, 8, 9, 40];

        var result = _fixture.Run(maxLimit, registers);
        result.Should().BeEquivalentTo((int[][]) [[1, 4, 5], [8, 9], [40]], "[[1, 4, 5], [8, 9], [40]] is optimal solution");
    }
    
    [Fact]
    public void Should_Join_Chunks_If_Possible_2()
    {
        const int maxLimit = 4;
        int[] registers = [1, 4, 5, 8, 9, 40];

        var result = _fixture.Run(maxLimit, registers);
        result.Should().BeEquivalentTo((int[][]) [[1], [4, 5], [8, 9], [40]], "[[1], [4, 5], [8, 9], [40]] is optimal solution");
    }
    
    [Fact]
    public void Should_Join_Chunks_If_Possible_3()
    {
        const int maxLimit = 6;
        int[] registers = [1, 2, 3, 4, 5, 6];

        var result = _fixture.Run(maxLimit, registers);
        result.Should().BeEquivalentTo((int[][]) [[1, 2, 3, 4, 5, 6]], "[[1, 2, 3, 4, 5, 6]] is optimal solution");
    }
    
    [Fact]
    public void Should_Join_Chunks_If_Possible_4()
    {
        const int maxLimit = 25;
        int[] registers = [1, 15, 25];

        var result = _fixture.Run(maxLimit, registers);
        result.Should().BeEquivalentTo((int[][]) [[1, 15, 25]], "[[1, 15, 25]] is optimal solution");
    }

    //[[0], [4, 6, 7], [8, 9, 10, 11], [13, 14]]
    [Theory]
    [InlineData(256, 131_072)]
    public void Should_Handle_Large_Amount_Of_Registers_Better_Than_Straightforward_Greedy(int maxLimit, int count)
    {
        var registers = Enumerable.Range(0, count)
            .Select(_ => RandomNumberGenerator.GetInt32(0, count))
            .Distinct()
            .OrderBy(x => x)
            .ToArray();
        _fixture.Run(maxLimit, registers);
    }

    [Fact]
    public void Test_Case_1()
    {
        int[][] source = [[0], [4, 6, 7], [8, 9, 10, 11], [13, 14]];
        var registers = source.SelectMany(x => x).ToArray();
        _fixture.Run(4, registers);
    }
}