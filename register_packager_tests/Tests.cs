using System.IO.Hashing;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using FluentAssertions;
using register_packager;
using Xunit.Abstractions;

namespace register_packager_tests;

public class Fixture
{
    public record struct RunResult(Chunk[] GreedyChunks, Chunk[] ResultChunks);

    private ITestOutputHelper _testOutputHelper = null!;
    internal void Inject(ITestOutputHelper testOutputHelper) => _testOutputHelper = testOutputHelper;
    internal RunResult Run(int maxLimit, bool legacy_coilsCompatibility, int[] registers)
    {
        var options = new ChunkPreparerOptions() { Legacy_CoilsCompatibility = legacy_coilsCompatibility, MaxLimit = maxLimit, ReadOnlyMode = true };
        var preparer = new ChunkNodePreparer(options);
        var packager = new ReadChunkPackager(options, preparer);
        var chunks = preparer.Prepare(registers) as ReadChunkNodeResult;
        ArgumentNullException.ThrowIfNull(chunks);

        var result = packager.Package(chunks.Head).GetChunks().ToArray();
        var greedy = chunks.Head.GetChunks().ToArray();

        DefaultAsserts(options, registers, greedy, result);
        return new(greedy, result);
    }
    internal int CalculateGarbage(ReadOnlySpan<int> registers)
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
    internal record struct GenerateRegistersOptions(
        int Count,
        int Min = 0,
        int Max = 1_000_000);
    internal async Task<int[]> GetOrGenerateRegisters(GenerateRegistersOptions options)
    {
        var fileName = GetFileName();
        if (File.Exists(fileName))
        {
            var content = await File.ReadAllLinesAsync(fileName);
            return content.Select(int.Parse).ToArray();
        }
        else
        {
            var registers = Enumerable.Range(0, options.Count)
                .Select(_ => RandomNumberGenerator.GetInt32(options.Min, options.Max))
                .Distinct()
                .OrderBy(x => x)
                .ToArray();
            var content = string.Join('\n', registers);
            await File.WriteAllTextAsync(fileName, content);
            return registers;
        }

        string GetFileName() => $"{Hash(options.ToString())}.txt";
    }
    private static Guid Hash(string src)
    {
        var hash = new XxHash128();
        hash.Append(MemoryMarshal.AsBytes(src.AsSpan()));
        Span<byte> buffer = stackalloc byte[16];
        hash.GetCurrentHash(buffer);
        return new Guid(buffer, true);
    }
    private static void DefaultAsserts(ChunkPreparerOptions options, int[] registers, Chunk[] greedyChunks, Chunk[] chunks)
    {
        chunks.Should().NotBeEmpty();
        
        var flattenChunks = chunks.SelectMany(x => x).ToArray();
        flattenChunks.Should().BeEquivalentTo(registers);

        chunks.Should().AllSatisfy(x => CalculateDistance(x.AsArray()).Should().BeLessThanOrEqualTo(options.MaxLimit));
        chunks.Should().HaveCountLessThanOrEqualTo(greedyChunks.Length);

        if (flattenChunks.Length == greedyChunks.Length)
        {
            chunks.Sum(x => CalculateDistance(x.AsArray())).Should().BeLessThanOrEqualTo(greedyChunks.Sum(x => x.CalculateDistance()));
        }

        if (options.Legacy_CoilsCompatibility)
        {
            chunks.Should().AllSatisfy(x => IsLegacy_CoilsCompatible(x.AsArray()).Should().BeTrue());
        }
    }
    private static int CalculateDistance(ReadOnlySpan<int> registers) => registers.Length > 0 ? registers[^1] - registers[0] + 1 : 0;
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

