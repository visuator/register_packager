namespace register_packager;

internal record ChunkNodeResult(ChunkNode Head);
internal record WriteChunkNodeResult(ChunkNode Head) : ChunkNodeResult(Head);
internal record ReadChunkNodeResult(int Depth, ChunkNode Head) : ChunkNodeResult(Head);
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
        var depth = 0;
        while (index < registers.Length)
        {
            var distance = registers[index] - registers[index - 1];
            var t = currentLimit + distance;
            if (!options.ChunkOptions.IsLegacy_CoilsCompatible(t))
            {
                node = node.Append(registers[chunkStart..index]);
                depth++;

                currentLimit = 1;
                chunkStart = index;
            }
            else
            {
                currentLimit += distance;
                if (currentLimit > options.MaxLimit || !options.ReadOnlyMode && distance > 1)
                {
                    node = node.Append(registers[chunkStart..index]);
                    depth++;

                    currentLimit = 1;
                    chunkStart = index;
                }
            }
            index++;
        }
        _ = node.Append(registers[chunkStart..index]);
        depth++;

        var result = head.Next;
        ArgumentNullException.ThrowIfNull(result);

        return options.ReadOnlyMode ? new ReadChunkNodeResult(depth, result) : new WriteChunkNodeResult(result);
    }

    internal ReadChunkNodeResult Prepare(ChunkNode source)
    {
        var current = new ChunkNode(Chunk.Empty);
        var head = current;
        var depth = 0;
        var concat = new List<int>();

        var next = source;
        var currentLimit = 1;

        while (next is not null && next.Chunk.Length != 0)
        {
            foreach (var c in next.Chunk)
            {
                if (concat.Count == 0)
                {
                    concat.Add(c);
                    continue;
                }

                var prev = concat[^1];
                var delta = c - prev;
                var t = currentLimit + delta;

                if (t <= options.MaxLimit && options.ChunkOptions.IsLegacy_CoilsCompatible(t))
                {
                    concat.Add(c);
                    currentLimit += delta;
                }
                else
                {
                    current = current.Append(concat.ToArray());
                    concat.Clear();
                    concat.Add(c);
                    currentLimit = 1;
                    depth++;
                }
            }
            next = next.Next;
        }

        if (concat.Count > 0)
        {
            current.Append(concat.ToArray());
        }

        ArgumentNullException.ThrowIfNull(head.Next);
        return new(depth, head.Next);
    }
}
