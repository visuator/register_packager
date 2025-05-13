namespace register_packager;

public class ChunkPackager
{
    private readonly ChunkPreparerOptions _options;

    public ChunkPackager(Action<ChunkPreparerOptions> setup)
    {
        _options = new();
        setup(_options);
    }

    public ChunkPackager(ChunkPreparerOptions options)
    {
        _options = options;
    }

    public int[][] Package(int[] registers)
    {
        var preparer = new ChunkNodePreparer(_options);
        var packager = new ReadChunkPackager(_options, preparer);
        return (preparer.Prepare(registers) switch
            {
                WriteChunkNodeResult wr => wr.Head.GetChunks(),
                ReadChunkNodeResult rr => packager.Package(rr.Head).GetChunks(),
                _ => throw new InvalidOperationException("unknown mode")
            })
            .Select(x => x.ToArray())
            .ToArray();
    }
}