        var (_, result) = _fixture.Run(maxLimit, false, registers);
        result.Select(x => x.AsArray()).Should().BeEquivalentTo((int[][]) [[1, 10]], "limit not exceeded");
    }
    
    [Fact]
    public void Should_Pack_Two_Registers_Into_Two_Package()
    {
        const int maxLimit = 5;
        int[] registers = [1, 10];

        var (_, result) = _fixture.Run(maxLimit, false, registers);
        result.Select(x => x.AsArray()).Should().BeEquivalentTo((int[][]) [[1], [10]], "limit exceeded");
    }
    
    [Fact]
    public void Should_Choose_The_Best_Combination_With_Less_Garbage_1()
    {
        const int maxLimit = 15;
        int[] registers = [1, 15, 20];

        var (_, result) = _fixture.Run(maxLimit, false, registers);
        result.Select(x => x.AsArray()).Should().BeEquivalentTo((int[][]) [[1], [15, 20]], "20 - 15 < 15 - 1");
    }
    
    [Fact]
    public void Should_Choose_The_Best_Combination_With_Less_Garbage_2()
    {
        const int maxLimit = 25;
        int[] registers = [1, 25, 45];

        var (_, result) = _fixture.Run(maxLimit, false, registers);
        result.Select(x => x.AsArray()).Should().BeEquivalentTo((int[][]) [[1], [25, 45]], "45 - 25 < 25 - 1");
    }
    
    [Fact]
    public void Should_Choose_The_Best_Combination_With_Less_Garbage_3()
    {
        const int maxLimit = 5;
        int[] registers = [1, 4, 5, 8, 9, 40];

        var (_, result) = _fixture.Run(maxLimit, false, registers);
        result.Select(x => x.AsArray()).Should().BeEquivalentTo((int[][]) [[1, 4, 5], [8, 9], [40]], "[[1, 4, 5], [8, 9] [40]] is optimal solution");
    }
    
    [Fact]
    public void Should_Join_Chunks_If_Possible_1()
    {
        const int maxLimit = 5;
        int[] registers = [1, 4, 5, 8, 9, 40];

        var (_, result) = _fixture.Run(maxLimit, false, registers);
        result.Select(x => x.AsArray()).Should().BeEquivalentTo((int[][]) [[1, 4, 5], [8, 9], [40]], "[[1, 4, 5], [8, 9], [40]] is optimal solution");
    }
    
    [Fact]
    public void Should_Join_Chunks_If_Possible_2()
    {
        const int maxLimit = 4;
        int[] registers = [1, 4, 5, 8, 9, 40];

        var (_, result) = _fixture.Run(maxLimit, false, registers);
        result.Select(x => x.AsArray()).Should().BeEquivalentTo((int[][]) [[1], [4, 5], [8, 9], [40]], "[[1], [4, 5], [8, 9], [40]] is optimal solution");
    }
    
    [Fact]
    public void Should_Join_Chunks_If_Possible_3()
    {
        const int maxLimit = 6;
        int[] registers = [1, 2, 3, 4, 5, 6];

        var (_, result) = _fixture.Run(maxLimit, false, registers);
        result.Select(x => x.AsArray()).Should().BeEquivalentTo((int[][]) [[1, 2, 3, 4, 5, 6]], "[[1, 2, 3, 4, 5, 6]] is optimal solution");
    }
    
    [Fact]
    public void Should_Join_Chunks_If_Possible_4()
    {
        const int maxLimit = 25;
        int[] registers = [1, 15, 25];

        var (_, result) = _fixture.Run(maxLimit, false, registers);
        result.Select(x => x.AsArray()).Should().BeEquivalentTo((int[][]) [[1, 15, 25]], "[[1, 15, 25]] is optimal solution");
    }

    [Fact]
    public void Test_Case_1()
    {
        var registers = File.ReadAllText("registers.txt").Split(", ").Select(int.Parse).OrderBy(x => x).ToArray();

        _fixture.Run(125, false, registers);
    }
    
    [Fact]
    public async Task Should_Handle_Large_Amount_Of_Registers_Better_Than_Straightforward_Greedy_Default()
    {
        var registers = await _fixture.GetOrGenerateRegisters(new(10_000));
        var (greedy, result) = _fixture.Run(125, false, registers);

        var message = $"{JoinChunks(greedy)} -> [{SumGarbage(greedy)}]\n" +
                      $"{JoinChunks(result)} -> [{SumGarbage(result)}]";

        await Verify(message);

        string JoinChunks(Chunk[] chunks) => string.Join(", ", chunks.Select(x => x.ToString()));
        int SumGarbage(Chunk[] chunks) => chunks.Sum(x => _fixture.CalculateGarbage(x.AsArray()));
    }

    [Fact]
    public async Task Should_Handle_Large_Amount_Of_Registers_Better_Than_Straightforward_Greedy_Coils()
    {
        var registers = await _fixture.GetOrGenerateRegisters(new(10_000));
        var (greedy, result) = _fixture.Run(2000, true, registers);

        var message = $"{JoinChunks(greedy)} -> [{SumGarbage(greedy)}]\n" +
                      $"{JoinChunks(result)} -> [{SumGarbage(result)}]";

        await Verify(message);

        string JoinChunks(Chunk[] chunks) => string.Join(", ", chunks.Select(x => x.ToString()));
        int SumGarbage(Chunk[] chunks) => chunks.Sum(x => _fixture.CalculateGarbage(x.AsArray()));
    }
}
