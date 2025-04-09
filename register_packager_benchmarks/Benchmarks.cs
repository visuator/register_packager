using BenchmarkDotNet.Attributes;
using register_packager;

namespace register_packager_benchmarks;

[SimpleJob]
[MemoryDiagnoser]
public class Benchmarks
{
    private int[] _registers = null!;
    
    [GlobalSetup]
    public void Setup()
    {
        _registers = File
            .ReadAllText(@"<path>")
            .Split(", ").Select(int.Parse).ToArray();
    }
    
    [Benchmark]
    public void On100_000_RegistersWithMax256()
    {
        _ = Algorithm.Solve(256, _registers);
    }
}