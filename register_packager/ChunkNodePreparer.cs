namespace register_packager;

internal record ChunkNodeResult(ChunkNode Head);
internal record WriteChunkNodeResult(ChunkNode Head) : ChunkNodeResult(Head);
internal record ReadChunkNodeResult(ChunkNode Head) : ChunkNodeResult(Head);
internal class ChunkNodePreparer(ChunkPreparerOptions options)
{
    internal ChunkNodeResult Prepare(int[] registers)
    {
        ArgumentOutOfRangeException.ThrowIfZero(registers.Length);

        var node = new ChunkNode(Chunk.Empty);
        var head = node;

        var chunkStart = 0;
        var currentLimit = 1;
        var index = 1;
        while (index < registers.Length)
        {
            var distance = registers[index] - registers[index - 1];
            if (!options.ChunkOptions.IsLegacy_CoilsCompatible(currentLimit + distance))
            {
                node = node.Append(registers[chunkStart..index]);
                currentLimit = 1;
                chunkStart = index;
            }
            else
            {
                currentLimit += distance;
                if (currentLimit > options.MaxLimit || !options.ReadOnlyMode && distance > 1)
                {
                    node = node.Append(registers[chunkStart..index]);
                    currentLimit = 1;
                    chunkStart = index;
                }
            }
            index++;
        }
        _ = node.Append(registers[chunkStart..index]);

        var result = head.Next;
        ArgumentNullException.ThrowIfNull(result);

        return options.ReadOnlyMode ? new ReadChunkNodeResult(result) : new WriteChunkNodeResult(result);
    }

    internal ChunkNode Prepare(ChunkNode source)
    {
        var node = new ChunkNode(Chunk.Empty);
        var head = node;
        var concat = new List<int>();

        var next = source;
        var currentLimit = 1;

        while (next is not null && next.Chunk.Length != 0)
        {
            foreach (var current in next.Chunk)
            {
                if (concat.Count == 0)
                {
                    concat.Add(current);
                    continue;
                }

                var previous = concat[^1];
                var delta = current - previous;
                var distance = currentLimit + delta;

                if (distance <= options.MaxLimit && options.ChunkOptions.IsLegacy_CoilsCompatible(distance))
                {
                    concat.Add(current);
                    currentLimit += delta;
                }
                else
                {
                    node = node.Append(concat.ToArray());
                    concat = [current];
                    currentLimit = 1;
                }
            }
            next = next.Next;
        }

        if (concat.Count > 0)
        {
            node.Append(concat.ToArray());
        }

        ArgumentNullException.ThrowIfNull(head.Next);
        return head.Next;
    }
}
