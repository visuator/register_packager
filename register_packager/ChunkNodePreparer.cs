namespace register_packager;


internal record ChunkNodeResult(ChunkNode Head);
internal record WriteChunkNodeResult(ChunkNode Head) : ChunkNodeResult(Head);
internal record ReadChunkNodeResult(ChunkNode Head) : ChunkNodeResult(Head);
internal class ChunkNodePreparer(ChunkPreparerOptions options)
{
    internal ChunkNodeResult Prepare(int[] registers)
    {
        var node = PrepareInternal(registers);
        return options.ReadOnlyMode ? new ReadChunkNodeResult(node) : new WriteChunkNodeResult(node);
    }
    internal ChunkNode Prepare(ChunkNode head) => PrepareInternal(head.GetChunks().SelectMany(x => x).ToArray());

    private ChunkNode PrepareInternal(ReadOnlySpan<int> registers)
    {
        ArgumentOutOfRangeException.ThrowIfZero(registers.Length);

        var head = ChunkNode.CreateFictiveNode();
        var chunkStart = 0;
        var currentLimit = 1;
        var index = 1;
        while (index < registers.Length)
        {
            if (options.Legacy_CoilsCompatibility && !Chunk.IsLegacy_CoilsCompatible(registers[chunkStart..(index + 1)]))
            {
                head.Append(registers[chunkStart..index]);
                currentLimit = 1;
                chunkStart = index;
            }
            else
            {
                var distance = registers[index] - registers[index - 1];
                currentLimit += distance;
                if (currentLimit > options.MaxLimit || !options.ReadOnlyMode && distance > 1)
                {
                    head.Append(registers[chunkStart..index]);
                    currentLimit = 1;
                    chunkStart = index;
                }
            }
            index++;
        }
        head.Append(registers[chunkStart..index]);
        ArgumentNullException.ThrowIfNull(head.Next);

        return head.Next;
    }
}
