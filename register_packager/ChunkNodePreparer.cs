namespace register_packager;


internal record ChunkNodeResult(ChunkNode Head);
internal record WriteChunkNodeResult(ChunkNode Head) : ChunkNodeResult(Head);
internal record ReadChunkNodeResult(ChunkNode Head) : ChunkNodeResult(Head);
internal static class ChunkNodePreparer
{
    internal static ChunkNodeResult Prepare(ChunkPreparerOptions options, int[] registers)
    {
        ArgumentOutOfRangeException.ThrowIfZero(registers.Length);

        var head = ChunkNode.CreateFictiveNode();
        var chunkStart = 0;
        var currentLimit = 1;
        var index = 1;
        while (index < registers.Length)
        {
            if (options.Legacy_CoilsCompatibility && !Chunk.IsLegacy_CoilsCompatible(registers.AsSpan()[chunkStart..(index + 1)]))
            {
                AppendReset();
            }
            else
            {
                var distance = registers[index] - registers[index - 1];
                currentLimit += distance;
                if (currentLimit > options.MaxLimit || (!options.ReadOnlyMode && distance > 1))
                {
                    AppendReset();
                }
            }
            index++;
        }
        head.Append(registers[chunkStart..index]);
        ArgumentNullException.ThrowIfNull(head.Next);

        return options.ReadOnlyMode ? new ReadChunkNodeResult(head.Next) : new WriteChunkNodeResult(head.Next);

        void AppendReset()
        {
            head.Append(registers[chunkStart..index]);
            currentLimit = 1;
            chunkStart = index;
        }
    }
}
