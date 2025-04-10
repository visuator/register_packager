namespace register_packager;

public static class ChunkPreparer
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
            var distance = registers[index] - registers[index - 1];
            currentLimit += distance;
            if (currentLimit > options.MaxLimit)
            {
                root.Append(registers[chunkStart..index]);
                currentLimit = 1;
                chunkStart = index;
            }
            index++;
        }
        root.Append(registers[chunkStart..index]);
        ArgumentNullException.ThrowIfNull(root.Next);
        
        return root.Next;
    }
}