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
            .ReadAllText(@"C:\Users\Workstation\RiderProjects\register_packager\register_packager_benchmarks\bin\Release\net9.0\registers.txt")
            .Split(", ").Select(int.Parse).ToArray();
    }
    
    [Benchmark]
    public void On16394_RegistersWithMax256()
    {
        _ = new ChunkPackager(x =>
        {
            x.MaxLimit = 256;
            x.ReadOnlyMode = true;
        }).Package(_registers);
    }
}
