using System.Security.Cryptography;
using FluentAssertions;
using register_packager;
using Xunit;
using Xunit.Abstractions;

namespace register_packager_tests;

public class Fixture
{
    private ITestOutputHelper _testOutputHelper = null!;
    public void Inject(ITestOutputHelper testOutputHelper) => _testOutputHelper = testOutputHelper;
    public int[][] Run(int maxLimit, bool legacy_coilsCompatibility, int[] registers)
    {
        var options = new ChunkPreparerOptions() { Legacy_CoilsCompatibility = legacy_coilsCompatibility, MaxLimit = maxLimit, ReadOnlyMode = true };
        var preparer = new ChunkNodePreparer(options);
        var packager = new ChunkPackager(options);
        var result = packager.Package(registers);
        var greedy = preparer.Prepare(registers).Head.GetChunks().ToArray();

        //File.Delete("registers.txt");
        //File.WriteAllText("registers.txt", string.Join(", ", registers));
        //_testOutputHelper.WriteLine($"[{string.Join(", ", registers)}]");
        //_testOutputHelper.WriteLine(string.Empty);
        //_testOutputHelper.WriteLine($"[{string.Join(", ", greedy.Select(x => $"[{string.Join(", ", x.Registers)}]"))}] -> [Chunks: {greedy.Length}, Garbage: {greedy.Sum(x => x.CalculateGarbage())}]");
        //_testOutputHelper.WriteLine(string.Empty);
        _testOutputHelper.WriteLine($"[{string.Join(", ", result.Select(x => $"[{string.Join(", ", x)}]"))}] -> [Chunks: {result.Length}, Garbage: {result.Sum(x => CalculateGarbage(x))}]");
        DefaultAsserts(options, registers, greedy, result);
        return result;
    }
    private static void DefaultAsserts(ChunkPreparerOptions options, int[] registers, Chunk[] greedyChunks, int[][] chunks)
    {
        chunks.Should().NotBeEmpty();
        
        var flattenChunks = chunks.SelectMany(x => x).ToArray();
        flattenChunks.Should().BeEquivalentTo(registers);

        chunks.Should().AllSatisfy(x => CalculateDistance(x).Should().BeLessThanOrEqualTo(options.MaxLimit));
        chunks.Should().HaveCountLessThanOrEqualTo(greedyChunks.Length);

        if (flattenChunks.Length == greedyChunks.Length)
        {
            chunks.Sum(x => CalculateGarbage(x)).Should().BeLessThanOrEqualTo(greedyChunks.Sum(x => x.CalculateGarbage()));
        }

        if (options.Legacy_CoilsCompatibility)
        {
            chunks.Should().AllSatisfy(x => IsLegacy_CoilsCompatible(x).Should().BeTrue());
        }
    }
    private static int CalculateDistance(ReadOnlySpan<int> registers) => registers.Length > 0 ? registers[^1] - registers[0] + 1 : 0;
    private static int CalculateGarbage(ReadOnlySpan<int> registers)
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
    private static bool IsLegacy_CoilsCompatible(ReadOnlySpan<int> registers)
    {
        var distance = CalculateDistance(registers);
        return distance <= 256 || distance % 8 == 0;
    }
}
public class Tests : IClassFixture<Fixture>
{
    private readonly Fixture _fixture;
    private readonly ITestOutputHelper _testOutputHelper;

    public Tests(Fixture fixture, ITestOutputHelper testOutputHelper)
    {
        _fixture = fixture;
        _testOutputHelper = testOutputHelper;
        _fixture.Inject(testOutputHelper);
    }
    
    [Fact]
    public void Should_Pack_Two_Registers_Into_One_Package()
    {
        const int maxLimit = 10;
        int[] registers = [1, 10];

        var result = _fixture.Run(maxLimit, false, registers);
        result.Should().BeEquivalentTo((int[][]) [[1, 10]], "limit not exceeded");
    }
    
