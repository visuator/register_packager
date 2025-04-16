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
        var result = new Algorithm(x =>
        {
            x.MaxLimit = maxLimit;
            x.Legacy_CoilsCompatibility = legacy_coilsCompatibility;
        }).Solve(registers);
        var greedy = GreedyPreparer.Prepare(new ChunkPreparerOptions() { Legacy_CoilsCompatibility = legacy_coilsCompatibility, MaxLimit = maxLimit }, registers).GetChunks().ToArray();
        
        //File.WriteAllText("registers.txt", string.Join(", ", registers));
        //_testOutputHelper.WriteLine($"[{string.Join(", ", registers)}]");
        //_testOutputHelper.WriteLine(string.Empty);
        //_testOutputHelper.WriteLine($"[{string.Join(", ", greedy.Select(x => $"[{string.Join(", ", x.Registers)}]"))}] -> [Chunks: {greedy.Length}, Garbage: {greedy.Sum(x => x.CalculateGarbage())}]");
        //_testOutputHelper.WriteLine(string.Empty);
        _testOutputHelper.WriteLine($"[{string.Join(", ", result.Select(x => $"[{string.Join(", ", x)}]"))}] -> [Chunks: {result.Length}, Garbage: {result.Sum(x => Chunk.CalculateGarbage(x))}]");
        DefaultAsserts(maxLimit, registers, greedy, result);
        return result;
    }
    private static void DefaultAsserts(int maxLimit, int[] registers, Chunk[] greedyChunks, int[][] chunks)
    {
        chunks.Should().NotBeEmpty();
        
        var flattenChunks = chunks.SelectMany(x => x).ToArray();
        flattenChunks.Should().BeEquivalentTo(registers);
        
        chunks.Should().AllSatisfy(x => Chunk.ExcessLimit(maxLimit, x).Should().BeFalse());
        chunks.Should().HaveCountLessThanOrEqualTo(greedyChunks.Length);
        chunks.Sum(x => Chunk.CalculateGarbage(x)).Should().BeLessThanOrEqualTo(greedyChunks.Sum(x => x.CalculateGarbage()));
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

    [Fact]
    public void Test_Case_1()
    {
        int[][] source =
        [
            [
                33, 35, 36, 38, 39, 40, 41, 43, 44, 45, 46, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 60, 61, 62, 63,
                64, 105, 106, 107, 108, 109, 110, 111, 112, 113, 115, 116, 117, 118, 119, 120, 121, 122, 123, 125, 126,
                127, 128, 129, 130, 131, 132, 133, 135, 136, 137, 138, 139, 140, 141, 142, 143, 145, 146, 147, 148, 149,
                150, 151, 152, 153, 155, 156, 157, 158, 159, 160, 161, 162, 163, 165, 166, 167, 168, 169, 170, 171, 172,
                173, 175, 176, 177, 178, 179, 180, 181, 182, 183, 185, 186, 187, 188, 189, 190, 191, 192, 193, 195, 196,
                197, 198, 199, 200, 201, 202, 203, 205, 206, 207, 208, 209, 210, 211, 212, 213, 215, 216, 217, 218, 219,
                220, 221, 222, 223, 225, 226, 227, 228, 229, 230, 231, 232, 233, 235, 236, 237, 238, 239, 240, 241, 242,
                243, 245, 246, 247, 248, 249, 250, 251, 252, 253, 255, 256, 257, 258, 259, 260, 261, 262, 263, 265, 266,
                267, 268, 269, 270, 271, 272, 273, 275, 276, 277, 278, 279, 280, 281, 282, 283, 285, 286, 287, 288, 289,
                290, 291, 292, 293, 295, 296, 297, 298, 299, 300, 301, 302, 303, 305, 306, 307, 308, 309, 310, 311, 312,
                313, 315, 316, 317, 318, 319, 320, 321, 322, 323, 325, 326, 327, 328, 329, 330, 331, 332, 333, 335, 336,
                337, 338, 339, 340, 341, 342, 343, 345, 346, 347, 348, 349, 350, 351, 352, 353, 355, 356, 357, 358, 359,
                360, 361, 362, 363, 365, 366, 367, 368, 369, 370, 371, 372, 373, 375, 376, 377, 378, 379, 380, 381, 382,
                383, 385, 386, 387, 388, 389, 390, 391, 392, 393, 395, 396, 397, 398, 399, 400, 401, 402, 403, 405, 406,
                407, 408, 409, 410, 411, 412, 413, 415, 416, 417, 418, 419, 420, 421, 422, 423, 425, 426, 427, 428, 429,
                430, 431, 432, 433, 435, 436, 437, 438, 439, 440, 441, 442, 443
            ]
        ];
        foreach (var registers in source)
        {
            _fixture.Run(125, false, registers);
        }
    }

    [Fact]
    public void Test_Case_2()
    {
        int[] source = [1, 2, 3, 4, 5, 10, 11, 12, 13, 16, 17, 19, 21, 23];
        _fixture.Run(10, false, source);
    }
    
    [Theory]
    [InlineData(1024,  true, 16394)]
    [InlineData(125,  false, 16394)]
    public void Should_Handle_Large_Amount_Of_Registers_Better_Than_Straightforward_Greedy(int maxLimit, bool legacy_coilsCompatibility, int count)
    {
        var registers = Enumerable.Range(0, count)
            .Select(_ => RandomNumberGenerator.GetInt32(0, count))
            .Distinct()
            .OrderBy(x => x)
            .ToArray();
        var result = _fixture.Run(maxLimit, legacy_coilsCompatibility, registers);
        if (legacy_coilsCompatibility)
        {
            result.Should().AllSatisfy(x =>
            {
                Chunk.IsLegacy_CoilsCompatible(x).Should().BeTrue();
            });
        }
    }
}