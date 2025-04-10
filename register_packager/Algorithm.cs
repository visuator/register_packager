namespace register_packager;

public class Algorithm
{
    private readonly ChunkPreparerOptions _options;

    public Algorithm(Action<ChunkPreparerOptions> setup)
    {
        _options = new();
        setup(_options);
    }

    public int[][] Solve(int[] registers)
    {
        var root = ChunkPreparer.Prepare(_options, registers);
        var result = Worker.Work(_options, root);
        return result.GetRegisterChunks().ToArray();
    }
}