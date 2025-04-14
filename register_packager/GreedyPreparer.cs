namespace register_packager;

public static class GreedyPreparer
{
    public static ChunkNode Prepare(ChunkPreparerOptions options, int[] registers)
    {
        ArgumentOutOfRangeException.ThrowIfZero(registers.Length);

        var root = ChunkNode.CreateFictiveNode();
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
                currentLimit += registers[index] - registers[index - 1];
                if (currentLimit > options.MaxLimit)
                {
                    AppendReset();
                }   
            }
            index++;
        }
        root.Append(registers[chunkStart..index]);
        ArgumentNullException.ThrowIfNull(root.Next);
        
        return root.Next;

        void AppendReset()
        {
            root.Append(registers[chunkStart..index]);
            currentLimit = 1;
            chunkStart = index;
        }
    }
}