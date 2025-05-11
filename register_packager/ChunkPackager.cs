namespace register_packager;

public class ChunkPackager
{
    private readonly ChunkPreparerOptions _options;

    public ChunkPackager(Action<ChunkPreparerOptions> setup)
    {
        _options = new();
        setup(_options);
    }

    public int[][] Package(int[] registers)
    {
        return (ChunkNodePreparer.Prepare(_options, registers) switch
            {
                WriteChunkNodeResult wr => wr.Head.GetChunks(),
                ReadChunkNodeResult rr => ReadChunkPackager.Package(_options, rr.Head).GetChunks(),
                _ => throw new InvalidOperationException("unknown mode")
            })
            .Select(x => x.Registers)
            .ToArray();
    }
}
