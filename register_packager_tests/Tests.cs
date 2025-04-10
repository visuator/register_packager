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
    public int[][] Run(int maxLimit, int[] registers)
    {
        var result = new Algorithm(x =>
        {
            x.MaxLimit = maxLimit;
        }).Solve(registers);
        var greedy = Chunk(maxLimit, registers).ToArray();
        
        File.WriteAllText("registers.txt", string.Join(", ", registers));
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
    private readonly ITestOutputHelper _testOutputHelper;
    public Tests(ITestOutputHelper testOutputHelper, Fixture fixture)
    {
        _fixture = fixture;
        _fixture.Inject(testOutputHelper);
        _testOutputHelper = testOutputHelper;
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

    [Fact]
    public void Test_Case_1_2ML_02()
    {
        const int maxLimit = 125;
        int[] registers =
        [
            33, 35, 36, 38, 39, 40, 41, 43, 44, 45, 46, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 60, 61, 62, 63, 64,
            65, 66, 67, 68, 69, 70, 71, 72, 73, 75, 76, 77, 78, 79, 80, 81, 82, 83, 85, 86, 87, 88, 89, 90, 91, 92, 93,
            95, 96, 97, 98, 99, 100, 101, 102, 103, 105, 106, 107, 108, 109, 110, 111, 112, 113, 115, 116, 117, 118,
            119, 120, 121, 122, 123, 125, 126, 127, 128, 129, 130, 131, 132, 133, 135, 136, 137, 138, 139, 140, 141,
            142, 143, 145, 146, 147, 148, 149, 150, 151, 152, 153, 155, 156, 157, 158, 159, 160, 161, 162, 163, 165,
            166, 167, 168, 169, 170, 171, 172, 173, 175, 176, 177, 178, 179, 180, 181, 182, 183, 185, 186, 187, 188,
            189, 190, 191, 192, 193, 195, 196, 197, 198, 199, 200, 201, 202, 203, 205, 206, 207, 208, 209, 210, 211,
            212, 213, 215, 216, 217, 218, 219, 220, 221, 222, 223, 225, 226, 227, 228, 229, 230, 231, 232, 233, 235,
            236, 237, 238, 239, 240, 241, 242, 243, 245, 246, 247, 248, 249, 250, 251, 252, 253, 255, 256, 257, 258,
            259, 260, 261, 262, 263, 265, 266, 267, 268, 269, 270, 271, 272, 273, 275, 276, 277, 278, 279, 280, 281,
            282, 283, 285, 286, 287, 288, 289, 290, 291, 292, 293, 295, 296, 297, 298, 299, 300, 301, 302, 303, 305,
            306, 307, 308, 309, 310, 311, 312, 313, 315, 316, 317, 318, 319, 320, 321, 322, 323, 325, 326, 327, 328,
            329, 330, 331, 332, 333, 335, 336, 337, 338, 339, 340, 341, 342, 343, 346, 347, 348, 349, 350, 351
        ];
        int[][] chunks =
        [
            [
                33, 35, 36, 38, 39, 40, 41, 43, 44, 45, 46, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 60, 61, 62, 63,
                64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 75, 76, 77, 78, 79, 80, 81, 82, 83, 85, 86, 87, 88, 89, 90, 91,
                92, 93, 95, 96, 97, 98, 99, 100, 101, 102, 103
            ],
            [
                105, 106, 107, 108, 109, 110, 111, 112, 113, 115, 116, 117, 118, 119, 120, 121, 122, 123, 125, 126, 127,
                128, 129, 130, 131, 132, 133, 135, 136, 137, 138, 139, 140, 141, 142, 143, 145, 146, 147, 148, 149, 150,
                151, 152, 153, 155, 156, 157, 158, 159, 160, 161, 162, 163, 165, 166, 167, 168, 169, 170, 171, 172, 173,
                175, 176, 177, 178, 179, 180, 181, 182, 183, 185, 186, 187, 188, 189, 190, 191, 192, 193, 195, 196, 197,
                198, 199, 200, 201, 202, 203, 205, 206, 207, 208, 209, 210, 211, 212, 213, 215, 216, 217, 218, 219, 220,
                221, 222, 223, 225, 226, 227, 228, 229
            ],
            [
                230, 231, 232, 233, 235, 236, 237, 238, 239, 240, 241, 242, 243, 245, 246, 247, 248, 249, 250, 251, 252,
                253, 255, 256, 257, 258, 259, 260, 261, 262, 263, 265, 266, 267, 268, 269, 270, 271, 272, 273, 275, 276,
                277, 278, 279, 280, 281, 282, 283, 285, 286, 287, 288, 289, 290, 291, 292, 293, 295, 296, 297, 298, 299,
                300, 301, 302, 303, 305, 306, 307, 308, 309, 310, 311, 312, 313, 315, 316, 317, 318, 319, 320, 321, 322,
                323, 325, 326, 327, 328, 329, 330, 331, 332, 333, 335, 336, 337, 338, 339, 340, 341, 342, 343, 346, 347,
                348, 349, 350, 351
            ]
        ];

        var sourceGarbage = chunks.Sum(Fixture.CalculateGarbage);
        var newChunks = _fixture.Run(maxLimit, registers);
        var newGarbage = newChunks.Sum(Fixture.CalculateGarbage);

        newGarbage.Should().BeLessThanOrEqualTo(sourceGarbage);
        newChunks.Should().HaveCountLessThanOrEqualTo(chunks.Length);
        
        _testOutputHelper.WriteLine($"Source = [Chunks: {chunks.Length}, Garbage: {sourceGarbage}]");
        _testOutputHelper.WriteLine($"New = [Chunks: {newChunks.Length}, Garbage: {newGarbage}]");
    }
    
    [Theory]
    [InlineData(256, 100_000)]
    public void Should_Handle_Large_Amount_Of_Registers_Better_Than_Straightforward_Greedy(int maxLimit, int count)
    {
        var registers = Enumerable.Range(0, count)
            .Select(_ => RandomNumberGenerator.GetInt32(0, count))
            .Distinct()
            .OrderBy(x => x)
            .ToArray();
        _fixture.Run(maxLimit, registers);
    }
}