    [Fact]
    public void Should_Pack_Two_Registers_Into_Two_Package()
    {
        const int maxLimit = 5;
        int[] registers = [1, 10];

        var result = _fixture.Run(maxLimit, false, registers);
        result.Should().BeEquivalentTo((int[][]) [[1], [10]], "limit exceeded");
    }
    
    [Fact]
    public void Should_Choose_The_Best_Combination_With_Less_Garbage_1()
    {
        const int maxLimit = 15;
        int[] registers = [1, 15, 20];

        var result = _fixture.Run(maxLimit, false, registers);
        result.Should().BeEquivalentTo((int[][]) [[1], [15, 20]], "20 - 15 < 15 - 1");
    }
    
    [Fact]
    public void Should_Choose_The_Best_Combination_With_Less_Garbage_2()
    {
        const int maxLimit = 25;
        int[] registers = [1, 25, 45];

        var result = _fixture.Run(maxLimit, false, registers);
        result.Should().BeEquivalentTo((int[][]) [[1], [25, 45]], "45 - 25 < 25 - 1");
    }
    
    [Fact]
    public void Should_Choose_The_Best_Combination_With_Less_Garbage_3()
    {
        const int maxLimit = 5;
        int[] registers = [1, 4, 5, 8, 9, 40];

        var result = _fixture.Run(maxLimit, false, registers);
        result.Should().BeEquivalentTo((int[][]) [[1, 4, 5], [8, 9], [40]], "[[1, 4, 5], [8, 9] [40]] is optimal solution");
    }
    
    [Fact]
    public void Should_Join_Chunks_If_Possible_1()
    {
        const int maxLimit = 5;
        int[] registers = [1, 4, 5, 8, 9, 40];

        var result = _fixture.Run(maxLimit, false, registers);
        result.Should().BeEquivalentTo((int[][]) [[1, 4, 5], [8, 9], [40]], "[[1, 4, 5], [8, 9], [40]] is optimal solution");
    }
    
    [Fact]
    public void Should_Join_Chunks_If_Possible_2()
    {
        const int maxLimit = 4;
        int[] registers = [1, 4, 5, 8, 9, 40];

        var result = _fixture.Run(maxLimit, false, registers);
        result.Should().BeEquivalentTo((int[][]) [[1], [4, 5], [8, 9], [40]], "[[1], [4, 5], [8, 9], [40]] is optimal solution");
    }
    
    [Fact]
    public void Should_Join_Chunks_If_Possible_3()
    {
        const int maxLimit = 6;
        int[] registers = [1, 2, 3, 4, 5, 6];

        var result = _fixture.Run(maxLimit, false, registers);
        result.Should().BeEquivalentTo((int[][]) [[1, 2, 3, 4, 5, 6]], "[[1, 2, 3, 4, 5, 6]] is optimal solution");
    }
    
    [Fact]
    public void Should_Join_Chunks_If_Possible_4()
    {
        const int maxLimit = 25;
        int[] registers = [1, 15, 25];

        var result = _fixture.Run(maxLimit, false, registers);
        result.Should().BeEquivalentTo((int[][]) [[1, 15, 25]], "[[1, 15, 25]] is optimal solution");
    }
    
    [Theory]
    [InlineData(1024,  true, 16394)]
    [InlineData(256,  false, 16394)]
    public void Should_Handle_Large_Amount_Of_Registers_Better_Than_Straightforward_Greedy(int maxLimit, bool legacy_coilsCompatibility, int count)
    {
        var registers = Enumerable.Range(0, count)
            .Select(_ => RandomNumberGenerator.GetInt32(0, 1_000_000))
            .Distinct()
            .OrderBy(x => x)
            .ToArray();
        _fixture.Run(maxLimit, legacy_coilsCompatibility, registers);
    }

    [Fact]
    public void Registers16394()
    {
        var registers = File.ReadAllText("registers.txt").Split(", ").Select(int.Parse).ToArray();
        _fixture.Run(256, false, registers);
    }
}
