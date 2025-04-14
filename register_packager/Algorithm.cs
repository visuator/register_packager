namespace register_packager;

public class Algorithm
{
    private readonly ChunkPreparerOptions _options;

    public Algorithm(Action<ChunkPreparerOptions> setup)
    {
        _options = new();
        setup(_options);
    }

    public int[][] Solve(int[] registers) =>
        Worker.Work(_options, GreedyPreparer.Prepare(_options, registers)).GetChunks()
            .Select(x => x.Registers)
            .ToArray();
}